import matplotlib
import sys

try:
    print(f"matplotlib version: {matplotlib.__version__}")
    sys.exit(0)
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)