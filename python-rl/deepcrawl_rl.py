import numpy as np

from tensorforce.agents import PPOAgent, VPGAgent, DQNAgent
from tensorforce.execution import Runner

import time
import os

import json


from unity_env_wrapper import UnityEnvWrapper
from export_graph import export_pb
from deepcrawl_runner import DeepCrawlRunner
from reward_model.reward_model import RewardModel

import tensorflow as tf
import argparse

from torchsummary import summary

import datetime

# Neural network structure
net = [
    [
        {
            'type' : 'input',
            'names' : ['global_in']
        },
        {
            "type": "embedding",
            "indices": 12,
            "size": 32
        },
        {
            "type": "conv2d",
            "size": 32,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            "type": "conv2d",
            "size": 64,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            'type': 'flatten'
        },
        {
            'type' : 'output',
            'name' : 'global_out'
        }
    ],
    [
        {
            'type' : 'input',
            'names' : ['local_in']
        },
        {
            "type": "embedding",
            "indices": 12,
            "size": 32
        },
        {
            "type": "conv2d",
            "size": 32,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            "type": "conv2d",
            "size": 64,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            'type': 'flatten'
        },
        {
            'type' : 'output',
            'name' : 'local_out'
        }
    ],
    [
        {
            'type' : 'input',
            'names' : ['local_in_two']
        },
        {
            "type": "embedding",
            "indices": 12,
            "size": 32
        },
        {
            "type": "conv2d",
            "size": 32,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            "type": "conv2d",
            "size": 64,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            'type': 'flatten'
        },
        {
            'type' : 'output',
            'name' : 'local_out_two'
        }
    ],
    [
        {
            'type' : 'input',
            'names' : ['stats']
        },
        {
            "type": "embedding",
            "indices": 82,
            "size": 64
        },
        {
            'type': 'flatten'
        },
        {
            "type": "dense",
            "size": 256,
            "activation": 'relu'
        },
        {
            'type' : 'output',
            'name' : 'stats_out'
        }
    ],
    [
        {
            'type' : 'input',
            'names': ['global_out', 'local_out', 'local_out_two', 'stats_out'],
            'aggregation_type': 'concat',
        },
        {
            "type": "dense",
            "size": 256,
            "activation": 'relu'
        },
        {
            'type' : 'output',
            'name' : 'first_FC'
        }
    ],
    [
        {
            'type' : 'input',
            'names': ['action']
        },
        {
            'type': 'flatten'
        },
        {
            'type': 'output',
            'name': 'action_out'
        }
    ],
    [
        {
            'type' : 'input',
            'names': ['first_FC', 'action_out'],
            'aggregation_type': 'concat',
        },
        {
            "type": "internal_lstm",
            "size": 256,
        }
    ]
]

# Baseline net structure
baseline = [
    [
        {
            'type' : 'input',
            'names' : ['global_in']
        },
        {
            "type": "embedding",
            "indices": 12,
            "size": 32
        },
        {
            "type": "conv2d",
            "size": 32,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            "type": "conv2d",
            "size": 64,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            'type': 'flatten'
        },
        {
            'type' : 'output',
            'name' : 'global_out'
        }
    ],
    [
        {
            'type' : 'input',
            'names' : ['local_in']
        },
        {
            "type": "embedding",
            "indices": 12,
            "size": 32
        },
        {
            "type": "conv2d",
            "size": 32,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            "type": "conv2d",
            "size": 64,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            'type': 'flatten'
        },
        {
            'type' : 'output',
            'name' : 'local_out'
        }
    ],
    [
        {
            'type' : 'input',
            'names' : ['local_in_two']
        },
        {
            "type": "embedding",
            "indices": 12,
            "size": 32
        },
        {
            "type": "conv2d",
            "size": 32,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            "type": "conv2d",
            "size": 64,
            "window": (3, 3),
            "stride": 1,
            "activation": 'relu'
        },
        {
            'type': 'flatten'
        },
        {
            'type' : 'output',
            'name' : 'local_out_two'
        }
    ],
    [
        {
            'type' : 'input',
            'names' : ['stats']
        },
        {
            "type": "embedding",
            "indices": 82,
            "size": 64
        },
        {
            'type': 'flatten'
        },
        {
            "type": "dense",
            "size": 256,
            "activation": 'relu'
        },
        {
            'type' : 'output',
            'name' : 'stats_out'
        }
    ],
    [
        {
            'type' : 'input',
            'names': ['global_out', 'local_out', 'local_out_two', 'stats_out'],
            'aggregation_type': 'concat',
        },
        {
            "type": "dense",
            "size": 256,
            "activation": 'relu'
        },
        {
            "type": "dense",
            "size": 256,
            "activation": 'relu'
        }
    ]
]

#gpu_options = tf.GPUOptions(per_process_gpu_memory_fraction=0.35)
gpu_options = None

'''--------------------------'''
'''         Arguments        '''
'''--------------------------'''

parser = argparse.ArgumentParser()

