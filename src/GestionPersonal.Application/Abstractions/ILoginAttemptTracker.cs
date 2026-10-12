namespace GestionPersonal.Application.Abstractions;

public interface ILoginAttemptTracker
{
    bool IsLockedOut(int workerId);

    void RecordFailure(int workerId);

    void Reset(int workerId);
}
