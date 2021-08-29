$global:projectIdentifier = "Illusion.Common"

$global:buildQueueVariable = ""
$global:buildSeparator = ";"

Function TryAddToMap($map, [string]$dependency, [string]$project)
{
    if ($map.ContainsKey($dependency)) {
        $map[$dependency] = $map[$dependency] + ";" + $project;
    }
    else {
        $map.Add($dependency, $project);
    }
}


$editedFiles = git diff HEAD HEAD~ --name-only
Write-Output "-> Changed Files:"
Write-Output $editedFiles
Write-Output ""
Write-Output ""

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

$editedFiles | ForEach-Object {	

    if ($_.StartsWith($global:projectIdentifier)) {
        $parts = $_.Split("/")
        $folder = $parts[0]
        $isFolder = Test-Path $folder -PathType Container

        if ($isFolder -eq 1) {
            $project = $folder.Replace("$global:projectIdentifier.", "")
            Write-Output "$project" # We know this project was changed

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
                        Write-Output "  + $dependency"
                    }
                }
            }
        }
    }
}

[int]$ms = $stopwatch.Elapsed.Milliseconds
Write-Output "Duration: $ms ms"

$changedProjects = New-Object Collections.Generic.HashSet[string]
$a = $changedProjects.Add('t')
$a = $changedProjects.Add('t')
$a = $changedProjects.Add('a')

Function BuildDependencyMap()
{
    $dependencyMap = @{}

    $folders = Get-ChildItem -Path . -Directory | Where-Object { $_.Name.StartsWith($global:projectIdentifier) }
    foreach ($folder in $folders)
    {
        $project = $folder.Name.Replace("$global:projectIdentifier.", "")
    
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

$map = BuildDependencyMap
Write-Output $map

# Write-Output $dependencyMap["Core"].Split(";", [System.StringSplitOptions]::RemoveEmptyEntries)