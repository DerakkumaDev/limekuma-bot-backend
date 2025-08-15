using Limekuma.Prober.DivingFish.Enums;
using Limekuma.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish;

public abstract class DfClient
{
    protected HttpClient _httpClient;
    protected JsonSerializerOptions _jsonOptions;

    protected DfClient()
    {
        _httpClient = new()
        {
            BaseAddress = new("https://www.diving-fish.com/"),
            DefaultRequestVersion =  HttpVersion.Version11,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
        };

        _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    protected async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        Optional<T, StatusResponse> response =
            await _httpClient.GetFromJsonAsync<Optional<T, StatusResponse>>(path, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response");
        if (response is StatusResponse status)
        {
            throw new InvalidOperationException(status.Message);
        }

        return (T?)response ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    protected async Task<HttpResponseMessage> PostAsync<TValue>(string path, TValue value,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(path, value, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return response;
    }

    protected async Task<TResult> PostAsync<TValue, TResult>(string path, TValue value,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage responseMessage = await PostAsync(path, value, cancellationToken);
        Optional<TResult, StatusResponse> response =
            await responseMessage.Content.ReadFromJsonAsync<Optional<TResult, StatusResponse>>(_jsonOptions,
                cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response");
        if (response is StatusResponse status)
        {
            throw new InvalidOperationException(status.Message);
        }

        return (TResult?)response ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    protected record StatusResponse
    {
        [JsonPropertyName("status")]
        public ResponseStatus? Status { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }
    }
}