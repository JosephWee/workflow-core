using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Interface;

namespace WorkflowCore.Sample04
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IServiceProvider serviceProvider = ConfigureServices();

            //start the workflow host
            var host = serviceProvider.GetService<IWorkflowHost>();
            host.RegisterWorkflow<EventSampleWorkflow, MyDataClass>();
            host.Start();

            var initialData = new MyDataClass();
            var workflowId = host.StartWorkflow("EventSampleWorkflow", 1, initialData).Result;
            
            Console.WriteLine("Enter value to publish");
            string value = Console.ReadLine();
            host.PublishEvent("MyEvent", workflowId, value);

            //MyDataClass data = new MyDataClass()
            //{
            //    Value1 = value
            //};
            //host.PublishEvent("MyEvent", workflowId, data, DateTime.Now);

            Console.ReadLine();
            host.Stop();
        }

        private static IServiceProvider ConfigureServices()
        {
            //setup dependency injection
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();

            List<Type> typesToMap = new List<Type>();
            typesToMap.Add(typeof(MyDataClass));

            foreach (var typeToMap in typesToMap)
            {
                if (!BsonClassMap.IsClassMapRegistered(typeToMap))
                {
                    var classMap = new BsonClassMap(typeToMap);
                    classMap.AutoMap();
                    classMap.SetIgnoreExtraElements(true);

                    BsonClassMap.RegisterClassMap(classMap);
                }
            }

            //services.AddWorkflow();
            //services.AddWorkflow(x => x.UseMongoDB(@"mongodb://localhost:27017", "workflow"));
            //services.AddWorkflow(x => x.UseSqlServer(@"Server=.;Database=WorkflowCore;Trusted_Connection=True;", true, true));
            //services.AddWorkflow(x => x.UsePostgreSQL(@"Server=127.0.0.1;Port=5432;Database=workflow;User Id=postgres;", true, true));
            //services.AddWorkflow(x => x.UseSqlite(@"Data Source=database.db;", true));            

            services.AddWorkflow(x =>
            {
                x.UseAzureSynchronization(@"UseDevelopmentStorage=true");
                x.UseMongoDB(@"mongodb://localhost:27017", "workflow");
            });

            //services.AddWorkflow(x =>
            //{
            //    x.UseSqlServer(@"Data Source=.\SQLEXPRESS2022;Initial Catalog=WorkflowCore;Integrated Security=True;Trust Server Certificate=True;", true, true);
            //    x.UseSqlServerLocking(@"Data Source=.\SQLEXPRESS2022;Initial Catalog=WorkflowCore;Integrated Security=True;");
            //});

            //services.AddWorkflow(cfg =>
            //{
            //    var ddbConfig = new AmazonDynamoDBConfig() { RegionEndpoint = RegionEndpoint.USWest2 };

            //    cfg.UseAwsDynamoPersistence(new EnvironmentVariablesAWSCredentials(), ddbConfig, "sample4");
            //    cfg.UseAwsDynamoLocking(new EnvironmentVariablesAWSCredentials(), ddbConfig, "workflow-core-locks");
            //    cfg.UseAwsSimpleQueueService(new EnvironmentVariablesAWSCredentials(), new AmazonSQSConfig() { RegionEndpoint = RegionEndpoint.USWest2 });                
            //});

            //services.AddWorkflow(cfg =>
            //{
            //    cfg.UseRedisPersistence("localhost:6379", "sample4");
            //    cfg.UseRedisLocking("localhost:6379");
            //    cfg.UseRedisQueues("localhost:6379", "sample4");
            //    cfg.UseRedisEventHub("localhost:6379", "channel1");
            //});

            //services.AddWorkflow(x =>
            //{
            // x.UseMongoDB(@"mongodb://192.168.0.12:27017", "workflow");
            //x.UseRabbitMQ(new ConnectionFactory() { HostName = "localhost" });
            //x.UseRedlock(redis);
            //});

            //services.AddWorkflow(x =>
            //{
            //    x.UseMongoDB(@"mongodb://localhost:27017", "workflow");
            //    //x.UseSqlServerBroker(@"Data Source=.\SQLEXPRESS2022;Initial Catalog=WorkflowCore;Integrated Security=True;Trust Server Certificate=True;", true, true);
            //    //x.UseSqlServerLocking(@"Data Source=.\SQLEXPRESS2022;Initial Catalog=WorkflowCore;Integrated Security=True;");
            //});

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }

        private static IConnectionMultiplexer redis;
    }
}
