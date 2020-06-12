docker run -d --hostname dev-rabbit-server-node1 --name rabbit-server-node1 -p 5672:5672 rabbitmq:latest

docker run -d --hostname dev-rabbit-management --name rabbit-management -p 5672:5672 -p 15672:15672 rabbitmq:management