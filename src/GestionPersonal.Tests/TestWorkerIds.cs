using System.Threading;

namespace GestionPersonal.Tests;

public static class TestWorkerIds
{
    private static int _next = 100_000;

    public static int New() => Interlocked.Increment(ref _next);
}
