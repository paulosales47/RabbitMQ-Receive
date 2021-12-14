using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;


public class Receive{
    
    public  static void Main(){

        var factory = new ConnectionFactory(){ HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "task_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.BasicQos(0, 1, false);
            
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) => 
            { 
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                System.Console.WriteLine("[x] Received {0}", message);
                int dots = message.Split('.').Length - 1;
                Thread.Sleep(dots * 3000);
                Console.WriteLine("[X] Done");
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: "task_queue", autoAck: false, consumer: consumer);

            Console.WriteLine("Press [enter] to exit");
            Console.ReadKey();
        }        

    }

}
