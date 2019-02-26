from tensorforce.environments import Environment
import numpy as np
import math
import signal
import time
# Load UnityEnvironment and my wrapper
from mlagents.envs import UnityEnvironment

class UnityEnvWrapper(Environment):
    def __init__(self, game_name = None, no_graphics = True, seed = None, worker_id=0, size_global = 8, size_two = 5, with_local = True, size_three = 3, with_stats=True, size_stats = 1,
                 with_previous=True, manual_input = False, config = None, curriculum = None, verbose = False, agent_separate = False, agent_stats = 6,
                 with_class=False, with_hp = False, size_class = 3, double_agent = False):

        self.probabilities = []
        self.size_global = size_global
        self.size_two = size_two
        self.with_local = with_local
        self.size_three = size_three
        self.with_stats = with_stats
        self.size_stats = size_stats
        self.manual_input = manual_input
        self.with_previous = with_previous
        self.config = config
        self.curriculum = curriculum
        self.verbose = verbose
        self.agent_separate = agent_separate
        self.agent_stats = agent_stats
        self.with_class = with_class
        self.with_hp = with_hp
        self.size_class = size_class
        self.double_agent = double_agent
        self.game_name = game_name
        self.no_graphics = no_graphics
        self.seed = seed
        self.worker_id = worker_id
        self.unity_env = self.open_unity_environment(game_name, no_graphics, seed, worker_id)
        self.default_brain = self.unity_env.brain_names[0]

    count = 0

    def get_input_observation(self, env_info, action = None):
        size = self.size_global * self.size_global

        global_in = env_info.vector_observations[0][:size]
        global_in = np.flip(np.transpose(np.reshape(global_in, (self.size_global, self.size_global))), 0)

        if self.with_local:
            local_in = env_info.vector_observations[0][size:(size + (self.size_two * self.size_two))]
            local_in = np.flip(np.transpose(np.reshape(local_in, (self.size_two, self.size_two))), 0)

            local_in_two = env_info.vector_observations[0][(size + (self.size_two * self.size_two)):(
                    size + (self.size_two * self.size_two) + (self.size_three * self.size_three))]
            local_in_two = np.flip(np.transpose(np.reshape(local_in_two, (self.size_three, self.size_three))), 0)

        if self.with_local and self.with_stats:
            stats = env_info.vector_observations[0][
                    (size + (self.size_two * self.size_two) + (self.size_three * self.size_three)):
                    (size + (self.size_two * self.size_two) + (self.size_three * self.size_three) + self.size_stats)]

        if self.with_local and self.with_stats and self.with_hp:
            hp = env_info.vector_observations[0][
                 (size + (self.size_two * self.size_two) + (self.size_three * self.size_three)):
                 (size + (self.size_two * self.size_two) + (self.size_three * self.size_three) + 4)]
            stats = env_info.vector_observations[0][
                    (size + (self.size_two * self.size_two) + (self.size_three * self.size_three) + 4):
                    (size + (self.size_two * self.size_two) + (
                                self.size_three * self.size_three) + self.size_stats + 4)]
            self.size_stats += 4

        if self.with_local and self.with_stats and self.with_class:
            agent_class = env_info.vector_observations[0][
                          (size + (self.size_two * self.size_two) + (
                                      self.size_three * self.size_three) + self.size_stats):
                          (size + (self.size_two * self.size_two) + (
                                      self.size_three * self.size_three) + self.size_stats + self.size_class)]

            enemy_class = env_info.vector_observations[0][
                          (size + (self.size_two * self.size_two) + (
                                      self.size_three * self.size_three) + self.size_stats + self.size_class):
                          (size + (self.size_two * self.size_two) + (
                                      self.size_three * self.size_three) + self.size_stats + self.size_class * 2)]

        observation = {
            'global_in': global_in,
            'local_in': local_in
        }

        if self.with_local:
            observation = {
                'global_in': global_in,
                'local_in': local_in,
                'local_in_two': local_in_two
            }
        if self.with_local and self.with_stats:
            observation = {
                'global_in': global_in,
                'local_in': local_in,
                'local_in_two': local_in_two,
                'stats': stats
            }

        if self.with_local and self.with_stats and self.with_previous:
            action_vector = np.zeros(17)
            action_vector[action] = 1

            observation = {
                'global_in': global_in,
                'local_in': local_in,
                'local_in_two': local_in_two,
                'stats': stats,
                'action': action_vector
            }

        if self.with_local and self.with_stats and self.with_previous and self.with_class:
            action_vector = np.zeros(17)
            if action != None:
                action_vector[action] = 1

            observation = {
                'global_in': global_in,
                'local_in': local_in,
                'local_in_two': local_in_two,
                'stats': stats,
                'agent_class': agent_class,
                'enemy_class': enemy_class,
                'action': action_vector
            }

        if self.with_local and self.with_stats and self.with_previous and self.with_hp:
            action_vector = np.zeros(17)
            if action != None:
                action_vector[action] = 1

            observation = {
                'global_in': global_in,
                'local_in': local_in,
                'local_in_two': local_in_two,
                'hp': hp,
                'stats': stats,
                'action': action_vector
            }

        return observation

    def execute(self, action):

        if self.manual_input:
            input_action = input('...')

            try:
                action = int(input_action)
            except ValueError:
                pass

        env_info = None
        signal.alarm(0)
        while env_info == None:
            signal.signal(signal.SIGALRM, self.handler)
            signal.alarm(3000)
            try:
                env_info = self.unity_env.step([action])[self.default_brain]
            except Exception as exc:
                self.close()
                self.unity_env = self.open_unity_environment(self.game_name, self.no_graphics, seed = int(time.time()),
                                                             worker_id=self.worker_id)
                env_info = self.unity_env.reset(train_mode=True, config=self.config)[self.default_brain]
                print("The environment didn't respond, it was necessary to close and reopen it")

        if self.double_agent:
            while len(env_info.vector_observations) <= 0:
                env_info = self.unity_env.step()[self.default_brain]

        reward = env_info.rewards[0]
        done = env_info.local_done[0]

        observation = self.get_input_observation(env_info, action)

        self.count += 1

        if self.verbose:
            print('action = ' + str(action))
            print('reward = ' + str(reward))
            print(observation['global_in'])
            print(observation['stats'])
            print(observation['local_in'])
            print(observation['local_in_two'])
            print('timestep = ' + str(self.count))

        return [observation, done, reward]

    def set_config(self, config):
        self.config = config

    def handler(self, signum, frame):
        print("Timeout!")
        raise Exception("end of time")

    def reset(self):

        self.count = 0

        env_info = None

        while env_info == None:
            signal.signal(signal.SIGALRM, self.handler)
            signal.alarm(60)
            try:
                env_info = self.unity_env.reset(train_mode=True, config=self.config)[self.default_brain]
            except Exception as exc:
                self.close()
                self.unity_env = self.open_unity_environment(self.game_name, self.no_graphics, seed=int(time.time()),
                                                             worker_id=self.worker_id)
                env_info = self.unity_env.reset(train_mode=True, config=self.config)[self.default_brain]
                print("The environment didn't respond, it was necessary to close and reopen it")

        if self.double_agent:
            while len(env_info.vector_observations) <= 0:
                env_info = self.unity_env.step()[self.default_brain]

        observation = self.get_input_observation(env_info)

        if self.verbose:
            print(observation['global_in'])
            print(observation['stats'])
            print(observation['local_in'])
            print(observation['local_in_two'])

        return observation

    def close(self):
        self.unity_env.close()

    def open_unity_environment(self, game_name, no_graphics, seed, worker_id):
        return UnityEnvironment(game_name, no_graphics=no_graphics, seed=seed, worker_id=worker_id)

    def add_probs(self, probs):
        self.probabilities.append(probs[0])

    def get_last_entropy(self):
        entropy = 0
        for prob in self.probabilities[-1]:
            entropy += prob*math.log(prob)

        return -entropy

    @property
    def states(self):
        return dict(shape=(84,), type='float')

    @property
    def actions(self):
        return dict(type='float', num_actions=4)


class Info():
    def __init__(self, string):
        self.item = string

    def items(self):

        return self.item, self.item


