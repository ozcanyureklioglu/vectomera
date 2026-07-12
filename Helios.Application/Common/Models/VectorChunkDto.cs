namespace Helios.Application.Common.Models;

public class VectorChunkDto
{
    public string ChunkText { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public int ChunkIndex { get; set; }
    public int TokenCount { get; set; }
}
