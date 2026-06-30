using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using PushAndPull.Domain.Auth.Exception;
using PushAndPull.Global.Auth;
using PushAndPull.Global.Auth.Dto;

namespace PushAndPull.Test.Global.Auth;

public class SteamAuthTicketValidatorTests
{
    private const string Ticket = "valid-ticket";
    private const ulong SteamId = 76561198000000001UL;
    private const ulong OwnerSteamId = 76561198000000002UL;

    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Steam:WebApiKey"] = "secret-api-key",
            ["Steam:AppId"] = "480",
        }).Build();

    private static SteamAuthTicketValidator BuildValidator(
        Func<HttpRequestMessage, HttpResponseMessage> respond,
        IConfiguration? config = null)
    {
        var httpClient = new HttpClient(new StubHandler(respond));
        return new SteamAuthTicketValidator(httpClient, config ?? BuildConfig());
    }

    private static HttpResponseMessage Ok(SteamAuthResponseParams @params) =>
        new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(
                new SteamAuthResponse(new SteamAuthResponseData(@params, null)))
        };

    private static SteamAuthResponseParams SuccessParams(
        string result = "OK",
        ulong steamId = SteamId,
        ulong ownerSteamId = SteamId,
        bool vacBanned = false,
        bool publisherBanned = false) =>
        new(result, steamId.ToString(), ownerSteamId.ToString(), vacBanned, publisherBanned);

    public class WhenTheTicketIsBlank
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task It_ThrowsInvalidTicketExceptionWithoutCallingSteam(string ticket)
        {
            var handler = new StubHandler(_ => throw new InvalidOperationException("must not be called"));
            var sut = new SteamAuthTicketValidator(new HttpClient(handler), BuildConfig());

            await Assert.ThrowsAsync<InvalidTicketException>(() => sut.ValidateAsync(ticket));
            Assert.Equal(0, handler.CallCount);
        }
    }

    public class WhenSteamReturnsANonSuccessStatusCode
    {
        [Fact]
        public async Task It_ThrowsSteamApiExceptionCarryingTheStatusCode()
        {
            var sut = BuildValidator(_ => new HttpResponseMessage(HttpStatusCode.BadGateway));

            var ex = await Assert.ThrowsAsync<SteamApiException>(() => sut.ValidateAsync(Ticket));
            Assert.Equal((int)HttpStatusCode.BadGateway, ex.UpstreamStatusCode);
        }
    }

    public class WhenTheResponseContainsAnErrorInsteadOfParams
    {
        [Fact]
        public async Task It_ThrowsInvalidTicketException()
        {
            var sut = BuildValidator(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new SteamAuthResponse(
                    new SteamAuthResponseData(null, new SteamAuthResponseError(101, "Invalid ticket"))))
            });

            await Assert.ThrowsAsync<InvalidTicketException>(() => sut.ValidateAsync(Ticket));
        }
    }

    public class WhenTheResponseHasNeitherParamsNorError
    {
        [Fact]
        public async Task It_ThrowsSteamApiException()
        {
            var sut = BuildValidator(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new SteamAuthResponse(
                    new SteamAuthResponseData(null, null)))
            });

            await Assert.ThrowsAsync<SteamApiException>(() => sut.ValidateAsync(Ticket));
        }
    }

    public class WhenTheResultIsNotOk
    {
        [Fact]
        public async Task It_ThrowsInvalidTicketException()
        {
            var sut = BuildValidator(_ => Ok(SuccessParams(result: "Expired")));

            await Assert.ThrowsAsync<InvalidTicketException>(() => sut.ValidateAsync(Ticket));
        }
    }

    public class WhenTheSteamIdCannotBeParsed
    {
        [Fact]
        public async Task It_ThrowsSteamApiException()
        {
            var sut = BuildValidator(_ => Ok(
                new SteamAuthResponseParams("OK", "not-a-number", OwnerSteamId.ToString(), false, false)));

            await Assert.ThrowsAsync<SteamApiException>(() => sut.ValidateAsync(Ticket));
        }
    }

    public class WhenTheOwnerSteamIdCannotBeParsed
    {
        [Fact]
        public async Task It_ThrowsSteamApiException()
        {
            var sut = BuildValidator(_ => Ok(
                new SteamAuthResponseParams("OK", SteamId.ToString(), "not-a-number", false, false)));

            await Assert.ThrowsAsync<SteamApiException>(() => sut.ValidateAsync(Ticket));
        }
    }

    public class WhenTheUserIsVacBanned
    {
        [Fact]
        public async Task It_ThrowsVacBannedException()
        {
            var sut = BuildValidator(_ => Ok(SuccessParams(vacBanned: true)));

            await Assert.ThrowsAsync<VacBannedException>(() => sut.ValidateAsync(Ticket));
        }
    }

    public class WhenTheUserIsPublisherBanned
    {
        [Fact]
        public async Task It_ThrowsPublisherBannedException()
        {
            var sut = BuildValidator(_ => Ok(SuccessParams(publisherBanned: true)));

            await Assert.ThrowsAsync<PublisherBannedException>(() => sut.ValidateAsync(Ticket));
        }
    }

    public class WhenValidationSucceeds
    {
        [Fact]
        public async Task It_ReturnsTheSteamIds()
        {
            var sut = BuildValidator(_ => Ok(SuccessParams(steamId: SteamId, ownerSteamId: SteamId)));

            var result = await sut.ValidateAsync(Ticket);

            Assert.Equal(SteamId, result.SteamId);
        }

        [Fact]
        public async Task It_FlagsFamilySharingWhenOwnerDiffersFromUser()
        {
            var sut = BuildValidator(_ => Ok(SuccessParams(steamId: SteamId, ownerSteamId: OwnerSteamId)));

            var result = await sut.ValidateAsync(Ticket);

            Assert.True(result.IsFamilySharing);
        }
    }

    public class WhenTheWebApiKeyIsConfigured
    {
        [Fact]
        public void It_UsesTheWebApiKeyValueDirectly()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Steam:WebApiKey"] = "secret-api-key",
                ["Steam:AppId"] = "480",
            }).Build();

            _ = new SteamAuthTicketValidator(
                new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK))), config);
        }
    }

    public class WhenTheConfigurationIsInvalid
    {
        private static SteamAuthTicketValidator Build(IConfiguration config) =>
            new(new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK))), config);

        [Fact]
        public void It_ThrowsWhenTheWebApiKeyIsMissing()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Steam:AppId"] = "480",
            }).Build();

            Assert.Throws<ArgumentException>(() => Build(config));
        }

        [Fact]
        public void It_ThrowsWhenTheAppIdIsNotAnInteger()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Steam:WebApiKey"] = "secret-api-key",
                ["Steam:AppId"] = "not-an-int",
            }).Build();

            Assert.Throws<ArgumentException>(() => Build(config));
        }
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
        : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(respond(request));
        }
    }
}
