using ExampleApp.Domain;

namespace ExampleApp.Application;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(string id, CancellationToken ct = default);
}