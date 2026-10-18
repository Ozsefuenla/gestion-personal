using FluentValidation;

namespace GestionPersonal.Application.Platform;

public sealed class ResetCenterUserPasswordCommandValidator : AbstractValidator<ResetCenterUserPasswordCommand>
{
    public ResetCenterUserPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(4).WithMessage("La contraseña debe tener al menos 4 caracteres.");
    }
}
