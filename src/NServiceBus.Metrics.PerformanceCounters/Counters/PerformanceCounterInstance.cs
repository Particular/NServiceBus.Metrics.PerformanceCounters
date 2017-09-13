using System.Diagnostics;

class PerformanceCounterInstance : IPerformanceCounterInstance
{
    public PerformanceCounterInstance(PerformanceCounter counter)
    {
        this.counter = counter;
    }

    public void Increment()
    {
        counter.Increment();
    }

    public void IncrementBy(long value)
    {
        counter.IncrementBy(value);
    }

    public void Dispose()
    {
        counter?.Dispose();
    }

    PerformanceCounter counter;

    public long RawValue
    {
        get => counter.RawValue;
        set => counter.RawValue = value;
    }
}