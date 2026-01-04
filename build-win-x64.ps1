$runtimeID = "win-x64"
$publishConfiguration = "Release"

Write-Host "--- Starting Stalkiana .NET Publish Script (Windows) ---" -ForegroundColor Cyan

$stalkianaBasePath = Join-Path -Path $env:USERPROFILE -ChildPath ".stalkiana"

if (-not (Test-Path -Path $stalkianaBasePath -PathType Container)) {
    Write-Host "Creating output directory: $stalkianaBasePath"
    try {

        New-Item -Path $stalkianaBasePath -ItemType Directory -Force -ErrorAction Stop | Out-Null
        Write-Host "Successfully created output directory: $stalkianaBasePath" -ForegroundColor Green
    }
    catch {

        Write-Error "Failed to create output directory '$stalkianaBasePath'. Error: $($_.Exception.Message)"
        if ($_.Exception.InnerException) {
            Write-Error "Inner Exception: $($_.Exception.InnerException.Message)"
        }
        exit 1 
    }
}
else {
    Write-Host "Output directory already exists: $stalkianaBasePath"
}

Write-Host "Publishing project/solution for Runtime ID: $runtimeID to $stalkianaBasePath..."

$projectPath = $PWD.Path 

$dotnetArgs = @(
    "publish",
    "`"$projectPath`"",                   
    "-c", $publishConfiguration,          
    "-r", $runtimeID,                     
    "--self-contained", "true",           
    "-p:PublishSingleFile=true",          
    "-p:PublishDir=$stalkianaBasePath"
)

$exitCode = -1

try {
    Write-Host "Running: dotnet $($dotnetArgs -join ' ')"
    & dotnet $dotnetArgs
    $exitCode = $LASTEXITCODE
}
catch {

    Write-Error "Failed to start 'dotnet publish'. Ensure the .NET SDK is installed and in your PATH."
    Write-Error "Error details: $($_.Exception.Message)"

    $exitCode = if ($process -and $process.HasExited) { $process.ExitCode } else { 1 }
}

if ($exitCode -eq 0) {
    Write-Host "--- Publish Successful ---`n" -ForegroundColor Green

    $absoluteOutputDir = $stalkianaBasePath 

    Write-Host "The application was published to: $absoluteOutputDir" -ForegroundColor Green
    Write-Host "`nAttempting to permanently add '$absoluteOutputDir' to the User PATH..." -ForegroundColor Yellow

    $scope = "User" 
    try {

        $currentPath = [Environment]::GetEnvironmentVariable("Path", $scope)
        if ($null -eq $currentPath) { $currentPath = "" } 

        $pathArray = $currentPath -split ';' | Where-Object { $_ -ne '' }

        if (-not ($pathArray -contains $absoluteOutputDir)) {

            if ([string]::IsNullOrEmpty($currentPath) -or $currentPath -eq ';') {

                $newPath = $absoluteOutputDir
            }
            else {

                $newPath = "$absoluteOutputDir;$($currentPath.Trim(';'))"
            }

            [Environment]::SetEnvironmentVariable("Path", $newPath, $scope)

            Write-Host "Successfully added '$absoluteOutputDir' to the permanent $scope PATH." -ForegroundColor Green
            Write-Host "IMPORTANT: You MUST restart PowerShell (or log out and log back in) for this change to take full effect in new sessions." -ForegroundColor Yellow
            Write-Host "For the current session, you can update the PATH by running:" -ForegroundColor Yellow
            Write-Host "  `$env:Path += ';$newPath'" -ForegroundColor Yellow
        }
        else {
            Write-Host "'$absoluteOutputDir' is already present in the $scope PATH. No changes made." -ForegroundColor Cyan
        }
    }
    catch {

        Write-Error "Failed to modify the permanent $scope PATH. Error: $($_.Exception.Message)"
        if ($scope -eq "Machine") {
            Write-Warning "Modifying the Machine PATH requires running PowerShell as Administrator."
        }
        Write-Warning "You may need to add '$absoluteOutputDir' to your PATH manually."
    }
}
else {

    Write-Error "--- Publish Failed ---"
    Write-Error "'dotnet publish' command failed with exit code $exitCode."
    exit $exitCode 
}

Write-Host "`n--- Script Finished ---" -ForegroundColor Cyan