rabbitmq-plugins enable rabbitmq_management

rabbitmq-plugins enable rabbitmq_web_stomp

echo "create admin user"
rabbitmqctl add_user admin konbini62
rabbitmqctl set_user_tags admin administrator
rabbitmqctl set_permissions -p / admin ".*" ".*" ".*"