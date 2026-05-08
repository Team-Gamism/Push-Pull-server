using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using PushAndPull.Domain.Auth.Config;
using PushAndPull.Domain.Auth.Exception;
using PushAndPull.Global.Auth;
using PushAndPull.Global.Filter;

namespace PushAndPull.Test.Service.Auth;

public class SteamCircuitBreakerTests
{
    private static IConfiguration BuildConfig(int minimumThroughput = 2, int timeoutSeconds = 5)
        => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Steam:WebApiKey"] = "test-key-name",
            ["test-key-name"] = "test-api-key",
            ["Steam:AppId"] = "480",
            ["Steam:CircuitBreaker:FailureRatio"] = "0.5",
            ["Steam:CircuitBreaker:SamplingDurationSeconds"] = "2",
            ["Steam:CircuitBreaker:MinimumThroughput"] = minimumThroughput.ToString(),
            ["Steam:CircuitBreaker:BreakDurationSeconds"] = "1",
            ["Steam:CircuitBreaker:RequestTimeoutSeconds"] = timeoutSeconds.ToString(),
        }).Build();

    private static (IAuthTicketValidator Validator, CountingHandler Handler) BuildValidator(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond,
        IConfiguration? config = null,
        TimeProvider? timeProvider = null)
    {
        var resolvedConfig = config ?? BuildConfig();
        var handler = new CountingHandler(respond);
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(resolvedConfig);
        services.AddLogging();
        if (timeProvider != null)
            services.AddSingleton<TimeProvider>(timeProvider);
        services.AddAuthServices(resolvedConfig);
        services.AddHttpClient<IAuthTicketValidator, SteamAuthTicketValidator>()
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        var provider = services.BuildServiceProvider();
        return (provider.GetRequiredService<IAuthTicketValidator>(), handler);
    }

    private static HttpResponseMessage ServerError()
        => new(HttpStatusCode.InternalServerError);

    public class WhenFailuresBelowThreshold
    {
        [Fact]
        public async Task It_DoesNotOpenCircuit()
        {
            var (validator, handler) = BuildValidator(
                (_, _) => Task.FromResult(ServerError()),
                BuildConfig(minimumThroughput: 3));

            // MinimumThroughput - 1 = 2 failures; CB should NOT open
            for (var i = 0; i < 2; i++)
                await Assert.ThrowsAsync<SteamApiException>(() => validator.ValidateAsync("ticket"));

            // Third call goes to actual handler (circuit closed), not BrokenCircuitException
            await Assert.ThrowsAsync<SteamApiException>(() => validator.ValidateAsync("ticket"));
            Assert.Equal(3, handler.CallCount);
        }
    }

    public class WhenFailureRatioExceeded
    {
        [Fact]
        public async Task It_OpensCircuitAndNextCallThrowsSteamCircuitOpenException()
        {
            var (validator, handler) = BuildValidator(
                (_, _) => Task.FromResult(ServerError()));

            // Reach MinimumThroughput (2) with 100% failure rate
            for (var i = 0; i < 2; i++)
                await Assert.ThrowsAsync<SteamApiException>(() => validator.ValidateAsync("ticket"));

            // Circuit is now open — next call must NOT reach handler
            await Assert.ThrowsAsync<SteamCircuitOpenException>(
                () => validator.ValidateAsync("ticket"));

            Assert.Equal(2, handler.CallCount);
        }
    }

    public class WhenCircuitOpenAndBreakDurationElapsed
    {
        [Fact]
        public async Task It_AllowsProbeRequest()
        {
            var fakeTime = new FakeTimeProvider();
            var (validator, handler) = BuildValidator(
                (_, _) => Task.FromResult(ServerError()),
                timeProvider: fakeTime);

            for (var i = 0; i < 2; i++)
                await Assert.ThrowsAsync<SteamApiException>(() => validator.ValidateAsync("ticket"));

            // Confirm circuit is open
            await Assert.ThrowsAsync<SteamCircuitOpenException>(
                () => validator.ValidateAsync("ticket"));

            // Advance past BreakDuration (1s) without real wall-clock wait
            fakeTime.Advance(TimeSpan.FromSeconds(2));

            // Probe request should reach the handler (still fails, but circuit was half-open)
            await Assert.ThrowsAsync<SteamApiException>(() => validator.ValidateAsync("ticket"));
            Assert.Equal(3, handler.CallCount);
        }
    }

    public class WhenSteamApiHangs
    {
        [Fact]
        public async Task It_TimesOutAndContributesToCircuitBreaker()
        {
            var (validator, handler) = BuildValidator(
                async (_, ct) =>
                {
                    await Task.Delay(Timeout.Infinite, ct);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                },
                BuildConfig(minimumThroughput: 2, timeoutSeconds: 1));

            // Two timeout failures should open the circuit
            for (var i = 0; i < 2; i++)
                await Assert.ThrowsAsync<SteamApiException>(() => validator.ValidateAsync("ticket"));

            await Assert.ThrowsAsync<SteamCircuitOpenException>(
                () => validator.ValidateAsync("ticket"));

            Assert.Equal(2, handler.CallCount);
        }
    }

    public class WhenCircuitOpenExceptionIsThrown
    {
        private readonly CircuitBreakerExceptionFilter _sut = new();

        private static ExceptionContext CreateContext(Exception ex)
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor());
            return new ExceptionContext(actionContext, []) { Exception = ex };
        }

        [Fact]
        public void It_Returns503CommonApiResponse()
        {
            var context = CreateContext(
                new SteamCircuitOpenException(new Exception("circuit open")));

            _sut.OnException(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, result.StatusCode);
            Assert.True(context.ExceptionHandled);
        }

        [Fact]
        public void It_DoesNotHandleOtherExceptions()
        {
            var context = CreateContext(new InvalidOperationException("other"));

            _sut.OnException(context);

            Assert.Null(context.Result);
            Assert.False(context.ExceptionHandled);
        }
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow = DateTimeOffset.UtcNow;
        private long _timestamp = global::System.Diagnostics.Stopwatch.GetTimestamp();

        public override DateTimeOffset GetUtcNow() => _utcNow;
        public override long GetTimestamp() => _timestamp;
        public override long TimestampFrequency => global::System.Diagnostics.Stopwatch.Frequency;

        public void Advance(TimeSpan duration)
        {
            _utcNow = _utcNow.Add(duration);
            _timestamp += (long)(duration.TotalSeconds * global::System.Diagnostics.Stopwatch.Frequency);
        }
    }

    private sealed class CountingHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
        : HttpMessageHandler
    {
        private int _callCount;
        public int CallCount => _callCount;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _callCount);
            return await respond(request, cancellationToken);
        }
    }
}
