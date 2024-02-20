namespace NServiceBus
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Exposes windows performance counters configuration on <see cref="EndpointConfiguration"/>.
    /// </summary>
    public static class PerformanceCountersExtensions
    {
        /// <summary>
        /// Enables receive statistics and critical time performance counters.
        /// </summary>
        /// <param name="endpointConfiguration">The <see cref="EndpointConfiguration" /> instance to apply the settings to.</param>
        public static PerformanceCountersSettings EnableWindowsPerformanceCounters(this EndpointConfiguration endpointConfiguration)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("Windows Performance Counters are not supported on this platform.");
            }

            ArgumentNullException.ThrowIfNull(endpointConfiguration);

            endpointConfiguration.EnableMetrics();
            endpointConfiguration.EnableFeature<PerformanceCountersFeature>();

            return new PerformanceCountersSettings(endpointConfiguration);
        }
    }
}