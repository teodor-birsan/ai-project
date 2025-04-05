from typing import Dict

from mlagents.trainers.buffer import AgentBuffer
from mlagents.trainers.optimizer.torch_optimizer import TorchOptimizer


class CustomOptimizer(TorchOptimizer):
    def update(self, batch: AgentBuffer, num_sequences: int) -> Dict[str, float]:
        pass
