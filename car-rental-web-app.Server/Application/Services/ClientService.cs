using CarRental.Application.DTOs.Clients;
using CarRental.Application.DTOs.Rentals;
using CarRental.Application.Interfaces;
using CarRental.Application.Mappings;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;

namespace CarRental.Application.Services;

public class ClientService : IClientService
{
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _audit;

    public ClientService(IUnitOfWork uow, IAuditService audit)
    {
        _uow = uow;
        _audit = audit;
    }

    public async Task<IEnumerable<ClientDto>> GetAllAsync(string? search = null, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var results = await _uow.Clients.SearchAsync(search, ct);
            return results.Select(c => c.ToDto());
        }

        var all = await _uow.Clients.GetAllAsync(ct);
        return all.Select(c => c.ToDto());
    }

    public async Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByIdAsync(id, ct);
        return client?.ToDto();
    }

    public async Task<ClientDto> CreateAsync(CreateClientRequest request, CancellationToken ct = default)
    {
        if (await _uow.Clients.EmailExistsAsync(request.Email, null, ct))
            throw new InvalidOperationException($"A client with email '{request.Email}' already exists.");

        var client = Client.Create(request.FullName, request.Email, request.Phone, request.Address);
        await _uow.Clients.AddAsync(client, ct);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(null, "Client", client.Id, "ClientAdded",
            $"Client {client.FullName} ({client.Email}) created.", ct);

        return client.ToDto();
    }

    public async Task<ClientDto> UpdateAsync(int id, UpdateClientRequest request, CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Client {id} not found.");

        if (await _uow.Clients.EmailExistsAsync(request.Email, id, ct))
            throw new InvalidOperationException($"Email '{request.Email}' is already used by another client.");

        client.SetFullName(request.FullName);
        client.SetEmail(request.Email);
        client.SetPhone(request.Phone);
        client.SetAddress(request.Address);
        client.SetIsActive(request.IsActive);

        _uow.Clients.Update(client);
        await _uow.SaveChangesAsync(ct);

        return client.ToDto();
    }

    public async Task FlagAsync(int id, FlagClientRequest request, int userId, CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Client {id} not found.");

        client.Flag(request.Reason);
        _uow.Clients.Update(client);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(userId, "Client", id, "ClientFlagged",
            $"Client {client.FullName} flagged. Reason: {request.Reason}", ct);
    }

    public async Task UnflagAsync(int id, int userId, CancellationToken ct = default)
    {
        var client = await _uow.Clients.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Client {id} not found.");

        client.Unflag();
        _uow.Clients.Update(client);
        await _uow.SaveChangesAsync(ct);

        await _audit.LogAsync(userId, "Client", id, "ClientUnflagged",
            $"Client {client.FullName} unflagged.", ct);
    }

    public async Task<IEnumerable<RentalListItemDto>> GetRentalHistoryAsync(int clientId, CancellationToken ct = default)
    {
        var _ = await _uow.Clients.GetByIdAsync(clientId, ct)
            ?? throw new KeyNotFoundException($"Client {clientId} not found.");

        var rentals = await _uow.Rentals.GetByClientAsync(clientId, ct);
        return rentals.OrderByDescending(r => r.StartDate).Select(r => r.ToListDto());
    }
}
