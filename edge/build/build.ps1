##################################################
# Build dotnet iot edge modules
#
# todo: parameterize
###################################################


param(
  [string]$path = $PSScriptRoot,
  [string[]]$targets = 'default'
)

function Invoke-Dotnet {
  write-host $args
  dotnet $args
  if ($LASTEXITCODE) { throw "Dotnet failed" }
}

# Bootstrap posh-build
$build_dir = Join-Path $path "."
if (! (Test-Path (Join-Path $build_dir "Posh-Build.ps1"))) { Write-Host "Installing posh-build..."; New-Item -Type Directory $build_dir -ErrorAction Ignore | Out-Null; Save-Script "Posh-Build" -Path $build_dir }
. (Join-Path $build_dir "Posh-Build.ps1")


target default -depends compile, test

target compile {

  Invoke-Dotnet publish "..\modules\BalloonModule\BalloonModule.csproj"
  Invoke-Dotnet publish "..\modules\SerialModule\SerialModule.csproj"
  Invoke-Dotnet publish "..\modules\TrackerModule\TrackerModule.csproj"
  Invoke-Dotnet publish "..\modules\StorageModule\StorageModule.csproj"
  
}

target test {
  
  # This runs "dotnet test". Change to Invoke-Xunit to invoke "dotnet xunit"
  Invoke-Tests "..\..\tests\BalloonModule.Test\BalloonModule.Test.csproj"
  Invoke-Tests "..\..\tests\SerialModule.Test\SerialModule.Test.csproj"
  Invoke-Tests "..\..\tests\TrackerModule.Test\TrackerModule.Test.csproj"

}



Start-Build $targets