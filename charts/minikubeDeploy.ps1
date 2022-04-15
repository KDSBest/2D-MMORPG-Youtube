
Write-Host "Install Prometheus"
cd .\prometheus\charts\kube-prometheus-stack\
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo add grafana https://grafana.github.io/helm-charts
helm dependency build
helm upgrade -i -n prometheus --create-namespace prometheus ./

cd ..
cd ..
cd ..

Write-Host "Install Traefik"
helm upgrade traefik ./traefik/traefik/ -f ./traefik-values.yaml -f ./traefik-values.minikube.yaml --namespace traefik --create-namespace -i
kubectl apply -f ./TraefikServiceMonitor.yaml --namespace prometheus

Write-Host "Install Redis Exporter"
helm upgrade -i -n prometheus --set "redisAddress=redis://redis-svc.mmorpg.svc.cluster.local:6379" --create-namespace redis-exporter ./prometheus/charts/prometheus-redis-exporter
kubectl apply -f ./RedisExporterServiceMonitor.yaml --namespace prometheus

Write-Host "Install Game"

cd mmorpg

helm upgrade -i -n mmorpg -f ./values.minikube.yaml --create-namespace mmorpg ./

cd ..
