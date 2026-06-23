using Axso.PowerPositionReporter.Application.Abstractions;

namespace Axso.PowerPositionReporter.Infrastructure.Time;

/// <summary>
/// Uses the system clock as the source of Europe/London report extract time.
/// </summary>
public sealed class SystemClock : IClock
{
    private static readonly TimeZoneInfo LondonTimeZone = GetLondonTimeZone();

    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemClock"/> class.
    /// </summary>
    /// <param name="timeProvider">Time provider used to read the current UTC time.</param>
    public SystemClock(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public DateTime GetLocalNow()
    {
        return TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), LondonTimeZone).DateTime;
    }

    /// <summary>
    /// Resolves the Europe/London time zone across Windows and non-Windows hosts.
    /// </summary>
    /// <returns>The Europe/London time zone.</returns>
    private static TimeZoneInfo GetLondonTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        }
    }
}
