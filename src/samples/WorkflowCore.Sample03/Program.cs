using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;


namespace WorkflowCore.Sample03
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IServiceProvider serviceProvider = ConfigureServices();

            //start the workflow host
            var host = serviceProvider.GetService<IWorkflowHost>();
            host.RegisterWorkflow<PassingDataWorkflow, MyDataClass>();
            host.RegisterWorkflow<PassingDataWorkflow2, Dictionary<string, int>>();
            host.Start();

            var initialData = new MyDataClass
            {
                Value1 = 2,
                Value2 = 3
            };

            host.StartWorkflow("PassingDataWorkflow", 1, initialData);


            var initialData2 = new Dictionary<string, int>
            {
                ["Value1"] = 7,
                ["Value2"] = 2
            };

            //host.StartWorkflow("PassingDataWorkflow2", 1, initialData2);

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
            //services.AddWorkflow(x => x.UseSqlServer("Data Source=.\\SQLEXPRESS2022;Initial Catalog=WorkflowCore;Integrated Security=True;Trust Server Certificate=True;", true, true));
            services.AddWorkflow(x => x.UseMongoDB(@"mongodb://localhost:27017", "workflow")); 
            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }
    }
}
