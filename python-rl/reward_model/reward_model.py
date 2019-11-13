import torch
import torch.nn as nn
import torch.nn.functional as F

import numpy as np
import pickle

from reward_model.utils import RunningStat

# Reward model based on GAN. The model must decide if a (state,action) is from an
# expert demonstration or from the policy.
class RewardModel(nn.Module):
    def __init__(self, obs_size, inner_size, actions_size, policy, gamma = 0.99, **kwargs):
        super(RewardModel, self).__init__(**kwargs)

        # Define the NN model. For now, it will be the same as the policy

        # Reward Function
        # Embeddings Layer

        # self.stats_embs = nn.Embedding(num_embeddings=82, embedding_dim=64)

        # Convolutional Layers
        self.map_embs_1 = nn.Embedding(num_embeddings=12, embedding_dim=32)
        self.conv11 = nn.Conv2d(in_channels=32, out_channels=32, kernel_size=(3, 3), padding=1)
        # self.batch11 = nn.AvgPool2d((2,2))
        self.conv12 = nn.Conv2d(in_channels=32, out_channels=32, kernel_size=(3, 3), padding=1)
        # self.batch12 = nn.MaxPool2d((2,2))

        # MLP
        self.mlp = nn.Linear(100 + 17, 32)

        self.map_embs_2 = nn.Embedding(num_embeddings=12, embedding_dim=32)
        self.conv21 = nn.Conv2d(in_channels=32, out_channels=32, kernel_size=(3, 3), padding=1)
        # self.batch21 = nn.BatchNorm2d(num_features=32)
        self.conv22 = nn.Conv2d(in_channels=32, out_channels=32, kernel_size=(3, 3), padding=1)
        # self.batch22 = nn.BatchNorm2d(num_features=64)
        #
        # self.conv31 = nn.Conv2d(in_channels=32, out_channels=32, kernel_size=(3, 3), padding=1)
        # self.batch31 = nn.BatchNorm2d(num_features=32)
        # self.conv32 = nn.Conv2d(in_channels=32, out_channels=64, kernel_size=(3, 3), padding=1)
        # self.batch32 = nn.BatchNorm2d(num_features=64)
        #
        # # Dense stats Layer
        # self.dense41 = nn.Linear(in_features=704, out_features=256)
        # self.batch41 = nn.BatchNorm1d(num_features=256)

        # Dense layers
        # self.dense1 = nn.Linear(in_features=(6400+1600+576+256), out_features=256)
        self.dense1 = nn.Linear(in_features=(3200 + 17), out_features=32)
        # self.batch1 = nn.BatchNorm1d(num_features=256)
        # self.dense2 = nn.Linear(in_features=128, out_features=32)
        # self.batch2 = nn.BatchNorm1d(num_features=256)
        # Reward output
        self.output = nn.Linear(in_features=32, out_features=1)

        # Value Function
        self.map_embs_1_fn = nn.Embedding(num_embeddings=12, embedding_dim=32)
        self.conv11_fn = nn.Conv2d(in_channels=32, out_channels=32, kernel_size=(3, 3), padding=1)
        #self.batch11_fn = nn.AvgPool2d((2,2))
        self.conv12_fn = nn.Conv2d(in_channels=32, out_channels=32, kernel_size=(3, 3), padding=1)
        # self.batch12_fn = nn.MaxPool2d((2,2))

        self.map_embs_2_fn = nn.Embedding(num_embeddings=12, embedding_dim=32)
        self.conv21_fn = nn.Conv2d(in_channels=32, out_channels=32, kernel_size=(3, 3), padding=1)
        # self.batch21 = nn.BatchNorm2d(num_features=32)
        self.conv22_fn = nn.Conv2d(in_channels=32, out_channels=32, kernel_size=(3, 3), padding=1)
        # self.batch22 = nn.BatchNorm2d(num_features=64)

        # self.dense1_fn = nn.Linear(in_features=(6400+1600+576+256), out_features=256)
        self.dense1_fn = nn.Linear(in_features=(3200), out_features=32)
        # self.batch1_fn = nn.BatchNorm1d(num_features=256)
        # self.dense2_fn = nn.Linear(in_features=128, out_features=32)
        # self.batch2_fn = nn.BatchNorm1d(num_features=128)
        self.output_fn = nn.Linear(in_features=32, out_features=1)

        self.optimizer = torch.optim.Adam(params=self.parameters(), lr=0.00003)

        # Initialize some model attributes
        # RunningStat to normalize reward from the model
        self.r_norm = RunningStat(1)
        # Discount factor
        self.gamma = gamma
        # Policy agent needed to compute the discriminator
        self.policy = policy
        # Demonstrations buffer
        self.expert_traj = None
        self.validation_traj = None
        # Num of actions available in the environment
        self.actions_size = actions_size

    # The net ouput two branhces: the reward model and the value function. This function returns the shared layers
    # between the two functions
    def shared_layers(self, obs):
        # Initialize all the inputs
        global_batch = torch.from_numpy(np.stack([np.asarray(state['global_in']) for state in obs])).long()
        local_batch = torch.from_numpy(np.stack([np.asarray(state['local_in']) for state in obs])).long()
        local_two_batch = torch.from_numpy(np.stack([np.asarray(state['local_in_two']) for state in obs])).long()
        stats = torch.from_numpy(np.stack([np.asarray(state['stats']) for state in obs])).long()

        #acts = torch.from_numpy(np.stack([np.asarray(state['action']) for state in obs])).float()

        # Pass through the net
        # First branch (global view)
        # We add a BatchNormalization to all conv layers
        global_batch = self.map_embs(global_batch)
        global_batch = global_batch.view(-1, 32, 10, 10)
        # global_batch = F.relu(self.conv11(global_batch))
        global_batch = F.leaky_relu(self.batch11(self.conv11(global_batch)))
        #global_batch = F.relu(self.conv12(global_batch))
        global_batch = F.leaky_relu(self.batch12(self.conv12(global_batch)))

        # Second branch (local 5x5 view)
        local_batch = self.map_embs(local_batch)
        local_batch = local_batch.view(-1, 32, 5, 5)
        # local_batch = F.relu(self.conv21(local_batch))
        local_batch = F.leaky_relu(self.batch21(self.conv21(local_batch)))
        # local_batch = F.relu(self.conv22(local_batch))
        local_batch = F.leaky_relu(self.batch22(self.conv22(local_batch)))

        # Third branch (local 3x3 view)
        local_two_batch = self.map_embs(local_two_batch)
        local_two_batch = local_two_batch.view(-1, 32, 3, 3)
        # local_two_batch = F.relu(self.conv31(local_two_batch))
        local_two_batch = F.leaky_relu(self.batch31(self.conv31(local_two_batch)))
        # local_two_batch = F.relu(self.conv32(local_two_batch))
        local_two_batch = F.leaky_relu(self.batch32(self.conv32(local_two_batch)))

        # Fourth branch
        stats = self.stats_embs(stats)
        stats = stats.view(stats.shape[0], -1)
        # stats = F.relu(self.dense41(stats))
        stats = F.leaky_relu(self.batch41(self.dense41(stats)))

        # Concatenate all branches
        global_batch = global_batch.view(global_batch.shape[0], -1)
        local_batch = local_batch.view(local_batch.shape[0], -1)
        local_two_batch = local_two_batch.view(local_two_batch.shape[0], -1)
        all_branches = torch.cat((global_batch, local_batch, local_two_batch, stats), 1)

        # Return the global branch
        return all_branches

    def reward_function(self, obs, acts):
        global_batch = torch.from_numpy(np.stack([np.asarray(state['global_in']) for state in obs])).long()
        global_batch = self.filter_global_state(global_batch)

        # Create the one-hot encoding of the actions
        acts = torch.from_numpy(np.asarray([acts]))
        acts = torch.transpose(acts, 0, 1)
        hot_acts = torch.zeros(acts.size()[0], self.actions_size)
        hot_acts = hot_acts.scatter_(1, acts.long(), 1)

        global_batch = self.map_embs_1(global_batch)
        global_batch = global_batch.view(-1, 32, 10, 10)
        global_batch = F.relu((self.conv11(global_batch)))
        global_batch = F.relu((self.conv12(global_batch)))
        global_batch = global_batch.view(global_batch.shape[0], -1)

        global_batch = torch.cat((global_batch, hot_acts), 1)

        global_batch = F.relu(self.dense1(global_batch))
        # global_batch = F.relu(self.dense2(global_batch))
        reward = self.output(global_batch)

        return reward

    def reward_mlp(self, obs, acts):
        global_batch = torch.from_numpy(np.stack([np.asarray(state['global_in']) for state in obs])).float()
        global_batch = global_batch.view(-1, 10 * 10)
        # Create the one-hot encoding of the actions
        acts = torch.from_numpy(np.asarray([acts]))
        acts = torch.transpose(acts, 0, 1)
        hot_acts = torch.zeros(acts.size()[0], self.actions_size)
        hot_acts = hot_acts.scatter_(1, acts.long(), 1)

        global_batch = torch.cat((global_batch, hot_acts), 1)

        global_batch = self.mlp(global_batch)
        reward = self.output(global_batch)
        return reward


    def value_function(self, obs, obs_n):
        global_batch = torch.from_numpy(np.stack([np.asarray(state['global_in']) for state in obs])).long()
        global_batch = self.filter_global_state(global_batch)

        global_batch = self.map_embs_1_fn(global_batch)
        global_batch = global_batch.view(-1, 32, 10, 10)
        global_batch = F.relu((self.conv11_fn(global_batch)))
        global_batch = F.relu((self.conv12_fn(global_batch)))
        global_batch = global_batch.view(global_batch.shape[0], -1)
        # global_batch = F.relu(self.dense2_fn(global_batch))

        # global_batch = torch.cat((global_batch, local_batch), 1)
        global_batch = F.relu(self.dense1_fn(global_batch))

        value_fn = self.output_fn(global_batch)

        global_batch_n = torch.from_numpy(np.stack([np.asarray(state['global_in']) for state in obs_n])).long()
        global_batch_n = self.filter_global_state(global_batch_n)

        global_batch_n = self.map_embs_1_fn(global_batch_n)
        global_batch_n = global_batch_n.view(-1, 32, 10, 10)
        global_batch_n = F.relu((self.conv11_fn(global_batch_n)))
        global_batch_n = F.relu((self.conv12_fn(global_batch_n)))
        global_batch_n = global_batch_n.view(global_batch_n.shape[0], -1)

        # global_batch_n = torch.cat((global_batch_n, local_batch_n), 1)
        global_batch_n = F.relu(self.dense1_fn(global_batch_n))
        # global_batch_n = F.relu(self.dense2_fn(global_batch_n))

        value_fn_n = self.output_fn(global_batch_n)

        return value_fn, value_fn_n

    def filter_global_state(self, global_in):
        global_in[global_in > 3] = 0
        global_in[global_in == 0] = 1
        return global_in

    # Forward function
    def forward(self, obs, acts, obs_n=None):
        #rewards = self.reward_function(obs, acts)
        rewards = self.reward_mlp(obs, acts)
        # TODO: Normalize rewards ?????
        # rewards = rewards.detach().numpy()
        # self.push_reward(rewards)
        # rewards = self.normalize_rewards(rewards)
        # rewards = torch.from_numpy(rewards)
        return rewards

    # Forward function
    def _forward(self, obs, acts, obs_n = None):

        # # Create the one-hot encoding of the actions
        # acts = torch.from_numpy(np.asarray([acts]))
        # acts = torch.transpose(acts, 0, 1)
        # hot_acts = torch.zeros(acts.size()[0], self.actions_size)
        # hot_acts = hot_acts.scatter_(1, acts.long(), 1)

        # Get the output from the layers from the state input
        all_branches = self.shared_layers(obs)

        # Final layers of the reward model output
        # all_branches_r = F.relu(self.dense1(all_branches))
        all_branches_r = F.leaky_relu(self.batch1(self.dense1(all_branches)))
        # all_branches_r = torch.cat((all_branches_r, hot_acts), 1)
        # all_branches_r = F.relu(self.dense2(all_branches_r))
        all_branches_r = F.leaky_relu(self.batch2(self.dense2(all_branches_r)))
        reward = self.output(all_branches_r)

        if obs_n is not None:
            # Value Function with shared net
            # Value fn output
            # all_branches_fn = F.relu(self.dense1_fn(all_branches))
            all_branches_fn = F.leaky_relu(self.batch1_fn(self.dense1_fn(all_branches)))
            # all_branches_fn = F.relu(self.dense2_fn(all_branches_fn))
            all_branches_fn = F.leaky_relu(self.batch2_fn(self.dense2_fn(all_branches_fn)))
            value_fn = self.output_fn(all_branches_fn)

            # Get the ouput of the state_n for the value_fn_n
            all_branches_fn_n = self.shared_layers(obs_n)

            # Value fn output
            # all_branches_fn_n = F.relu(self.dense1_fn(all_branches_fn_n))
            all_branches_fn_n = F.leaky_relu(self.batch1_fn(self.dense1_fn(all_branches_fn_n)))
            # all_branches_fn_n = F.relu(self.dense2_fn(all_branches_fn_n))
            all_branches_fn_n = F.leaky_relu(self.batch2_fn(self.dense2_fn(all_branches_fn_n)))
            value_fn_n = self.output_fn(all_branches_fn_n)

        if obs_n is not None:
            return reward + self.gamma*value_fn_n - value_fn
        else:
            return reward

    # Forward that returns the discriminator value (not only the reward)
    def forward_disc(self, obs, acts, probs, obs_n):
        # x = self.forward(obs, acts, obs_n)
        x = self.forward(obs, acts)
        disc = self.discriminator(x, probs)
        out = torch.log(disc) - torch.log(1 - disc)
        return out, disc

    # Normalize the reward for each frame of the sequence.
    # Since the reward predictor is ultimately used to
    # compare two sums over timesteps, its scale is arbitrary,
    # and we normalize it to have a standard deviation of 0.05
    def push_reward(self, rewards):
        for r in rewards:
            self.r_norm.push(r)

    def normalize_rewards(self, rewards):

        #rewards = rewards.detach().numpy()
        rewards -= self.r_norm.mean
        rewards /= (self.r_norm.std + 1e-12)
        rewards *= 0.05
        #rewards = torch.from_numpy(rewards)

        return rewards

    def discriminator(self, rewards, probs):
        disc = torch.div(torch.exp(rewards), (torch.add(torch.exp(rewards), torch.from_numpy(probs))))
        return disc

    # Compute a step to train the model. It takes a batch from both one or more trajectories and
    # demonstrations, and it tries to decide which is from expert and from policy.
    def train_step(self, expert_traj, policy_traj, num_itr=5, batch_size = 16):

        # Train Mode
        self.train()

        for it in range(num_itr):

            expert_batch_idxs = np.random.randint(0, len(expert_traj['obs']), batch_size)
            policy_batch_idxs = np.random.randint(0, len(policy_traj['obs']), batch_size)

            expert_obs = [expert_traj['obs'][id] for id in expert_batch_idxs]
            policy_obs = [policy_traj['obs'][id] for id in policy_batch_idxs]

            expert_obs_n = [expert_traj['obs_n'][id] for id in expert_batch_idxs]
            policy_obs_n = [policy_traj['obs_n'][id] for id in policy_batch_idxs]

            expert_acts = [expert_traj['acts'][id] for id in expert_batch_idxs]
            policy_acts = [policy_traj['acts'][id] for id in policy_batch_idxs]

            policy_probs = [policy_traj['probs'][id] for id in policy_batch_idxs]
            policy_probs = np.asarray(policy_probs)

            expert_probs = []
            for (index, state) in enumerate(expert_obs):
                _, probs = self.select_action(state)
                expert_probs.append(probs[expert_acts[index]])

            expert_probs = np.asarray(expert_probs)

            real_output = self.forward(expert_obs, expert_acts)
            fake_output = self.forward(policy_obs, policy_acts)

            real_disc = self.discriminator(real_output, expert_probs)
            fake_disc = self.discriminator(fake_output, policy_probs)

            real_labels = torch.ones_like(real_disc)
            fake_labels = torch.zeros_like(fake_disc)

            all_disc = torch.cat((real_disc, fake_disc))
            all_labels = torch.cat((real_labels, fake_labels))

            self.optimizer.zero_grad()
            loss = - torch.mean(torch.log(real_disc) + (torch.log(1 - fake_disc)))
            loss.backward()
            self.optimizer.step()

        if self.validation_traj is not None:
            self.eval()
            val_probs = []
            for (index, state) in enumerate(self.validation_traj['obs']):
                _, probs = self.select_action(state)
                val_probs.append(probs[self.validation_traj['acts'][index]])
            val_probs = np.asarray(val_probs)
            val_output = self.forward(self.validation_traj['obs'], self.validation_traj['acts'])
            val_disc = self.discriminator(val_output, val_probs)
            val_loss = torch.mean(val_disc)
            val_loss = val_loss.detach().numpy()
        else:
            val_loss = 0

        return loss, val_loss

    # Select action from the policy and fetch the probability distribution over the action space
    def select_action(self, state):
        act, fetch = self.policy.act(states=state, deterministic=True, independent = True, fetch_tensors=['probabilities'])
        probs = np.squeeze(fetch['probabilities'])
        return (act, probs)

    # Update demonstrations
    def set_demonstrations(self, demonstrations, validations):
        total = len(demonstrations['obs'])

        self.expert_traj = demonstrations

        if validations is not None:
            self.validation_traj = validations

    # Create and return some demonstrations [(states, actions, frames)]. The order of the demonstrations must be from
    # best to worst. The number of demonstrations is given by the user
    def create_demonstrations(self, env, save_demonstrations = True, inference = False):
        end = False

        # Initialize trajectories buffer
        expert_traj = {
            'obs': [],
            'obs_n': [],
            'acts': [],
            'probs': []
        }

        val_traj = {
            'obs': [],
            'obs_n': [],
            'acts': [],
            'probs': []
        }

        count = 1
        while not end:
            # Make another demonstration
            print('Demonstration n° ' + str(count))
            # Reset the environment
            state = env.reset()
            states = [state]
            actions = []
            done = False
            count = 0
            # New sequence of states and actions
            while not done:
                try:
                    # Input the action and save the new state and action
                    count += 1
                    print("Timestep: " + str(count))
                    print(state['global_in'])
                    action = input('action: ')
                    if action == "f":
                        done = True
                        continue
                    state_n, done, reward = env.execute(action)
                    action = env.command_to_action(action)
                    # If inference is true, print the reward
                    if inference:
                        self.eval()
                        _, probs = self.select_action(state)
                        reward = self.forward([state], [action])
                        # print('Discriminator probability: ' + str(disc))
                        print('Unnormalize reward: ' + str(reward))
                        reward = reward.detach().numpy()
                        #reward = self.normalize_rewards(reward)
                        print('Normalize reward: ' + str(reward))
                        print('Probability of state space: ')
                        print(probs)
                    state = state_n
                    states.append(state)
                    actions.append(action)
                except Exception as e:
                    print(e)
                    continue

            if not inference:
                y = None
                while y != 'y' and y != 'n':
                    y = input('Do you want to save this demonstration? [y/n] ')

                    if y == 'y':
                        # Update expert trajectories
                        expert_traj['obs'].extend(np.array(states[:-1]))
                        expert_traj['obs_n'].extend(np.array(states[1:]))
                        expert_traj['acts'].extend(np.array(actions))
                        count += 1
                    else:
                        y = input('Do you want to save this demonstration as validation? [y/n] ')
                        if y == 'y':
                            val_traj['obs'].extend(np.array(states[:-1]))
                            val_traj['obs_n'].extend(np.array(states[1:]))
                            val_traj['acts'].extend(np.array(actions))

            y = None
            while y!='y' and y!='n':
                if not inference:
                    y = input('Do you want to create another demonstration? [y/n] ')
                else:
                    y = input('Do you want to try another episode? [y/n] ')

                if y == 'n':

                    end = True

        if len(val_traj['obs']) <= 0:
            val_traj = None

        # Save demonstrations to file
        if save_demonstrations and not inference:
            print('Saving the demonstrations...')
            self.save_demonstrations(expert_traj, val_traj)
            print('Demonstrations saved!')

        if not inference:
            self.set_demonstrations(expert_traj, val_traj)

        return expert_traj, val_traj

    # Save demonstrations dict to file
    def save_demonstrations(self, demonstrations, validations):
        with open('reward_model/dems/dems.pkl', 'wb') as f:
            pickle.dump(demonstrations, f, pickle.HIGHEST_PROTOCOL)
        with open('reward_model/dems/vals.pkl', 'wb') as f:
            pickle.dump(validations, f, pickle.HIGHEST_PROTOCOL)

    # Load demonstrations from file
    def load_demonstrations(self):
        with open('reward_model/dems/dems.pkl', 'rb') as f:
            expert_traj = pickle.load(f)

        with open('reward_model/dems/vals.pkl', 'rb') as f:
            val_traj = pickle.load(f)

        self.set_demonstrations(expert_traj, val_traj)

        return expert_traj, val_traj