﻿namespace NServiceBus.Metrics.PerformanceCounters
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    static class PowerShellCounterWriter
    {
        public static void WriteScript(string scriptPath, IEnumerable<TimerDefinition> timers, IEnumerable<MeterDefinition> meters, Dictionary<string, string> legacyInstanceNameMap)
        {
            var outputPath = Path.Combine(scriptPath, "CreateNSBPerfCounters.ps1");
            using (var streamWriter = File.CreateText(outputPath))
            {
                var stringBuilder = new StringBuilder();

                var slaCounterDefinition = @"New-Object System.Diagnostics.CounterCreationData ""SLA violation countdown"", ""Seconds until the SLA for this endpoint is breached."",  NumberOfItems32";
                stringBuilder.AppendLine(slaCounterDefinition.PadLeft(slaCounterDefinition.Length + 8));

                foreach (var timer in timers)
                {
                    var timerDefinition = $@"New-Object System.Diagnostics.CounterCreationData ""{timer.Name}"", ""{timer.Description}"",  NumberOfItems32";
                    stringBuilder.AppendLine(timerDefinition.PadLeft(timerDefinition.Length + 8));
                }

                foreach (var meter in meters)
                {
                    string instanceName;
                    legacyInstanceNameMap.TryGetValue(meter.Name, out instanceName);

                    var meterDefinition = $@"New-Object System.Diagnostics.CounterCreationData ""{instanceName ?? meter.Name}"", ""{meter.Description}"",  RateOfCountsPerSecond32";
                    stringBuilder.AppendLine(meterDefinition.PadLeft(meterDefinition.Length + 8));
                }

                streamWriter.Write(Template, stringBuilder);
            }
        }

        const string Template = @"
#requires -RunAsAdministrator
Function InstallNSBPerfCounters {{

    $category = @{{Name=""NServiceBus""; Description=""NServiceBus statistics""}}
    $counters = New-Object System.Diagnostics.CounterCreationDataCollection
    $counters.AddRange(@(
{0}
    ))
    if ([System.Diagnostics.PerformanceCounterCategory]::Exists($category.Name)) {{
        [System.Diagnostics.PerformanceCounterCategory]::Delete($category.Name)
    }}
    [void] [System.Diagnostics.PerformanceCounterCategory]::Create($category.Name, $category.Description, [System.Diagnostics.PerformanceCounterCategoryType]::MultiInstance, $counters)
    [System.Diagnostics.PerformanceCounter]::CloseSharedResources()
}}
InstallNSBPerfCounters
";
    }
}