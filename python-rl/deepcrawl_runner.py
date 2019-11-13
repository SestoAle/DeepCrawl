from tensorforce.execution.runner import Runner
import time
from six.moves import xrange
import warnings
from inspect import getargspec
from reward_model.reward_model import RewardModel

import numpy as np

class DeepCrawlRunner(Runner):

    def __init__(self, agent, environment, repeat_actions=1, history=None, id_=0, curriculum = None, reward_model = None,
                 num_policy_updates = 3):
        self.mean_entropies = []
        self.std_entropies = []
        # TODO: change this
        self.real_episode_rewards = []
        self.reward_model_loss = []
        self.reward_model_val_loss = []
        self.history = history
        self.curriculum = curriculum
        self.reward_model = reward_model
        self.num_policy_updates = num_policy_updates
        super(DeepCrawlRunner, self).__init__(agent, environment, repeat_actions, history)

    def set_curriculum(self, curriculum, total_timesteps):

        if curriculum == None:
            return None

        lessons = np.cumsum(curriculum['thresholds'])

        curriculum_step = 0

        for (index, l) in enumerate(lessons):
            if total_timesteps > l:
                curriculum_step = index + 1

        parameters = curriculum['parameters']
        config = {}

        for (par, value) in parameters.items():
            config[par] = value[curriculum_step]

        return config

    def run(self, num_timesteps=None, num_episodes=None, max_episode_timesteps=None, deterministic=False,
            episode_finished=None, summary_report=None, summary_interval=None, timesteps=None, episodes=None, testing=False, sleep=None
            ):

        # deprecation warnings
        if timesteps is not None:
            num_timesteps = timesteps
            warnings.warn("WARNING: `timesteps` parameter is deprecated, use `num_timesteps` instead.",
                          category=DeprecationWarning)
        if episodes is not None:
            num_episodes = episodes
            warnings.warn("WARNING: `episodes` parameter is deprecated, use `num_episodes` instead.",
                          category=DeprecationWarning)

        # figure out whether we are using the deprecated way of "episode_finished" reporting
        old_episode_finished = False
        if episode_finished is not None and len(getargspec(episode_finished).args) == 1:
            old_episode_finished = True

        # Keep track of episode reward and episode length for statistics.
        self.start_time = time.time()

        self.agent.reset()

        if num_episodes is not None:
            num_episodes += self.agent.episode

        if num_timesteps is not None:
            num_timesteps += self.agent.timestep

        i = 0

        # Initialize Reward Model
        if self.reward_model is not None and not testing:
            config = self.set_curriculum(self.curriculum, np.sum(self.episode_timesteps))
            self.environment.set_config(config)
            y = input('Do you want to create new demonstrations? [y/n] ')
            if y == 'y':
                dems, vals = self.reward_model.create_demonstrations(env=self.environment)
            else:
                print('Loading demonstrations...')
                dems, vals = self.reward_model.load_demonstrations()

            print('Demonstrations loaded! We have ' + str(len(dems['obs'])) + " timesteps in these demonstrations")
            print('and ' + str(len(vals['obs'])) + " timesteps in these validations.")

        # episode loop
        while True:
            # # Initialize buffer for reward model
            # if self.reward_model is not None:
            #     policy_traj = {
            #         'obs': [],
            #         'obs_n': [],
            #         'acts': [],
            #         'probs': []
            #     }

            episode_start_time = time.time()
            # Set the correct curriculum phase
            config = self.set_curriculum(self.curriculum, np.sum(self.episode_timesteps))

            if i ==0:
               print(config)
            i=1
            self.environment.set_config(config)

            state = self.environment.reset()
            self.agent.reset()

            # Initialize utility buffers
            # TODO: remove this
            # states = [state]
            # acts = []
            # all_probs = []
            self.local_entropies = []

            # Update global counters.
            self.global_episode = self.agent.episode  # global value (across all agents)
            self.global_timestep = self.agent.timestep  # global value (across all agents)

            episode_reward = 0
            real_episode_reward = 0
            self.current_timestep = 0

            # time step (within episode) loop
            while True:
                action, fetches = self.agent.act(states=state, deterministic=deterministic, fetch_tensors=['probabilities'])
                probs = fetches['probabilities']
                self.environment.add_probs(probs)
                self.local_entropies.append(self.environment.get_last_entropy())

                reward = 0
                for _ in xrange(self.repeat_actions):
                    state_n, terminal, step_reward = self.environment.execute(action=action)
                    real_episode_reward += step_reward

                    # Compute the reward from reward model
                    if self.reward_model is not None:
                        #reward_from_model = self.reward_model.forward([state], [action])
                        self.reward_model.eval()
                        reward_from_model = self.reward_model.forward([state], [action])
                        # Normalize reward
                        reward_from_model = reward_from_model.detach().numpy()
                        self.reward_model.push_reward(reward_from_model)
                        reward_from_model = self.reward_model.normalize_rewards(reward_from_model)
                        reward_from_model = np.squeeze(reward_from_model)
                        # step_reward += reward_from_model
                        step_reward = reward_from_model

                    state = state_n

                    reward += step_reward
                    if terminal:
                        break

                if max_episode_timesteps is not None and self.current_timestep >= max_episode_timesteps:
                    terminal = True

                if not testing:
                    self.agent.observe(terminal=terminal, reward=reward)

                self.global_timestep += 1
                self.current_timestep += 1
                episode_reward += reward

                # Update utility buffers
                # states.append(state)
                # acts.append(action)
                # all_probs.append(np.squeeze(probs)[action])

                if terminal or self.agent.should_stop():  # TODO: should_stop also terminate?
                    break

                if sleep is not None:
                    time.sleep(sleep)

            # Update our episode stats.
            time_passed = time.time() - episode_start_time
            self.episode_rewards.append(episode_reward)
            self.real_episode_rewards.append(real_episode_reward)
            self.episode_timesteps.append(self.current_timestep)
            self.episode_times.append(time_passed)
            self.mean_entropies.append(np.mean(self.local_entropies))
            self.std_entropies.append(np.std(self.local_entropies))

            self.global_episode += 1

            # IRL setting
            # If reward_model, update the buffer to train it
            if self.reward_model is not None and not testing:
                if self.global_episode % self.num_policy_updates == 0:
                    policy_traj = self.get_experience(max_episode_timesteps)
                    loss, val_loss = self.reward_model.train_step(self.reward_model.expert_traj, policy_traj)
                    self.reward_model_loss.append(loss.detach().numpy())
                    self.reward_model_val_loss.append(val_loss)

            # Check, whether we should stop this run.
            if episode_finished is not None:
                # deprecated way (passing in only runner object):
                if old_episode_finished:
                    if not episode_finished(self):
                        break
                # new unified way (passing in BaseRunner AND some worker ID):
                elif not episode_finished(self, self.id):
                    break
            if (num_episodes is not None and self.global_episode >= num_episodes) or \
                    (num_timesteps is not None and self.global_timestep >= num_timesteps) or \
                    self.agent.should_stop():
                break

    def reset(self, history=None):
        super(DeepCrawlRunner, self).reset(history)
        if(history != None):
            self.std_entropies = history.get("std_entropies", list())
            self.mean_entropies = history.get("mean_entropies", list())


    # IRL settings
    def get_experience(self, max_episode_timesteps):
        policy_traj = {
            'obs': [],
            'obs_n': [],
            'acts': [],
            'probs': []
        }

        # For policy update number
        for ep in range(self.num_policy_updates):
            states = []
            probs = []
            actions = []
            state = self.environment.reset()
            states.append(state)

            step = 0
            # While the episode si not finished
            while True:
                step += 1
                # Get the experiences that are not saved in the agent
                action, fetch = self.agent.act(states=state, deterministic=False, independent=True,
                                               fetch_tensors=['probabilities'])
                c_probs = np.squeeze(fetch['probabilities'])
                state, terminal, step_reward = self.environment.execute(action=action)

                states.append(state)
                actions.append(action)
                probs.append(c_probs[action])

                if terminal or step >= max_episode_timesteps:
                    break

            # Saved the last episode experiences
            policy_traj['obs'].extend(states[:-1])
            policy_traj['obs_n'].extend(states[1:])
            policy_traj['acts'].extend(actions)
            policy_traj['probs'].extend(probs)

        # Return all the experience
        return policy_traj