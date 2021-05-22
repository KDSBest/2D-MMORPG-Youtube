helm repo add stable https://charts.helm.sh/stable
helm install prometheus stable/prometheus-operator --namespace prometheus --create-namespace
helm install redis-exporeter --namespace prometheus --set "redisAddress=redis://redis-svc.mmorpg.svc.cluster.local:6379" stable/prometheus-redis-exporter

https://rtfm.co.ua/en/kubernetes-a-clusters-monitoring-with-the-prometheus-operator/#Adding_new_application_under_monitoring

 kubectl -n prometheus get deploy redis-exporeter-prometheus-redis-exporter -o yaml 
 to get:
  app: prometheus-redis-exporter
    app.kubernetes.io/managed-by: Helm
    chart: prometheus-redis-exporter-3.5.1
	
kubectl -n prometheus get prometheus -o yaml
to get:
 serviceMonitorSelector:
      matchLabels:
        release: prometheus
		
Redis ServiceMonitor
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: monitoring-redis-svc
  namespace: prometheus
  labels:
    app: redis-svc-service-monitor
    serviceapp: redis-servicemonitor
    release: prometheus
spec:
  selector:
    matchLabels:
        app.kubernetes.io/managed-by: Helm
        chart: prometheus-redis-exporter-3.5.1
  endpoints:
  - interval: 15s
    path: /metrics
    targetPort: 9121
  namespaceSelector:
    matchNames:
    - prometheus
	
	
Add Dashboard ID:
763