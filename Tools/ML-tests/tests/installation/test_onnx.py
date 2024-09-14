import onnx
import sys

try:
    print(f"onnx version: {onnx.__version__}")
    sys.exit(0)
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)