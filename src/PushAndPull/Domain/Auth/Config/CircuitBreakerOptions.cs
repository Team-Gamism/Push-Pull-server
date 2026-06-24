namespace PushAndPull.Domain.Auth.Config;

public class CircuitBreakerOptions
{
    public double FailureRatio { get; init; } = 0.5;
    public int SamplingDurationSeconds { get; init; } = 30;
    public int MinimumThroughput { get; init; } = 5;
    public int BreakDurationSeconds { get; init; } = 60;
    public int RequestTimeoutSeconds { get; init; } = 10;
}
