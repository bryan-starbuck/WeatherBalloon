##################################################
# Create deployment template files with version
#
# todo: need to parameterize the container registry 
# in the deployment templates
###################################################

param(
  [string]$label = '0.0.15-arm32v7'
)

$Credentials = convertfrom-stringdata (get-content ./credentials.txt -raw)


((Get-Content -path ..\balloon.deployment.json -Raw) -replace '\$label', $label) | Set-Content -Path ..\balloon_build.deployment.json

((Get-Content -path ..\balloon_build.deployment.json -Raw) -replace '\$password', $Credentials.'password') | Set-Content -Path ..\balloon_build.deployment.json


((Get-Content -path ..\tracker.deployment.json -Raw) -replace '\$label', $label) | Set-Content -Path ..\tracker_build.deployment.json

((Get-Content -path ..\tracker_build.deployment.json -Raw) -replace '\$password', $Credentials.'password') | Set-Content -Path ..\tracker_build.deployment.json