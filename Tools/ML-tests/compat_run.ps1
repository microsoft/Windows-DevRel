param (
    [string]$pythonPath,
    [string]$hfToken,
    [switch]$Debug,
    [string]$useCacheDir,
    [string[]]$librariesToTest,
    [string[]]$workflowsToTest 
)

if (-not $pythonPath) {
    $pythonPath = "python"
}

try {
    $architecture = & $pythonPath "architecture.py"
}
catch {
    Write-Host "Python executable not recognized. Please check the path: $pythonPath" -BackgroundColor Red
}

$date = Get-Date -Format "dd-MM-yyyy_HH-mm-ss"
$transcriptFile = "log/output_$($architecture)_$date.log"

Start-Transcript -Path $transcriptFile -Append

Write-Host "Using Python (processor/arch/version): $architecture" -BackgroundColor Blue

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
                $_ | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
            } else {
                $_.Delete()
            }
        }
    } catch {}
    Remove-Item -Recurse -Force .\$envName -ErrorAction SilentlyContinue
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

$libraryResults = @()
if (-not $PSBoundParameters.ContainsKey('librariesToTest') -or $librariesToTest.Count -gt 0) {
    if ($librariesToTest -and $librariesToTest.Count -ge 0) {
        $librariesToRun = @{}
        foreach ($library in $librariesToTest) {
            if ($libraries.Contains($library)) {
                $librariesToRun[$library] = $libraries[$library]
            } else {
                Write-Host "Library $library not found in predefined list" -BackgroundColor Yellow
            }
        }
    } else {
        $librariesToRun = $libraries
    }

    $libraryExecutionTime = Measure-Command {
        Write-Host "Starting libraries installation test..." -BackgroundColor Green
     
        foreach ($library in $librariesToRun.Keys) {
            $result = Test-Library -library $library -libraries $library -testScript $libraries[$library]
            $libraryResults += [PSCustomObject]@{
                Library = $result.Library
                Result  = $result.Result
            }
        }
    }

    if (-not $Debug) {
        Remove-Item -Recurse -Force ".temp" -ErrorAction SilentlyContinue
    }
}

$workflows = [ordered]@{
    "torch" = @{
        "testScript" = "tests\workflow\test_torch.py"
        # you can use internal wheel to test workflow on arm - "X:\Python\wheels\apl\torch-2.7.0-cp312-cp312-win_arm64.whl"
        "libraries"  = @("torch", "numpy==1.26.4", "matplotlib")
    }
    # "torchvision" = @{
    #     "testScript" = "tests\workflow\test_torchvision.py"
    #     # you can use internal wheel to test workflow on arm - "X:\Python\wheels\apl\torch-2.7.0-cp312-cp312-win_arm64.whl"
    #     # "libraries"  = @("torch", "torchvision", "numpy")
    #     "libraries"  = @("X:\Python\wheels\apl\torch-2.7.0-cp312-cp312-win_arm64.whl", "X:\Python\wheels\apl\torchvision-0.22.0a0+fab1188-cp312-cp312-win_arm64.whl", "X:\Python\wheels\apl\torchaudio-2.6.0a0+f084f34-cp312-cp312-win_arm64.whl", "numpy")
    # }
    "tensorflow" = @{
        "testScript" = "tests\workflow\test_tensorflow.py"
        "libraries"  = @("tensorflow", "numpy")
    }
    "onnxruntime" = @{
        "testScript" = "tests\workflow\test_onnxruntime.py"
        # you have to use onnxruntime-qnn for arm64
        "libraries"  = @("onnxruntime", "requests", "numpy")
    }
    "olive-ai" = @{
        "testScript" = "tests\workflow\test_olive.py"
        # you have to use onnxruntime-qnn for arm64, as well as internal torch "X:\Python\wheels\apl\torch-2.7.0-cp312-cp312-win_arm64.whl"
        "libraries"  = @("onnxruntime","numpy==1.26.4", "huggingface_hub[cli]", "git+https://github.com/microsoft/Olive.git@main") # "git+https://github.com/microsoft/TransformerCompression.git@quarot-main")
        "requirementsUrl" = "https://raw.githubusercontent.com/microsoft/Olive/main/examples/phi3/requirements.txt"
    }
    "jax" = @{
        "testScript" = "tests\workflow\test_jax.py"
        "libraries"  = @("jax", "jaxlib", "numpy")
    }
}

$workflowResults = @()
if (-not $PSBoundParameters.ContainsKey('workflowsToTest') -or $workflowsToTest.Count -gt 0) {
    if ($workflowsToTest -and $workflowsToTest.Count -gt 0) {
        $workflowsToRun = @{}
        foreach ($workflow in $workflowsToTest) {
            if ($workflows.Contains($workflow)) {
                $workflowsToRun[$workflow] = $workflows[$workflow]
            } else {
                Write-Host "Workflow $workflow not found in predefined list" -BackgroundColor Yellow
            }
        }
    } else {
        $workflowsToRun = $workflows
    }

    $workflowExecutionTime = Measure-Command {
        Write-Host "Starting workflow tests..."  -BackgroundColor Green
        
        foreach ($workflow in $workflowsToRun.Keys) {
            $testScript = $workflows[$workflow]["testScript"]
            $libraries = $workflows[$workflow]["libraries"]
            $requirementsUrl = $workflows[$workflow]["requirementsUrl"]

            $result = Test-Library -library $workflow -testScript $testScript -libraries $libraries -requirementsUrl $requirementsUrl
            $workflowResults += [PSCustomObject]@{
                Framework = $result.Library
                Result  = $result.Result
            }
        }
    }

    if (-not $Debug) {
        Remove-Item -Recurse -Force ".temp" -ErrorAction SilentlyContinue
    }
}

if ($workflowResults.Count -gt 0 -or $libraryResults.Count -gt 0) {
    
    Write-Host "Tests completed and environments cleaned up." -BackgroundColor Green
    Write-Host

    if ($workflowResults.Count -gt 0) {
        Write-Host "Workflows Results:"
        $workflowResults | Format-Table -Property Framework, Result
        Write-Host "Execution Time: $($workflowExecutionTime.ToString("hh'h 'mm'm 'ss's'")) or $($workflowExecutionTime.TotalSeconds) seconds" -BackgroundColor Blue
    }

    if ($workflowResults.Count -gt 0 -and $libraryResults.Count -gt 0) {
        Write-Host
    }

    if ($libraryResults.Count -gt 0) {
        Write-Host "Library Results:"
        $libraryResults | Format-Table -Property Library, Result
        Write-Host "Execution Time: $($libraryExecutionTime.ToString("hh'h 'mm'm 'ss's'")) or $($libraryExecutionTime.TotalSeconds) seconds" -BackgroundColor Blue
    }
}

Stop-Transcript