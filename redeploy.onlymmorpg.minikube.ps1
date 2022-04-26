Write-Host "Baue Images"

cd MMORPGServer
./buildDockerAll.ps1
cd ..

Write-Host "Deploy to Minikube"
cd charts
./minikubeReploy.ps1
cd ..
