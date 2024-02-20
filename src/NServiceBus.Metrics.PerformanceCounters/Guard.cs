#nullable enable

using System;
using System.Runtime.CompilerServices;

static class Guard
{
    public static void ThrowIfNegativeOrZero(TimeSpan argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(paramName);
        }
    }
}