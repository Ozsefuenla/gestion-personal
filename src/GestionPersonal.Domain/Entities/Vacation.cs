using GestionPersonal.Domain.Enums;

namespace GestionPersonal.Domain.Entities;

public sealed class Vacation
{
    public Guid Id { get; private set; }
    public int WorkerId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public VacationStatus Status { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? DecidedAt { get; private set; }

    private Vacation()
    {
    }

    public static Vacation Request(int workerId, DateOnly startDate, DateOnly endDate, DateTimeOffset now)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("La fecha de fin no puede ser anterior a la de inicio.", nameof(endDate));
        }

        return new Vacation
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            StartDate = startDate,
            EndDate = endDate,
            Status = VacationStatus.Pending,
            RequestedAt = now
        };
    }

    public void Approve(DateTimeOffset now)
    {
        EnsurePending();
        Status = VacationStatus.Approved;
        DecidedAt = now;
    }

    public void Reject(DateTimeOffset now)
    {
        EnsurePending();
        Status = VacationStatus.Rejected;
        DecidedAt = now;
    }

    private void EnsurePending()
    {
        if (Status != VacationStatus.Pending)
        {
            throw new InvalidOperationException("Solo se puede decidir sobre una solicitud pendiente.");
        }
    }
}
