try:
    import sys
    import onnxruntime
    import numpy as np
    import requests

except Exception as e:
    print(f"ERROR:{e}")
    sys.exit(1)

# Function to download the ONNX model
def download_model(url, model_path):
    response = requests.get(url)
    response.raise_for_status()  # Ensure the request was successful
    with open(model_path, 'wb') as f:
        f.write(response.content)

# Function to run the ONNX model on CPU using ONNX Runtime
def run_model(model_path):
    try:
        # Load the ONNX model
        session = onnxruntime.InferenceSession(model_path, providers=['CPUExecutionProvider'])

        # Get input shape for the model
        input_shape = session.get_inputs()[0].shape

        # Prepare a random input with the required shape
        dummy_input = np.random.random(input_shape).astype(np.float32)

        # Get input name for the model
        input_name = session.get_inputs()[0].name

        # Run the model
        outputs = session.run(None, {input_name: dummy_input})

        print("Model output:", outputs)

        return 0  # Success

    except Exception as e:
        print(f"Error running the model: {e}")
        return 1  # Failure

if __name__ == "__main__":
    model_url = "https://github.com/onnx/models/raw/main/Computer_Vision/mobilenetv3_small_050_Opset17_timm/mobilenetv3_small_050_Opset17.onnx"
    model_path = ".temp/mobilenetv3_small_050_Opset17.onnx"

    try:
        download_model(model_url, model_path)
        exit_status = run_model(model_path)
    except Exception as e:
        print(f"Error: {e}")
        exit_status = 1  # Failure

    sys.exit(exit_status)