using System.Text.Json;
using Server.Application.Port.Output;
using Server.Domain.Exception.Auth;
using Server.Infrastructure.Auth.Dto;

namespace Server.Infrastructure.Auth;

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
        _apiKey = configuration["Steam:WebApiKey"] 
                  ?? throw new ArgumentException("API_KEY_REQUIRED");
        
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
        var json = await response.Content.ReadAsStringAsync();
        
        var steamResponse = JsonSerializer.Deserialize<SteamAuthResponse>(
            json,
            JsonOptions
        );
        
        if (steamResponse?.Response.Params == null)
            throw new SteamApiException("INVALID_RESPONSE");

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