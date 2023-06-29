import numpy as np
from time import time
from datetime import datetime
from itertools import chain
from typing import Dict, NamedTuple
from mlagents.torch_utils import torch, default_device

from mlagents.trainers.buffer import AgentBuffer, BufferKey
from mlagents.trainers.torch_entities.components.reward_providers.base_reward_provider import (
    BaseRewardProvider,
)
from mlagents.trainers.settings import CuriositySettings

from mlagents_envs.base_env import BehaviorSpec, ObservationSpec
from mlagents_envs import logging_util
from mlagents.trainers.torch_entities.agent_action import AgentAction
from mlagents.trainers.torch_entities.action_flattener import ActionFlattener
from mlagents.trainers.torch_entities.utils import ModelUtils
from mlagents.trainers.torch_entities.networks import NetworkBody
from mlagents.trainers.torch_entities.layers import LinearEncoder, linear_layer
from mlagents.trainers.trajectory import ObsUtil

logger = logging_util.get_logger(__name__)


class ActionPredictionTuple(NamedTuple):
    continuous: torch.Tensor
    discrete: torch.Tensor


class CuriosityRewardProvider(BaseRewardProvider):
    beta = 0.2  # Forward vs Inverse loss weight
    loss_multiplier = 10.0  # Loss multiplier

    def __init__(self, specs: BehaviorSpec, settings: CuriositySettings) -> None:
        super().__init__(specs, settings)
        self._ignore_done = True
        self._network = CuriosityNetwork(specs, settings)
        self._network.to(default_device())


        # we are separating out, optimizing 2 separate things while training
        # forward is just curr state and action used to predict next state
        forwardModelParams = self._network.forward_model_next_state_prediction.parameters()
        self.optimizerForward = torch.optim.Adam(
            forwardModelParams, lr=settings.learning_rate
        )

        # # not directly going to use inverse, only the feature encoder aspect will be saved
        # inverseModelParams = (chain(chain(self._network.inverse_model_action_encoding.parameters(),
        #     self._network.newFeatureEncoder.parameters()),
        #     self._network.discrete_action_prediction.parameters()))

        # self.optimizerInverse = torch.optim.Adam(
        #     inverseModelParams, lr=settings.learning_rate
        # )

        # not directly going to use inverse, only the feature encoder aspect will be saved
        futureModelParams = (chain(chain(self._network.future_forward_model.parameters(),
            self._network.newFeatureEncoder.parameters()),
            self._network.discrete_action_prediction.parameters()))

        self.optimizerFuture = torch.optim.Adam(
            futureModelParams, lr=settings.learning_rate
        )

        self._has_updated_once = False

    def evaluate(self, mini_batch: AgentBuffer) -> np.ndarray:
        with torch.no_grad():
            rewards = ModelUtils.to_numpy(self._network.compute_reward(mini_batch, True))
        rewards = np.minimum(rewards, 1.0 / self.strength)
        return rewards * self._has_updated_once

    def update(self, mini_batch: AgentBuffer) -> Dict[str, np.ndarray]:
        self._has_updated_once = True
        forward_loss = self._network.compute_forward_loss(mini_batch, True)
        # inverse_loss = self._network.compute_inverse_loss(mini_batch)
        future_loss = self._network.compute_forward_loss(mini_batch, False)

        # using both of them to calculate the gradient
        # loss = self.loss_multiplier * (
        #     self.beta * forward_loss + (1.0 - self.beta) * inverse_loss
        # )
        self.optimizerForward.zero_grad() # sets gradients to 0 to start so no weird carry over
        forward_loss.backward() # backpropagation
        self.optimizerForward.step() # gradient descent

        # self.optimizerInverse.zero_grad() # sets gradients to 0 to start so no weird carry over
        # inverse_loss.backward() # backpropagation
        # self.optimizerInverse.step() # gradient descent
        self.optimizerFuture.zero_grad() # sets gradients to 0 to start so no weird carry over
        future_loss.backward() # backpropagation
        self.optimizerFuture.step() # gradient descent

        # reward = torch.mean(self._network.compute_reward(mini_batch, True))

        return {
            "Losses/Curiosity Forward Loss": forward_loss.item(),
            # "Losses/Curiosity Inverse Loss": inverse_loss.item(),
            "Losses/Curiosity Future Loss": future_loss.item(),
            # "Reward/Curiosity": reward.item()
        }

    def get_modules(self):
        return {f"Module:{self.name}": self._network}


