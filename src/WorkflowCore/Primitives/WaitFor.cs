using System;
using System.Collections.Concurrent;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Primitives
{
    public class WaitFor : StepBody
    {
#if DEBUG
        public static ConcurrentDictionary<string, int> StepExecutionCount = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, int> EffectiveDateChangedCount = new ConcurrentDictionary<string, int>();
        public static ConcurrentDictionary<string, DateTime> StepLastExecutionDate = new ConcurrentDictionary<string, DateTime>();
        public static ConcurrentDictionary<string, DateTime> EffectiveDateLastChangedDate = new ConcurrentDictionary<string, DateTime>();
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

                return ExecutionResult.WaitForEvent(EventName, EventKey, effectiveDate);
            }

            EventData = context.ExecutionPointer.EventData;
            return ExecutionResult.Next();
        }
    }
}
