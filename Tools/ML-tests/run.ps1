param (
    [string]$pythonPath
)

function Initialize-VirtualEnv {
    param (
        [string]$envName
    )
    Write-Output "Creating virtual environment: $envName"
    & $pythonPath -m venv $envName

    # Activate the virtual environment
    $envActivate = ".\$envName\Scripts\Activate.ps1"
    Write-Output "Activating virtual environment: $envName"
    & $envActivate
}

function Install-And-Test-Library {
    param (
        [string]$library,
        [string]$testScript
    )
    
    Write-Output "Installing $library"
    pip install $library
    
    Write-Output "Running test script: $testScript"
    python $testScript

	return $LASTEXITCODE
}

function Cleanup-VirtualEnv {
    param (
        [string]$envName
    )
    Write-Output "Removing virtual environment: $envName"
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
        Write-Output "Error occurred: $_"
    }
    finally {
        # Deactivate and Cleanup virtual environment
        Write-Output "Deactivating virtual environment: $envName"
        & deactivate
        
        Cleanup-VirtualEnv -envName $envName
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

Write-Output "Starting tests on libraries..."
$results = @()
foreach ($library in $libraries.Keys) {
    $result = Test-Library -library $library -testScript $libraries[$library]
    $results += $result
}

Write-Output "All tests completed and environments cleaned up."
Write-Output "Results:"
$results | Format-Table -AutoSize