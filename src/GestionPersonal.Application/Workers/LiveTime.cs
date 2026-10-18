namespace GestionPersonal.Application.Workers;

public static class LiveTime
{
    public static (TimeSpan Worked, TimeSpan Paused) Compute(
        WorkerStatusResponse worker,
        DateTimeOffset utcNow)
    {
        var elapsed = utcNow - worker.ComputedAt;

        if (elapsed <= TimeSpan.Zero)
        {
            return (worker.WorkedTime, worker.PausedTime);
        }

        return worker.CurrentStatus switch
        {
            "in-progress" => (worker.WorkedTime + elapsed, worker.PausedTime),
            "paused" => (worker.WorkedTime, worker.PausedTime + elapsed),
            _ => (worker.WorkedTime, worker.PausedTime)
        };
    }
}
