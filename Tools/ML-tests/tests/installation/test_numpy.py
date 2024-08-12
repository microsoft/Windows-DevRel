import numpy
import sys

try:
    print(f"numpy version: {numpy.__version__}")
    sys.exit(0)
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)