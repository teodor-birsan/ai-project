from mlagents.trainers.behavior_id_utils import BehaviorIdentifiers
from mlagents.trainers.optimizer.torch_optimizer import TorchOptimizer
from mlagents.trainers.policy import Policy
from mlagents.trainers.trainer.off_policy_trainer import OffPolicyTrainer
from mlagents.trainers.trajectory import Trajectory
from mlagents_envs.base_env import BehaviorSpec


class CustomSACTrainer(OffPolicyTrainer):
    def _process_trajectory(self, trajectory: Trajectory) -> None:
        pass

    def create_policy(self, parsed_behavior_id: BehaviorIdentifiers, behavior_spec: BehaviorSpec) -> Policy:
        pass

    def create_optimizer(self) -> TorchOptimizer:
        pass
