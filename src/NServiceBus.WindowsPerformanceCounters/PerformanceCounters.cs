﻿namespace NServiceBus
{
    using System;
    using Settings;

    public class PerformanceCounters
    {
        SettingsHolder settings;

        internal PerformanceCounters(SettingsHolder settings)
        {
            this.settings = settings;
        }

        public PerformanceCounters EnableSLACounters(Action<PerformanceSettings> customizations)
        {
            customizations(new PerformanceSettings(settings));
            return this;
        }
    }
}
