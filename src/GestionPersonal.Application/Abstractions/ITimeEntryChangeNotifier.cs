namespace GestionPersonal.Application.Abstractions;

public interface ITimeEntryChangeNotifier
{
    void Raise(int centerId);

    IDisposable Subscribe(Action<int> handler);
}
