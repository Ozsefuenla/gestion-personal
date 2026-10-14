using FluentValidation;

namespace GestionPersonal.Application.Workers;

public sealed class RenameWorkerCommandValidator : AbstractValidator<RenameWorkerCommand>
{
    public RenameWorkerCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar los 200 caracteres.");
    }
}
