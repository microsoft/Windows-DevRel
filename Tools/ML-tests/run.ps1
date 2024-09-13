param (
    [string]$pythonPath,
    [string]$hfToken,
    [switch]$Debug,
    [string]$useCacheDir
)

$date = Get-Date -Format "dd-MM-yyyy_HH-mm-ss"

Start-Transcript -Path "output.$date.log" -Append
# Write-Host current timestamp
Write-Host "$date"

if (-not $pythonPath) {
    $pythonPath = "python"
}

try {
    $pythonVersion = & $pythonPath --version
    Write-Host "Using Python version: $pythonVersion" -BackgroundColor Green
}
catch {
    Write-Host "Python executable not recognized. Please check the path: $pythonPath" -BackgroundColor Red
    exit 1
}

if ($hfToken) {
    [System.Environment]::SetEnvironmentVariable("HF_TOKEN", $hfToken, "Process")
}
else {
    Write-Host "Huggingface token not provided. Olive-ai tests will fail." -BackgroundColor Red
}

function Write-LogMessage {
    param (
        [string]$message
    )
    foreach ($line in $log) {
        Write-Host $line
    }
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
function Install-RequirementsFromUrl {
    param (
        [string]$url
    )
    $reqFile = ".temp\requirements.txt"
    Invoke-WebRequest -Uri $url -OutFile "$reqFile"
    Write-Host "Installing requirements.txt (This may take a while)"

    $command = "pip install -r $reqFile --disable-pip-version-check"
    if ($useCacheDir -ne $true) {
        $command += " --no-cache-dir"
    }

    $log = Invoke-Expression "$command 2>&1"
    Write-LogMessage $log
}
function Install-And-Test-Library {
    param (
        [string[]]$libraries,
        [string]$testScript
    )
    
    # Install required libraries
    foreach ($library in $libraries) {
        Write-Host "Installing $library"

        $command = "pip install $library --disable-pip-version-check"
        if ($useCacheDir -ne $true) {
            $command += " --no-cache-dir"
        }

        $log = Invoke-Expression "$command 2>&1"
        Write-LogMessage $log
    }
    
    Write-Host "Running test script: $testScript (This may take a while)"
    $log = python $testScript
    Write-LogMessage $log
    
	return $LASTEXITCODE
}

function Remove-VirtualEnv {
    param (
        [string]$envName
    )
    Write-Host "Removing virtual environment: $envName"
    try {
        Get-ChildItem -Recurse .\$envName | ForEach-Object {
            if ($_.PSIsContainer) {
                $_ | Remove-Item -Recurse -Force
            } else {
                $_.Delete()
            }
        }
    } catch {}
    Remove-Item -Recurse -Force .\$envName
}

function Test-Library {
    param (
        [string]$library,
        [string]$testScript,
        [string[]]$libraries,
        [string]$requirementsUrl
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
        if ($requirementsUrl) {
            Install-RequirementsFromUrl -url $requirementsUrl
        }

        $exitCode = Install-And-Test-Library -libraries $libraries -testScript $testScript

        if ($exitCode -eq 0) {
            $result.Result = "Success"
        }
    }
	catch {
        Write-Host "Error occurred: $_"
    }
    finally {
        if (-not $Debug) {
            # Deactivate and Cleanup virtual environment
            Write-Host "Deactivating virtual environment: $envName"
            & deactivate

            Start-Sleep -Seconds 3

            Remove-VirtualEnv -envName $envName
        }
    }

	return $result
}

$libraries = [ordered]@{
    "numpy==1.26.4" = "tests\installation\test_numpy.py"
    "numpy==2.0.1"  = "tests\installation\test_numpy.py"
    "onnx"          = "tests\installation\test_onnx.py"
    "scipy"         = "tests\installation\test_scipy.py"
    "scikit-learn"  = "tests\installation\test_sklearn.py"
    "pandas"        = "tests\installation\test_pandas.py"
    "matplotlib"    = "tests\installation\test_matplotlib.py"
}
$executionTime = Measure-Command {
    Write-Host "Starting libraries installation test.."
    $results = @()
    foreach ($library in $libraries.Keys) {
        $result = Test-Library -library $library -libraries $library -testScript $libraries[$library]
        $results += [PSCustomObject]@{
            Library = $result.Library
            Result  = $result.Result
        }
    }
}

if (-not $Debug) {
    Remove-Item -Recurse -Force ".temp"
}

Write-Host "Installation tests completed and environments cleaned up." -BackgroundColor Green
Write-Host "Results:"
$results | Format-Table -Property Library, Result
Write-Host "Execution Time: $($executionTime.ToString("hh'h 'mm'm 'ss's'")) or $($executionTime.TotalSeconds) seconds" -BackgroundColor Green


$workflows = [ordered]@{
    "torch" = @{
        "testScript" = "tests\workflow\test_torch.py"
        "libraries"  = @("torch", "numpy==1.26.4", "onnx")
    }
    "onnxruntime" = @{
        "testScript" = "tests\workflow\test_onnxruntime.py"
        "libraries"  = @("onnxruntime", "requests", "numpy")
    }
    "olive-ai" = @{
        "testScript" = "tests\workflow\test_olive.py"
        "libraries"  = @("numpy==1.26.4", "huggingface_hub[cli]", "git+https://github.com/microsoft/Olive.git@main") # "git+https://github.com/microsoft/TransformerCompression.git@quarot-main")
        "requirementsUrl" = "https://raw.githubusercontent.com/microsoft/Olive/main/examples/phi3/requirements.txt"
    }
    "jax" = @{
        "testScript" = "tests\workflow\test_jax.py"
        "libraries"  = @("jax", "onnx")
    }
}

$executionTime = Measure-Command {
    Write-Host "Starting workflow test.."
    $results = @()
    foreach ($workflow in $workflows.Keys) {
        $testScript = $workflows[$workflow]["testScript"]
        $libraries = $workflows[$workflow]["libraries"]
        $requirementsUrl = $workflows[$workflow]["requirementsUrl"]

        $result = Test-Library -library $workflow -testScript $testScript -libraries $libraries -requirementsUrl $requirementsUrl
        $results += [PSCustomObject]@{
            Framework = $result.Library
            Result  = $result.Result
        }
    }
}

if (-not $Debug) {
    Remove-Item -Recurse -Force ".temp"
}

Write-Host "Workflow tests completed and environments cleaned up." -BackgroundColor Green
Write-Host "Results:"
$results | Format-Table -Property Framework, Result
Write-Host "Execution Time: $($executionTime.ToString("hh'h 'mm'm 'ss's'")) or $($executionTime.TotalSeconds) seconds" -BackgroundColor Green

Stop-Transcript