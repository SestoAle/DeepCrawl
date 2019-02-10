from tensorforce.execution.runner import Runner
import time
from six.moves import xrange
import warnings
from inspect import getargspec

import numpy as np

class DeepCrawlRunner(Runner):

    def __init__(self, agent, environment, repeat_actions=1, history=None, id_=0, curriculum = None):
        self.mean_entropies = []
        self.std_entropies = []
        self.history = history
        self.curriculum = curriculum
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

        # episode loop
        while True:
            episode_start_time = time.time()

            config = self.set_curriculum(self.curriculum, np.sum(self.episode_timesteps))
            if i ==0:
               print(config)
            i=1
            self.environment.set_config(config)
            state = self.environment.reset()
            self.agent.reset()
            self.local_entropies = []

            # Update global counters.
            self.global_episode = self.agent.episode  # global value (across all agents)
            self.global_timestep = self.agent.timestep  # global value (across all agents)

            episode_reward = 0
            self.current_timestep = 0

            # time step (within episode) loop
            while True:
                action, fetches = self.agent.act(states=state, deterministic=deterministic, fetch_tensors=['probabilities'])
                probs = fetches['probabilities']
                self.environment.add_probs(probs)
                self.local_entropies.append(self.environment.get_last_entropy())

                reward = 0
                for _ in xrange(self.repeat_actions):
                    state, terminal, step_reward = self.environment.execute(action=action)
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

                if terminal or self.agent.should_stop():  # TODO: should_stop also terminate?
                    break

                if sleep is not None:
                    time.sleep(sleep)

            # Update our episode stats.
            time_passed = time.time() - episode_start_time
            self.episode_rewards.append(episode_reward)
            self.episode_timesteps.append(self.current_timestep)
            self.episode_times.append(time_passed)
            self.mean_entropies.append(np.mean(self.local_entropies))
            self.std_entropies.append(np.std(self.local_entropies))

            self.global_episode += 1

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