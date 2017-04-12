namespace NServiceBus.Metrics.PerformanceCounters
{
    class TimerDefinition
    {
        public TimerDefinition(string name, string description, string unit, string[] tags = null)
        {
            Name = name;
            Description = description;
            Unit = unit;
            Tags = tags;
        }

        public string Description { get; }

        public string Name { get; }

        public string Unit { get; }

        public string[] Tags { get; }
    }
}