using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Metrics.PerformanceCounters;

class PerformanceCounterUpdater
{
    public PerformanceCounterUpdater(PerformanceCountersCache cache, Dictionary<string, CounterInstanceName?> legacyInstanceNameMap, string endpointName, TimeSpan? resetTimersAfter = null)
    {
        resetEvery = resetTimersAfter ?? TimeSpan.FromSeconds(2);
        this.legacyInstanceNameMap = legacyInstanceNameMap;
        this.endpointName = endpointName;
        this.cache = cache;
        counterCleanupTokenSource = new CancellationTokenSource();
        // initialize to an armed state
        OnReceivePipelineCompleted();
    }

    // no Task.Run() here since CleanupAndSwallowExceptions yields with an await almost immediately
    public void Start() => cleaner = CleanupAndSwallowExceptions(counterCleanupTokenSource.Token);

    public Task Stop(CancellationToken cancellationToken = default)
    {
        counterCleanupTokenSource.Cancel();
        return cleaner;
    }

    public void Observe(ProbeContext context)
    {
        foreach (var signalProbe in context.Signals)
        {
            legacyInstanceNameMap.TryGetValue(signalProbe.Name, out var instanceName);

            var performanceCounterInstance = cache.Get(instanceName ?? new CounterInstanceName(signalProbe.Name, endpointName));

            signalProbe.Register((ref SignalEvent e) => performanceCounterInstance.Increment());
        }

        foreach (var durationProbe in context.Durations)
        {
            if (durationProbe.Name is CounterNameConventions.ProcessingTime or CounterNameConventions.CriticalTime)
            {
                var key = new CounterInstanceName(durationProbe.Name, endpointName);
                var performanceCounterInstance = cache.Get(key);
                resettable[key] = performanceCounterInstance;
                durationProbe.Register((ref DurationEvent d) => performanceCounterInstance.RawValue = (long)d.Duration.TotalSeconds);
            }

            var averageTimerCounter = cache.Get(new CounterInstanceName(durationProbe.Name.GetAverageTimerCounterName(), endpointName));
            var baseAverageTimerCounter = cache.Get(new CounterInstanceName(durationProbe.Name.GetAverageTimerBaseCounterName(), endpointName));

            durationProbe.Register((ref DurationEvent d) =>
            {
                var performanceCounterTicks = d.Duration.Ticks * Stopwatch.Frequency / TimeSpan.TicksPerSecond;
                averageTimerCounter.IncrementBy(performanceCounterTicks);
                baseAverageTimerCounter.Increment();
            });
        }
    }

    readonly PerformanceCountersCache cache;
    readonly Dictionary<string, CounterInstanceName?> legacyInstanceNameMap;
    readonly ConcurrentDictionary<CounterInstanceName, IPerformanceCounterInstance> resettable = new ConcurrentDictionary<CounterInstanceName, IPerformanceCounterInstance>();
    long lastCompleted;

    public void OnReceivePipelineCompleted()
    {
        Volatile.Write(ref lastCompleted, NowTicks);
    }

    async Task CleanupAndSwallowExceptions(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(resetEvery, cancellationToken).ConfigureAwait(false);

                var idleFor = NowTicks - Volatile.Read(ref lastCompleted);
                if (idleFor > resetEvery.Ticks)
                {
                    foreach (var performanceCounter in resettable)
                    {
                        performanceCounter.Value.RawValue = 0;
                    }
                }
            }
            catch (Exception ex) when (ex.IsCausedBy(cancellationToken))
            {
                // private token, updater is being stopped, don't log the exception because the Task.Delay stack trace is not interesting
                break;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to reset performance counter buffers", ex);
            }
        }
    }

    static long NowTicks => DateTime.UtcNow.Ticks;

    static readonly ILog logger = LogManager.GetLogger<PerformanceMonitorUsersInstaller>();

    readonly TimeSpan resetEvery;
    readonly string endpointName;
    readonly CancellationTokenSource counterCleanupTokenSource;

    Task cleaner;
}
