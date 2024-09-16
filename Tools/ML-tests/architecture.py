import platform; 

def is_processor_arm():
    processor = platform.processor()
    return "ARM" in processor or "aarch64" in processor

compiler = platform.python_compiler()
start_index = compiler.rfind('(') + 1
end_index = compiler.rfind(')')
architecture = compiler[start_index:end_index].strip()

version = platform.python_version()

print(f"{'ARM' if is_processor_arm() else 'x86'}_{architecture}_{version}")