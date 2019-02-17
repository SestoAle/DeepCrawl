# DeepCrawl

<p align="center">
<img  src="https://i.imgur.com/CXtA96C.png" width="80%" height="80%"/>
</p>

DeepCrawl is a turn-based strategy game for mobile platforms, where all the enemies 
are trained with Deep Reinforcement Learning algorithms [[1]](#references).

The game is designed to be hard, yet fair: the player will have to explore the 
dungeons and defeat all the guardians of the rooms, paying attention to every 
moves the AI does!

<p align="center">
<img  src="https://media.giphy.com/media/1zlUEEeKTZTnaCXiww/giphy.gif" width="60%" height="60%"/>
</p>

The game was developed in Unity, while the AI was built through Tensorforce [[2]](#references) and
Unity ML-Agents.  

The project was part of a Master thesis in Computer Engineering at 
Università degli Studi di Firenze, with title "DeepCrawl: Deep Reinforcement
Learning for turn-based strategy games".

#### Table of content
* [Installation](#installation)
* [Usage and examples](#usage-and-examples)
* [Proposed model](#proposed-model)
* [Documents](#documents)
* [License](#license)
* [References](#references)

## Installation
You can try the Android version of the game by downloading the apk at [link](https://drive.google.com/uc?export=download&confirm=LXxR&id=1Zrbnicqz6T4ty83Eo6fHZjRPPYXrFyA9).

If you want to check the game code and test the DRL algorithms, first download 
the repository:
```bash
git clone https://github.com/SestoAle/DeepCrawl.git
``` 
then install all the [prerequisites](#prerequsites) and follow these instructions.
### Unity
1. Open the folder ```DeepCrawl-Unity``` with Unity Editor;
2. Download ```TensorFlowSharp``` 
plugin [here](https://s3.amazonaws.com/unity-ml-agents/0.5/TFSharpPlugin.unitypackage) 
and import it in the project. More information at [Unity ML-Agents](https://github.com/Unity-Technologies/ml-agents);
3. Close and re-open the editor.

### Prerequsites
| Software                                                 | Version         | Required |
| ---------------------------------------------------------|-----------------| ---------|
| **Python**                                               |     tested on v3.7      |    Yes   |
| **Unity** | tested on v2018.3.5f1 | Yes |
| **ml-agents** | tested on v0.5.0 | Yes | 
| **Tensorflow** | tested on v1.12.0 | Yes |
| **Tensorforce** | tested on v0.4.3 | Yes |

## Usage and examples
There are more than one methods to train one agent with the model described in [proposed model](#proposed-model) section:
* for Linux and MacOS systems, the repository provides a built environment which can be
used to start the training without Unity Editor. For Linux systems, you have to extract the game from the zip file in the 
```envs``` folder:
```bash
python3 deepcrawl_rl.py --game-name="envs/DeepCrawl-training-env"
```  

* you can start the training directly from Unity Editor. First you have to 
change the flag ```isTraining``` in ```BoardManagerSystem``` game object, then 
change the ```TrainBrain``` game object to ```External``` (for more information,
see [Unity ML-Agents](https://github.com/Unity-Technologies/ml-agents)). After that, run the command and follow the 
instructions on screen:
```bash
python3 deepcrawl_rl.py 
```  

You can specify the agent statistics by modifying the curriculum json in the 
```deepcrawl_rl.py``` file. For more information see [proposed model](#proposed-model).


When the training is done, a ```agent.bytes``` file will be automatically stored 
in ```saved``` folder; you can import the file in Unity Editor and assign it 
to any internal brain.

## Proposed model
In this section will be described the main components of the DRL model.
### Neural net

<p align="center">
<img  src="https://i.imgur.com/FZzxbux.png" width="80%" height="80%"/>
</p>


### Reward function

<p align="center">
<img  src="https://i.imgur.com/S5LOfj0.png" width="80%" height="80%" style="padding:10px"/>
</p>


### Algorithm
The algorithm used in this project is Proximal Policy Optimization [[3]](#references)
(implemented in TensorForce). To see the whole set of hyperparameters, open
```deepcrawl_rl.py``` file.

### Training Set-up
The agent will be trained in a random room with curriculum learning: the values
can be defined in the json in the ```deepcrawl_rl.py``` file. 
In here, you can also specify the agent parameters (such as ATK, DEF and DEX), 
what values to change and the number of steps to change phase.
```python
curriculum = {
    'current_step': 0,
    'thresholds': [2.5e6, 2e6, 1.5e6, 1e6],
    'parameters':
        {
            'minTargetHp': [1,10,10,10,10],
            'maxTargetHp': [1,10,20,20,20],
            'minAgentHp': [5,5,5,5,5],
            'maxAgentHp': [20,20,20,20,20],
            'minNumLoot': [0.2,0.2,0.2,0.08,0.04],
            'maxNumLoot': [0.2,0.2,0.2,0.2,0.2],
            'numActions': [17,17,17,17,17],
            # Agent parameters
            'agentDes': [3,3,3,3,3],
            'agentAtk': [3,3,3,3,3],
            'agentDef': [3,3,3,3,3]
        }
}
```
## Reports
A copy of the thesis (italian) can be found 
<a href="https://github.com/SestoAle/DeepCrawl/raw/master/documents/thesis.pdf" download="thesis.pdf">here</a>.

A copy of the presentation (italian) can be found
<a href="https://github.com/SestoAle/DeepCrawl/raw/master/documents/presentation.pdf" download="presentation.pdf">here</a>.

## License
Licensed under the term of [MIT License](https://github.com/SestoAle/DeepCrawl/blob/master/LICENSE).


## References
[1]: Volodymyr Mnih et al. *Human-level control through deep reinforcement learning*. Nature, 518(7540):529–533, 2015. 

[2]: Alexander Kuhnle et al. *Tensorforce: a tensorflow library for applied reinforcement learning*. Web page, 2017

[3]: John Schulman et al. *Proximal policy optimization algorithms*. CoRR, abs/1707.06347, 2017. 

