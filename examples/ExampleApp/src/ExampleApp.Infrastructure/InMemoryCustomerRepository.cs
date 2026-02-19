using ExampleApp.Application;
using ExampleApp.Domain;

namespace ExampleApp.Infrastructure;

public sealed class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly Dictionary<string, Customer> _db = new();

    public Task<Customer?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        _db.TryGetValue(id, out var customer);
        return Task.FromResult(customer);
    }
}