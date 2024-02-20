using System;

struct CounterInstanceName : IEquatable<CounterInstanceName>
{
    public bool Equals(CounterInstanceName other)
    {
        return string.Equals(CounterName, other.CounterName, StringComparison.InvariantCultureIgnoreCase) && string.Equals(InstanceName, other.InstanceName, StringComparison.InvariantCultureIgnoreCase);
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is not CounterInstanceName)
        {
            return false;
        }
        return Equals((CounterInstanceName)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (StringComparer.InvariantCultureIgnoreCase.GetHashCode(CounterName) * 397) ^ StringComparer.InvariantCultureIgnoreCase.GetHashCode(InstanceName);
        }
    }

    public readonly string CounterName;
    public readonly string InstanceName;

    public CounterInstanceName(string counterName, string instanceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(counterName);
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceName);

        CounterName = counterName;
        InstanceName = instanceName;
    }
}