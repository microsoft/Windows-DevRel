import os
import subprocess
import sys
import urllib.request

# Define the URL of the script
url = "https://github.com/google/jax/raw/main/examples/onnx2xla.py"
script_name = "onnx2xla.py"
destination_folder = ".temp"
destination_file_script = os.path.join(destination_folder, script_name)

# Download the script using urllib
urllib.request.urlretrieve(url, destination_file_script)

# Execute the downloaded script
result = subprocess.run([sys.executable, script_name], capture_output=True, check=True, text=True, cwd=destination_folder)

# Print the exit code and exit with the same code
exit_code = result.returncode
print(result.stdout)

sys.exit(exit_code)