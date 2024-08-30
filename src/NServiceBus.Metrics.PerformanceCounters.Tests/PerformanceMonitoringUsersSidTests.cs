using NUnit.Framework;

[TestFixture]
public class PerformanceMonitoringUsersSidTests
{
    [Test]
    public void GetUserName()
    {
        Assert.That(PerformanceMonitoringUsersSid.Get(), Is.Not.Null);
    }
}