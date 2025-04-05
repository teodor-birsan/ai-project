from setuptools import setup

from trainer_plugins.ppo.ppo_trainer import CustomPPOTrainer

setup(
    name="custom_algorithm",
    version="1.0",
    entry_points={
        CustomPPOTrainer: [
            "custom_ppo=ppo.ppo_trainer:get_type_and_settings"
        ]
    }
)
