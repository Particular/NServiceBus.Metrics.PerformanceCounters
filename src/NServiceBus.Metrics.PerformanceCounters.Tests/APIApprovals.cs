namespace NServiceBus.Metrics.PerformanceCounters.Tests
{
    using NUnit.Framework;
    using Particular.Approvals;
    using PublicApiGenerator;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        public void Approve()
        {
            var publicApi = typeof(PerformanceCountersFeature).Assembly.GeneratePublicApi(new ApiGeneratorOptions
            {
                ExcludeAttributes = new[]
                {
                "System.Runtime.Versioning.TargetFrameworkAttribute",
                "System.Reflection.AssemblyMetadataAttribute"
            }
            });
#if NET7_0
            Approver.Verify(publicApi, scenario: "net7.0");
#elif NET6_0
            Approver.Verify(publicApi, scenario: "net6.0");
#elif NET472
            Approver.Verify(publicApi, scenario: "net472");
#else
        throw new Exception("Unknown target framework in API Approval test");
#endif
        }
    }
}