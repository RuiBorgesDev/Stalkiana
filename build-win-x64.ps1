$runtimeID = "win-x64"
$publishConfiguration = "Release"
$relativeOutputDir = "publish_output"

Write-Host "--- Starting Stalkiana .NET Publish Script (Windows) ---" -ForegroundColor Cyan

$outputDir = Join-Path -Path $PWD.Path -ChildPath $relativeOutputDir

if (-not (Test-Path -Path $outputDir -PathType Container)) {
    Write-Host "Creating output directory: $outputDir"
    try {
        New-Item -Path $outputDir -ItemType Directory -Force -ErrorAction Stop | Out-Null
    }
    catch {
        Write-Error "Failed to create output directory '$outputDir'. Error: $_"
        exit 1
    }
}
else {
    Write-Host "Output directory already exists: $outputDir"
}

Write-Host "Publishing project/solution for Runtime ID: $runtimeID to $outputDir..."

$dotnetArgs = @(
    "publish",
    "-c", $publishConfiguration,
    "-r", $runtimeID,
    "--self-contained", "true",
    "-p:PublishSingleFile=true",
    "-p:PublishDir=$outputDir"
)

try {
    Write-Host "Running: dotnet $($dotnetArgs -join ' ')"
    $process = Start-Process dotnet -ArgumentList $dotnetArgs -Wait -PassThru -NoNewWindow
    $exitCode = $process.ExitCode
}
catch {
    Write-Error "Failed to start 'dotnet publish'. Ensure the .NET SDK is installed and in your PATH."
    Write-Error $_.Exception.Message
    $exitCode = if ($process) { $process.ExitCode } else { -1 }
}

if ($exitCode -eq 0) {
    Write-Host "--- Publish Successful ---`n" -ForegroundColor Green

    $absoluteOutputDir = $outputDir

    Write-Host "Attempting to permanently add '$absoluteOutputDir' to the User PATH..." -ForegroundColor Yellow

    $scope = "User"
    try {
        $currentPath = [Environment]::GetEnvironmentVariable("Path", $scope)

        $pathArray = $currentPath -split ';' | Where-Object { $_ -ne '' }

        if (-not ($pathArray -contains $absoluteOutputDir)) {
            if ([string]::IsNullOrEmpty($currentPath)) {
                $newPath = $absoluteOutputDir
            }
            else {
                $newPath = "$absoluteOutputDir;$currentPath"
            }

            [Environment]::SetEnvironmentVariable("Path", $newPath, $scope)

            Write-Host "Successfully added '$absoluteOutputDir' to the permanent $scope PATH." -ForegroundColor Green
            Write-Host "IMPORTANT: You MUST restart PowerShell (or log out/in) for this change to take effect in new sessions." -ForegroundColor Yellow
        }
        else {
            Write-Host "'$absoluteOutputDir' is already present in the $scope PATH. No changes made." -ForegroundColor Cyan
        }
    }
    catch {
        Write-Error "Failed to modify the permanent $scope PATH. Error: $_"
        if ($scope -eq "Machine") {
            Write-Warning "Modifying the Machine PATH requires running PowerShell as Administrator."
        }
    }

}
else {
    Write-Error "--- Publish Failed ---"
    Write-Error "'dotnet publish' command failed with exit code $exitCode."
    exit $exitCode
}

Write-Host "`n--- Script Finished ---" -ForegroundColor Cyan