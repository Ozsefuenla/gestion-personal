using FluentValidation;

namespace GestionPersonal.Application.Platform;

public sealed class CreateCenterAdminCommandValidator : AbstractValidator<CreateCenterAdminCommand>
{
    public CreateCenterAdminCommandValidator()
    {
        RuleFor(x => x.CenterId)
            .GreaterThan(0).WithMessage("El centro es obligatorio.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El usuario es obligatorio.")
            .MaximumLength(100).WithMessage("El usuario no puede superar los 100 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(4).WithMessage("La contraseña debe tener al menos 4 caracteres.");
    }
}
