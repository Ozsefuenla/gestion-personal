using FluentValidation;

namespace GestionPersonal.Application.Platform;

public sealed class CreateCenterUserCommandValidator : AbstractValidator<CreateCenterUserCommand>
{
    public CreateCenterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El usuario es obligatorio.")
            .MaximumLength(100).WithMessage("El usuario no puede superar los 100 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(4).WithMessage("La contraseña debe tener al menos 4 caracteres.");
    }
}
