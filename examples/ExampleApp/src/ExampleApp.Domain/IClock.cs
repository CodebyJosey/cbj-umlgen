namespace ExampleApp.Domain;

public interface IClock
{
    DateTime UtcNow { get; }
}