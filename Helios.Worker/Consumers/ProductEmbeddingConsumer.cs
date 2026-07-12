using Helios.Application.Common.Events;
using Helios.Application.Common.Interfaces;
using Helios.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;

namespace Helios.Worker.Consumers;

public class ProductEmbeddingConsumer : IConsumer<ProductEmbeddingEvent>
{
    private readonly ILogger<ProductEmbeddingConsumer> _logger;
    private readonly ITextEmbeddingService _embeddingService;
    private readonly IAppDbContext _context;

    public ProductEmbeddingConsumer(
        ILogger<ProductEmbeddingConsumer> logger,
        ITextEmbeddingService embeddingService,
        IAppDbContext context)
    {
        _logger = logger;
        _embeddingService = embeddingService;
        _context = context;
    }

    public async Task Consume(ConsumeContext<ProductEmbeddingEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("Processing ProductEmbeddingEvent for ProductId: {ProductId}", message.ProductId);

        if (string.IsNullOrWhiteSpace(message.Description))
        {
            _logger.LogInformation("Description is empty. Removing existing vector chunks if any for ProductId: {ProductId}", message.ProductId);
            
            // If description is cleared, we should remove existing embeddings
            await _context.ProductVectorChunks
                .Where(x => x.ProductId == message.ProductId)
                .ExecuteDeleteAsync(context.CancellationToken);
                
            return;
        }

        // Generate embeddings and chunks
        var chunks = await _embeddingService.GenerateChunksAndEmbeddingsAsync(message.Description, cancellationToken: context.CancellationToken);
        
        if (chunks == null || !chunks.Any())
        {
            _logger.LogWarning("No chunks generated for ProductId: {ProductId}", message.ProductId);
            return;
        }

        // Remove old chunks
        await _context.ProductVectorChunks
            .Where(x => x.ProductId == message.ProductId)
            .ExecuteDeleteAsync(context.CancellationToken);

        // Map and save new chunks
        var vectorChunks = chunks.Select(chunk => new ProductVectorChunk
        {
            ProductId = message.ProductId,
            ChunkText = chunk.ChunkText,
            Embedding = new Vector(chunk.Embedding),
            ChunkIndex = chunk.ChunkIndex,
            TokenCount = chunk.TokenCount
        }).ToList();

        _context.ProductVectorChunks.AddRange(vectorChunks);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Successfully saved {Count} vector chunks for ProductId: {ProductId}", vectorChunks.Count, message.ProductId);
    }
}
