using System;

interface IPerformanceCounterInstance : IDisposable
{
    void Increment();
    long RawValue { get; set; }
}