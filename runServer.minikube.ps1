Write-Host "Baue Images"

cd MMORPGServer
./buildDockerAll.ps1
cd ..

Write-Host "Deploy to Minikube"
cd charts
./minikubeDeploy.ps1
cd ..
