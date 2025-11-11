using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;

class PerformanceCountersFeature : Feature
{
    protected override void Setup(FeatureConfigurationContext context)
    {
        context.ThrowIfSendOnly();

        var endpoint = context.Settings.EndpointName();

        var legacyInstanceNameMap = new Dictionary<string, CounterInstanceName?>
        {
            {
                "# of message failures / sec", new CounterInstanceName(MessagesFailuresPerSecondCounterName, endpoint)
            },
            {
                "# of messages pulled from the input queue / sec", new CounterInstanceName(MessagesPulledPerSecondCounterName, endpoint)
            },
            {
                "# of messages successfully processed / sec", new CounterInstanceName(MessagesProcessedPerSecondCounterName, endpoint)
            }
        };

        cache = new PerformanceCountersCache();
        updater = new PerformanceCounterUpdater(cache, legacyInstanceNameMap, endpoint);

        context.RegisterStartupTask(new Cleanup(this));
        context.AddInstaller<PerformanceMonitorUsersInstaller>();

        context.Pipeline.OnReceivePipelineCompleted((_, __) =>
        {
            updater.OnReceivePipelineCompleted();
            return Task.CompletedTask;
        });

        var options = context.Settings.Get<MetricsOptions>();

        options.RegisterObservers(probeContext =>
        {
            updater.Observe(probeContext);
        });
    }

    PerformanceCounterUpdater updater;
    PerformanceCountersCache cache;

    public const string MessagesPulledPerSecondCounterName = "# of msgs pulled from the input queue /sec";
    public const string MessagesProcessedPerSecondCounterName = "# of msgs successfully processed / sec";
    public const string MessagesFailuresPerSecondCounterName = "# of msgs failures / sec";
    public const string CriticalTimeCounterName = "Critical Time";
    public const string ProcessingTimeCounterName = "Processing Time";

    class Cleanup(PerformanceCountersFeature feature) : FeatureStartupTask, IDisposable
    {
        public void Dispose()
        {
            feature.updater = null;
            feature.cache.Dispose();
        }

        protected override Task OnStart(IMessageSession session, CancellationToken cancellationToken = default)
        {
            feature.updater.Start();
            return Task.CompletedTask;
        }

        protected override Task OnStop(IMessageSession session, CancellationToken cancellationToken = default) => feature.updater.Stop(cancellationToken);
    }
}