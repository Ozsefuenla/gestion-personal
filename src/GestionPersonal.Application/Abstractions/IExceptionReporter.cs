namespace GestionPersonal.Application.Abstractions;

public interface IExceptionReporter
{
    void Report(ReportedException reportedException);
}
