import os
import subprocess
import sys
import urllib.request

import numpy as np
import torch

# Define the URL of the script
url = "https://raw.githubusercontent.com/pytorch/examples/refs/heads/main/time_sequence_prediction/train.py"
script_name = "train.py"
destination_folder = ".temp"
destination_file_script = os.path.join(destination_folder, script_name)

# Download the script using urllib
urllib.request.urlretrieve(url, destination_file_script)
with open(destination_file_script, "r") as f:
    content = f.read()
content = content.replace(
    "data = torch.load('traindata.pt')",
    "data = torch.load('traindata.pt', weights_only=False)"
)

with open(destination_file_script, "w") as f:
    f.write(content)

# Create a toy dataset
np.random.seed(2)

T = 20
L = 1000
N = 100

x = np.empty((N, L), 'int64')
x[:] = np.array(range(L)) + np.random.randint(-4 * T, 4 * T, N).reshape(N, 1)
data = np.sin(x / 1.0 / T).astype('float64')
torch.save(data, open(os.path.join(destination_folder, 'traindata.pt'), 'wb'))

# Execute the downloaded script
result = subprocess.run([sys.executable, script_name], capture_output=True, check=True, text=True, cwd=destination_folder)

# Print the exit code and exit with the same code
exit_code = result.returncode
print(result.stdout)

sys.exit(exit_code)