parser.add_argument('-mn', '--model-name', help="The name of the model", default="")
parser.add_argument('-gn', '--game-name', help="The name of the environment", default="envs/DeepCrawl-guided-learning-3")
# TODO: delete this
# parser.add_argument('-gn', '--game-name', help="The name of the environment", default=None)
parser.add_argument('-ne', '--num-episodes', help="Specify the number of episodes after which the environment is restarted", default=3000)
parser.add_argument('-wk', '--worker-id', help="The id for the worker", default=1)
args = parser.parse_args()


'''--------------------------'''
'''   Algorithm parameters   '''
'''--------------------------'''

# Create a Proximal Policy Optimization agent
agent = PPOAgent(
    # Inputs structure
    states={
        'global_in': {'shape': (10, 10), 'type': 'int'},
        'local_in': {'shape': (5, 5), 'type': 'int'},
        'local_in_two': {'shape': (3, 3), 'type': 'int'},
        'stats': {'shape': (11), 'type': 'int'},
        'action': {'shape': (17), 'type': 'float'}
    },
    # Actions structure
    actions={
        'type': 'int',
        'num_actions': 8
    },
    network=net,
    # Agent
    states_preprocessing=None,
    reward_preprocessing=None,
    # MemoryModel
    update_mode=dict(
        unit='episodes',
        # 10 episodes per update
        batch_size=1,
        # Every 10 episodes
        frequency=1
    ),
    memory=dict(
        type='latest',
        include_next_states=False,
        capacity=1000
    ),
    # DistributionModel
    distributions=None,

    discount = 0.99,
    entropy_regularization=0.01,
    gae_lambda = None,
    likelihood_ratio_clipping=0.2,

    baseline_mode='states',
    baseline = dict(
        type = 'custom',
        network = baseline
    ),
    baseline_optimizer = dict(
        type = 'multi_step',
        optimizer = dict(
            type = 'adam',
            learning_rate = 5e-4
        ),
        num_steps = 5
    ),

    # PPOAgent
    step_optimizer=dict(
        type='adam',
        learning_rate= 5e-6
    ),
    subsampling_fraction=0.1,
    optimization_steps=50,
    execution=dict(
        type='single',
        session_config = None,
        #session_config = tf.ConfigProto(gpu_options=gpu_options),
        distributed_spec=None
    )
)

# Work ID of the environment. To use the unity editor, the ID must be 0. To use more environments in parallel, use
# different ids
work_id = args.worker_id

# Number of episodes of a single run
num_episodes = args.num_episodes
# Number of timesteps within an episode
num_timesteps = 100
lstm = True

# Curriculum structure; here you can specify also the agent statistics (ATK, DES, DEF and HP)
curriculum = {
    'current_step': 0,
    'thresholds': [2.5e6, 2e6, 1.5e6, 1e6],
    'parameters':
        {
            'minTargetHp': [1,10,10,10,10],
            'minAgentHp': [5,5,5,5,5],
            'numActions': [17,17,17,17,17],
            'maxTargetHp': [1,10,20,20,20],
            'maxAgentHp': [20,20,20,20,20],
            'maxNumLoot': [0.2,0.2,0.2,0.2,0.2],
            'minNumLoot': [0.2,0.2,0.2,0.08,0.04],
            # Agent statistics
            'agentAtk': [3,3,3,3,3],
            'agentDef': [3,3,3,3,3],
            'agentDes': [3,3,3,3,3]
        }
}

model_name = args.model_name

# Name of the enviroment to load. To use Unity Editor, must be None
game_name = args.game_name

'''--------------------------'''
'''     Run the algorithm    '''
'''--------------------------'''

use_model = None

if model_name == "" or model_name == " " or model_name == None:

    while(use_model != 'y' and use_model != 'n'):
        use_model = input('Do you want to use a previous saved model? [y/n] ')

    if (use_model == 'y'):
        model_name = input('Name of the model: ')
    else:
        model_name = input('Specify the name to save the model: ')
try:
    with open("arrays/" + model_name + ".json") as f:
        history = json.load(f)
except:
    history = None

print('')
print('--------------')
print('Agent stats: ')
print('Optimizer: ' + str(agent.optimizer['optimizer']['optimizer']))
print('Baseline: ' + str(agent.baseline_optimizer['optimizer']))
print('Discount: ' + str(agent.discount))
print('Update mode: ' + str(agent.update_mode))
print('Work id ' + str(work_id))
print("Game name: " + str(game_name))
print('--------------')
print('Net config: ' + str(agent.network))
print('--------------')
print('')

step = 0
reward = 0.0
ist_step = 0
start_time = time.time()

environment = None

