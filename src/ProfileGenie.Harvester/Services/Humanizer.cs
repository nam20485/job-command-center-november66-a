namespace ProfileGenie.Harvester.Services;

/// <summary>
/// Provides humanization utilities to make automation appear more natural.
/// </summary>
public class Humanizer
{
    private static readonly Random _random = new();

    /// <summary>
    /// Gets a random delay for human-like timing.
    /// </summary>
    public TimeSpan GetRandomDelay(int minMs = 500, int maxMs = 2000)
    {
        var delay = _random.Next(minMs, maxMs);
        return TimeSpan.FromMilliseconds(delay);
    }

    /// <summary>
    /// Simulates human-like typing delay.
    /// </summary>
    public async Task HumanTypeDelayAsync()
    {
        var delay = GetRandomDelay(50, 200);
        await Task.Delay(delay);
    }
}
