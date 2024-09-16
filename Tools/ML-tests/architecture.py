import platform; 

compiler = platform.python_compiler()
start_index = compiler.rfind('(') + 1
end_index = compiler.rfind(')')
architecture = compiler[start_index:end_index].strip()

machine = platform.machine()
version = platform.python_version()

print(f"{machine}_{architecture}_{version}")