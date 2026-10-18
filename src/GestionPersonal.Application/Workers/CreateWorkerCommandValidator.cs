using FluentValidation;

namespace GestionPersonal.Application.Workers;

public sealed class CreateWorkerCommandValidator : AbstractValidator<CreateWorkerCommand>
{
    public CreateWorkerCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar los 200 caracteres.");

        RuleFor(x => x.Pin)
            .NotEmpty().WithMessage("El PIN es obligatorio.")
            .Matches(@"^\d{4}$").WithMessage("El PIN debe tener exactamente 4 dígitos.");

        RuleFor(x => x.CenterId)
            .GreaterThan(0).WithMessage("El centro es obligatorio.");
    }
}
