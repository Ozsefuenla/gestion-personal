using FluentValidation;

namespace GestionPersonal.Application.Centers;

public sealed class CenterLoginCommandValidator : AbstractValidator<CenterLoginCommand>
{
    public CenterLoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El usuario del centro es obligatorio.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}
