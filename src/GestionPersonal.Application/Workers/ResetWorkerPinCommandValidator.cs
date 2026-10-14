using FluentValidation;

namespace GestionPersonal.Application.Workers;

public sealed class ResetWorkerPinCommandValidator : AbstractValidator<ResetWorkerPinCommand>
{
    public ResetWorkerPinCommandValidator()
    {
        RuleFor(x => x.Pin)
            .NotEmpty().WithMessage("El PIN es obligatorio.")
            .Matches(@"^\d{4}$").WithMessage("El PIN debe tener exactamente 4 dígitos.");
    }
}
