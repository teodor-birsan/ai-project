from mlagents.trainers.behavior_id_utils import BehaviorIdentifiers
from mlagents.trainers.optimizer.torch_optimizer import TorchOptimizer
from mlagents.trainers.policy import Policy
from mlagents.trainers.trainer.on_policy_trainer import OnPolicyTrainer
from mlagents.trainers.trajectory import Trajectory
from mlagents_envs.base_env import BehaviorSpec


class CustomPPOTrainer(OnPolicyTrainer):
    def create_optimizer(self) -> TorchOptimizer:
        pass

    def _process_trajectory(self, trajectory: Trajectory) -> None:
        pass

    def create_policy(self, parsed_behavior_id: BehaviorIdentifiers, behavior_spec: BehaviorSpec) -> Policy:
        pass
