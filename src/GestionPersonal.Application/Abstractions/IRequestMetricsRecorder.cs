namespace GestionPersonal.Application.Abstractions;

public interface IRequestMetricsRecorder
{
    void Record(RequestMetric requestMetric);
}
