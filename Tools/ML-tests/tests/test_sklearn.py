import sklearn
import sys

try:
    print(f"scikit-learn version: {sklearn.__version__}")
    sys.exit(0)
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)