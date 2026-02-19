using ExampleApp.Domain;

namespace ExampleApp.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}