$global:projectIdentifier = "Illusion.Common"

$global:buildQueueVariable = ""
$global:buildSeparator = ";"


Function BuildDependencyMap()
{
    $dependencyMap = @{}

    $folders = Get-ChildItem -Path . -Directory -Name | Where-Object { $_.StartsWith($global:projectIdentifier) }
    foreach ($folder in $folders)
    {
        $project = $folder.Replace("$global:projectIdentifier.", "")
        $csproj = "$folder/$folder.csproj"
        
        $output = & dotnet list "$csproj" reference
        ForEach ($line in $($output -split "`r`n"))
        {
            $result = [regex]::Match($line, "$global:projectIdentifier.(\w*).csproj")
            if ($result.Success -eq 1)
            {
                $group = $result.Groups[1];
                if ($group.Success -eq 1 -and $group.Value -notcontains $project)
                {
                    $dependency = $group.Value;
    
                    $old = $dependencyMap[$dependency];
                    $dependencyMap[$dependency] = "$old;$project"
                }
            }
        }
    }
    
    $newMap = @{}

    foreach($key in $dependencyMap.Keys)
    {
        $newMap[$key] = $dependencyMap[$key].Split(";", [System.StringSplitOptions]::RemoveEmptyEntries)
    }

    return $newMap;
}

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

$dependencyMap = BuildDependencyMap
Write-Host "Dependency Map"
$dependencyMap.Keys | ForEach-Object { 
    Write-Host "    -> $_" -NoNewline
    Write-Host " + " -NoNewline
    Write-Host $dependencyMap[$_]
}
Write-Host ""

$editedFiles = git diff HEAD HEAD~ --name-only
Write-Host "Changed Files:"
$editedFiles | ForEach-Object { Write-Host "    -> $_" }
Write-Host ""
Write-Host ""

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

$changedProjects = New-Object Collections.Generic.HashSet[string]
$map = @{};

foreach($editedFile in $editedFiles)
{
    if ($editedFile.StartsWith($global:projectIdentifier)) {
        $parts = $editedFile.Split("/")
        $folder = $parts[0]
        $isFolder = Test-Path $folder -PathType Container

        if ($isFolder -eq 1) {
            $project = $folder.Replace("$global:projectIdentifier.", "")
            $a = $changedProjects.Add($project)

            $csproj = "$folder/$folder.csproj"
            $output = & dotnet list "$csproj" reference

            ForEach ($line in $($output -split "`r`n"))
            {
                $result = [regex]::Match($line, "$global:projectIdentifier.(\w*).csproj")
                if ($result.Success -eq 1)
                {
                    $group = $result.Groups[1];
                    if ($group.Success -eq 1 -and $group.Value -notcontains $project)
                    {
                        $dependency = $group.Value;

                        $old = $map[$project];
                        $map[$project] = "$old;$dependency"
                    }
                }
            }

            # todo: move this out of the changedFile loop
            $list = $map[$project]
            if ($null -ne $list) 
            {
                $map[$project] = $map[$project].Split(";", [System.StringSplitOptions]::RemoveEmptyEntries)
            }
        }
    }
}

Write-Host "Changed Projects"
foreach($proj in $changedProjects)
{
    AppendQueueVariable $proj
    Write-Host "    -> $proj"
    $deps = $map[$proj];
    foreach($dep in $deps)
    {
        AppendQueueVariable $dep
        Write-Host "        + $dep"
    }
}

[int]$ms = $stopwatch.Elapsed.Milliseconds
Write-Host "Duration: $ms ms"

Write-Host "Build Queue: $global:buildQueueVariable"
Write-Host "##vso[task.setvariable variable=buildQueue;isOutput=true]$global:buildQueueVariable"