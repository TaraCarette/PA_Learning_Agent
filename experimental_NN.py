import torch
from typing import Dict

from mlagents.trainers.torch_entities.utils import ModelUtils


from mlagents.trainers.torch_entities.layers import LinearEncoder, linear_layer
# from mlagents_envs.base_env import BehaviorSpec
# from mlagents.trainers.torch_entities.networks import NetworkBody



# buffer = AgentBuffer()
# print("begin")
# print(buffer)
# print("end")


# learningRate = settings.learning_rate
learningRate = 0.0003

# spec

# settings
hiddenUnits = 4 # 128 seems default? buy my input is size 4 and seems to care so


actionSize = 7
# self._action_spec = specs.action_spec
# self._action_flattener = ActionFlattener(self._action_spec)

# specs: BehaviorSpec, settings: CuriositySettings
# self._action_spec = specs.action_spec
# state_encoder_settings = settings.network_settings


class PredictorNetwork(torch.nn.Module):
    def __init__(self) -> None:
        super().__init__()

        # the state encoder
        # for my first layer, I think this should just be the raw data?
        # part of Network body, might need to understand the forward
        # for now, going to skip, make it the raw data


        # the actual predictor network that is run once have the needed inputs
        # in predict next state, which is called when calculating loss for 1 step
        # aka one update
        self.forward_model_next_state_prediction = torch.nn.Sequential(
            # can write this myself, check out class to help
            LinearEncoder( # fancy mlp
                # normally returns sigmoid of data - kinda spits out 1 hot encoding?
                hiddenUnits
                + actionSize,
                1,
                256,
            ),
            # adding another layer as don't want output to be the swish
            # recovers from reduction of swish?
            linear_layer(256, hiddenUnits),
        )

    # these next get state functions, pull different mini batches from buffer
    # use those to get the required encoding
    def get_current_state(self, mini_batch: Dict) -> torch.Tensor:
        """
        Extracts the current state embedding from a mini_batch.
        """
        print("get current state")
        hidden = mini_batch["curr"]["state"]
        print("end get current state")
        return hidden

    def get_next_state(self, mini_batch: Dict) -> torch.Tensor:
        """
        Extracts the next state embedding from a mini_batch.
        """
        print("get next state")
        hidden = mini_batch["next"]["state"]
        print("end getting next state")
        return hidden

    def predict_next_state(self, mini_batch: Dict) -> torch.Tensor:
        """
        Uses the current state embedding and the action of the mini_batch to predict
        the next state embedding.
        """
        # forward pass of predicting next state
        print("begin predicting next state")
        action = mini_batch["curr"]["action"] # am just gonna grab the next action in very simple way
        forward_model_input = torch.cat( # grabbing the st and the action, to feed into model
            (self.get_current_state(mini_batch), action), dim=0 #note I changed from 1 to 0
        )
        print(forward_model_input)

        # now that have input, call function with it, and output we return is prediction
        # that is the model I want, have notes above on how can recreate for myself
        pred = self.forward_model_next_state_prediction(forward_model_input)
        print("end predicting next state")
        return pred


    def compute_reward(self, mini_batch: Dict) -> torch.Tensor:
        """
        Calculates the curiosity reward for the mini_batch. Corresponds to the error
        between the predicted and actual next state.
        """
        # get predicted next and actual next, compare to get reward
        predicted_next_state = self.predict_next_state(mini_batch)
        target = self.get_next_state(mini_batch)
        sq_difference = 0.5 * (target - predicted_next_state) ** 2
        sq_difference = torch.sum(sq_difference, dim=0) # note changed to 0
        print(sq_difference)
        print("end computing reward")
        return sq_difference

    def compute_forward_loss(self, mini_batch: Dict) -> torch.Tensor:
        """
        Computes the loss for the next state prediction
        """
        # use loss for gradient
        # can calculate this anyways I want
        print("begin computing loss")
        loss = torch.mean(
            self.compute_reward(mini_batch)
        )
        print("end computing loss")
        return loss


print("begin")
# curiosity network would have all bits
# network = CuriosityNetwork(specs, settings)
# for now, just the one forward pass optimizing, not both parts at once
network = PredictorNetwork()

optimizer = torch.optim.Adam(
    network.parameters(), lr=learningRate
)

data = {"curr": {"state": torch.tensor([0, 0, 1, 0], dtype=torch.float), "action": torch.tensor([0, 1, 0, 0 , 0, 0, 0], dtype=torch.float)},
"next": {"state": torch.tensor([0, 0, 0, 1], dtype=torch.float), "action": torch.tensor([1, 0, 0, 0 , 0, 0, 0])}}

print("before call for update")
# doing a forwards step
forward_loss = network.compute_forward_loss(data)
print("after update")
optimizer.zero_grad() # sets gradients to 0 to start so no weird carry over
forward_loss.backward() # backpropagation
optimizer.step() # gradient descent
print("optimizing step")
