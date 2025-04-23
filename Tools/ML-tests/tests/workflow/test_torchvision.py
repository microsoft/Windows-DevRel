import os
import subprocess
import sys
import urllib.request

# Define the URL of the script
url = "https://raw.githubusercontent.com/pytorch/examples/refs/heads/main/mnist/main.py"
script_name = "main.py"
destination_folder = ".temp"
destination_file_script = os.path.join(destination_folder, script_name)

# Download the script using urllib
urllib.request.urlretrieve(url, destination_file_script)
with open(destination_file_script, "r") as f:
    content = f.read()
content = content.replace(
    "datasets.MNIST('../data'",
    "datasets.MNIST('data'"
)

with open(destination_file_script, "w") as f:
    f.write(content)

# Execute the downloaded script
result = subprocess.run([sys.executable, script_name, "--epochs", "1"], capture_output=True, check=True, text=True, cwd=destination_folder)

# Print the exit code and exit with the same code
exit_code = result.returncode
print(result.stdout)

sys.exit(exit_code)