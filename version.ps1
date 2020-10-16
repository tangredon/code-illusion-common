if ($env:APPVEYOR_PULL_REQUEST_NUMBER -and ($env:APPVEYOR_PULL_REQUEST_HEAD_REPO_NAME -ne 'tangredon/code-illusion'))
{ 
	Write-Host "Skip generating version for external repo"
	exit
}

Invoke-Expression "& gitversion /l console /output buildserver /updateAssemblyInfo"
