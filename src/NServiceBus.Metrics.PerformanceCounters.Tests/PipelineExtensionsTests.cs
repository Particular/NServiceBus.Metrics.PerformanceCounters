using System;
using System.Collections.Generic;
using NServiceBus;
using NUnit.Framework;

[TestFixture]
public class PipelineExtensionsTests
{

    [Test]
    public void Should_extract_timeSent_from_headers()
    {
        var dateTime = new DateTimeOffset(2000, 1, 1, 1, 1, 1, new TimeSpan());
        var headers = new Dictionary<string, string>
        {
            {
                Headers.TimeSent, DateTimeOffsetHelper.ToWireFormattedString(dateTime)
            }
        };
        var pipelineCompleted = PipelineHelper.BuildPipelineCompleted(
            headers: headers,
            startedAt: new DateTime(2000, 1, 1, 1, 1, 2, DateTimeKind.Utc),
            completedAt: new DateTime(2000, 1, 1, 1, 1, 3, DateTimeKind.Utc));

        pipelineCompleted.TryGetTimeSent(out var timeSent);
        Assert.That(timeSent, Is.EqualTo(dateTime));
    }
}