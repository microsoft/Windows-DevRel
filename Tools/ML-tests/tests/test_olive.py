import olive
from olive.common.version import __version__
import sys

try:
    print(f"olive version: {__version__}")
    sys.exit(0)
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)