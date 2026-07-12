using Helios.Application.Common.Models;
using Helios.Application.Features.Products.Dtos;
using Helios.Application.Features.Products.Requests;

namespace Helios.Application.Common.Interfaces;

/// <summary>
/// Ürün işlemleri için uygulama servis sözleşmesi.
/// Implementasyon Infrastructure katmanında yapılır.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Ürünleri listeler. Opsiyonel metin ile isim, SKU veya marka üzerinden filtreler.
    /// </summary>
    Task<ApiResponse<List<ProductDto>>> GetProductsAsync(string? searchText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni bir ürün oluşturur. Başarılı olunca embedding kuyruğuna mesaj gönderir.
    /// </summary>
    Task<ApiResponse<Guid>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut bir ürünü günceller. Başarılı olunca embedding kuyruğuna mesaj gönderir.
    /// </summary>
    Task<ApiResponse<Guid>> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
}
