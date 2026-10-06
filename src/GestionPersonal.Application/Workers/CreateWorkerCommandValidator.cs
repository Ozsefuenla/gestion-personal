using FluentValidation;

namespace GestionPersonal.Application.Workers;

public sealed class CreateWorkerCommandValidator : AbstractValidator<CreateWorkerCommand>
{
    private static readonly string[] AllowedRoles = ["admin", "worker"];

    public CreateWorkerCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar los 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es obligatorio.")
            .Must(IsAllowedRole).WithMessage("El rol debe ser 'admin' o 'worker'.");
    }

    private static bool IsAllowedRole(string? role) =>
        role is not null && AllowedRoles.Contains(role.Trim().ToLowerInvariant());
}
