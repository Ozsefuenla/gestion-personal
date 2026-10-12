using System.Collections.Concurrent;
using GestionPersonal.Application.Abstractions;

namespace GestionPersonal.Infrastructure.Security;

public sealed class InMemoryLoginAttemptTracker : ILoginAttemptTracker
{
    private sealed record State(int Failures, DateTimeOffset LockedUntil);

    private readonly ConcurrentDictionary<int, State> _states = new();

    private const int MaxFailures = 5;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    public bool IsLockedOut(int workerId)
    {
        return _states.TryGetValue(workerId, out var state) && state.LockedUntil > DateTimeOffset.UtcNow;
    }

    public void RecordFailure(int workerId)
    {
        var now = DateTimeOffset.UtcNow;

        _states.AddOrUpdate(
            workerId,
            _ => new State(1, DateTimeOffset.MinValue),
            (_, existing) => existing.Failures + 1 >= MaxFailures
                ? new State(0, now.Add(LockDuration))
                : new State(existing.Failures + 1, existing.LockedUntil));
    }

    public void Reset(int workerId)
    {
        _states.TryRemove(workerId, out _);
    }
}
