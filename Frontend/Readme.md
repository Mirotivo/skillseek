dotnet new mvc -n skillseek


mkdir ssl
openssl req -x509 -newkey rsa:4096 -keyout ssl/skillseek.key -out ssl/skillseek.crt -days 365
openssl pkcs12 -export -out ssl/certificate.pfx -inkey ssl/skillseek.key -in ssl/skillseek.crt
openssl x509 -in ssl/skillseek.crt -noout -text

docker build -t skillseek-frontend .
docker run -d -p 8000:8080 --name skillseek-frontend-container skillseek-frontend
docker exec -it skillseek-frontend-container bash
docker logs skillseek-frontend-container
docker stop skillseek-frontend-container
docker rm skillseek-frontend-container
docker rmi skillseek-frontend



