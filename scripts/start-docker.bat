@echo off
docker-compose up -d
echo All services are starting...
echo Web UI will be available at: http://localhost:3000
echo API will be available at: http://localhost:5211/api
echo Use 'docker-compose logs -f' to view logs