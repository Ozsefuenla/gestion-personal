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

    public async Task<WorkerStatus> GetWorkerStatusAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<WorkerStatus>($"/api/workers/{workerId}/status", cancellationToken)
            ?? throw new InvalidOperationException("Respuesta inválida del servidor.");
    }

    public async Task<WorkerStatus> LoginAsync(string pin, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("/api/workers/login", new { pin }, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<WorkerStatus>(cancellationToken)
                ?? throw new InvalidOperationException("Respuesta inválida del servidor.");
        }

        throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
    }

    public async Task ChangePinAsync(Guid workerId, string currentPin, string newPin, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync($"/api/workers/{workerId}/change-pin", new { currentPin, newPin }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
        }
    }

    public Task StartAsync(Guid workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/start", workerId, cancellationToken);

    public Task PauseAsync(Guid workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/pause", workerId, cancellationToken);

    public Task ResumeAsync(Guid workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/resume", workerId, cancellationToken);

    public Task EndAsync(Guid workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/end", workerId, cancellationToken);

    public async Task<QuickClockResult> QuickClockAsync(string pin, string action, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("/api/time-entries/quick-clock", new { pin, action }, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<QuickClockResult>(cancellationToken)
                ?? throw new InvalidOperationException("Respuesta inválida del servidor.");
        }

        throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
    }

    private async Task PostAsync(string path, Guid workerId, CancellationToken cancellationToken)
    {
        var response = await _http.PostAsJsonAsync(path, new { workerId }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ApiProblem>(cancellationToken);

            if (problem?.Errors is { Count: > 0 })
            {
                var first = problem.Errors.Values.SelectMany(v => v).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(first))
                {
                    return first;
                }
            }

            return problem?.Detail ?? problem?.Title ?? "No se pudo realizar el fichaje.";
        }
        catch
        {
            return "No se pudo realizar el fichaje.";
        }
    }

    private sealed record ApiProblem(string? Title, string? Detail, Dictionary<string, string[]>? Errors);
}
