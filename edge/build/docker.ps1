##################################################
# Build and publish docker containers
#
# todo: parameterize
###################################################

param(
  [string]$version = '0.0.15',
  [string]$target = 'arm32v7'
)


docker build  --rm -f "..\modules\BalloonModule\Dockerfile.$target" -t starbucktest2.azurecr.io/balloonmodule:$version-$target "..\modules\BalloonModule" ; if ($?) { docker push starbucktest2.azurecr.io/balloonmodule:$version-$target }
if ($LASTEXITCODE) { throw "BalloonModule Docker build failed" }

docker build  --rm -f "..\modules\TrackerModule\Dockerfile.$target" -t starbucktest2.azurecr.io/trackermodule:$version-$target "..\modules\TrackerModule" ; if ($?) { docker push starbucktest2.azurecr.io/trackermodule:$version-$target }
if ($LASTEXITCODE) { throw "TrackerModule Docker build failed" }

docker build  --rm -f "..\modules\SerialModule\Dockerfile.$target" -t starbucktest2.azurecr.io/serialmodule:$version-$target "..\modules\SerialModule" ; if ($?) { docker push starbucktest2.azurecr.io/serialmodule:$version-$target }
if ($LASTEXITCODE) { throw "SerialModule Docker build failed" }

docker build  --rm -f "..\modules\IngestionModule\Dockerfile.$target" -t starbucktest2.azurecr.io/ingestionmodule:$version-$target "..\modules\IngestionModule" ; if ($?) { docker push starbucktest2.azurecr.io/ingestionmodule:$version-$target }
if ($LASTEXITCODE) { throw "IngestionModule Docker build failed" }

docker build  --rm -f "..\modules\StorageModule\Dockerfile.$target" -t starbucktest2.azurecr.io/storagemodule:$version-$target "..\modules\StorageModule" ; if ($?) { docker push starbucktest2.azurecr.io/storagemodule:$version-$target }
if ($LASTEXITCODE) { throw "StorageModule Docker build failed" }