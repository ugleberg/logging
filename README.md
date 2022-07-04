# logging

**Run Elasticsearch in Docker:**
```
docker run -d --restart always --name elasticsearch --net elastic -p 9200:9200 -e discovery.type=single-node -e ES_JAVA_OPTS="-Xms1g -Xmx1g" -e xpack.security.enabled=false -it elasticsearch:8.3.1
```
**Run Kibana in Docker:**
```
docker run -d --restart always --name kibana --net elastic -p 5601:5601  -it kibana:8.3.1
```
