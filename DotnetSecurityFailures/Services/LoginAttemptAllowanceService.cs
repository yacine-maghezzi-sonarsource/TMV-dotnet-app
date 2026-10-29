using System.Collections.Concurrent;

namespace DotnetSecurityFailures.Services;

public class LoginAttemptAllowanceService
{
    private const int MaxAttempts = 25;
    private readonly ConcurrentDictionary<string, int> _attemptCounts = new();

    public bool TryConsumeAttempt(string subject)
    {
        var updatedCount = _attemptCounts.AddOrUpdate(subject, 1, static (_, currentCount) => currentCount + 1);
        return updatedCount <= MaxAttempts;
    }
}
