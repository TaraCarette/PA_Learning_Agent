# PA_Learning_Agent
Storing the code for the simulation and learning methods of a PA agent


Need to follow the environment file.
However, also need mlagents.
Because I am changing the mlagents package, you will need to download the correct mlagents from the git repo, then run pip install -e over the folder as in the installation instructions for mlagents. I am still figuring out which files I will change in order to prevent an entire duplicate of the mlagents package in this repo. So watch out for that.

Using Unity version 2021.3.16f1



Installing
Basically just following instructions in here: https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Installation.md
Specifically local install version
(skipping some steps as done in my git repo)

Download Unity Hub
Use that to download Unity Editor
This project works with 2021.3.16 (multiple, I hope). Probably works with more. Am trying with newer seeing if works

download a virtual environment to manage various packages (really though, need specific pytorch and numpy so want one)
I used conda myself
added a file with the download versions to git to check, but should be fine to just download as instructions say

create the virtual environment
with python version 3.9.16

conda create --name trainer-env python=3.9
conda activate trainer-env

then install pytorch
python -m pip install torch~=1.7.1 -f https://download.pytorch.org/whl/torch_stable.html


then locally download github of mlagents
we're aiming for version 0.30.0

so run
git clone --branch release_20 https://github.com/Unity-Technologies/ml-agents.git

then inside that downloaded foler run
( likely will force numpy to verion 1.21, may need to do manually)
python -m pip install -e ./ml-agents-envs
python -m pip install -e ./ml-agents

should now be able to run the mlagents-learn command
also now environment file in this repo should match what you get when try conda list


Then, we need to hook up the files to our unity project 
So we need to first open up out project

so do
git clone https://github.com/TaraCarette/PA_Learning_Agent.git

Then in unity, open up the subfolder 'PA Prototype' as a project


be in my githubthen inside project, need to link up the mlagent files

Then after, will need to use the given file in that git repo to modify the mlagents package in order to run my version of PA learning
