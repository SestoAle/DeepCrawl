import gym
import numpy as np
import random
import tensorflow as tf
import matplotlib.pyplot as plt


env = gym.make('FrozenLake-v0')

tf.reset_default_graph()

# Create the network

inputs1 = tf.placeholder(shape=[1,16], dtype=tf.float32)
W = tf.Variable(tf.random_uniform([16,4],0,0.01))
Qout = tf.matmul(inputs1, W)
predict = tf.argmax(Qout, 1)

nextQ = tf.placeholder(shape=[1,4], dtype=tf.float32)
loss = tf.reduce_sum(tf.square(nextQ - Qout))
trainer = tf.train.GradientDescentOptimizer(learning_rate=0.1)
updateModel = trainer.minimize(loss)

# Training the network
init = tf.global_variables_initializer()

# Set learning parameters
y = 0.99
e = 0.1
num_episodes = 2000

# Create list to contain total rewards and steps per episode
jList = []
rList = []

with tf.Session() as sess:
    sess.run(init)
    for i in range(num_episodes):
        # Reset environment and get first state
        s = env.reset()
        rAll = 0
        d = False
        j = 0

        while j < 99:
            j += 1
            # Choose an action by greedily from Q-network (with a chance to choose a random action)
            a, allQ = sess.run([predict, Qout], feed_dict={inputs1:np.identity(16)[s:s+1]})

            if(np.random.rand(1) < e):
                a[0] = env.action_space.sample()

            # Get new reward from action got above
            new_s, r, done, _ = env.step(a[0])

            # Get the Q' values by feeding the new state through NN
            Q1 = sess.run(Qout, feed_dict={inputs1:np.identity(16)[new_s:new_s+1]})

            # Get max(Q') and set our target value for chosen action
            maxQ1 = np.max(Q1)
            targetQ = allQ
            targetQ[0, a[0]] = r + y*maxQ1

            # Train our network using target and predicetd Q values
            _, W1 = sess.run([updateModel, W], feed_dict={inputs1:np.identity(16)[s:s+1], nextQ:targetQ})

            rAll += r
            s = new_s
            if done:
                # Reduce a chance of random action as we train the model
                e = 1.0/((i/50) + 10)
                break

            jList.append(j)
            rList.append(rAll)
print("Percent of succesful episodes: " + str(sum(rList)/num_episodes) + "%")

plt.plot(rList)

plt.plot(jList)