# Callback function printing episode statistics
def episode_finished(r, worker_id, num_callback_episodes = 10):
    global step
    global reward
    global ist_step
    global start_time
    step += 1
    ist_step += r.episode_timestep
    reward += r.episode_rewards[-1]
    # print('Reward @ episode {}: {}'.format(step, np.mean(r.episode_rewards[-1:])))
    if((step % num_callback_episodes) == 0):
        print('Average cumulative estimated reward for ' + str(num_callback_episodes) + ' episodes @ episode ' + str(step) + ': ' + str(np.mean(r.episode_rewards[-num_callback_episodes:])))
        print('Average cumulative real reward for ' + str(num_callback_episodes) + ' episodes @ episode ' + str(step) + ': ' + str(np.mean(r.real_episode_rewards[-num_callback_episodes:])))
        if r.reward_model is not None:
            print('Reward Model Loss @ episode {}: {}'.format(step, np.mean(r.reward_model_loss[-num_callback_episodes:])))
            print('Reward Model Validation Loss @ episode {}: {}'.format(step, np.mean(r.reward_model_val_loss[-num_callback_episodes:])))
        print('The agent made ' + str(sum(r.episode_timesteps)) + ' steps so far')
        timer(start_time, time.time())
        reward = 0.0

    # If num_episodes is not defined, save the model every 3000 episodes
    if(num_episodes == None):
        save = 3000
    else:
        save = num_episodes

    if(step % save == 0):
       save_model(r)

    return True

def timer(start,end):
    hours, rem = divmod(end-start, 3600)
    minutes, seconds = divmod(rem, 60)
    print("Time passed: {:0>2}:{:0>2}:{:05.2f}".format(int(hours),int(minutes),seconds))

def save_model(runner):
    global history
    # Save the runner statistics
    history = {
        "episode_rewards": runner.episode_rewards,
        "real_episode_rewards": runner.real_episode_rewards,
        "episode_timesteps": runner.episode_timesteps,
        "mean_entropies": runner.mean_entropies,
        "std_entropies": runner.std_entropies,
    }

    # Save the model and the runner statistics
    runner.agent.save_model('saved/' + model_name, append_timestep=False)
    json_str = json.dumps(history)
    f = open("arrays/" + model_name + ".json", "w")
    f.write(json_str)
    f.close()

try:
    while True:

        if game_name == None:
            print("You're starting the training with Unity Editor. You can test the correct interactions between "
                  "Unity and Tensorforce, but for a complete training you must start it with a built environment.")

        # Close the environment
        if environment != None:
            environment.close()

        # If model name is not None, restore the parameters
        if use_model == 'y':
            directory = os.path.join(os.getcwd(), "saved/")
            agent.restore_model(directory, model_name)

        # Open the environment with all the desired flags
        environment = UnityEnvWrapper(game_name, no_graphics=False, seed=int(time.time()),
                                      worker_id=work_id, with_stats=True, size_stats=11,
                                      size_global=10, agent_separate=False, with_class=False, with_hp=False,
                                      with_previous=lstm, verbose=False, manual_input=False)

        '''--------------------------'''
        '''       Reward Model       '''
        '''--------------------------'''
        GAN_reward = RewardModel(obs_size=12, inner_size=10, actions_size=17, policy=agent)



        # Create the runner to run the algorithm
        runner = DeepCrawlRunner(agent=agent, environment=environment, history=history, curriculum=curriculum, reward_model=GAN_reward)

        # Start learning for num_episodes episodes. After that, save the model, close the environment and reopen it.
        # Do this to avoid memory leaks or environment errors
        runner.run(episodes=num_episodes, max_episode_timesteps=100, episode_finished=episode_finished)

        use_model = 'y'

        print('')
        print('--------------')
        print('Agent stats: ')
        print('Optimizer: ' + str(agent.optimizer['optimizer']['optimizer']))
        print('Baseline: ' + str(agent.baseline_optimizer['optimizer']))
        print('Discount: ' + str(agent.discount))
        print('Update mode: ' + str(agent.update_mode))
        print('Work id ' + str(work_id))
        print("Game name: " + str(game_name))
        print('--------------')
        print('Net config: ' + str(agent.network))
        print('--------------')
        print('')

finally:

    '''--------------------------'''
    '''      End of the run      '''
    '''--------------------------'''

    print("Learning finished. Total episodes: {ep}. Average reward of last 100 episodes: {ar}.".format(
        ep=runner.episode,
        ar=np.mean(runner.episode_rewards[-100:]))
    )

    '''--------------------------'''
    '''     Try reward model     '''
    '''--------------------------'''
    GAN_reward.create_demonstrations(environment, inference = True, save_demonstrations = False)

    # Save the model and the runner statistics
    if model_name == "" or model_name == " " or model_name == None:
        saveName = input('Do you want to specify a save name? [y/n] ')
        if(saveName == 'y'):
            saveName = input('Specify the name ')
        else:
            saveName = 'Model'
    else:
        saveName = model_name

    save_model(runner)

    # Close the runner
    runner.close()

    # Freeze the TensorFlow graph and save .bytes file. All the output layers to fetch must be specified
    if lstm:
        export_pb('./saved/' + saveName, 'ppo/actions-and-internals/categorical/sample/Select,ppo/actions-and-internals/layered-network/apply/internal_lstm0/apply/stack')
    else:
        export_pb('./saved/' + saveName, 'ppo/actions-and-internals/categorical/sample/Select')

    print("Model saved with name " + saveName)
