using GestionPersonal.Application.Workers;

namespace GestionPersonal.Tests.Services;

public sealed class LiveTimeTests
{
    private static readonly DateTimeOffset ComputedAt = new(2026, 10, 18, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void InProgress_AddsElapsedToWorked()
    {
        var worker = Build("in-progress", worked: TimeSpan.FromMinutes(30), paused: TimeSpan.FromMinutes(5));

        var (worked, paused) = LiveTime.Compute(worker, ComputedAt.AddSeconds(10));

        Assert.Equal(TimeSpan.FromMinutes(30) + TimeSpan.FromSeconds(10), worked);
        Assert.Equal(TimeSpan.FromMinutes(5), paused);
    }

    [Fact]
    public void Paused_AddsElapsedToPaused()
    {
        var worker = Build("paused", worked: TimeSpan.FromMinutes(30), paused: TimeSpan.FromMinutes(5));

        var (worked, paused) = LiveTime.Compute(worker, ComputedAt.AddSeconds(10));

        Assert.Equal(TimeSpan.FromMinutes(30), worked);
        Assert.Equal(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(10), paused);
    }

    [Theory]
    [InlineData("finished")]
    [InlineData("idle")]
    public void Inactive_DoesNotAdvance(string status)
    {
        var worker = Build(status, worked: TimeSpan.FromMinutes(30), paused: TimeSpan.FromMinutes(5));

        var (worked, paused) = LiveTime.Compute(worker, ComputedAt.AddMinutes(10));

        Assert.Equal(TimeSpan.FromMinutes(30), worked);
        Assert.Equal(TimeSpan.FromMinutes(5), paused);
    }

    [Fact]
    public void NegativeElapsed_DoesNotSubtract()
    {
        var worker = Build("in-progress", worked: TimeSpan.FromMinutes(30), paused: TimeSpan.Zero);

        var (worked, paused) = LiveTime.Compute(worker, ComputedAt.AddSeconds(-5));

        Assert.Equal(TimeSpan.FromMinutes(30), worked);
        Assert.Equal(TimeSpan.Zero, paused);
    }

    private static WorkerStatusResponse Build(string status, TimeSpan worked, TimeSpan paused) =>
        new(
            Id: 1,
            FullName: "Test Worker",
            CurrentStatus: status,
            StartedAt: ComputedAt,
            CurrentPausedAt: status == "paused" ? ComputedAt : null,
            WorkedTime: worked,
            PausedTime: paused,
            DailyHours: TimeSpan.FromHours(8),
            MustChangePin: false,
            Timeline: [],
            ComputedAt: ComputedAt);
}
