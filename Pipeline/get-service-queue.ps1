
$global:buildQueueVariable = ""
$global:buildSeparator = ";"

Function AppendQueueVariable([string]$folderName)
{
	$folderNameWithSeparator = -join($folderName, $global:buildSeparator)

	if ($global:buildQueueVariable -notmatch $folderNameWithSeparator)
	{
        $global:buildQueueVariable = -join($global:buildQueueVariable, $folderNameWithSeparator)
	}
}

if ($env:BUILDQUEUEINIT)
{
	Write-Host "Build Queue Init: $env:BUILDQUEUEINIT"
	Write-Host "##vso[task.setvariable variable=buildQueue;isOutput=true]$env:BUILDQUEUEINIT"
	exit 0
}

# Get all files that were changed
$editedFiles = git diff HEAD HEAD~ --name-only

# Check each file that was changed and add that Service to Build Queue
$editedFiles | ForEach-Object {	
    Switch -Wildcard ($_ ) {		
        "Illusion.Common.Core/*" { 
			Write-Host "Illusion.Common.Core changed"
			AppendQueueVariable "Illusion.Common.Core"
		}
        # The rest of your path filters
    }
}

Write-Host "Build Queue: $global:buildQueueVariable"
Write-Host "##vso[task.setvariable variable=buildQueue;isOutput=true]$global:buildQueueVariable"