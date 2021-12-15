using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;


public class Receive{
    
    public static void Main(string[] args){

        if(args.Length < 1)
            return;

        var factory = new ConnectionFactory(){ HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.ExchangeDeclare(exchange: "topic_logs", type: ExchangeType.Topic);
            var queueName = channel.QueueDeclare().QueueName;

            System.Console.WriteLine("[*] Waiting for logs");

            foreach(var topic in args){
                channel.QueueBind(queue: queueName, exchange: "topic_logs", routingKey: topic);
            }

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) => 
            { 
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                System.Console.WriteLine("[x] Received '{0}':'{1}'", routingKey, message);
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            Console.WriteLine("Press [enter] to exit");
            Console.ReadKey();
        }        

    }

}
