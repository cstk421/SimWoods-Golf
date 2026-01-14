using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SimLock.Common;

/// <summary>
/// Client service for communicating with the SimLock activation server.
/// </summary>
public class ActivationService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl;

    public ActivationService(string serverUrl = "https://activation.neutrocorp.com:8443")
    {
        _serverUrl = serverUrl.TrimEnd('/');

        // Configure HttpClient to accept self-signed certificates (for dev)
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Checks email for available licenses and auto-activates if available.
    /// </summary>
    public async Task<ActivationResult> CheckAndActivateAsync(string email)
    {
        try
        {
            var machineId = MachineIdentifier.GetMachineId();

            var request = new EmailCheckRequest
            {
                Email = email,
                MachineId = machineId
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_serverUrl}/api/check-email", request);

            var result = await response.Content.ReadFromJsonAsync<EmailCheckResponse>();

            return new ActivationResult
            {
                Success = response.IsSuccessStatusCode && (result?.Success ?? false),
                Message = result?.Message ?? "Unknown error",
                MachineId = machineId,
                LicenseKey = result?.LicenseKey,
                RemainingActivations = result?.RemainingActivations,
                ActivationDate = result?.ActivationDate
            };
        }
        catch (HttpRequestException ex)
        {
            return new ActivationResult
            {
                Success = false,
                Message = $"Network error: Unable to reach activation server. {ex.Message}"
            };
        }
        catch (TaskCanceledException)
        {
            return new ActivationResult
            {
                Success = false,
                Message = "Connection timeout: Activation server did not respond."
            };
        }
        catch (Exception ex)
        {
            return new ActivationResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Activates a license on this machine.
    /// </summary>
    public async Task<ActivationResult> ActivateAsync(string email, string licenseKey)
    {
        try
        {
            var machineId = MachineIdentifier.GetMachineId();

            var request = new ActivationRequest
            {
                Email = email,
                LicenseKey = licenseKey,
                MachineId = machineId
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_serverUrl}/api/activate", request);

            var result = await response.Content.ReadFromJsonAsync<ActivationResponse>();

            return new ActivationResult
            {
                Success = response.IsSuccessStatusCode && (result?.Success ?? false),
                Message = result?.Message ?? "Unknown error",
                MachineId = machineId,
                ActivationDate = result?.ActivationDate
            };
        }
        catch (HttpRequestException ex)
        {
            return new ActivationResult
            {
                Success = false,
                Message = $"Network error: Unable to reach activation server. {ex.Message}"
            };
        }
        catch (TaskCanceledException)
        {
            return new ActivationResult
            {
                Success = false,
                Message = "Connection timeout: Activation server did not respond."
            };
        }
        catch (Exception ex)
        {
            return new ActivationResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Deactivates the license on this machine, freeing it for use elsewhere.
    /// </summary>
    public async Task<ActivationResult> DeactivateAsync(string email, string licenseKey, string machineId)
    {
        try
        {
            var request = new ActivationRequest
            {
                Email = email,
                LicenseKey = licenseKey,
                MachineId = machineId
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_serverUrl}/api/deactivate", request);

            var result = await response.Content.ReadFromJsonAsync<ActivationResponse>();

            return new ActivationResult
            {
                Success = response.IsSuccessStatusCode && (result?.Success ?? false),
                Message = result?.Message ?? "Unknown error"
            };
        }
        catch (HttpRequestException ex)
        {
            return new ActivationResult
            {
                Success = false,
                Message = $"Network error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new ActivationResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Checks if the license is currently activated on this machine.
    /// </summary>
    public async Task<ActivationResult> CheckActivationAsync(string email, string licenseKey, string machineId)
    {
        try
        {
            var request = new ActivationRequest
            {
                Email = email,
                LicenseKey = licenseKey,
                MachineId = machineId
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_serverUrl}/api/check", request);

            var result = await response.Content.ReadFromJsonAsync<ActivationResponse>();

            return new ActivationResult
            {
                Success = response.IsSuccessStatusCode,
                IsActivated = result?.IsActivated ?? false,
                Message = result?.Message ?? "Unknown error"
            };
        }
        catch (HttpRequestException)
        {
            // If we can't reach the server, assume offline mode - trust local config
            return new ActivationResult
            {
                Success = true,
                IsActivated = true, // Trust local config when offline
                Message = "Offline mode: Using cached activation status"
            };
        }
        catch (Exception ex)
        {
            return new ActivationResult
            {
                Success = false,
                IsActivated = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Result from activation operations.
/// </summary>
public class ActivationResult
{
    public bool Success { get; set; }
    public bool IsActivated { get; set; }
    public string Message { get; set; } = "";
    public string? MachineId { get; set; }
    public string? LicenseKey { get; set; }
    public string? ActivationDate { get; set; }
    public int? RemainingActivations { get; set; }
}

/// <summary>
/// Request model for email check API calls.
/// </summary>
internal class EmailCheckRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("machine_id")]
    public string MachineId { get; set; } = "";
}

/// <summary>
/// Response model from email check API.
/// </summary>
internal class EmailCheckResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("license_key")]
    public string? LicenseKey { get; set; }

    [JsonPropertyName("remaining_activations")]
    public int? RemainingActivations { get; set; }

    [JsonPropertyName("activation_date")]
    public string? ActivationDate { get; set; }
}

/// <summary>
/// Request model for activation API calls.
/// </summary>
internal class ActivationRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("license_key")]
    public string LicenseKey { get; set; } = "";

    [JsonPropertyName("machine_id")]
    public string MachineId { get; set; } = "";
}

/// <summary>
/// Response model from activation API.
/// </summary>
internal class ActivationResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("is_activated")]
    public bool IsActivated { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("activation_date")]
    public string? ActivationDate { get; set; }
}
