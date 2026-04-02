using System.Net.Http.Json;
using System.Text.Json;
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
        var apiKeySecretName = configuration["Steam:WebApiKey"]
                             ?? throw new ArgumentException("STEAM_API_KEY_SECRET_NAME_REQUIRED");
        _apiKey = configuration[apiKeySecretName]
                  ?? throw new ArgumentException("STEAM_API_KEY_NOT_FOUND_IN_KEY_VAULT");

        if (!int.TryParse(configuration["Steam:AppId"], out _appId))
            throw new ArgumentException("APPID_REQUIRED");
    }

    public async Task<AuthTicketValidationResult> ValidateAsync(string ticket)
    {
        ValidateTicketFormat(ticket);

        try
        {
            var response = await CallSteamApiAsync(ticket);
            var steamResponse = await ParseResponseAsync(response);
            return ValidateAndCreateResult(steamResponse);
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

    private async Task<HttpResponseMessage> CallSteamApiAsync(string ticket)
    {
        var url = BuildSteamApiUrl(ticket);

        var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

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
        HttpResponseMessage response)
    {
        var steamResponse = await response.Content.ReadFromJsonAsync<SteamAuthResponse>(JsonOptions);

        if (steamResponse?.Response.Params == null)
        {
            var error = steamResponse?.Response.Error;
            throw new SteamApiException(error != null
                ? $"STEAM_ERROR [{error.ErrorCode}]: {error.ErrorDesc}"
                : "INVALID_RESPONSE");
        }

        return steamResponse;
    }

    private static AuthTicketValidationResult ValidateAndCreateResult(
        SteamAuthResponse steamResponse)
    {
        var param = steamResponse.Response.Params!;

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
