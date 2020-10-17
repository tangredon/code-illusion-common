[string] $tagBranch = "release/";
[string] $branchName = "$env:GitVersion_BranchName";
[string] $tag = "$env:GitVersion_SemVer";
[string] $pr = "$env:APPVEYOR_PULL_REQUEST_NUMBER";

Write-Host "Running tag script...";
Write-Host "Branch: $branchName";
Write-Host "PR: $pr";

If ([string]::IsNullOrEmpty($branchName)) {
    Write-Host "No branch found, skipping ..."
    exit
}

If (-not [string]::IsNullOrEmpty($pr)) {
    Write-Host "PR build, skipping ..."
    exit
}

If ($branchName.StartsWith($tagBranch)) 
{
    Write-Host "Tagging commit with $tag ...";
    Invoke-Expression "& git remote add appveyor `"https://$($env:access_token):x-oauth-basic@github.com/tangredon/code-illusion.git`"";
    Invoke-Expression "& git tag `"$tag`"";
    Invoke-Expression "& git push appveyor -q `"$tag`""
}
Else {
    Write-Host "Current branch not selected for tagging.";
}