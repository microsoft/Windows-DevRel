import scipy
import sys

try:
    print(f"scipy version: {scipy.__version__}")
    sys.exit(0)
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)