using System;
using NUnit.Framework;

[TestFixture]
public class EstimatedTimeToSLABreachCounterTests
{
    [Test]
    public void Single_DataPoint_should_result_in_MaxValue()
    {
        var mockPerformanceCounter = new MockIPerformanceCounter();
        var counter = new EstimatedTimeToSLABreachCounter(TimeSpan.FromSeconds(2), mockPerformanceCounter);
        counter.Update(
            sent: new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero),
            processingStarted: new DateTimeOffset(2000, 1, 1, 1, 1, 2, TimeSpan.Zero),
            processingEnded: new DateTimeOffset(2000, 1, 1, 1, 1, 3, TimeSpan.Zero));
        Assert.That(mockPerformanceCounter.RawValue, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void Exceed_SLA_should_result_in_Zero()
    {
        var mockPerformanceCounter = new MockIPerformanceCounter();
        var counter = new EstimatedTimeToSLABreachCounter(TimeSpan.FromSeconds(2), mockPerformanceCounter);
        counter.Update(
            sent: new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero),
            processingStarted: new DateTimeOffset(2000, 1, 1, 1, 1, 2, TimeSpan.Zero),
            processingEnded: new DateTimeOffset(2000, 1, 1, 1, 1, 3, TimeSpan.Zero));
        counter.Update(
            sent: new DateTimeOffset(2000, 1, 1, 1, 1, 4, TimeSpan.Zero),
            processingStarted: new DateTimeOffset(2000, 1, 1, 1, 1, 5, TimeSpan.Zero),
            processingEnded: new DateTimeOffset(2000, 1, 1, 1, 1, 10, TimeSpan.Zero));
        Assert.That(mockPerformanceCounter.RawValue, Is.EqualTo(0));
    }

    [Test]
    public void Within_SLA_should_result_in_difference()
    {
        var mockPerformanceCounter = new MockIPerformanceCounter();
        var counter = new EstimatedTimeToSLABreachCounter(TimeSpan.FromSeconds(20), mockPerformanceCounter);
        counter.Update(
            sent: new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero),
            processingStarted: new DateTimeOffset(2000, 1, 1, 1, 1, 2, TimeSpan.Zero),
            processingEnded: new DateTimeOffset(2000, 1, 1, 1, 1, 3, TimeSpan.Zero));
        counter.Update(
            sent: new DateTimeOffset(2000, 1, 1, 1, 1, 4, TimeSpan.Zero),
            processingStarted: new DateTimeOffset(2000, 1, 1, 1, 1, 5, TimeSpan.Zero),
            processingEnded: new DateTimeOffset(2000, 1, 1, 1, 1, 10, TimeSpan.Zero));
        Assert.That(mockPerformanceCounter.RawValue, Is.EqualTo(24));
    }

}