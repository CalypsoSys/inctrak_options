git pull
docker build -t inctrak .
docker rmi $(docker images -f "dangling=true" -q)
docker save inctrak > /tmp/inctrak.tar
