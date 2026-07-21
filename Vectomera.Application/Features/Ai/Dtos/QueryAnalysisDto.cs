namespace Vectomera.Application.Features.Ai.Dtos;

public class QueryAnalysisDto
{
    public List<string> VectorSearchList { get; set; } = new();
    public List<string> VectorEntity { get; set; } = new();
    public List<string> EntitySearch { get; set; } = new();
}
