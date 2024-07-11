import torch
import sys

try:
    print(f"torch version: {torch.__version__}")
    sys.exit(0)
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)