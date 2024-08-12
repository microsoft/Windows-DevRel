import pandas as pd
import sys

try:
    print(f"pandas version: {pd.__version__}")
    sys.exit(0)
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)