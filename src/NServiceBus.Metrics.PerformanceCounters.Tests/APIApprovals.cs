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

#if NETFRAMEWORK
            Approver.Verify(publicApi, scenario: "net4x");
#else
            Approver.Verify(publicApi, scenario: "dotnet");
#endif
        }
    }
}