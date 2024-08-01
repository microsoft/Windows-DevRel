param (
    [string]$pythonPath
)

if (-not $pythonPath) {
    $pythonPath = "python"
}

$pythonVersion = & $pythonPath --version
Write-Host "Using Python version: $pythonVersion"

function Initialize-VirtualEnv {
    param (
        [string]$envName
    )
    Write-Host "Creating virtual environment: $envName"
    & $pythonPath -m venv $envName

    # Activate the virtual environment
    $envActivate = ".\$envName\Scripts\Activate.ps1"
    Write-Host "Activating virtual environment: $envName"
    & $envActivate
}

function Install-And-Test-Library {
    param (
        [string]$library,
        [string]$testScript
    )
    
    Write-Host "Installing $library"
    $log = pip install $library 
    Write-Host $log
    
    Write-Host "Running test script: $testScript"
    $log = python $testScript
    Write-Host $log

	return $LASTEXITCODE
}

function Remove-VirtualEnv {
    param (
        [string]$envName
    )
    Write-Host "Removing virtual environment: $envName"
    Remove-Item -Recurse -Force .\$envName
}

function Test-Library {
    param (
        [string]$library,
        [string]$testScript
    )

    $envName = "env_$library"
	$result = @{
        Library = $library
        Result  = "Failed"
    }
    
    # Create virtual environment
    Initialize-VirtualEnv -envName $envName

    try {
        # Install and test library
        $exitCode = Install-And-Test-Library -library $library -testScript $testScript

        if ($exitCode -eq 0) {
            $result.Result = "Success"
        }
    }
	catch {
        Write-Host "Error occurred: $_"
    }
    finally {
        # Deactivate and Cleanup virtual environment
        Write-Host "Deactivating virtual environment: $envName"
        & deactivate
        
        Remove-VirtualEnv -envName $envName
    }

	return $result
}

$libraries = @{
    "numpy"         = "tests\test_numpy.py"
    "torch"         = "tests\test_torch.py"
    "jax"           = "tests\test_jax.py"
    "onnxruntime"   = "tests\test_onnxruntime.py"
    "olive-ai"      = "tests\test_olive.py"
    "scikit-learn"  = "tests\test_sklearn.py"
    "pandas"        = "tests\test_pandas.py"
    "matplotlib"    = "tests\test_matplotlib.py"
}

Write-Host "Starting tests on libraries..."
$results = @()
foreach ($library in $libraries.Keys) {
    $result = Test-Library -library $library -testScript $libraries[$library]
    $results += [PSCustomObject]@{
        Library = $result.Library
        Result  = $result.Result
    }
}

Write-Host "All tests completed and environments cleaned up."
Write-Host "Results:"
$results | Format-Table -Property Library, Result