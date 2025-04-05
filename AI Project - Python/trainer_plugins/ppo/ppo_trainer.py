from typing import Union, Type, Dict, Any, cast

from mlagents.trainers.behavior_id_utils import BehaviorIdentifiers
from mlagents.trainers.buffer import BufferKey, RewardSignalUtil
from mlagents.trainers.optimizer.torch_optimizer import TorchOptimizer
from mlagents.trainers.policy import Policy
from mlagents.trainers.policy.torch_policy import TorchPolicy
from mlagents.trainers.ppo.trainer import TRAINER_NAME
from mlagents.trainers.settings import TrainerSettings
from mlagents.trainers.torch_entities.networks import SimpleActor, SharedActorCritic
from mlagents.trainers.trainer.on_policy_trainer import OnPolicyTrainer
from mlagents.trainers.trainer.trainer_utils import get_gae
from mlagents.trainers.trajectory import Trajectory
from mlagents_envs.base_env import BehaviorSpec
import numpy as np

from trainer_plugins.ppo.PPOSettings import PPOSettings
from trainer_plugins.ppo.ppo_optimizer import CustomOptimizer


class CustomPPOTrainer(OnPolicyTrainer):
    def __init__(
            self,
            behavior_name: str,
            reward_buff_cap: int,
            trainer_settings: TrainerSettings,
            training: bool,
            load: bool,
            seed: int,
            artifact_path: str,
    ):
        super().__init__(behavior_name, reward_buff_cap, trainer_settings, training, load, seed, artifact_path)
        self.hyperparameters: PPOSettings = cast(
            PPOSettings, self.trainer_settings.hyperparameters
        )
        self.seed = seed
        self.shared_critic = self.hyperparameters.shared_critic
        self.policy: TorchPolicy = None  # type: ignore

    def create_optimizer(self) -> TorchOptimizer:
        return CustomOptimizer(  # type: ignore
            cast(TorchPolicy, self.policy), self.trainer_settings  # type: ignore
        )  # type: ignore

    def _process_trajectory(self, trajectory: Trajectory) -> None:
        super()._process_trajectory(trajectory)  # handle generic processing logic
        agent_id = trajectory.agent_id  # all agents should have the same id
        agent_buffer_trajectory = trajectory.to_agentbuffer()  # convert trajectory to agent buffer
        self._warn_if_group_reward(agent_buffer_trajectory)  # group reward warning if training multiple agents

        if self.is_training:
            self.policy.actor.update_normalization(agent_buffer_trajectory)
            self.optimizer.critic.update_normalization(agent_buffer_trajectory)

        value_estimates, value_next, value_memories = self.optimizer.get_trajectory_value_estimates(
            agent_buffer_trajectory,
            trajectory.next_obs,
            trajectory.done_reached and not trajectory.interrupted
        )

        if value_memories is not None:
            agent_buffer_trajectory[BufferKey.CRITIC_MEMORY].set(value_memories)

        for name, v in value_estimates.items():
            agent_buffer_trajectory[RewardSignalUtil.value_estimates_key(name)].extend(v)
            self._stats_reporter.add_stat(
                f"Policy/{self.optimizer.reward_signals[name].name.capitalize()} Value Estimate",
                np.mean(v)
            )

        self.collected_rewards["environment"][agent_id] += np.sum(
            agent_buffer_trajectory[BufferKey.ENVIRONMENT_REWARDS]
        )
        for name, reward_signal in self.optimizer.reward_signals.items():
            evaluate_result = (
                    reward_signal.evaluate(agent_buffer_trajectory) * reward_signal.strength
            )
            agent_buffer_trajectory[RewardSignalUtil.rewards_key(name)].extend(evaluate_result)
            self.collected_rewards[name][agent_id] += np.sum(evaluate_result)

        tmp_advantages = []
        tmp_returns = []
        for name in self.optimizer.reward_signals:
            bootstrap_value = value_next[name]

            local_rewards = agent_buffer_trajectory[
                RewardSignalUtil.rewards_key(name)
            ].get_batch()
            local_value_estimates = agent_buffer_trajectory[
                RewardSignalUtil.value_estimates_key(name)
            ].get_batch()

            local_advantage = get_gae(
                rewards=local_rewards,
                value_estimates=local_value_estimates,
                value_next=bootstrap_value,
                gamma=self.optimizer.reward_signals[name].gamma,
                lambd=self.hyperparameters.lambd,
            )
            local_return = local_advantage + local_value_estimates
            agent_buffer_trajectory[RewardSignalUtil.returns_key(name)].set(local_return)
            agent_buffer_trajectory[RewardSignalUtil.advantage_key(name)].set(local_advantage)
            tmp_advantages.append(local_advantage)
            tmp_returns.append(local_return)

        global_advantages = list(np.mean(np.array(tmp_advantages, dtype=np.float32), axis=0))
        global_returns = list(np.mean(np.array(tmp_returns, dtype=np.float32), axis=0))
        agent_buffer_trajectory[BufferKey.ADVANTAGES].set(global_advantages)
        agent_buffer_trajectory[BufferKey.DISCOUNTED_RETURNS].set(global_returns)

        self._append_to_update_buffer(agent_buffer_trajectory)

        if trajectory.done_reached:
            self._update_end_episode_stats(agent_id, self.optimizer)

    def create_policy(self, parsed_behavior_id: BehaviorIdentifiers, behavior_spec: BehaviorSpec) -> Policy:
        actor_cls: Union[
            Type[SimpleActor],
            Type[SharedActorCritic]
        ] = SimpleActor
        actor_kwargs: Dict[str, Any] = {
            "conditional_sigma": False,
            "tanh_squash": True
        }

        if self.shared_critic:
            reward_signal_configs = self.trainer_settings.reward_signals
            reward_signal_names = [
                key.value for key,
                _ in reward_signal_configs.items()
            ]
            actor_cls = SharedActorCritic
            actor_kwargs.update({"stream_names": reward_signal_names})

        policy = TorchPolicy(
            self.seed,
            behavior_spec,
            self.trainer_settings.network_settings,
            actor_cls,
            actor_kwargs,
        )
        return policy

    @staticmethod
    def get_type_and_settings():
        return {CustomPPOTrainer.get_trainer_name(): CustomPPOTrainer}, {
            CustomPPOTrainer.get_trainer_name(): PPOSettings}
