using Helios.Application.Common.Interfaces;
using Helios.Application.Common.Models;
using Helios.Application.Common.Utils;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Helios.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ApiResponse<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateProductCommandHandler(IAppDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ApiResponse<Guid>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            return ApiResponse<Guid>.Fail("Product not found.");
        }



        // Generate Slug if Name changed
        if (product.Name != request.Name)
        {
            string slug = SlugHelper.GenerateSlug(request.Name);
            bool slugExists = await _context.Products.AnyAsync(p => p.Slug == slug && p.Id != product.Id, cancellationToken);
            if (slugExists)
            {
                slug = $"{slug}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            product.Slug = slug;
        }

        product.Name = request.Name;
        product.Sku = request.Sku;
        product.Description = request.Description;
        product.SearchText = request.SearchText;

        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new Common.Events.ProductEmbeddingEvent
        {
            ProductId = product.Id,
            Description = product.Description
        }, cancellationToken);

        return ApiResponse<Guid>.Ok(product.Id, "Product updated successfully.");
    }
}
