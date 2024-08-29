using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NUnit.Framework;

[TestFixture]
public class IntegrationTests
{
    const string EndpointName = "PerfCountersIntegrationTests";
    static readonly ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

    [Test]
    public async Task Ensure_counters_are_written()
    {
        string message = null;

        var endpointConfiguration = new EndpointConfiguration(EndpointName);
        endpointConfiguration.UseTransport(new LearningTransport());
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();

        endpointConfiguration.DefineCriticalErrorAction(
            (context, _) =>
            {
                message = context.Error;
                ManualResetEvent.Set();
                return Task.FromResult(0);
            });

        var performanceCounters = endpointConfiguration.EnableWindowsPerformanceCounters();
        performanceCounters.EnableSLAPerformanceCounters(TimeSpan.FromSeconds(10));

        var endpoint = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        var criticalTime = GetCounter(PerformanceCountersFeature.CriticalTimeCounterName);
        var processingTime = GetCounter(PerformanceCountersFeature.ProcessingTimeCounterName);

        Assert.That(criticalTime.RawValue, Is.EqualTo(0));
        Assert.That(processingTime.RawValue, Is.EqualTo(0));

        var cancellation = new CancellationTokenSource();

        var criticalTimeReading = ReadNonZero(criticalTime, cancellation.Token);
        var processingTimeReading = ReadNonZero(processingTime, cancellation.Token);

        await endpoint.SendLocal(new MyMessage(), cancellationToken: cancellation.Token)
            .ConfigureAwait(false);

        ManualResetEvent.WaitOne();

        await Task.Delay(1500, cancellation.Token)
            .ConfigureAwait(false);

        await endpoint.Stop(cancellation.Token)
            .ConfigureAwait(false);

        await cancellation.CancelAsync();
        var slaPerCounter = GetCounter(SLAMonitoringFeature.CounterName);
        var messagesFailuresPerSecondCounter = GetCounter(PerformanceCountersFeature.MessagesFailuresPerSecondCounterName);
        var messagesProcessedPerSecondCounter = GetCounter(PerformanceCountersFeature.MessagesProcessedPerSecondCounterName);
        var messagesPulledPerSecondCounter = GetCounter(PerformanceCountersFeature.MessagesPulledPerSecondCounterName);
        Assert.That(await criticalTimeReading, Is.True);
        Assert.That(await processingTimeReading, Is.True);
        Assert.That(slaPerCounter.RawValue, Is.Not.EqualTo(0));
        Assert.That(messagesFailuresPerSecondCounter.RawValue, Is.EqualTo(0));
        Assert.That(messagesProcessedPerSecondCounter.RawValue, Is.Not.EqualTo(0));
        Assert.That(messagesPulledPerSecondCounter.RawValue, Is.Not.EqualTo(0));

        Assert.IsNull(message);
    }

    static async Task<bool> ReadNonZero(PerformanceCounter counter, CancellationToken cancellationToken)
    {
        while (counter.RawValue == 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Short and don't want to return false, not throw
            await Task.Delay(TimeSpan.FromMilliseconds(10), CancellationToken.None);
        }

        return true;
    }

    static PerformanceCounter GetCounter(string counterName) => new PerformanceCounter("NServiceBus", counterName, EndpointName, true);

    public class MyHandler : IHandleMessages<MyMessage>
    {
        public Task Handle(MyMessage message, IMessageHandlerContext context)
        {
            ManualResetEvent.Set();
            return Task.Delay(TimeSpan.FromMilliseconds(1000));
        }
    }

    public class MyMessage : ICommand;
}