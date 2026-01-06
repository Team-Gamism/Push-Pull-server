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
    
    public SteamAuthTicketValidator(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Steam:WebApiKey"] 
                  ?? throw new ArgumentException("REQUIRED_APIKEY");
        _appId = int.Parse(configuration["Steam:AppId"]
                           ?? throw new ArgumentException("REQUIRED_APPID"));
    }
    
    public async Task<AuthTicketValidationResult> ValidateAsync(string ticket)
    {
        if (string.IsNullOrEmpty(ticket))
            throw new InvalidTicketException("EMPTY_TICKET");

        try
        {
            var url = $"https://api.example.com" +
                      $"?key={Uri.EscapeDataString(_apiKey)}" +
                      $"&appid={_appId}" +
                      $"&ticket={Uri.EscapeDataString(ticket)}";
        
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new SteamApiException(
                    $"STATUS_CODE: {response.StatusCode}",
                    (int)response.StatusCode
                );
        
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        
            var json = await response.Content.ReadAsStringAsync();

            var steamResponse =
                JsonSerializer.Deserialize<SteamAuthResponse>(json, options);

            var param = steamResponse?.Response.Params;
            if (param == null)
                throw new SteamApiException("INVAILD_RESPONSE");
            
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
        catch (HttpRequestException ex)
        {
            throw new SteamApiException("FAIL_TO_CONNECT", ex);
        }
        catch (JsonException ex)
        {
            throw new SteamApiException("FAIL_TO_PARSE", ex);
        }
    }
}