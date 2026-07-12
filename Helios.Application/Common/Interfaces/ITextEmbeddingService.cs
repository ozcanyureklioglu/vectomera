using Helios.Application.Common.Models;

namespace Helios.Application.Common.Interfaces;

public interface ITextEmbeddingService
{
    Task<List<VectorChunkDto>> GenerateChunksAndEmbeddingsAsync(string text, int maxTokensPerChunk = 512, CancellationToken cancellationToken = default);
}
