using Helios.Application.Common.Interfaces;
using Helios.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Text;
using OllamaSharp;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0050 // TextChunker is experimental
#pragma warning disable SKEXP0070 // Ollama is experimental

namespace Helios.Infrastructure.Services;

public class SemanticKernelEmbeddingService : ITextEmbeddingService
{
    private readonly IEmbeddingGenerationService<string, float> _embeddingGenerator;

    public SemanticKernelEmbeddingService(IConfiguration configuration)
    {
        var endpoint = configuration["OllamaOptions:Endpoint"] ?? "http://localhost:11434";
        var modelId = configuration["OllamaOptions:EmbeddingModel"] ?? "nomic-embed-text";

        #pragma warning disable CS0618 // Temporarily disable just in case OllamaApiClient is also warning or something else is
        var ollamaClient = new OllamaApiClient(endpoint, modelId);
        _embeddingGenerator = ollamaClient.AsEmbeddingGenerationService();
        #pragma warning restore CS0618
    }

    public async Task<List<VectorChunkDto>> GenerateChunksAndEmbeddingsAsync(string text, int maxTokensPerChunk = 512, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<VectorChunkDto>();

        var lines = TextChunker.SplitPlainTextLines(text, maxTokensPerLine: maxTokensPerChunk);
        var paragraphs = TextChunker.SplitPlainTextParagraphs(
            lines, 
            maxTokensPerParagraph: maxTokensPerChunk,
            overlapTokens: maxTokensPerChunk / 10);

        var result = new List<VectorChunkDto>();

        for (int i = 0; i < paragraphs.Count; i++)
        {
            var chunkText = paragraphs[i];
            
            // Generate embedding for the chunk
            var embedding = await _embeddingGenerator.GenerateEmbeddingAsync(chunkText, cancellationToken: cancellationToken);
            
            result.Add(new VectorChunkDto
            {
                ChunkIndex = i,
                ChunkText = chunkText,
                Embedding = embedding.ToArray(),
                TokenCount = chunkText.Length / 4 // Basic approximation, real tokenizer would be better but this is sufficient for metadata
            });
        }

        return result;
    }
}
