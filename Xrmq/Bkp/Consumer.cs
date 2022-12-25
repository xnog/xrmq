// using System.Text;
// using RabbitMQ.Client;
// using RabbitMQ.Client.Events;

// namespace X
// {
//     public class Consumer
//     {
//         private readonly IConnectionFactory connectionFactory;

//         public Consumer(IConnectionFactory connectionFactory)
//         {
//             this.connectionFactory = connectionFactory;
//         }

//         public List<byte[]> Consume(string queue)
//         {
//             // var connectionFactory = new ConnectionFactory() { HostName = "localhost" };
//             using (var connection = connectionFactory.CreateConnection())
//             using (var channel = connection.CreateModel())
//             {
//                 channel.ExchangeDeclare("dlx.exchange", "direct", true, false);
//                 channel.ExchangeDeclare("delay.exchange", "x-delayed-message", true, false, new Dictionary<string, object>() {
//                     { "x-delayed-type", "direct" }
//                 });

//                 channel.QueueDeclare("payment.create.dead", true, false, false);
//                 channel.QueueBind("payment.create.dead", "dlx.exchange", "payment.create.dead");
//                 channel.QueueDeclare("payment.create", true, false, false, new Dictionary<string, object> {
//                     { "x-dead-letter-exchange", "dlx.exchange" },
//                     { "x-dead-letter-routing-key", "payment.create.dead" }
//                 });

//                 var consumer = new EventingBasicConsumer(channel);
//                 consumer.Received += (model, ea) =>
//                 {
//                     try
//                     {
//                         var message = Encoding.UTF8.GetString(ea.Body.ToArray());
//                         Console.WriteLine(" [x] Received {0}", message);
//                         throw new Exception();
//                     }
//                     catch (Exception e)
//                     {
//                         Console.WriteLine("error", e.Message);
//                     }
//                     finally
//                     {
//                         channel.BasicAck(ea.DeliveryTag, false);
//                     }
//                 };

//                 channel.BasicConsume("payment.create", false, consumer);

//                 return null;
//             }
//         }
//     }
// }
