﻿[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute(@"NServiceBus.Metrics.PerformanceCounters.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100D32BC524DCB1205998C155A4F36BF873587D3602822ECD7B49CD775B2E6A006EE6B9164AB2E3103A6A4D1310C6E5C26818A32FE86710141A2D1F02EB564381CD64C88131BCCA478CDB5072F06DB991DE33DAC1C82BAF40D9F61DD6B40300A4673B693B51CD10A8B9B7D8AB64450431FA422514D6DABCAF70DF785B1E4E6E8AAF")]
[assembly: System.Runtime.Versioning.TargetFrameworkAttribute(".NETFramework,Version=v4.5.2", FrameworkDisplayName=".NET Framework 4.5.2")]

namespace NServiceBus.Metrics.PerformanceCounters
{
    
    [System.AttributeUsageAttribute(System.AttributeTargets.Assembly | System.AttributeTargets.All)]
    public sealed class PerformanceCounterSettingsAttribute : System.Attribute
    {
        public PerformanceCounterSettingsAttribute() { }
        public bool CSharp { get; set; }
        public bool Powershell { get; set; }
        public string ScriptPromotionPath { get; set; }
    }
}
namespace NServiceBus
{
    
    public class static PerformanceCountersExtensions
    {
        public static NServiceBus.PerformanceCountersSettings EnableWindowsPerformanceCounters(this NServiceBus.EndpointConfiguration endpointConfiguration) { }
    }
    public class PerformanceCountersSettings
    {
        public void EnableSLAPerformanceCounters(System.TimeSpan sla) { }
        [System.ObsoleteAttribute("This interval is no longer used for reporting. Counters values are updated as soo" +
            "n as they are reported. Will be treated as an error from version 2.0.0. Will be " +
            "removed in version 3.0.0.", false)]
        public void UpdateCounterEvery(System.TimeSpan updateInterval) { }
    }
}