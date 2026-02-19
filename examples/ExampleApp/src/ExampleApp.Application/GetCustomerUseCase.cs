using ExampleApp.Domain;

namespace ExampleApp.Application;

public sealed class GetCustomerUseCase
{
    private readonly ICustomerRepository _repo;
    private readonly IClock _clock;

    public GetCustomerUseCase(ICustomerRepository repo, IClock clock)
    {
        _repo = repo;
        _clock = clock;
    }

    public Task<Customer?> ExecuteAsync(string id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);
}