﻿using System;

struct CounterInstanceName : IEquatable<CounterInstanceName>
{
    public bool Equals(CounterInstanceName other)
    {
        return string.Equals(CounterName, other.CounterName, StringComparison.InvariantCultureIgnoreCase) && string.Equals(InstanceName, other.InstanceName, StringComparison.InvariantCultureIgnoreCase);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        return obj is CounterInstanceName && Equals((CounterInstanceName)obj);
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
        Guard.AgainstNull(nameof(counterName), counterName);
        Guard.AgainstNull(nameof(instanceName), instanceName);

        CounterName = counterName;
        InstanceName = instanceName;
    }
}