namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus;
    using NUnit.Framework;

    [TestFixture]
    public class PerformanceCounterUpdaterTests
    {
        [Test]
        public void Meters_within_payload_should_be_converted_into_performance_counters()
        {
            var endpointName = "Sender@af016c07";
            var cache = new MockPerformanceCountersCache();

            var signals = new[]
            {
                new MockSignalProbe("signal 1"),
                new MockSignalProbe("signal 2"),
                new MockSignalProbe("signal 3"),
            };

            var sut = new PerformanceCounterUpdater(cache, [], endpointName);

            sut.Observe(new ProbeContext(new IDurationProbe[0], signals));

            var e = new SignalEvent();
            Enumerable.Range(0, 111).ToList().ForEach(_ => signals[0].Observers(ref e));
            Enumerable.Range(0, 222).ToList().ForEach(_ => signals[1].Observers(ref e));
            Enumerable.Range(0, 333).ToList().ForEach(_ => signals[2].Observers(ref e));

            var performanceCounterOne = cache.Get(new CounterInstanceName(signals[0].Name, endpointName));
            var performanceCounterTwo = cache.Get(new CounterInstanceName(signals[1].Name, endpointName));
            var performanceCounterThree = cache.Get(new CounterInstanceName(signals[2].Name, endpointName));

            Assert.Multiple(() =>
            {
                Assert.That(performanceCounterOne.RawValue, Is.EqualTo(111));
                Assert.That(performanceCounterTwo.RawValue, Is.EqualTo(222));
                Assert.That(performanceCounterThree.RawValue, Is.EqualTo(333));
            });
        }

        [Test]
        public void Signals_that_map_to_legacy_names_should_be_converted_to_counters_with_queueAddress_as_instance_name()
        {
            var cache = new MockPerformanceCountersCache();

            var legacyInstanceNameMap = new Dictionary<string, CounterInstanceName?>
            {
                { "# of message failures / sec", new CounterInstanceName("# of msgs failures / sec", "queueAddress") },
                { "# of messages pulled from the input queue / sec", new CounterInstanceName("# of msgs pulled from the input queue /sec", "queueAddress") },
                { "# of messages successfully processed / sec", new CounterInstanceName("# of msgs successfully processed / sec", "queueAddress") },
            };

            var sut = new PerformanceCounterUpdater(cache, legacyInstanceNameMap, "Sender@af016c07");

            var signals = new[]
            {
                new MockSignalProbe("# of message failures / sec"),
                new MockSignalProbe("# of messages pulled from the input queue / sec"),
                new MockSignalProbe("# of messages successfully processed / sec"),
            };

            sut.Observe(new ProbeContext(new IDurationProbe[0], signals));

            var e = new SignalEvent();

            Enumerable.Range(0, 111).ToList().ForEach(_ => signals[0].Observers(ref e));
            Enumerable.Range(0, 222).ToList().ForEach(_ => signals[1].Observers(ref e));
            Enumerable.Range(0, 333).ToList().ForEach(_ => signals[2].Observers(ref e));

            var performanceCounterOne = cache.Get(new CounterInstanceName("# of msgs failures / sec", "queueAddress"));
            var performanceCounterTwo = cache.Get(new CounterInstanceName("# of msgs pulled from the input queue /sec", "queueAddress"));
            var performanceCounterThree = cache.Get(new CounterInstanceName("# of msgs successfully processed / sec", "queueAddress"));

            Assert.Multiple(() =>
            {
                Assert.That(performanceCounterOne.RawValue, Is.EqualTo(111));
                Assert.That(performanceCounterTwo.RawValue, Is.EqualTo(222));
                Assert.That(performanceCounterThree.RawValue, Is.EqualTo(333));
            });
        }


        [Test]
        public void Duration_probes_within_payload_should_be_converted_into_performance_counters()
        {
            var cache = new MockPerformanceCountersCache();
            var sut = new PerformanceCounterUpdater(cache, [], "Sender@af016c07");

            var durationProbes = new[]
            {
                new MockDurationProbe("Critical Time"),
                new MockDurationProbe("Processing Time")
            };

            sut.Observe(new ProbeContext(durationProbes, new ISignalProbe[0]));

            var timeSpan1 = TimeSpan.FromSeconds(11);
            var d1 = new DurationEvent(timeSpan1, null);
            durationProbes[0].Observers(ref d1);

            var timeSpan2 = TimeSpan.FromSeconds(22);
            var d2 = new DurationEvent(timeSpan2, null);
            durationProbes[1].Observers(ref d2);

            // asserting the number of accessed counters
            Assert.That(cache.Count, Is.EqualTo(3 + 3));

            var counter1 = cache.Get(new CounterInstanceName("Critical Time", "Sender@af016c07"));
            var counter1average = cache.Get(new CounterInstanceName("Critical Time Average", "Sender@af016c07"));
            var counter1averageBase = cache.Get(new CounterInstanceName("Critical Time AverageBase", "Sender@af016c07"));

            var counter2 = cache.Get(new CounterInstanceName("Processing Time", "Sender@af016c07"));
            var counter2average = cache.Get(new CounterInstanceName("Processing Time Average", "Sender@af016c07"));
            var counter2averageBase = cache.Get(new CounterInstanceName("Processing Time AverageBase", "Sender@af016c07"));

            Assert.Multiple(() =>
            {
                Assert.That(counter1.RawValue, Is.EqualTo(11));
                Assert.That(counter1average.RawValue, Is.EqualTo(CalculateAverageTimerCounterUpdate(timeSpan1)));
                Assert.That(counter1averageBase.RawValue, Is.EqualTo(1));

                Assert.That(counter2.RawValue, Is.EqualTo(22));
                Assert.That(counter2average.RawValue, Is.EqualTo(CalculateAverageTimerCounterUpdate(timeSpan2)));
                Assert.That(counter2averageBase.RawValue, Is.EqualTo(1));
            });
        }

        [Test]
        public void Duration_probes_within_payload_should_be_converted_into_performance_counters_with_no_raw_value_if_not_critical_or_processing_time()
        {
            var cache = new MockPerformanceCountersCache();
            var sut = new PerformanceCounterUpdater(cache, [], "Sender@af016c07");

            var durationProbes = new[]
            {
                new MockDurationProbe("Any Other Timer"),
            };

            sut.Observe(new ProbeContext(durationProbes, new ISignalProbe[0]));

            var timeSpan = TimeSpan.FromSeconds(11);
            var d = new DurationEvent(timeSpan, null);
            durationProbes[0].Observers(ref d);

            // asserting the number of accessed counters
            Assert.That(cache.Count, Is.EqualTo(2));

            var counter1 = cache.Get(new CounterInstanceName("Any Other Timer", "Sender@af016c07"));
            var counter1average = cache.Get(new CounterInstanceName("Any Other Timer Average", "Sender@af016c07"));
            var counter1averageBase = cache.Get(new CounterInstanceName("Any Other Timer AverageBase", "Sender@af016c07"));

            Assert.Multiple(() =>
            {
                Assert.That(counter1.RawValue, Is.EqualTo(0));
                Assert.That(counter1average.RawValue, Is.EqualTo(CalculateAverageTimerCounterUpdate(timeSpan)));
                Assert.That(counter1averageBase.RawValue, Is.EqualTo(1));
            });
        }

        static long CalculateAverageTimerCounterUpdate(TimeSpan d)
        {
            return d.Ticks * Stopwatch.Frequency / TimeSpan.TicksPerSecond;
        }
        [Test]
        public async Task Durations_should_be_reported_as_zeros_after_specific_period_from_last_observed_pipeline_completion()
        {
            const string endpoint = "Sender@af016c07";
            var resetTimersAfter = TimeSpan.FromMilliseconds(500);

            var cache = new MockPerformanceCountersCache();
            var updater = new PerformanceCounterUpdater(cache, [], endpoint, resetTimersAfter);

            var durationProbes = new[]
            {
                new MockDurationProbe("Critical Time"),
                new MockDurationProbe("Processing Time")
            };

            // update before timeout
            updater.Observe(new ProbeContext(durationProbes, new ISignalProbe[0]));

            updater.Start();

            durationProbes[0].Raise(TimeSpan.FromSeconds(11));
            durationProbes[1].Raise(TimeSpan.FromSeconds(22));

            await Task.Delay(TimeSpan.FromTicks(resetTimersAfter.Ticks * 2));

            var performanceCounterOne = cache.Get(new CounterInstanceName("Critical Time", endpoint));
            var performanceCounterTwo = cache.Get(new CounterInstanceName("Processing Time", endpoint));

            await updater.Stop();

            Assert.That(performanceCounterOne.RawValue, Is.EqualTo(0));
            Assert.That(performanceCounterTwo.RawValue, Is.EqualTo(0));
        }

        [Test]
        public void Durations_should_be_reported_properly_after_observed_pipeline_completion()
        {
            const string endpoint = "Sender@af016c07";
            var resetTimersAfter = TimeSpan.FromMilliseconds(100);

            var cache = new MockPerformanceCountersCache();
            var updater = new PerformanceCounterUpdater(cache, [], endpoint, resetTimersAfter);

            var durationProbes = new[]
            {
                new MockDurationProbe("Critical Time"),
                new MockDurationProbe("Processing Time")
            };

            // update before timeout
            updater.Observe(new ProbeContext(durationProbes, new ISignalProbe[0]));

            updater.Start();

            Thread.Sleep(TimeSpan.FromTicks(resetTimersAfter.Ticks * 2));

            updater.OnReceivePipelineCompleted();

            durationProbes[0].Raise(TimeSpan.FromSeconds(11));
            durationProbes[1].Raise(TimeSpan.FromSeconds(22));

            var performanceCounterOne = cache.Get(new CounterInstanceName("Critical Time", endpoint));
            var performanceCounterTwo = cache.Get(new CounterInstanceName("Processing Time", endpoint));

            Assert.That(performanceCounterOne.RawValue, Is.EqualTo(11));
            Assert.That(performanceCounterTwo.RawValue, Is.EqualTo(22));
        }
    }

    class MockPerformanceCountersCache : PerformanceCountersCache
    {
        protected override IPerformanceCounterInstance CreateInstance(CounterInstanceName counterInstanceName)
        {
            return new MockIPerformanceCounter();
        }

        public int Count => CountCounters();
    }

    class MockSignalProbe : ISignalProbe
    {
        public MockSignalProbe(string name)
        {
            Name = name;
        }

        public void Register(OnEvent<SignalEvent> observer)
        {
            Observers += observer;
        }

        public string Name { get; }
        public string Description => string.Empty;
        public OnEvent<SignalEvent> Observers = (ref SignalEvent e) => { };
    }

    class MockDurationProbe : IDurationProbe
    {
        public MockDurationProbe(string name)
        {
            Name = name;
        }

        public void Register(OnEvent<DurationEvent> observer)
        {
            Observers += observer;
        }

        public void Raise(TimeSpan timespan, string messageType = null)
        {
            var duration = new DurationEvent(timespan, messageType);
            Observers(ref duration);
        }

        public string Name { get; }
        public string Description => string.Empty;
        public OnEvent<DurationEvent> Observers = (ref DurationEvent d) => { };
    }
}