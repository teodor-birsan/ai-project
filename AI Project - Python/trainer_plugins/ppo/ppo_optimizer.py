from typing import Dict, cast
from mlagents.torch_utils import default_device, torch
from mlagents.trainers.buffer import AgentBuffer, BufferKey
from mlagents.trainers.optimizer.torch_optimizer import TorchOptimizer
from mlagents.trainers.policy.torch_policy import TorchPolicy
from mlagents.trainers.torch_entities.networks import ValueNetwork
from mlagents.trainers.torch_entities.utils import ModelUtils

import PPOSettings

class CustomOptimizer(TorchOptimizer):
    def __init__(self, policy:TorchPolicy, trainer_settings: PPOSettings):
        super().__init__(policy, trainer_settings)
        reward_signal_configs = trainer_settings.reward_signals
        reward_signal_names = [key.value for key, _ in reward_signal_configs.items()]

        self.hyperparameters: PPOSettings = cast(
            PPOSettings, trainer_settings.hyperparameters
        )

        params = list(self.policy.actor.parameters())

        if self.hyperparameters.shared_critic:
            self._critic = policy.actor
        else:
            self._critic = ValueNetwork(
                reward_signal_names,
                policy.behavior_spec.observation_specs,
                network_settings=trainer_settings.network_settings,
            )
            self._critic.to(default_device())
            params += list(self._critic.parameters())

        self.decay_learning_rate = ModelUtils.DecayedValue(
            self.hyperparameters.learning_rate_schedule,
            self.hyperparameters.learning_rate,
            1e-10,
            self.trainer_settings.max_steps,
        )
        self.decay_epsilon = ModelUtils.DecayedValue(
            self.hyperparameters.epsilon_schedule,
            self.hyperparameters.epsilon,
            0.1,
            self.trainer_settings.max_steps,
        )
        self.decay_beta = ModelUtils.DecayedValue(
            self.hyperparameters.beta_schedule,
            self.hyperparameters.beta,
            1e-5,
            self.trainer_settings.max_steps,
        )

        self.optimizer = torch.optim.Adam(
            params, lr=self.trainer_settings.hyperparameters.learning_rate
        )

        self.stats_name_to_update_name = {
            "Losses/Value Loss": "value_loss",
            "Losses/Policy Loss": "policy_loss",
        }

        self.stream_names = list(self.reward_signals.keys())

    def update(self, batch: AgentBuffer, num_sequences: int) -> Dict[str, float]:
        # Use precomputed decay values instead of ModelUtils.schedule_value
        decay_eps = self.decay_epsilon.get_value(self.policy.get_current_step())
        decay_beta = self.decay_beta.get_value(self.policy.get_current_step())
        decay_lr = self.decay_learning_rate.get_value(self.policy.get_current_step())

        ModelUtils.update_learning_rate(self.optimizer, decay_lr)

        current_obs = ModelUtils.list_to_tensor(batch[BufferKey.OBSERVATIONS])
        actions = ModelUtils.list_to_tensor(batch[BufferKey.ACTIONS])
        old_log_probs = ModelUtils.list_to_tensor(batch[BufferKey.LOG_PROBS])
        advantages = ModelUtils.list_to_tensor(batch[BufferKey.ADVANTAGES])
        target_values = ModelUtils.list_to_tensor(batch[BufferKey.RETURNS])
        act_masks = ModelUtils.list_to_tensor(batch[BufferKey.MASKS])
        loss_masks = ModelUtils.list_to_tensor(batch[BufferKey.MASKS])

        memories = ModelUtils.list_to_tensor(batch.get(BufferKey.MEMORY, []))
        value_memories = ModelUtils.list_to_tensor(batch.get(BufferKey.MEMORY, []))

        run_out = self.policy.actor.get_stats(
            current_obs,
            actions,
            masks=act_masks,
            memories=memories,
            sequence_length=self.policy.sequence_length,
        )
        log_probs = run_out["log_probs"]
        entropy = run_out["entropy"]

        values, _ = self._critic.critic_pass(  # Fixed: use self._critic instead of self.policy.critic
            current_obs,
            memories=value_memories,
            sequence_length=self.policy.sequence_length,
        )

        value_loss = ModelUtils.masked_mean((target_values - values) ** 2, loss_masks)

        policy_loss = ModelUtils.trust_region_policy_loss(
            advantages, log_probs, old_log_probs, loss_masks, decay_eps
        )

        loss = (
                policy_loss
                + 0.5 * value_loss
                - decay_beta * ModelUtils.masked_mean(entropy, loss_masks)
        )

        self.optimizer.zero_grad()
        loss.backward()
        self.optimizer.step()

        return {
            "Losses/Policy Loss": torch.abs(policy_loss).item(),
            "Losses/Value Loss": value_loss.item(),
            "Policy/Learning Rate": decay_lr,
            "Policy/Epsilon": decay_eps,
            "Policy/Beta": decay_beta,
        }

