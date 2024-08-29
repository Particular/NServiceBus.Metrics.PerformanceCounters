namespace Tests
{
    using System;
    using NUnit.Framework;
    using Particular.Approvals;

    [TestFixture]
    public class PerformanceCountersCacheTests
    {
        [Test]
        public void Get_the_same_counter_twice_returns_the_same()
        {
            var cache = new TestablePerformanceCountersCache();

            var counterName = "CounterName";
            var instanceName = "InstancenName";

            var firstCounter = cache.Get(new CounterInstanceName(counterName, instanceName));
            var secondCounter = cache.Get(new CounterInstanceName(counterName, instanceName));

            Assert.Multiple(() =>
            {
                Assert.That(firstCounter, Is.Not.Null);
                Assert.That(secondCounter, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(secondCounter, Is.SameAs(firstCounter));
                Assert.That(cache.CountersCreated, Is.EqualTo(1));
            });
        }

        [Test]
        public void Should_throw_for_endpoint_name_too_long()
        {
            var cache = new PerformanceCountersCache();

            var exception = Assert.Throws<Exception>(() =>
            {
                cache.Get(new CounterInstanceName("counter", new string('*', 129)));
            });
            Approver.Verify(exception.Message);
        }

        [Test]
        public void Dispose_should_dispose_counters()
        {
            var cache = new TestablePerformanceCountersCache();

            var someCounter = cache.Get(new CounterInstanceName("RandomName", "RandomInstanceName"));
            var anotherCounter = cache.Get(new CounterInstanceName("AnotherRandomName", "AnotherRandomInstance"));

            cache.Dispose();

            Assert.Multiple(() =>
            {
                Assert.That(((MockIPerformanceCounter)someCounter).Disposed, Is.True);
                Assert.That(((MockIPerformanceCounter)anotherCounter).Disposed, Is.True);
            });
        }

        class TestablePerformanceCountersCache : PerformanceCountersCache
        {
            public int CountersCreated { get; private set; }

            protected override IPerformanceCounterInstance CreateInstance(CounterInstanceName counterInstanceName)
            {
                CountersCreated++;
                return new MockIPerformanceCounter();
            }
        }
    }
}