using Helios.Application.Common.Interfaces;
using Helios.Application.Common.Models;
using Helios.Application.Common.Utils;
using Helios.Domain.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Helios.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ApiResponse<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateProductCommandHandler(IAppDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Check if brand exists
        var brandExists = await _context.Brands.AnyAsync(b => b.Id == request.BrandId, cancellationToken);
        if (!brandExists)
        {
            return ApiResponse<Guid>.Fail("Brand not found.");
        }

        // Check if category exists
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            return ApiResponse<Guid>.Fail("Category not found.");
        }

        // Generate Slug
        string slug = SlugHelper.GenerateSlug(request.Name);
        
        // Ensure slug is unique
        bool slugExists = await _context.Products.AnyAsync(p => p.Slug == slug, cancellationToken);
        if (slugExists)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

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

        // Attach properties if any
        if (request.PropertyValueIds != null && request.PropertyValueIds.Any())
        {
            foreach (var propertyValueId in request.PropertyValueIds.Distinct())
            {
                var propertyExists = await _context.PropertyValues.AnyAsync(pv => pv.Id == propertyValueId, cancellationToken);
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

        await _publishEndpoint.Publish(new Common.Events.ProductEmbeddingEvent
        {
            ProductId = product.Id,
            Description = product.Description
        }, cancellationToken);

        return ApiResponse<Guid>.Ok(product.Id, "Product created successfully.");
    }
}
