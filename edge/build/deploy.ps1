##################################################
# Deploy deployment json to iot edgedevices
#
# todo: parameterize
###################################################

az iot edge set-modules --device-id weathballoon-rc --hub-name StarbuckTest --content ../balloon_build.deployment.json

az iot edge set-modules --device-id TrackerOne --hub-name StarbuckTest --content ../tracker_build.deployment.json

az iot edge set-modules --device-id TrackerTwo-test --hub-name StarbuckTest --content ../tracker_build.deployment.json

az iot edge set-modules --device-id TrackerThree --hub-name StarbuckTest --content ../tracker_build.deployment.json
