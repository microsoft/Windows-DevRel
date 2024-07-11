import onnxruntime as rt
import sys

try:
    print(f"onnxruntime version: {rt.__version__}")
    sys.exit(0)
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)