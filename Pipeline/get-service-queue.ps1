
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
			AppendQueueVariable "Core"
		}
        "Illusion.Common.Authentication/*" { 
			Write-Host "Illusion.Common.Authentication changed"
			AppendQueueVariable "Core"
            AppendQueueVariable "Authentication"
		}
        "Illusion.Common.Consul/*" { 
			Write-Host "Illusion.Common.Consul changed"
			AppendQueueVariable "Consul"
		}
        "Illusion.Common.Domain/*" { 
			Write-Host "Illusion.Common.Domain changed"
			AppendQueueVariable "Domain"
		}
        "Illusion.Common.FeatureFlags/*" { 
			Write-Host "Illusion.Common.FeatureFlags changed"
            AppendQueueVariable "Core"
			AppendQueueVariable "FeatureFlags"
		}
        "Illusion.Common.MediatR/*" { 
			Write-Host "Illusion.Common.MediatR changed"
			AppendQueueVariable "MediatR"
		}
        "Illusion.Common.RabbitMq/*" { 
			Write-Host "Illusion.Common.RabbitMq changed"
            AppendQueueVariable "Core"
			AppendQueueVariable "RabbitMq"
		}
        "Illusion.Common.Telemetry/*" { 
			Write-Host "Illusion.Common.Telemetry changed"
            AppendQueueVariable "Core"
			AppendQueueVariable "Telemetry"
		}
        # The rest of your path filters
    }
}

Write-Host "Build Queue: $global:buildQueueVariable"
Write-Host "##vso[task.setvariable variable=buildQueue;isOutput=true]$global:buildQueueVariable"