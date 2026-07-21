using Vectomera.Application.Common.Models;
using Vectomera.Application.Features.Products.Dtos;
using Vectomera.Application.Features.Products.Requests;

namespace Vectomera.Application.Common.Interfaces;

/// <summary>
/// ÃœrÃ¼n iÅŸlemleri iÃ§in uygulama servis sÃ¶zleÅŸmesi.
/// Implementasyon Infrastructure katmanÄ±nda yapÄ±lÄ±r.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// ÃœrÃ¼nleri listeler. Opsiyonel metin ile isim, SKU veya marka Ã¼zerinden filtreler.
    /// </summary>
    Task<ApiResponse<List<ProductDto>>> GetProductsAsync(string? searchText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni bir Ã¼rÃ¼n oluÅŸturur. BaÅŸarÄ±lÄ± olunca embedding kuyruÄŸuna mesaj gÃ¶nderir.
    /// </summary>
    Task<ApiResponse<Guid>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut bir Ã¼rÃ¼nÃ¼ gÃ¼nceller. BaÅŸarÄ±lÄ± olunca embedding kuyruÄŸuna mesaj gÃ¶nderir.
    /// </summary>
    Task<ApiResponse<Guid>> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir Ã¼rÃ¼nÃ¼ siler.
    /// </summary>
    Task<ApiResponse<bool>> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
}
