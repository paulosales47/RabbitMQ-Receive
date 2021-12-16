using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;


public class Receive{
    
    public static void Main(){

        var factory = new ConnectionFactory(){ HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "rpc_queue", durable : false, exclusive: false, autoDelete: false, arguments: null);
            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: "rpc_queue", autoAck: false, consumer: consumer);
            System.Console.WriteLine("[x] Awaiting RPC requests");

            consumer.Received += (model, ea) => 
            {
                string? response = default;
                
                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;
                var replayProps = channel.CreateBasicProperties();
                replayProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    int n = int.Parse(message);

                    Console.WriteLine("[.] fib({0})", message);
                    response = fib(n).ToString();
                }
                catch(Exception ex){
                    Console.WriteLine("[.]" + ex.Message);
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response!);
                    channel.BasicPublish
                    (
                        exchange: "",
                        routingKey: props.ReplyTo,
                        basicProperties: replayProps,
                        body: responseBytes
                    );

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }

            };

            

            Console.WriteLine("Press [enter] to exit");
            Console.ReadKey();
        }        

    }

    private static int fib(int n){
        if(n == 0 || n == 1)
            return n;
        
        return fib(n - 1) + fib(n - 2);
    }
}
