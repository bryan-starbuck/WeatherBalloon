###############################################################################
## Main build pipeline script
## usage:
##   ./build_pipeline.ps1 -version #.#.# -target [arm32v7 (default) | amd64]
##
## performs Build, Test, Dockerization, Docker Publish, Device update
###############################################################################
param(
  [string]$version = '0.0.18',
  [string]$target = 'arm32v7'
)

Write-Host "Building all edge modules"

./version -label ($version + '-' + $target)

./build.ps1
if ($LASTEXITCODE) { throw "Build/Test failed" }

./docker.ps1 -version $version -target $target
if ($LASTEXITCODE) { throw "Docker failed" }

./deploy.ps1 
if ($LASTEXITCODE) { throw "Deploy failed" }


  
