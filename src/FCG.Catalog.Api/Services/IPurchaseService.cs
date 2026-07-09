using FCG.Catalog.Api.Contracts.Purchases;

namespace FCG.Catalog.Api.Services;

public interface IPurchaseService
{
    Task<PurchaseResponse> RequestPurchaseAsync(
        Guid userId,
        string userEmail,
        Guid gameId,
        CancellationToken cancellationToken = default);
}
