from mlagents.plugins import ML_AGENTS_TRAINER_TYPE
from setuptools import setup

setup(
    name="custom_algorithm",
    version="1.0",
    entry_points={
        ML_AGENTS_TRAINER_TYPE: [
            "custom_ppo=trainer_plugins.trainers.ppo.ppo_trainer:get_type_and_setting"
        ]
    }
)
