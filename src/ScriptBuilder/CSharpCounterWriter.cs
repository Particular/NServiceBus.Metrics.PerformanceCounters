﻿namespace NServiceBus.Metrics.PerformanceCounters
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    static class CSharpCounterWriter
    {
        public static void WriteCode(string scriptPath, IEnumerable<TimerDefinition> timers, IEnumerable<MeterDefinition> meters, Dictionary<string, string> legacyInstanceNameMap)
        {
            var outputPath = Path.Combine(scriptPath, "Counters.g.cs");
            using (var streamWriter = File.CreateText(outputPath))
            {
                var stringBuilder = new StringBuilder();

                var slaCounterDefinition = @"new CounterCreationData(""SLA violation countdown"", ""Seconds until the SLA for this endpoint is breached."", PerformanceCounterType.NumberOfItems32),";
                stringBuilder.AppendLine(slaCounterDefinition.PadLeft(slaCounterDefinition.Length + 8));

                foreach (var timer in timers)
                {
                    var timerDefinition = $@"new CounterCreationData(""{timer.Name}"", ""{timer.Description}"", PerformanceCounterType.NumberOfItems32),";
                    stringBuilder.AppendLine(timerDefinition.PadLeft(timerDefinition.Length + 8));
                }

                foreach (var meter in meters)
                {
                    string instanceName;
                    legacyInstanceNameMap.TryGetValue(meter.Name, out instanceName);

                    var meterDefinition = $@"new CounterCreationData(""{instanceName ?? meter.Name}"", ""{meter.Description}"", PerformanceCounterType.RateOfCountsPerSecond32),";
                    stringBuilder.AppendLine(meterDefinition.PadLeft(meterDefinition.Length + 8));
                }

                streamWriter.Write(Template, stringBuilder);
            }
        }

        const string Template = @"using System;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public static class CounterCreator 
{{
    public static void Create() 
    {{
        var counterCreationCollection = new CounterCreationDataCollection(Counters);
        try
        {{
            var install = false;
            var categoryName = ""NServiceBus"";
            if (PerformanceCounterCategory.Exists(categoryName))
            {{
                var category = PerformanceCounterCategory.GetCategories().Single(x => x.CategoryName == categoryName);
                var existingCounters = category.GetCounters();

                if (existingCounters.Length != counterCreationCollection.Count)
                {{
                    install = true;
                }}
                else
                {{
                    foreach (var counter in Counters)
                    {{
                        var foundCounter = existingCounters.FirstOrDefault(c => c.CounterName == counter.CounterName);
                        var found = foundCounter?.CounterName == counter.CounterName | foundCounter?.CounterType == counter.CounterType | foundCounter?.CounterHelp == counter.CounterHelp;
                        if (!found)
                        {{
                            install = true;
                        }}
                    }}
                }}
            }}

            if (install)
            {{
                if (PerformanceCounterCategory.Exists(categoryName))
                {{
                    PerformanceCounterCategory.Delete(categoryName);
                }}

                PerformanceCounterCategory.Create(
                    categoryName: categoryName,
                    categoryHelp: ""NServiceBus statistics"",
                    categoryType: PerformanceCounterCategoryType.MultiInstance,
                    counterData: counterCreationCollection);
                PerformanceCounter.CloseSharedResources();
            }}
        }} catch(Exception ex) when(ex is SecurityException || ex is UnauthorizedAccessException)
        {{
            throw new Exception(""Execution requires elevated permissions"", ex);
        }}
    }}

    static CounterCreationData[] Counters = new CounterCreationData[]
    {{
{0}
    }};
}}";
    }
}