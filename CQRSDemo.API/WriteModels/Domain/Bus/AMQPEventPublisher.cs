using CQRSDemo.API.WriteModels.Events;
using CQRSlite.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSDemo.API.WriteModels.Domain.Bus
{
	public class AMQPEventPublisher : IEventPublisher
	{
		private readonly ConnectionFactory connectionFactory;

		public AMQPEventPublisher(IWebHostEnvironment env, AMQPEventSubscriber aMQPEventSubscriber)
		{
			connectionFactory = new ConnectionFactory();
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
				.AddEnvironmentVariables();

			builder.Build().GetSection("amqp").Bind(connectionFactory);
		}

		public Task Publish<T>(T @event, CancellationToken cancellationToken) where T : class, IEvent
		{
			using (IConnection conn = connectionFactory.CreateConnection())
			{
				using (IModel channel = conn.CreateModel())
				{
					var queue = @event is CustomerCreatedEvent ?
						Constants.QUEUE_CUSTOMER_CREATED : @event is CustomerUpdatedEvent ?
							Constants.QUEUE_CUSTOMER_UPDATED : Constants.QUEUE_CUSTOMER_DELETED;
					channel.QueueDeclare(
						queue: queue,
						durable: false,
						exclusive: false,
						autoDelete: false,
						arguments: null
					);
					var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
					channel.BasicPublish(
						exchange: "",
						routingKey: queue,
						basicProperties: null,
						body: body
					);
				}
			}
			return Task.FromResult("");
		}
    }
}
