Run from PowerShell Admin:
```powershell
.\run.ps1 -pythonPath "C:\Program Files\Python312\python.exe" -hfToken "TOKEN" -Debug
```

### Typical python path
```
"C:\Users\${USER}\AppData\Local\Programs\Python\${VERSION}\python.exe"
"C:\Program Files\${VERSION}\python.exe"
```

### Reqiurements

[latest VC Redist](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version)

### Parameters

- pythonPath: Path to python executable
- hfToken: Hugging Face API token
- Debug: Debug mode (temp files are not deleted)
- useCacheDir: Use python cache directory
- librariesToTest: Custom list of libraries to test
- workflowsToTest: Custom list of workflows to test

### Example

#### skip all tests
```powershell
.\run.ps1 -pythonPath "C:\Program Files\Python312\python.exe"  -librariesToTest @() -workflowsToTest @()
```
#### run some tests
```powershell
.\run.ps1 -pythonPath "C:\Program Files\Python312\python.exe"  -librariesToTest "pandas", "scipy" -workflowsToTest "torch", "olive"
```