using FluentValidation;
using Helios.Application.Common.Interfaces;
using Helios.Application.Common.Models;
using Helios.Application.Common.Utils;
using Helios.Application.Features.Products.Dtos;
using Helios.Application.Features.Products.Requests;
using Helios.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Helios.Infrastructure.Services;

/// <summary>
/// IProductService implementasyonu.
/// Veritabanı işlemleri, validation ve mesaj kuyruğu bu sınıfta koordine edilir.
/// </summary>
public class ProductService : IProductService
{
    private readonly IAppDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductService(
        IAppDbContext context,
        IPublishEndpoint publishEndpoint,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<List<ProductDto>>> GetProductsAsync(
        string? searchText,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var lower = searchText.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(lower) ||
                p.Sku.ToLower().Contains(lower) ||
                p.Brand.Name.ToLower().Contains(lower));
        }

        var products = await query
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Sku,
                p.Slug,
                p.Description,
                p.Brand.Name,
                p.Category.Name))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ProductDto>>.Ok(products);
    }

    public async Task<ApiResponse<Guid>> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validation
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return ApiResponse<Guid>.Fail(errors);
        }

        // Marka kontrolü
        var brandExists = await _context.Brands.AnyAsync(b => b.Id == request.BrandId, cancellationToken);
        if (!brandExists)
            return ApiResponse<Guid>.Fail("Marka bulunamadı.");

        // Kategori kontrolü
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            return ApiResponse<Guid>.Fail("Kategori bulunamadı.");

        // Slug üret ve benzersizliği garantile
        var slug = SlugHelper.GenerateSlug(request.Name);
        var slugExists = await _context.Products.AnyAsync(p => p.Slug == slug, cancellationToken);
        if (slugExists)
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";

        var product = new Product
        {
            BrandId = request.BrandId,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Sku = request.Sku,
            Slug = slug,
            Description = request.Description,
            SearchText = request.SearchText
        };

        // Özellik değerleri ekle
        if (request.PropertyValueIds != null && request.PropertyValueIds.Count > 0)
        {
            foreach (var propertyValueId in request.PropertyValueIds.Distinct())
            {
                var propertyExists = await _context.PropertyValues
                    .AnyAsync(pv => pv.Id == propertyValueId, cancellationToken);

                if (propertyExists)
                {
                    product.ProductProperties.Add(new ProductProperty
                    {
                        PropertyValueId = propertyValueId
                    });
                }
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        // Embedding kuyruğuna mesaj gönder
        await _publishEndpoint.Publish(
            new Helios.Application.Common.Events.ProductEmbeddingEvent
            {
                ProductId = product.Id,
                Description = product.Description
            }, cancellationToken);

        return ApiResponse<Guid>.Ok(product.Id, "Ürün başarıyla oluşturuldu.");
    }

    public async Task<ApiResponse<Guid>> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validation
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return ApiResponse<Guid>.Fail(errors);
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
            return ApiResponse<Guid>.Fail("Ürün bulunamadı.");

        // İsim değiştiyse yeni slug üret
        if (product.Name != request.Name)
        {
            var slug = SlugHelper.GenerateSlug(request.Name);
            var slugExists = await _context.Products
                .AnyAsync(p => p.Slug == slug && p.Id != product.Id, cancellationToken);
            if (slugExists)
                slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
            product.Slug = slug;
        }

        product.Name = request.Name;
        product.Sku = request.Sku;
        product.Description = request.Description;
        product.SearchText = request.SearchText;

        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);

        // Embedding kuyruğuna mesaj gönder
        await _publishEndpoint.Publish(
            new Helios.Application.Common.Events.ProductEmbeddingEvent
            {
                ProductId = product.Id,
                Description = product.Description
            }, cancellationToken);

        return ApiResponse<Guid>.Ok(product.Id, "Ürün başarıyla güncellendi.");
    }
}
