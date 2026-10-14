using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestionPersonal.Web.Models;

namespace GestionPersonal.Web.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;
    private readonly CenterTokenStore _tokenStore;

    public ApiClient(HttpClient http, CenterTokenStore tokenStore)
    {
        _http = http;
        _tokenStore = tokenStore;
    }

    public async Task<IReadOnlyList<CenterSummary>> GetCentersAsync(CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/centers", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CenterSummary>>(cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<WorkerSummary>> ListCenterWorkersAsync(int centerId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, $"/api/centers/{centerId}/workers", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<WorkerSummary>>(cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<WorkerStatus>> GetWorkersStatusAsync(CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/workers/status", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<WorkerStatus>>(cancellationToken) ?? [];
    }

    public async Task<WorkerStatus> GetWorkerStatusAsync(int workerId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, $"/api/workers/{workerId}/status", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<WorkerStatus>(cancellationToken)
            ?? throw new InvalidOperationException("Respuesta inválida del servidor.");
    }

    public async Task<MonthlySummary> GetMonthlySummaryAsync(int workerId, int year, int month, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, $"/api/time-entries/monthly?workerId={workerId}&year={year}&month={month}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MonthlySummary>(cancellationToken)
            ?? throw new InvalidOperationException("Respuesta inválida del servidor.");
    }

    public async Task<WorkerStatus> LoginAsync(int workerId, string pin, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, "/api/workers/login", new { workerId, pin }, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<WorkerStatus>(cancellationToken)
                ?? throw new InvalidOperationException("Respuesta inválida del servidor.");
        }

        throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
    }

    public async Task<CenterSessionResponse> LoginCenterAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, "/api/centers/login", new { username, password }, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CenterSessionResponse>(cancellationToken)
                ?? throw new InvalidOperationException("Respuesta inválida del servidor.");
        }

        throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
    }

    public async Task ChangePinAsync(int workerId, string currentPin, string newPin, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, $"/api/workers/{workerId}/change-pin", new { currentPin, newPin }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
        }
    }

    public async Task EditActiveAsync(int workerId, DateTimeOffset start, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Patch, $"/api/time-entries/active/{workerId}", new { start }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
        }
    }

    public async Task DeleteActiveAsync(int workerId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Delete, $"/api/time-entries/active/{workerId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
        }
    }

    public Task StartAsync(int workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/start", workerId, cancellationToken);

    public Task PauseAsync(int workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/pause", workerId, cancellationToken);

    public Task ResumeAsync(int workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/resume", workerId, cancellationToken);

    public Task EndAsync(int workerId, CancellationToken cancellationToken = default) =>
        PostAsync("/api/time-entries/end", workerId, cancellationToken);

    public async Task<QuickClockResult> QuickClockAsync(int workerId, string pin, string action, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, "/api/time-entries/quick-clock", new { workerId, pin, action }, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<QuickClockResult>(cancellationToken)
                ?? throw new InvalidOperationException("Respuesta inválida del servidor.");
        }

        throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
    }

    public async Task<IReadOnlyList<VacationSummary>> GetVacationsAsync(int workerId, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, $"/api/vacations?workerId={workerId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<VacationSummary>>(cancellationToken) ?? [];
    }

    public async Task RequestVacationAsync(int workerId, DateOnly startDate, DateOnly endDate, string type, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Post, "/api/vacations", new { workerId, startDate, endDate, type }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
        }
    }

    public async Task DeleteVacationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Delete, $"/api/vacations/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
        }
    }

    public async Task<IReadOnlyList<VacationSummary>> GetCenterVacationsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, "/api/vacations/center", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<VacationSummary>>(cancellationToken) ?? [];
    }

    public async Task ApproveVacationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Patch, $"/api/vacations/{id}/approve", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
        }
    }

    public async Task RejectVacationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Patch, $"/api/vacations/{id}/reject", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await ReadErrorMessageAsync(response, cancellationToken));
        }
    }

    private async Task PostAsync(string path, int workerId, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Post, path, new { workerId }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, CancellationToken cancellationToken) =>
        SendAsync(method, path, null, cancellationToken);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        var token = _tokenStore.Token;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _http.SendAsync(request, cancellationToken);
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

            return problem?.Detail ?? problem?.Title ?? "No se pudo completar la operación.";
        }
        catch
        {
            return "No se pudo completar la operación.";
        }
    }

    private sealed record ApiProblem(string? Title, string? Detail, Dictionary<string, string[]>? Errors);
}
