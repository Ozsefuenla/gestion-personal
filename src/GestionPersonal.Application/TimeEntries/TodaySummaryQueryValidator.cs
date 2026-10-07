using FluentValidation;

namespace GestionPersonal.Application.TimeEntries;

public sealed class TodaySummaryQueryValidator : AbstractValidator<TodaySummaryQuery>
{
    public TodaySummaryQueryValidator()
    {
        RuleFor(x => x.WorkerId)
            .NotEmpty().WithMessage("El trabajador es obligatorio.");
    }
}
