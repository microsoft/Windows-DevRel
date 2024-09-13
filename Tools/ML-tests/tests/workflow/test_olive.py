import olive
from olive import __version__
import sys
import urllib.request
import os
import subprocess

#from huggingface_hub import hf_hub_download

# Define the URLs and the destination file paths
url_template = "https://github.com/microsoft/Olive/raw/main/examples/phi3/phi3_template.json"
url_script = "https://github.com/microsoft/Olive/raw/main/examples/phi3/phi3.py"
# url_quarot = "https://github.com/microsoft/Olive/raw/main/examples/phi3/pass_configs/quarot.json"
destination_folder = ".temp"
destination_file_template = os.path.join(destination_folder, "phi3_template.json")
destination_file_script = os.path.join(destination_folder, "phi3.py")

destination_pass_configs_folder = os.path.join(destination_folder, "pass_configs")
os.makedirs(destination_pass_configs_folder, exist_ok=True)
destination_file_quarot = os.path.join(destination_pass_configs_folder, "quarot.json")

# Create the destination folder if it doesn't exist
os.makedirs(destination_folder, exist_ok=True)

try:
    urllib.request.urlretrieve(url_template, destination_file_template)
    print(f"Downloaded file to {destination_file_template}")

    urllib.request.urlretrieve(url_script, destination_file_script)
    print(f"Downloaded file to {destination_file_script}")

    # urllib.request.urlretrieve(url_quarot, destination_file_quarot)
    # print(f"Downloaded file to {destination_file_quarot}")

    # Disable hardcoded quarot in phi3.py
    # with open(destination_file_script, 'r') as file:
    #     content = file.read()

    # content = content.replace('args.quarot = True', 'args.quarot = False')
    # with open(destination_file_script, 'w') as file:
    #     file.write(content)

    # print(f"Updated {destination_file_script} with args.quarot = False")

    # Print olive version
    print(f"olive version: {__version__}")

    # run huggingface cli login with token
    subprocess.run(["huggingface-cli", "login", "--token", os.getenv("HF_TOKEN")], check=True)
    

    log = subprocess.run([sys.executable, "phi3.py", "--target", "mobile",  "--inference", "--prompt", "type exactly this: Hello, World!", "--max_length", "50"], capture_output=True, check=True, text=True, cwd=destination_folder)
    print(log.stdout)
    print(log.stderr)

    if log.stdout.find("<|assistant|>\n Hello, World!") != -1:
        sys.exit(0)
    else:
        sys.exit(1)

except subprocess.CalledProcessError as e:
    print(f"{e.output}\n")
    print(f"{e}\n")
    print(f"{e.stderr}")
    sys.exit(1)

except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)