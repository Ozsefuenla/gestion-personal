namespace GestionPersonal.Application.Abstractions;

public interface IEventCollector
{
    void Emit(AppEvent appEvent);
}
