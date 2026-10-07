using System.Net.Http.Json;
using GestionPersonal.Web.Models;

namespace GestionPersonal.Web.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<WorkerStatus>> GetWorkersStatusAsync(CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<List<WorkerStatus>>("/api/workers/status", cancellationToken) ?? [];
    }

    public Task StartAsync(Guid workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/start", workerId, cancellationToken);

    public Task PauseAsync(Guid workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/pause", workerId, cancellationToken);

    public Task ResumeAsync(Guid workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/resume", workerId, cancellationToken);

    public Task EndAsync(Guid workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/end", workerId, cancellationToken);

    private async Task PostAsync(string path, Guid workerId, CancellationToken cancellationToken)
    {
        var response = await _http.PostAsJsonAsync(path, new { workerId }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
