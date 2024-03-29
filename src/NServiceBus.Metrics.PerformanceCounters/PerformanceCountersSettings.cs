﻿namespace NServiceBus
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using Configuration.AdvancedExtensibility;

    /// <summary>
    /// Windows performance counter configuration instance.
    /// </summary>
    public class PerformanceCountersSettings
    {
        EndpointConfiguration endpointConfiguration;

        internal PerformanceCountersSettings(EndpointConfiguration endpointConfiguration)
        {
            this.endpointConfiguration = endpointConfiguration;
        }

        /// <summary>
        /// Enables the Time To Breach SLA performance counter.
        /// </summary>
        /// <param name="sla">The SLA to use. Must be greater than <see cref="TimeSpan.Zero" />.</param>
        [SupportedOSPlatform("windows")]
        public void EnableSLAPerformanceCounters(TimeSpan sla)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("Windows Performance Counters are not supported on this platform.");
            }

            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sla, TimeSpan.Zero);

            endpointConfiguration.GetSettings().Set(SLAMonitoringFeature.EndpointSLAKey, sla);
            endpointConfiguration.EnableFeature<SLAMonitoringFeature>();
        }
    }
}