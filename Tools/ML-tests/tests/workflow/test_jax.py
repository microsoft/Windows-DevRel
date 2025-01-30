import os
import subprocess
import sys
import urllib.request

# Define the URL of the script
url_mnist = "https://github.com/jax-ml/jax/raw/refs/heads/main/examples/mnist_classifier.py"
url_dataset = "https://github.com/jax-ml/jax/raw/refs/heads/main/examples/datasets.py";
script_name = "mnist.py"
destination_folder = ".temp"
destination_file_mnist = os.path.join(destination_folder, script_name)
destination_file_dataset = os.path.join(destination_folder, "datasets.py")

# Download the script using urllib
urllib.request.urlretrieve(url_mnist, destination_file_mnist)
urllib.request.urlretrieve(url_mnist, destination_file_dataset)

# Execute the downloaded script
result = subprocess.run([sys.executable, script_name], capture_output=True, check=True, text=True, cwd=destination_folder)

# Print the exit code and exit with the same code
exit_code = result.returncode
print(result.stdout)

sys.exit(exit_code)