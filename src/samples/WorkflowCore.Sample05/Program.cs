using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using WorkflowCore.Interface;

namespace WorkflowCore.Sample05
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IServiceProvider serviceProvider = ConfigureServices();

            //start the workflow host
            var host = serviceProvider.GetService<IWorkflowHost>();
            host.RegisterWorkflow<DeferSampleWorkflow>();
            host.Start();

            host.StartWorkflow("DeferSampleWorkflow", 1, null, null);
                        
            Console.ReadLine();
            host.Stop();
        }

        private static IServiceProvider ConfigureServices()
        {
            //setup dependency injection
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            //services.AddWorkflow();
            services.AddWorkflow(x => x.UseSqlServer(@"Data Source=.\SQLEXPRESS2022;Initial Catalog=WorkflowCore;Integrated Security=True;Trust Server Certificate=True;", true, true));
            
            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }
    }
}
