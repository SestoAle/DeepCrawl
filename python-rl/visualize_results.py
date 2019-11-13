import matplotlib.pyplot as plt
import json
import numpy as np

import argparse

parser = argparse.ArgumentParser()

parser.add_argument('-mn', '--model-name', help="The name of the model", default='very_simple_irl')
parser.add_argument('-nm', '--num-mean', help="The number of the episode to compute the mean", default=5)
parser.add_argument('-sp', '--save-plot', help="if true save the plot in folder saved_plot", default=False)
args = parser.parse_args()

model_name = args.model_name
while model_name == "" or model_name == " " or model_name == None:
    model_name = input('Insert model name: ')

with open("arrays/" + model_name + ".json") as f:
    history = json.load(f)


episodes_reward = np.asarray(history.get("real_episode_rewards", list()))
mean_entropies = np.asarray(history.get("mean_entropies", list()))
std_entropies = np.asarray(history.get("std_entropies", list()))
episodes_success = episodes_reward > 0
episodes_timesteps = np.asarray(history.get("episode_timesteps", list()))

timesteps = np.asarray(history.get("episode_timesteps", list()))

cum_timesteps = np.cumsum(timesteps)


num_mean = int(args.num_mean)
save_plot = bool(args.save_plot)

waste = np.alen(episodes_reward)%num_mean

print("Mean of " + str(num_mean) + " episodes")

if waste != 0:
    print('Max reward: ' + str(np.max(np.mean(episodes_reward[:-waste].reshape(-1, num_mean), axis=1))))
    plt.figure(1)
    plt.title("Reward")
    num_episodes = np.asarray(range(1,np.size(np.mean(episodes_reward[:-waste].reshape(-1, num_mean), axis=1))+1))*num_mean
    plt.plot(num_episodes, np.mean(episodes_reward[:-waste].reshape(-1, num_mean), axis=1))
    plt.xlabel("Episodes")
    plt.ylabel("Mean Reward")
    if save_plot:
        plt.savefig("saved_plots/" + model_name + "_reward.png", dpi=300)

    plt.figure(2)
    plt.title("Entropy")
    plt.plot(num_episodes, np.mean(mean_entropies[:-waste].reshape(-1, num_mean), axis=1))
    plt.xlabel("Episodes")
    plt.ylabel("Mean Entropy")
    if save_plot:
        plt.savefig("saved_plots/" + model_name + "_entropy.png", dpi=300)

    plt.figure(3)
    plt.title("Success")
    plt.plot(num_episodes, np.mean(episodes_success[:-waste].reshape(-1, num_mean), axis=1))
    plt.xlabel("Episodes")
    plt.ylabel("Success Rate")
    if save_plot:
        plt.savefig("saved_plots/" + model_name + "_success.png", dpi=300)
    plt.show()

else:
    print('Max reward: ' + str(np.max(np.mean(episodes_reward.reshape(-1, num_mean), axis=1))))
    plt.figure(1)
    plt.title("Reward")
    num_episodes = np.asarray(
        range(1, np.size(np.mean(episodes_reward.reshape(-1, num_mean), axis=1)) + 1)) * num_mean
    plt.plot(num_episodes, np.mean(episodes_reward.reshape(-1, num_mean), axis=1))
    plt.xlabel("Episodes")
    plt.ylabel("Mean Reward")
    if save_plot:
        plt.savefig("saved_plots/" + model_name + "_reward.png", dpi=300)

    plt.figure(2)
    plt.title("Entropy")
    plt.plot(num_episodes, np.mean(mean_entropies.reshape(-1, num_mean), axis=1))
    plt.xlabel("Episodes")
    plt.ylabel("Mean Entropy")
    if save_plot:
        plt.savefig("saved_plots/" + model_name + "_entropy.png", dpi=300)

    plt.figure(3)
    plt.title("Success")
    plt.plot(num_episodes, np.mean(episodes_success.reshape(-1, num_mean), axis=1))
    plt.xlabel("Episodes")
    plt.ylabel("Success Rate")
    if save_plot:
        plt.savefig("saved_plots/" + model_name + "_success.png", dpi=300)
    plt.show()

print("Number of timesteps: " + str(np.sum(timesteps)))
print("Number of episodes: " + str(np.size(episodes_reward)))