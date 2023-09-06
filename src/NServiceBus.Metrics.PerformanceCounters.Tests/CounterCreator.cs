using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

[CompilerGenerated]
public static class CounterCreator
{
    public static void Create()
    {
        var counterCreationCollection = new CounterCreationDataCollection(Counters);
        try
        {
            var categoryName = "NServiceBus";

            if (PerformanceCounterCategory.Exists(categoryName))
            {
                foreach (CounterCreationData counter in counterCreationCollection)
                {
                    if (!PerformanceCounterCategory.CounterExists(counter.CounterName, categoryName))
                    {
                        PerformanceCounterCategory.Delete(categoryName);
                        break;
                    }
                }
            }

            if (PerformanceCounterCategory.Exists(categoryName) == false)
            {
                PerformanceCounterCategory.Create(
                    categoryName: categoryName,
                    categoryHelp: "NServiceBus statistics",
                    categoryType: PerformanceCounterCategoryType.MultiInstance,
                    counterData: counterCreationCollection);
            }

            PerformanceCounter.CloseSharedResources();
        }
        catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException)
        {
            throw new Exception("Execution requires elevated permissions", ex);
        }
    }

    static CounterCreationData[] Counters = new CounterCreationData[]
    {
        new CounterCreationData("SLA violation countdown", "Seconds until the SLA for this endpoint is breached.", PerformanceCounterType.NumberOfItems32),
        new CounterCreationData("Critical Time Average", "The time it took from sending to processing the message.", PerformanceCounterType.AverageTimer32),
        new CounterCreationData("Critical Time AverageBase", "The time it took from sending to processing the message.", PerformanceCounterType.AverageBase),
        new CounterCreationData("Critical Time", "The time it took from sending to processing the message.", PerformanceCounterType.NumberOfItems32),
        new CounterCreationData("Processing Time Average", "The time it took to successfully process a message.", PerformanceCounterType.AverageTimer32),
        new CounterCreationData("Processing Time AverageBase", "The time it took to successfully process a message.", PerformanceCounterType.AverageBase),
        new CounterCreationData("Processing Time", "The time it took to successfully process a message.", PerformanceCounterType.NumberOfItems32),
        new CounterCreationData("# of msgs failures / sec", "The current number of failed processed messages by the transport per second.", PerformanceCounterType.RateOfCountsPerSecond32),
        new CounterCreationData("# of msgs successfully processed / sec", "The current number of messages processed successfully by the transport per second.", PerformanceCounterType.RateOfCountsPerSecond32),
        new CounterCreationData("# of msgs pulled from the input queue /sec", "The current number of messages pulled from the input queue by the transport per second.", PerformanceCounterType.RateOfCountsPerSecond32),
        new CounterCreationData("Retries", "A message has been scheduled for retry (FLR or SLR)", PerformanceCounterType.RateOfCountsPerSecond32),

    };
}