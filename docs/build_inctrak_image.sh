1. cd ~/dotnet/inctrak_options
2. git pull
3. cd shared.inctrak.com/
4. docker build -t inctrak .
5. docker rmi $(docker images -f "dangling=true" -q)
6. docker save inctrak > /tmp/inctrak.tar
7. Copy to local machine (host)

1. Transfer inctrak.tar to production and update .lastgood
2. cd ~/docker/inctrak_demo_dotnet
3. docker-compose stop
4. cd ~/docker/inctrak_shared_dotnet
5. docker-compose stop
6. cd ..
7. docker load < inctrak.tar
8. docker rmi $(docker images -f "dangling=true" -q)
9. cd ~/docker/inctrak_demo_dotnet
10. docker-compose start
11. cd ~/docker/inctrak_shared_dotnet
12. docker-compose start

