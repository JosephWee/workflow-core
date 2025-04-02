using System;
using System.Collections.Concurrent;
using System.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Primitives
{
    public class WaitFor : StepBody
    {
#if DEBUG
        public static ConcurrentDictionary<string, int> StepExecutionCount = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, DateTime> StepLastExecutionDate = new ConcurrentDictionary<string, DateTime>();

        public static ConcurrentDictionary<string, int> IsNotPublishedCount = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, DateTime> IsNotPublishedDate = new ConcurrentDictionary<string, DateTime>();

        public static ConcurrentDictionary<string, int> EffectiveDateChangedCount = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, DateTime> EffectiveDateLastChangedDate = new ConcurrentDictionary<string, DateTime>();

        public static ConcurrentDictionary<string, int> IsPublishedCount = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, DateTime> IsPublishedDate = new ConcurrentDictionary<string, DateTime>();
#endif

        public string EventKey { get; set; }

        public string EventName { get; set; }

        public DateTime EffectiveDate { get; set; }

        public object EventData { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
#if DEBUG 
            StepExecutionCount.AddOrUpdate(EventKey, 1, (key, value) => ++value);
            StepLastExecutionDate.AddOrUpdate(EventKey, DateTime.Now, (key, value) => DateTime.Now);
#endif

            if (!context.ExecutionPointer.EventPublished)
            {
                DateTime effectiveDate = DateTime.MinValue;

                if (EffectiveDate != null)
                {
                    effectiveDate = EffectiveDate;

#if DEBUG 
                    EffectiveDateChangedCount.AddOrUpdate(EventKey, 1, (key, value) => ++value);
                    EffectiveDateLastChangedDate.AddOrUpdate(EventKey, DateTime.Now, (key, value) => DateTime.Now);
#endif
                }

#if DEBUG
                IsNotPublishedCount.AddOrUpdate(EventKey, 1, (key, value) => ++value);
                IsNotPublishedDate.AddOrUpdate(EventKey, DateTime.Now, (key, value) => DateTime.Now);
#endif

                OutputStatus();

                return ExecutionResult.WaitForEvent(EventName, EventKey, effectiveDate);
            }

#if DEBUG
            IsPublishedCount.AddOrUpdate(EventKey, 1, (key, value) => ++value);
            IsPublishedDate.AddOrUpdate(EventKey, DateTime.Now, (key, value) => DateTime.Now);
#endif

            OutputStatus();

            EventData = context.ExecutionPointer.EventData;
            return ExecutionResult.Next();
        }

        public void OutputStatus()
        {
            var keys = StepExecutionCount.Keys.ToList();

            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];

                //Console.WriteLine(
                    string.Format(
                        "Total: {0} ({1}), Not Published: {2} ({3}), Published: {4} ({5}), Effective Date Changed: {6} ({7})",
                        StepExecutionCount[key],
                        StepLastExecutionDate[key],
                        IsNotPublishedCount[key],
                        IsNotPublishedDate[key],
                        IsPublishedCount[key],
                        IsPublishedDate[key],
                        EffectiveDateChangedCount[key],
                        EffectiveDateLastChangedDate[key]
                    )
                );
            }
        }
    }
}
