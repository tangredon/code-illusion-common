$global:projectIdentifier = "Illusion.Common"

$editedFiles = git diff HEAD HEAD~ --name-only
Write-Output "-> Changed Files:"
Write-Output $editedFiles
Write-Output ""
Write-Output ""

$editedFiles | ForEach-Object {	
    # Write-Output $_

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