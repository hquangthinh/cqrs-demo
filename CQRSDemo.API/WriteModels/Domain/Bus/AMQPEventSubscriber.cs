using CQRSDemo.API.WriteModels.Events;
using CQRSDemo.API.WriteModels.Events.Handlers;
using CQRSlite.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSDemo.API.WriteModels.Domain.Bus
{
    public class Constants
    {
        public const string QUEUE_CUSTOMER_CREATED = "customer_created";
        public const string QUEUE_CUSTOMER_UPDATED = "customer_updated";
        public const string QUEUE_CUSTOMER_DELETED = "customer_deleted";
    }

    public class AMQPEventSubscriber
    {
        private readonly IBusEventHandler[] _handlers;

        private Dictionary<Type, MethodInfo> lookups = new Dictionary<Type, MethodInfo>();

        public AMQPEventSubscriber(IWebHostEnvironment env, IBusEventHandler[] handlers)
        {
            _handlers = handlers;
            foreach (var handler in _handlers)
            {
                var meth = (from m in handler.GetType()
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            let prms = m.GetParameters()
                            where prms.Count() == 1 && m.Name.Contains("Handle")
                            select new
                            {
                                EventType = handler.HandlerType,
                                Method = m
                            }).FirstOrDefault();
                if (meth != null)
                {
                    lookups.Add(meth.EventType, meth.Method);
                }
            }
            new Thread(() =>
            {
                Start(env.ContentRootPath);
            }).Start();
        }

        public void Start(string contentRootPath)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(contentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables();
            builder.Build().GetSection("amqp").Bind(connectionFactory);
            connectionFactory.AutomaticRecoveryEnabled = true;
            connectionFactory.NetworkRecoveryInterval = TimeSpan.FromSeconds(15);
            using (IConnection conn = connectionFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    DeclareQueues(channel);

                    var customerCreatedEventConsumer = new EventingBasicConsumer(channel);
                    var customerUpdatedEventConsumer = new EventingBasicConsumer(channel);
                    var customerDeletedEventConsumer = new EventingBasicConsumer(channel);

                    customerCreatedEventConsumer.Received += CustomerCreatedEventConsumer_Received;
                    customerUpdatedEventConsumer.Received += CustomerUpdatedEventConsumer_Received;
                    customerDeletedEventConsumer.Received += CustomerDeletedEventConsumer_Received;

                    while (true)
                    {
                        // Sleeps for 5 sec before trying again
                        Thread.Sleep(5000);
                        new Thread(() =>
                        {
                            channel.BasicConsume(queue: Constants.QUEUE_CUSTOMER_CREATED,
                                 autoAck: false,
                                 consumer: customerCreatedEventConsumer
                            );
                        }).Start();
                        new Thread(() =>
                        {
                            channel.BasicConsume(queue: Constants.QUEUE_CUSTOMER_UPDATED,
                                 autoAck: false,
                                 consumer: customerUpdatedEventConsumer
                            );
                        }).Start();
                        new Thread(() =>
                        {
                            channel.BasicConsume(queue: Constants.QUEUE_CUSTOMER_DELETED,
                                 autoAck: false,
                                 consumer: customerDeletedEventConsumer
                            );
                        }).Start();
                    }
                }
            }
        }

        private void CustomerCreatedEventConsumer_Received(object sender, BasicDeliverEventArgs eventArgsCreated)
        {
            if (eventArgsCreated != null)
            {
                string messageContent = Encoding.UTF8.GetString(eventArgsCreated.Body.ToArray());
                CustomerCreatedEvent _created = JsonConvert.DeserializeObject<CustomerCreatedEvent>(messageContent);
                HandleEvent(_created);
                ((EventingBasicConsumer)sender).Model.BasicAck(eventArgsCreated.DeliveryTag, false);
            }
        }

        private void CustomerUpdatedEventConsumer_Received(object sender, BasicDeliverEventArgs eventArgsUpdated)
        {
            if (eventArgsUpdated != null)
            {
                var messageContent = Encoding.UTF8.GetString(eventArgsUpdated.Body.ToArray());
                CustomerUpdatedEvent _updated = JsonConvert.DeserializeObject<CustomerUpdatedEvent>(messageContent);
                HandleEvent(_updated);
                ((EventingBasicConsumer)sender).Model.BasicAck(eventArgsUpdated.DeliveryTag, false);
            }
        }

        private void CustomerDeletedEventConsumer_Received(object sender, BasicDeliverEventArgs eventArgsDeleted)
        {
            if (eventArgsDeleted != null)
            {
                var messageContent = Encoding.UTF8.GetString(eventArgsDeleted.Body.ToArray());
                CustomerDeletedEvent _deleted = JsonConvert.DeserializeObject<CustomerDeletedEvent>(messageContent);
                HandleEvent(_deleted);
                ((EventingBasicConsumer)sender).Model.BasicAck(eventArgsDeleted.DeliveryTag, false);
            }
        }

        private void HandleEvent(IEvent @event)
        {
            var theHandler = _handlers.SingleOrDefault(x => x.HandlerType == @event.GetType());
            Task.Run(() =>
            {
                foreach (KeyValuePair<Type, MethodInfo> entry in lookups)
                {
                    if (entry.Key == @event.GetType())
                    {
                        entry.Value.Invoke(theHandler, new[] { (object)@event });
                    }
                }
            }).Wait();
        }

        private static void DeclareQueues(IModel channel)
        {
            channel.QueueDeclare(
                queue: Constants.QUEUE_CUSTOMER_CREATED,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            channel.QueueDeclare(
                queue: Constants.QUEUE_CUSTOMER_UPDATED,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            channel.QueueDeclare(
                queue: Constants.QUEUE_CUSTOMER_DELETED,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }
    }
}
