using System;
using NServiceBus;
using NServiceBus.Features;

static class Extensions
{
    public static bool TryGetTimeSent(this ReceivePipelineCompleted completed, out DateTimeOffset timeSent)
    {
        var headers = completed.ProcessedMessage.Headers;
        if (headers.TryGetValue(Headers.TimeSent, out var timeSentString))
        {
            timeSent = DateTimeOffsetHelper.ToDateTimeOffset(timeSentString);
            return true;
        }
        timeSent = DateTimeOffset.MinValue;
        return false;
    }

    public static void ThrowIfSendOnly(this FeatureConfigurationContext context)
    {
        var isSendOnly = context.Settings.GetOrDefault<bool>("Endpoint.SendOnly");
        if (isSendOnly)
        {
            throw new Exception("Windows performance counters are not supported on send only endpoints.");
        }
    }
}