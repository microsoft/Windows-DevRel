import jax
import sys

try:
    print(f"jax version: {jax.__version__}")
    sys.exit(0)
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)