# PA_Learning_Agent

### Overview
This repository contains both the code, and the data, for my Master's thesis. The thesis uses a Unity simulation in order to train agents of different body types. The agents are trained through my implementation of a perception-action architecture. How the different types of agents learn is then compared in order to better understand how body type impacts cognitive development.


### Installation
Installing the dependencies of this project fairly closely follow the instructions of how to install the local version of the `mlagents` package, which can be found [here][https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Installation.md].

Here are the specific instructions for this project, including the versions of what is used.

1. Install this repository on your computer
	```bash
	git install https://github.com/TaraCarette/PA_Learning_Agent.git
	```
2. Install Unity Hub [here][https://unity.com/download]
3. Use Unity Hub to install Unity. This project uses version 2021.3.16f1
4. Download a virtual environment to manage the different python packages that will be used
5. Create and activate a virtual environment using python version 3.9.16
	If you are using conda:
    ```bash
    conda create --name trainer-env python=3.9
	conda activate trainer-env
    ```
6. Install version 1.7.1 of pytorch
	```bash
	python -m pip install torch~=1.7.1 -f https://download.pytorch.org/whl/torch_stable.html
	```
7. Install version 0.30.0 of `mlagents` locally inside the local version of this repository 
	```bash
	git clone --branch release_20 https://github.com/Unity-Technologies/ml-agents.git
	python -m pip install -e ml-agents/ml-agents-envs
	python -m pip install -e ml-agents/ml-agents
	```
	This step will likely force the version of numpy in your environment to version 1.21 automatically. If it doesn't, you may need to do this manually, as it is the version compatible with this version of `mlagents`. You can check the `environment.yml` file in this repository to doublecheck the versions of any packages used.
8. Ensure that the `ml-agents/ml-agents/mlagents/trainers/stats.py` and `ml-agents/ml-agents/mlagents/trainers/torch_entities/components/reward_providers/curiosity_reward_provider.py` files are the versions from this repository.
9. In Unity Hub, open the subfolder `PA Prototype` as a project


### Usage
The test environment and the agent can be modified within the Unity project. If any of the changes impact the number of inputs an agent receives, the variable `featureEncoderSize` in the file `ml-agents/ml-agents/mlagents/trainers/torch_entities/components/reward_providers/curiosity_reward_provider.py` must be adjusted accordingly.

To change what statistics are printed out as you are running, change the `ml-agents/ml-agents/mlagents/trainers/stats.py` file.

To change how the PA learning system is being implemented, change the `ml-agents/ml-agents/mlagents/trainers/torch_entities/components/reward_providers/curiosity_reward_provider.py` file. This file will also need to be changed if you want to load a feature detector from another layer.

### Data
There are different types of data saved to this repository.

#### results
In the results folder are folders containing a variety of information about a run. It has the configuration file for that data run, the statistics gathered during that run, and the trained behaviour brain. If you run:
```bash
	tensorboard --logdir results
```
You can see graphically all of the data stored by each run.


#### my_feature_models
This is where the models created by the PA learning mechanism gets saved. Each run saves the feature detector it was training to be used by the next layer, the current forward model, and the future model, the forward model used to train the feature detector.