class CuriosityNetwork(torch.nn.Module):
    EPSILON = 1e-10

    def __init__(self, specs: BehaviorSpec, settings: CuriositySettings) -> None:
        super().__init__()
        self._action_spec = specs.action_spec

        state_encoder_settings = settings.network_settings
        if state_encoder_settings.memory is not None:
            state_encoder_settings.memory = None
            logger.warning(
                "memory was specified in network_settings but is not supported by Curiosity. It is being ignored."
            )

        # using to print stuff more easily
        self.once = True

        # something to set for training higher levels, will use to automatically change code as needed
        self.loadFeatureProcessor = False

        # set to decide where saving feature encoder
        featureFolder = "C:\\users\\terra\\desktop\\thesis\\PA_Learning_Agent\\my_feature_models\\"
        featureFileSave = "run"
        featureFileLoad = "layer_future_Feature_0627_19_09.pth"


        # if needed, load the old feature encoder to be used
        if self.loadFeatureProcessor:
            # defining the path to save to, includes bit to help increment later
            # including data on what loaded data was used
            self.featureSavePath = featureFolder + featureFileSave + "_Loaded_" + featureFileLoad.split(".")[0] + "_" + "Feature_"
            self.forwardSavePath = featureFolder + featureFileSave + "_Loaded_" + featureFileLoad.split(".")[0] + "_" + "Forward_"
            # self.inverseSavePath = featureFolder + featureFileSave + "_Loaded_" + featureFileLoad.split(".")[0] + "_" + "Inverse_"
            self.futureSavePath = featureFolder + featureFileSave + "_Loaded_" + featureFileLoad.split(".")[0] + "_" + "Future_"

            self.currFeatureEncoder = torch.load(featureFolder + featureFileLoad)
            # defining size features will be encoded as to define size of other networks
            featureEncoderSize = self.currFeatureEncoder._body_endoder.seq_layers[0].out_features
            # changing in state encoder settings as well since if different from default now
            obs = [ObservationSpec(shape=(featureEncoderSize,), dimension_property=(1,), observation_type=0,
                name='VectorSensor_size' + str(featureEncoderSize))]
        else:
            # defining the path to save to, includes bit to help increment later
            self.featureSavePath = featureFolder + featureFileSave + "_Feature_"
            self.forwardSavePath = featureFolder + featureFileSave + "_Forward_" 
            # self.inverseSavePath = featureFolder + featureFileSave + "_Inverse_"
            self.futureSavePath = featureFolder + featureFileSave + "_Future_"

            self.currFeatureEncoder = None

            # the size of other networks is based on raw data when we don't load
            featureEncoderSize = 55 # hardcoded from unity editor
            obs = specs.observation_specs


        # we create the network, but will not be using it, just recording it
        self.newFeatureEncoder = NetworkBody(
            obs, state_encoder_settings
        )


        self._action_flattener = ActionFlattener(self._action_spec)

        # # inverse model will be taking in input from newly training feature
        # self.inverse_model_action_encoding = torch.nn.Sequential(
        #     LinearEncoder(2 * state_encoder_settings.hidden_units, 1, 256)
        # )


        # forward taking in input from only currently loaded feature model or none
        self.future_forward_model = torch.nn.Sequential(
            # can write this myself, check out class to help
            LinearEncoder( # fancy mlp
                # normally returns sigmoid of data - kinda spits out 1 hot encoding?
                state_encoder_settings.hidden_units
                + self._action_flattener.flattened_size,
                1,
                256,
            ),
            # adding another layer as don't want output to be the swish
            # recovers from reduction of swish?
            linear_layer(256, state_encoder_settings.hidden_units),
        )


        if self._action_spec.continuous_size > 0:
            self.continuous_action_prediction = linear_layer(
                256, self._action_spec.continuous_size
            )
        if self._action_spec.discrete_size > 0:
            self.discrete_action_prediction = linear_layer(
                256, sum(self._action_spec.discrete_branches)
            )

        # forward taking in input from only currently loaded feature model or none
        self.forward_model_next_state_prediction = torch.nn.Sequential(
            # can write this myself, check out class to help
            LinearEncoder( # fancy mlp
                # normally returns sigmoid of data - kinda spits out 1 hot encoding?
                featureEncoderSize
                + self._action_flattener.flattened_size,
                1,
                256,
            ),
            # adding another layer as don't want output to be the swish
            # recovers from reduction of swish?
            linear_layer(256, featureEncoderSize),
        )

        print("Currently used feature encoder")
        print(self.currFeatureEncoder)
        print("Feature encoder being trained at this level")
        print(self.newFeatureEncoder)
        print("The shape of the forward model")
        print(self.forward_model_next_state_prediction)
        # print("The shape of the inverse model")
        # print(self.inverse_model_action_encoding)
        print("The shape of the future model")
        print(self.future_forward_model)

        



    def __del__(self):
        # add on timestamp at time complete
        # get month/day and time to minute in order to make unique file names 
        timestamp = datetime.fromtimestamp(time())
        timestamp = timestamp.strftime("%m%d_%H_%M")

        self.featureSavePath = self.featureSavePath + timestamp + ".pth"
        self.forwardSavePath = self.forwardSavePath + timestamp + ".pth"
        # self.inverseSavePath = self.inverseSavePath + timestamp + ".pth"
        self.futureSavePath = self.futureSavePath + timestamp + ".pth"

        print("Attempting to save file to " + self.featureSavePath)
        torch.save(self.newFeatureEncoder, self.featureSavePath)
        print("successfuly saved")

        print("Attempting to save file to " + self.forwardSavePath)
        torch.save(self.forward_model_next_state_prediction, self.forwardSavePath)
        print("successfuly saved")

        # print("Attempting to save file to " + self.inverseSavePath)
        # torch.save(self.inverse_model_action_encoding, self.inverseSavePath)
        # print("successfuly saved")

        print("Attempting to save file to " + self.futureSavePath)
        torch.save(self.future_forward_model, self.futureSavePath)
        print("successfuly saved")



    # these next get state functions, pull different mini batches from buffer
    # use those to get the required encoding
    def get_current_state(self, mini_batch: AgentBuffer, trainingNewEncoder: bool) -> torch.Tensor:
        """
        Extracts the current state embedding from a mini_batch.
        """
        if self.loadFeatureProcessor:
            n_obs = len(self.currFeatureEncoder.processors)
        else:
            n_obs = 1 # I think this works but might need to check

        np_obs = ObsUtil.from_buffer(mini_batch, n_obs)
        # Convert to tensors
        # this list is the raw state
        tensor_obs = [ModelUtils.list_to_tensor(obs) for obs in np_obs]

        # run through however many feature processors needed for current action
        if self.loadFeatureProcessor and trainingNewEncoder:
            preCurrState, _ = self.currFeatureEncoder.forward(tensor_obs)
            currState, _ = self.newFeatureEncoder.forward([preCurrState])
        elif self.loadFeatureProcessor:
            currState, _ = self.currFeatureEncoder.forward(tensor_obs)
        elif trainingNewEncoder:
            currState, _ = self.newFeatureEncoder.forward(tensor_obs)
        else:
            currState = tensor_obs[0]

        return currState

    def get_next_state(self, mini_batch: AgentBuffer, trainingNewEncoder: bool) -> torch.Tensor:
        """
        Extracts the next state embedding from a mini_batch.
        """
        if self.loadFeatureProcessor:
            n_obs = len(self.currFeatureEncoder.processors)
        else:
            n_obs = 1 # I think this works but might need to check

        np_obs = ObsUtil.from_buffer_next(mini_batch, n_obs) # the next is where different from get current
        # Convert to tensors
        tensor_obs = [ModelUtils.list_to_tensor(obs) for obs in np_obs]

        # run through however many feature processors needed for current action
        if self.loadFeatureProcessor and trainingNewEncoder:
            preCurrState, _ = self.currFeatureEncoder.forward(tensor_obs)
            currState, _ = self.newFeatureEncoder.forward([preCurrState])
        elif self.loadFeatureProcessor:
            currState, _ = self.currFeatureEncoder.forward(tensor_obs)
        elif trainingNewEncoder:
            currState, _ = self.newFeatureEncoder.forward(tensor_obs)
        else:
            currState = tensor_obs[0]

        return currState

    def predict_action(self, mini_batch: AgentBuffer) -> ActionPredictionTuple:
        """
        In the continuous case, returns the predicted action.
        In the discrete case, returns the logits.
        """
        inverse_model_input = torch.cat(
            # current with actual next state
            # this run is to train feature so always true
            (self.get_current_state(mini_batch, True), self.get_next_state(mini_batch, True)), dim=1
        )

        continuous_pred = None
        discrete_pred = None
        hidden = self.inverse_model_action_encoding(inverse_model_input)
        if self._action_spec.continuous_size > 0:
            continuous_pred = self.continuous_action_prediction(hidden)

        # forward pass of inverse model
        # seems to be just a linear layer? okay?
        if self._action_spec.discrete_size > 0:
            raw_discrete_pred = self.discrete_action_prediction(hidden)
            branches = ModelUtils.break_into_branches(
                raw_discrete_pred, self._action_spec.discrete_branches
            )
            branches = [torch.softmax(b, dim=1) for b in branches]
            discrete_pred = torch.cat(branches, dim=1)

        return ActionPredictionTuple(continuous_pred, discrete_pred)

    def predict_next_state(self, mini_batch: AgentBuffer, on_current_model: bool) -> torch.Tensor:
        """
        Uses the current state embedding and the action of the mini_batch to predict
        the next state embedding.
        """
        # forward pass of predicting next state
        actions = AgentAction.from_buffer(mini_batch)
        flattened_action = self._action_flattener.forward(actions) 
        if on_current_model:
            forward_model_input = torch.cat( # grabbing the st and the action, to feed into model
                # false because not training new feature, just want current
                (self.get_current_state(mini_batch, False), flattened_action), dim=1
            )

            # now that have input, call function with it, and output we return is prediction
            # that is the model I want, have notes above on how can recreate for myself
            return self.forward_model_next_state_prediction(forward_model_input)
        else:
            forward_model_input = torch.cat( # grabbing the st and the action, to feed into model
                # true because training new feature
                (self.get_current_state(mini_batch, True), flattened_action), dim=1
            )

            return self.future_forward_model(forward_model_input)


    def compute_inverse_loss(self, mini_batch: AgentBuffer) -> torch.Tensor:
        """
        Computes the inverse loss for a mini_batch. Corresponds to the error on the
        action prediction (given the current and next state).
        """
        # use difference between pred action and actual action to compute loss
        predicted_action = self.predict_action(mini_batch)
        actions = AgentAction.from_buffer(mini_batch)
        _inverse_loss = 0
        if self._action_spec.continuous_size > 0:
            sq_difference = (
                actions.continuous_tensor - predicted_action.continuous
            ) ** 2
            sq_difference = torch.sum(sq_difference, dim=1)
            _inverse_loss += torch.mean(
                ModelUtils.dynamic_partition(
                    sq_difference,
                    ModelUtils.list_to_tensor(
                        mini_batch[BufferKey.MASKS], dtype=torch.float
                    ),
                    2,
                )[1]
            )
        if self._action_spec.discrete_size > 0:
            true_action = torch.cat(
                ModelUtils.actions_to_onehot(
                    actions.discrete_tensor, self._action_spec.discrete_branches
                ),
                dim=1,
            )
            cross_entropy = torch.sum(
                -torch.log(predicted_action.discrete + self.EPSILON) * true_action,
                dim=1,
            )
            _inverse_loss += torch.mean(
                ModelUtils.dynamic_partition(
                    cross_entropy,
                    ModelUtils.list_to_tensor(
                        mini_batch[BufferKey.MASKS], dtype=torch.float
                    ),  # use masks not action_masks
                    2,
                )[1]
            )
        return _inverse_loss

    def compute_reward(self, mini_batch: AgentBuffer, on_current_model: bool) -> torch.Tensor:
        """
        Calculates the curiosity reward for the mini_batch. Corresponds to the error
        between the predicted and actual next state.
        """
        # get predicted next and actual next, compare to get reward
        if on_current_model:
            predicted_next_state = self.predict_next_state(mini_batch, True)
            # this is just independent forward model, so not training new feature encoder so always false
            target = self.get_next_state(mini_batch, False)
        else:
            predicted_next_state = self.predict_next_state(mini_batch, False)
            # training new feature encoder so true
            target = self.get_next_state(mini_batch, True)

        sq_difference = 0.5 * (target - predicted_next_state) ** 2
        sq_difference = torch.sum(sq_difference, dim=1)
        return sq_difference

    def compute_forward_loss(self, mini_batch: AgentBuffer, on_current_model: bool) -> torch.Tensor:
        """
        Computes the loss for the next state prediction
        """
        # use loss for gradient
        # can calculate this anyways I want
        return torch.mean(
            ModelUtils.dynamic_partition(
                self.compute_reward(mini_batch, on_current_model),
                ModelUtils.list_to_tensor(
                    mini_batch[BufferKey.MASKS], dtype=torch.float
                ),
                2,
            )[1]
        )