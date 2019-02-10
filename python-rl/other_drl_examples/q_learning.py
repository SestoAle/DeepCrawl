import numpy as np

env = gym.make('FrozenLake-v0')

# Initialize Q-table (space_size x action_size) to all zeros
Q = np.zeros([env.observation_space.n, env.action_space.n])

# Initialize learning paramters
lr = 0.8
y = 0.95
num_episodes = 2000

# Create a list of all rewards per episode
rList = []

for i in range (num_episodes):

    # Reset environment and get the first observation
    s = env.reset()
    rAll = 0
    j = 0

    # Q-table algorithm
    while j < 99:

        j += 1

        # Choose an action greedily from the Q-Table and adding some noise
        a = np.argmax(Q[s,:] + np.random.randn(1, env.action_space.n)*(1.0/(i+1)))

        # Get new state and reward for the action got above
        new_s, r, done, _ = env.step(a)

        # Update Q-table with new knowledge
        Q[s,a] = Q[s,a] + lr*(r + y*np.max(Q[new_s, :]) - Q[s,a])
        rAll += r
        s = new_s

        if done:
            break

    rList.append(rAll)

print("Score over time: " + str(sum(rList)/num_episodes))

print("Final Q-table values: ")
print(Q)

