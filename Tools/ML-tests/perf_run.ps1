param (
    [string]$pythonPath1,
    [string]$pythonPath2
)

if (-not $pythonPath1 -or -not $pythonPath2) {
    Write-Host "Please provide both Python executables for comparison." -BackgroundColor Red
    exit
}

try {
    # Set up the environment names for the performance tests

    $architecture1 = & $pythonPath1 "architecture.py"
    $architecture2 = & $pythonPath2 "architecture.py"
    $date = Get-Date -Format "dd-MM-yyyy_HH-mm-ss"

    $envName1 = ".temp/env_python1"
    $envName2 = ".temp/env_python2"

    if (-Not (Test-Path -Path "log")) {
        New-Item -ItemType Directory -Path "log"
    }

	$py1File = "log/py1_$($architecture1)_$date.json"
	$py2File = "log/py2_$($architecture2)_$date.json"
    $csvFile = "log/perf_run_$date.csv"
    $logFile = "log/perf_run_$date.txt"

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
            [string]$envName,
			[string]$pythonPath1
        )
        Write-Host "Creating virtual environment: $envName"
        & $pythonPath1 -m venv $envName

        # Activate the virtual environment
        $envActivate = ".\$envName\Scripts\Activate.ps1"
        Write-Host "Activating virtual environment: $envName"
        & $envActivate
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
	
	function CleanUp {
		Remove-VirtualEnv -envName $envName1
		Remove-VirtualEnv -envName $envName2

		Remove-Item -Force -Recurse .\venv
	}

	function Remove-PyPerformanceVenv {
        Write-Host "Removing virtual environment with pyperformance..."
        & python -m pyperformance venv remove
		& deactivate
    }

    function Install-PyPerformance {
        Write-Host "Installing pyperformance..."
        $log = Invoke-Expression "python -m pip install pyperformance --disable-pip-version-check 2>&1"
        Write-LogMessage $log
    }

    function Invoke-PerformanceTests {
        param (
            [string]$outputFile
        )
        Write-Host "Running performance tests..."
        
        # Change this values to run different tests
        #$tests = "async_tree_cpu_io_mixed,async_tree_eager_memoization,scimark,sympy,python_startup,gc_collect,float"
        $tests = "-asyncio_websockets"
        $command = "python -m pyperformance run --benchmarks=$tests -o $outputFile"
    
        $log = Invoke-Expression "$command 2>&1"
        Write-LogMessage $log
        
        return $LASTEXITCODE
    }

    function Compare-PerformanceResults {
        param (
            [string]$baselineFile,
            [string]$changedFile
        )

		Write-Host "Comparing performance results..."
        $compareCommand = "python -m pyperformance compare $baselineFile $changedFile -O table --csv $csvFile"
        $log = Invoke-Expression "$compareCommand 2>&1"
        Write-LogMessage $log
        
        $log | Out-File -FilePath $logFile -Append

        $data = Import-Csv -Path $csvFile

        $totalDifference = 0
        $count = 0

        foreach ($row in $data) {
            $baseValue = [float]$row.Base
            $changedValue = [float]$row.Changed

            $difference = ($baseValue - $changedValue) / $baseValue
            $totalDifference += $difference
            $count++
        }

        $averageDifference = ($totalDifference / $count * 100) | ForEach-Object { "{0:N2}" -f $_ }
        Write-Output "The avg speed gain: $averageDifference%" | Out-File -FilePath $logFile -Append
    }

    Initialize-VirtualEnv -envName $envName1 -pythonPath $pythonPath1
    Install-PyPerformance

	if (Invoke-PerformanceTests -outputFile $py1File) {
        Write-Host "Performance tests with $pythonPath1 failed." -BackgroundColor Red
        exit
    }
	
	Remove-PyPerformanceVenv

    Initialize-VirtualEnv -envName $envName2 -pythonPath $pythonPath2
    Install-PyPerformance

	if (Invoke-PerformanceTests -outputFile $py2File) {
        Write-Host "Performance tests with $pythonPath2 failed." -BackgroundColor Red
        exit
    }
	

    Compare-PerformanceResults -baselineFile $py1File -changedFile $py2File
	
	Remove-PyPerformanceVenv
    CleanUp

} catch {
    Write-Host "An error occurred: $_" -BackgroundColor Red
}