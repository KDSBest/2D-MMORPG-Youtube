Write-Host "Install Game"

cd mmorpg

helm uninstall --wait -n mmorpg mmorpg ./
helm upgrade -i -n mmorpg -f ./values.minikube.yaml --create-namespace mmorpg ./

cd ..
