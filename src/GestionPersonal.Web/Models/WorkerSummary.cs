namespace GestionPersonal.Web.Models;

public sealed record WorkerSummary(int Id, string FullName, string Role, bool Active, bool MustChangePin);
