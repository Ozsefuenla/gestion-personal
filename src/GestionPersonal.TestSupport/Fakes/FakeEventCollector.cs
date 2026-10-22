using GestionPersonal.Application.Abstractions;

namespace GestionPersonal.TestSupport.Fakes;

public sealed class FakeEventCollector : IEventCollector, IExceptionReporter, IRequestMetricsRecorder
{
    public List<AppEvent> Events { get; } = [];

    public List<ReportedException> Exceptions { get; } = [];

    public List<RequestMetric> Requests { get; } = [];

    public void Emit(AppEvent appEvent) => Events.Add(appEvent);

    public void Report(ReportedException reportedException) => Exceptions.Add(reportedException);

    public void Record(RequestMetric requestMetric) => Requests.Add(requestMetric);
}
