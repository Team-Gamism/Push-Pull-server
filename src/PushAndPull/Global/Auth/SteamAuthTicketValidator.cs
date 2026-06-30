using System.Net.Http.Json;
using System.Text.Json;
using Polly.CircuitBreaker;
using Polly.Timeout;
using PushAndPull.Domain.Auth.Exception;
using PushAndPull.Global.Auth.Dto;

namespace PushAndPull.Global.Auth;

public class SteamAuthTicketValidator : IAuthTicketValidator
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly int _appId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SteamAuthTicketValidator(
        HttpClient httpClient,
        IConfiguration configuration
        )
    {
        _httpClient = httpClient;
        var apiKey = configuration["Steam:WebApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("STEAM_API_KEY_REQUIRED");
        _apiKey = apiKey;

        if (!int.TryParse(configuration["Steam:AppId"], out _appId))
            throw new ArgumentException("APPID_REQUIRED");
    }

    public async Task<AuthTicketValidationResult> ValidateAsync(string ticket, CancellationToken ct = default)
    {
        ValidateTicketFormat(ticket);

        try
        {
            var response = await CallSteamApiAsync(ticket, ct);
            var steamResponse = await ParseResponseAsync(response, ct);
            return ValidateAndCreateResult(steamResponse);
        }
        catch (BrokenCircuitException ex)
        {
            throw new SteamCircuitOpenException(ex);
        }
        catch (TimeoutRejectedException ex)
        {
            throw new SteamApiException("STEAM_API_TIMEOUT", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new SteamApiException("FAIL_TO_CONNECT", ex);
        }
        catch (JsonException ex)
        {
            throw new SteamApiException("FAIL_TO_PARSE", ex);
        }
    }

    private static void ValidateTicketFormat(string ticket)
    {
        if (string.IsNullOrWhiteSpace(ticket))
            throw new InvalidTicketException("EMPTY_TICKET");
    }

    private async Task<HttpResponseMessage> CallSteamApiAsync(string ticket, CancellationToken ct)
    {
        var url = BuildSteamApiUrl(ticket);

        var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new SteamApiException(
                $"STATUS_CODE: {response.StatusCode}",
                (int)response.StatusCode
            );
        }

        return response;
    }

    private string BuildSteamApiUrl(string ticket)
    {
        return "https://api.steampowered.com/ISteamUserAuth/AuthenticateUserTicket/v1/?" +
               $"key={Uri.EscapeDataString(_apiKey)}" +
               $"&appid={_appId}" +
               $"&ticket={Uri.EscapeDataString(ticket)}";
    }

    private static async Task<SteamAuthResponse> ParseResponseAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        var steamResponse = await response.Content.ReadFromJsonAsync<SteamAuthResponse>(JsonOptions, ct);

        if (steamResponse?.Response.Params == null)
        {
            var error = steamResponse?.Response.Error;
            if (error != null)
                throw new InvalidTicketException($"STEAM_ERROR [{error.ErrorCode}]: {error.ErrorDesc}");

            throw new SteamApiException("INVALID_RESPONSE");
        }

        return steamResponse;
    }

    private static AuthTicketValidationResult ValidateAndCreateResult(
        SteamAuthResponse steamResponse)
    {
        var param = steamResponse.Response.Params;

        if (param.Result != "OK")
            throw new InvalidTicketException($"FAIL_TO_VALIDATE: {param.Result}");

        if (!ulong.TryParse(param.SteamId, out var steamId))
            throw new SteamApiException("INVALID_STEAM_ID");

        if (!ulong.TryParse(param.OwnerSteamId, out var ownerSteamId))
            throw new SteamApiException("INVALID_OWNER_STEAM_ID");

        var result = new AuthTicketValidationResult(
            steamId,
            ownerSteamId,
            param.VacBanned,
            param.PublisherBanned
        );

        if (result.VacBanned)
            throw new VacBannedException(steamId);

        if (result.PublisherBanned)
            throw new PublisherBannedException(steamId);

        return result;
    }
}
