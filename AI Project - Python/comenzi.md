***ASTEA TREBUIE RULATE IN POWERSHELL***
`$env:PYTHONPATH = "path-ul complet al proiectului in python"`
`conda activate mlagents`(sau ce nume are environmnet-ul cu pachetele mlagents)
`pip install -e ./trainer_plugins`: asta trebuie doar o data dupa ce se da clone la proiect de pe github
`cd .\config\ppo`: asta ca sa fie comanda de train mai scurta
`mlagents-train ./custom_ppo.yaml --run-id nume-run`
`$env:PYTHONPATH='D:\Facultate\Proiect AI\ai-project\AI Project - Python'`