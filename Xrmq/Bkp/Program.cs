// using System.Text;
// using Microsoft.Extensions.Logging;
// using RabbitMQ.Client;

// namespace X
// {
//     public class Program
//     {
//         public static void Main(string[] args)
//         {
//             var logger = LoggerFactory.Create(builder => {
//                 builder.SetMinimumLevel(LogLevel.Debug);
//                 builder.AddConsole();
//             }).CreateLogger<Xrmq>();
//             var properties = new XrmqProperties();

//             var xrmq = new Xrmq(logger, properties);
//             xrmq.QueueDeclare("payment.create", null);
//             xrmq.Consume<string>("payment.create", (model, evt) => {
//                 logger.LogDebug("fuckkk");
//                 var message = Encoding.UTF8.GetString(evt.Body.ToArray());
//                 logger.LogDebug("message {message}", message);

//                 throw new Exception("error");
//             }, null);

//             for(var i = 0; i < 1; i++)
//             {
//                 // new Thread(() => {
//                 //     xrmq.Publish(string.Empty, "payment.create", null, message);
//                 // }).Start();
//                 Task.Run(() => {
//                     xrmq.Publish(string.Empty, "payment.create", null, Encoding.UTF8.GetBytes("hello world"));
//                 });
//             }

//             Thread.Sleep(5000);

//             // while(true)
//             // {

//             // }
//         }
//     }
// }