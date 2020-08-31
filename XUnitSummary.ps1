#############################################################################################################
# .SYNOPSIS
#  Summarize the results of XUnit test run
#
# .DESCRIPTION
#  Read the TestResult.xml file (default) output by the XUnit Test runner and display
#  the results to the screen
#
# .PARAMETER InputFile
#  Input file name (alias -i)
#
# .PARAMETER v
#  A switch which when true prints out the script's version information (include type information)
#  
# .EXAMPLE
#  XUnitSummary.ps1 -i <InputFile.xml> -v
#
#  Summarize the results of XUnit test run
#
#  .EXAMPLE
#  XUnitSummary.ps1 -v
#
#  Short-hand syntax of the script using default TestResult.xml file
#
# .NOTES
#  You don't need to run this script with any special permissions
# 
#  Author: Warren Wiltshire (Warren@SeagullConsulting.Biz)
#  Date: 08/30/2020
#  Change Log:
#     0.9.0(08/30/2020) - Initial script
#############################################################################################################
Param (
	[parameter(Mandatory=$false)]
	[alias ('i')] [string] $InputFile = "TestResults/TestResult.xml",
	[switch] $v = $false	
)
# Define functions
function ScriptVersion
{
  return '0.9.0';
}
# Main Process
$ElapsedTime = [System.Diagnostics.Stopwatch]::StartNew()
Write-Host "Script Started at $(get-date)"
$total = 0
$passed = 0
$failed = 0
$skipped = 0
$global:ECnt = 0 
$psVersion = "{0}.{1}" -f $PSVersionTable.PSVersion.Major, $PSVersionTable.PSVersion.Minor 
if ([System.Environment]::Is64BitProcess) 
	{ Write-Host "Running PowerShell version $($psVersion) x64" }
else
	{ Write-Host "Running PowerShell version $($psVersion) x86" } 

Try {	
	$ver = ScriptVersion;

	if ($v)
		{ Write-Host "version: $ver"; }
	else {
		$output = "XUnit Test Summary({0}):" -f $InputFile
		Write-Host $output 
		Write-Host ""
		[xml]$xml = Get-Content $InputFile
		$tests = $xml.TestRun.Results.UnitTestResult | Sort-Object -property testName
		Write-Host "`t       Test        `t Status `tDuration(secs)"
		Write-Host "`t-------------------`t--------`t--------------"
		
		foreach ($test in $tests) {
			$total++
			$idx = $test.testName.LastIndexOf('.') + 1
			$output = $test.testName.Substring($idx)
			Write-Host "`t$output" -NoNewline
			if ($output.length -lt 8) 
				{ Write-Host "`t" -NoNewline }
			if ($output.length -lt 16) 
				{ Write-Host "`t" -NoNewline }
			$output = "`t{0}" -f $test.outcome
			switch ($test.outcome.ToUpper()) {
				"PASSED" { Write-Host "$output" -f Green -NoNewline; $passed++ }
				"FAILED" { Write-Host "$output" -f Red -NoNewline; $failed++	 }
				"NOTEXECUTED" { Write-Host "`tSkipped" -f Yellow -NoNewline ; $skipped++ }
			}
			$duration = ([TimeSpan]$test.duration).TotalSeconds
			$output = "{0:0.000}" -f $duration
			Write-Host "`t`t$output"
		}

		Write-Host ""
		$output = "Total: {0}, Passed: {1}, Failed: {2}, Skipped: {3}" -f $total, $passed, $failed, $skipped
		Write-Host $output
	}
}
Catch {
	$ex = $_.Exception
	$errLine = $_.InvocationInfo.ScriptLineNumber
	Write-Output ("`nError: {0} at line {1}`n" -f $ex.Message, $errLine)
	Exit 1
}

Write-Host ""
Write-Host "Done"
Write-Host "$ECnt Exceptions"
Write-Host "Script Ended at $(get-date)"
Write-Host "Total Elapsed Time: $($ElapsedTime.Elapsed.ToString())"
Read-Host "Press <enter> to exit."