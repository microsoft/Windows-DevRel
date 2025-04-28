#Requires -Version 5.1
<#
.SYNOPSIS
    Tests the installation of Python packages from a provided list file
    on the current architecture because they lack pre-built win_arm64 or abi-none wheels.
    It attempts to install each package from source in a clean virtual environment.

.DESCRIPTION
    This script takes a file containing a list of Python packages (one per line) and:
    1. Creates a temporary virtual environment for each package.
    2. Attempts to install the package using 'pip install'.
    3. Records whether the installation succeeded or failed (based on pip's exit code).
    4. Cleans up the virtual environment (unless -RetainEnvironments is specified).
    5. Logs all output to a transcript file.
    6. Prints a summary table of the installation results.

.PARAMETER pythonPath
    Specifies the path to the Python executable to use for creating virtual environments
    and running pip. Defaults to "python".

.PARAMETER RetainEnvironments
    If specified, the script will not remove the temporary virtual environments after testing,
    allowing for manual inspection.

.PARAMETER useCacheDir
    If specified, pip will use its cache directory during installation. Otherwise,
    `--no-cache-dir` is used.

.PARAMETER PackageListFile
    Path to a text file containing a list of Python packages to test, one package name per line.

.EXAMPLE
    .\orange_package_test.ps1 -PackageListFile "orange_packages.txt" -pythonPath C:\Python312\python.exe

.EXAMPLE
    .\orange_package_test.ps1 -PackageListFile "log\orange_packages_list_2023-10-30_15-30-45.txt" -RetainEnvironments -useCacheDir

.NOTES
    The script can be used with the output from 'get_orange_packages.py' which generates
    a list of PyPI packages that do not provide win_arm64 or abi-none wheels.
#>
param (
    [string]$pythonPath = "python",
    [switch]$RetainEnvironments,
    [switch]$useCacheDir,
    [Parameter(Mandatory=$true)]
    [string]$PackageListFile
)

# --- Initial Setup ---

# Determine Architecture and Python Version
try {
    # Use architecture.py script to get consistent architecture info
    $architectureInfo = & $pythonPath "architecture.py" 2>&1
    if ($LASTEXITCODE -ne 0) {
        # Join potential multi-line error output into a single string for the exception
        $errorOutput = ($architectureInfo -join ' ').Trim()
        throw "Failed to get architecture info from Python script: $errorOutput"
    }
    # If successful, $architectureInfo should contain the single line output
    # Trim potential whitespace
    $architectureInfo = $architectureInfo.Trim()
}
catch {
    Write-Host "Python executable not recognized or failed, or architecture.py script missing/failed. Please check the path: $pythonPath" -BackgroundColor Red
    Write-Host "Error details: $($_.Exception.Message)" -BackgroundColor Red
    exit 1
}

$date = Get-Date -Format "dd-MM-yyyy_HH-mm-ss"
$transcriptFile = "log/orange_test_$($architectureInfo)_$date.log"
$csvFile = "log/orange_test_$($architectureInfo)_$date.csv"

# Ensure log directory exists
if (-not (Test-Path -Path "log" -PathType Container)) {
    New-Item -ItemType Directory -Path "log" | Out-Null
}

Start-Transcript -Path $transcriptFile -Append

Write-Host "Using Python (processor/arch/version): $architectureInfo" -BackgroundColor Blue
Write-Host "Retain environments: $RetainEnvironments" -BackgroundColor Cyan
Write-Host "Using pip cache: $useCacheDir" -BackgroundColor Cyan
Write-Host "Package list file: $PackageListFile" -BackgroundColor Cyan

# --- Helper Functions ---

function Write-LogMessage {
    param (
        [Parameter(Mandatory=$true)]
        [string[]]$LogLines
    )
    # Write-Host adds extra newlines sometimes, Write-Output is cleaner for logs
    $LogLines | ForEach-Object { Write-Output $_ }
}

function Initialize-VirtualEnv {
    param (
        [Parameter(Mandatory=$true)]
        [string]$envName
    )
    Write-Host "Creating virtual environment: $envName"
    $log = & $pythonPath -m venv $envName 2>&1
    if ($LASTEXITCODE -ne 0) {
        # Make sure we're not passing null to Write-LogMessage
        if ($null -ne $log) {
            Write-LogMessage -LogLines $log
        } else {
            Write-Output "No error output was captured, but the command failed."
        }
        throw "Failed to create virtual environment $envName"
    }
    
    # Only call Write-LogMessage if we actually have output
    if ($null -ne $log -and $log.Count -gt 0) {
        Write-LogMessage -LogLines $log
    }

    # Get the correct activation path based on OS
    $envActivate = Join-Path -Path ".\$envName" -ChildPath "Scripts\Activate.ps1"
    if (-not (Test-Path $envActivate)) {
         throw "Activation script not found at $envActivate"
    }
    Write-Host "Activating virtual environment: $envName"
    . $envActivate
}

function Remove-VirtualEnv {
    param (
        [Parameter(Mandatory=$true)]
        [string]$envName
    )
    Write-Host "Removing virtual environment: $envName"
    # Attempt to remove robustly
    $ErrorActionPreference = 'SilentlyContinue'
    try {
        Remove-Item -Recurse -Force -Path ".\$envName"
    }
    finally {
        $ErrorActionPreference = 'Continue' # Reset error action preference
    }
    if (Test-Path -Path ".\$envName") {
        Write-Warning "Failed to completely remove virtual environment: $envName"
    }
}

function Test-OrangePackageInstallation {
    param (
        [Parameter(Mandatory=$true)]
        [string]$packageName
    )

    $envName = ".temp/env_orange_$($packageName -replace '[^a-zA-Z0-9]', '_')" # Sanitize name
    $result = @{
        Package = $packageName
        Result  = "Failed"
        Log     = ""
    }

    Write-Host "--- Testing Package: $packageName ---" -BackgroundColor Magenta

    # Ensure .temp directory exists
    if (-not (Test-Path -Path ".temp" -PathType Container)) {
        New-Item -ItemType Directory -Path ".temp" | Out-Null
    }

    # Create virtual environment
    try {
        Initialize-VirtualEnv -envName $envName
    }
    catch {
        Write-Host "Failed to initialize environment for $packageName. Error: $($_.Exception.Message)" -BackgroundColor Red
        $result.Log = "Environment creation failed: $($_.Exception.Message)"
        # No finally block needed here as cleanup happens outside or if RetainEnvironments is off
        return $result
    }

    try {
        # Attempt to install the package
        Write-Host "Attempting to install $packageName (This may take a while)..."
        $pipCommand = "pip install $packageName --disable-pip-version-check"
        if (-not $useCacheDir) {
            $pipCommand += " --no-cache-dir"
        }

        # Capture all output streams (stdout and stderr)
        $installLog = Invoke-Expression "$pipCommand 2>&1"
        $exitCode = $LASTEXITCODE

        Write-LogMessage -LogLines $installLog
        $result.Log = $installLog -join [Environment]::NewLine

        if ($exitCode -eq 0) {
            Write-Host "Installation of $packageName SUCCEEDED." -BackgroundColor Green
            $result.Result = "Success"
        } else {
            Write-Host "Installation of $packageName FAILED (Exit Code: $exitCode)." -BackgroundColor Red
            # Result is already "Failed"
        }
    }
    catch {
        Write-Host "An unexpected error occurred during installation of ${$packageName}: $_" -BackgroundColor Red
        $result.Log += "Unexpected script error: $($_.Exception.Message)"
        # Result is already "Failed"
    }
    finally {
        # Deactivate environment (best effort)
        if (Get-Command -Name deactivate -ErrorAction SilentlyContinue) {
            Write-Host "Deactivating virtual environment: $envName"
            try {
                deactivate
            } catch {
                Write-Warning "Failed to deactivate environment $envName. Error: $($_.Exception.Message)"
            }
        }

        # Cleanup virtual environment if not in RetainEnvironments mode
        if (-not $RetainEnvironments) {
            Start-Sleep -Seconds 2 # Give file handles time to release
            Remove-VirtualEnv -envName $envName
        } else {
            Write-Host "RetainEnvironments mode: Skipping cleanup for $envName"
        }
    }

    Write-Host "--- Finished Testing: $packageName ---"
    return $result
}

# --- Main Execution ---

# 1. Read the list of packages from the provided file
Write-Host "Reading package list from file: $PackageListFile" -BackgroundColor Yellow

if (-not (Test-Path $PackageListFile)) {
    Write-Host "Error: Package list file '$PackageListFile' not found." -BackgroundColor Red
    Stop-Transcript
    exit 1
}

$orangePackages = @()
try {
    # Read packages from file, filtering out empty lines and trimming whitespace
    $orangePackages = Get-Content -Path $PackageListFile | ForEach-Object { 
        if ($_ -ne $null) { "$_".Trim() } 
    } | Where-Object { $_ -ne '' }

    if ($orangePackages.Count -eq 0) {
         Write-Host "No packages found in the provided file." -BackgroundColor Yellow
         # Don't exit, just report no packages to test
    }
    else {
         Write-Host "Found $($orangePackages.Count) packages to test." -BackgroundColor Green
    }
}
catch {
    Write-Host "Error reading package list file '$PackageListFile': $($_.Exception.Message)" -BackgroundColor Red
    Stop-Transcript
    exit 1
}

# 2. Test installation for each package
$results = @()
$overallExecutionTime = Measure-Command {
    if ($orangePackages.Count -gt 0) {
        Write-Host "Starting installation tests for $($orangePackages.Count) packages..." -BackgroundColor Green

        foreach ($package in $orangePackages) {
            $testResult = Test-OrangePackageInstallation -packageName $package
            $results += [PSCustomObject]@{
                Library = $testResult.Package
                Result  = $testResult.Result
            }
        }
    } else {
        Write-Host "No packages identified to test." -BackgroundColor Yellow
    }
}

# 3. Clean up .temp directory if not in RetainEnvironments mode
if (-not $RetainEnvironments -and (Test-Path -Path ".temp")) {
    Write-Host "Cleaning up temporary directory '.temp'" 
    Remove-Item -Recurse -Force ".temp" -ErrorAction SilentlyContinue
}

# 4. Display Results
if ($results.Count -gt 0) {
    Write-Host "--- Overall Results ---" -BackgroundColor Green
    $results | Format-Table -Property Library, Result

    # Export results to CSV
    $results | Export-Csv -Path $csvFile -NoTypeInformation
    Write-Host "Results exported to CSV: $csvFile" -BackgroundColor Blue

    Write-Host "Total execution time: $($overallExecutionTime.ToString("hh'h 'mm'm 'ss's'")) ($($overallExecutionTime.TotalSeconds.ToString('F2')) seconds)" -BackgroundColor Blue
    Write-Host "Detailed logs available in: $transcriptFile" -BackgroundColor Blue

    $successCount = ($results | Where-Object { $_.Result -eq 'Success' }).Count
    $failCount = $results.Count - $successCount
    Write-Host "Summary: $successCount Succeeded, $failCount Failed."
}
elseif ($orangePackages.Count -eq 0) {
     Write-Host "No packages were tested." -BackgroundColor Yellow
}
else {
     Write-Host "No results were generated despite having packages to test. Check logs." -BackgroundColor Red
}

Write-Host "Script finished."
Stop-Transcript