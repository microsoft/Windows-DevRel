param (
    [string]$pythonPath
)

if (-not $pythonPath) {
    $pythonPath = "python"
}

try {
    $pythonVersion = & $pythonPath --version
    Write-Host "Using Python version: $pythonVersion"
}
catch {
    Write-Host "Python executable not recognized. Please check the path: $pythonPath"
    exit 1
}

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
        [string[]]$libraries,
        [string]$testScript
    )
    
    # Install required libraries
    foreach ($library in $libraries) {
        Write-Host "Installing $library"
        $log = pip install $library 
        Write-Host $log
    }
    
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
        [string]$testScript,
        [string[]]$libraries
    )

    $envName = ".temp/env_$library"
	$result = @{
        Library = $library
        Result  = "Failed"
    }
    
    # Create virtual environment
    Initialize-VirtualEnv -envName $envName

    try {
        # Install and test library
        $exitCode = Install-And-Test-Library -libraries $libraries -testScript $testScript

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

# $libraries = [ordered]@{
#     "numpy==1.26.4" = "tests\installation\test_numpy.py"
#     "numpy==2.0.1"  = "tests\installation\test_numpy.py"
#     "scipy"         = "tests\installation\test_scipy.py"
#     "scikit-learn"  = "tests\installation\test_sklearn.py"
#     "pandas"        = "tests\installation\test_pandas.py"
#     "matplotlib"    = "tests\installation\test_matplotlib.py"
# }
# $executionTime = Measure-Command {
#     Write-Host "Starting libraries installation test.."
#     $results = @()
#     foreach ($library in $libraries.Keys) {
#         $result = Test-Library -library $library -testScript $libraries[$library]
#         $results += [PSCustomObject]@{
#             Library = $result.Library
#             Result  = $result.Result
#         }
#     }
# }

# Remove-Item -Recurse -Force .temp

# Write-Host "Installation tests completed and environments cleaned up."
# Write-Host "Results:"
# $results | Format-Table -Property Library, Result
# Write-Host "Execution Time: $($executionTime.ToString("hh'h 'mm'm 'ss's'")) or $($executionTime.TotalSeconds) seconds"


$workflows = [ordered]@{
    # "torch" = @{
    #     "testScript" = "tests\workflow\test_torch.py"
    #     "libraries"  = @("torch", "numpy==1.26.4", "onnx")
    # }
    "onnxruntime" = @{
        "testScript" = "tests\workflow\test_onnxruntime.py"
        "libraries"  = @("onnxruntime", "requests", "numpy")
    }
    # "olive-ai" = @{
    #     "testScript" = "tests\workflow\test_olive.py"
    #     "libraries"  = @("olive-ai")
    # }
    # "jax" = @{
    #     "testScript" = "tests\workflow\test_jax.py"
    #     "libraries"  = @("jax")
    # }
}

$executionTime = Measure-Command {
    Write-Host "Starting workflow test.."
    $results = @()
    foreach ($workflow in $workflows.Keys) {
        $testScript = $workflows[$workflow]["testScript"]
        $libraries = $workflows[$workflow]["libraries"]
        $result = Test-Library -library $workflow -testScript $testScript -libraries $libraries
        $results += [PSCustomObject]@{
            Framework = $result.Library
            Result  = $result.Result
        }
    }
}

Remove-Item -Recurse -Force ".temp"

Write-Host "Workflow tests completed and environments cleaned up."
Write-Host "Results:"
$results | Format-Table -Property Framework, Result
Write-Host "Execution Time: $($executionTime.ToString("hh'h 'mm'm 'ss's'")) or $($executionTime.TotalSeconds) seconds"