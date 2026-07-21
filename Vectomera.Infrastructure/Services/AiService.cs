using Vectomera.Application.Common.Interfaces;
using Vectomera.Application.Common.Models;
using Vectomera.Application.Features.Ai.Dtos;
using Vectomera.Application.Features.Ai.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Vectomera.Domain.Entities;


#pragma warning disable SKEXP0070 // Ollama is experimental

namespace Vectomera.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly IAppDbContext _context;
    private readonly ITextEmbeddingService _embeddingService;
    private readonly IChatCompletionService _chatCompletionService;

    public AiService(
        IAppDbContext context,
        ITextEmbeddingService embeddingService,
        IConfiguration configuration)
    {
        _context = context;
        _embeddingService = embeddingService;

        var endpoint = configuration["OllamaOptions:Endpoint"] ?? "http://localhost:11434";
        var chatModelId = configuration["OllamaOptions:ChatModel"] ?? "gemma4:31b-cloud";

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            modelId: chatModelId,
            apiKey: "ollama", 
            endpoint: new Uri($"{endpoint.TrimEnd('/')}/v1")
        );

        var kernel = builder.Build();
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

    }

    public async Task<ApiResponse<AiAdviceResponse>> GetAdviceAsync(AiAdviceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return ApiResponse<AiAdviceResponse>.Fail("Soru boş olamaz.");

            // 1. Query Analysis Step
            var queryAnalysis = await QueryAnalyser(request.Query, cancellationToken);
            var vectorKeywords = queryAnalysis?.VectorSearchList?.Any() == true ? queryAnalysis.VectorSearchList : new List<string> { };
            var vectorEntities = queryAnalysis?.VectorEntity?.Any() == true ? queryAnalysis.VectorEntity : new List<string> { "ProductVectorChunk", "WarehouseInventoryVectorChunk", "ProductReviewVectorChunk" };
            Guid? categoryId = null;

            // 2. Vector Search Step
            var contextJson = await VectorSearch(vectorKeywords, vectorEntities, categoryId, request.Query, cancellationToken);

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("Sen Vectomera e-ticaret/depo sisteminin akıllı bir asistanısın. Lütfen kullanıcının sorusunu sadece aşağıda verilen 'Bağlam (Context)' bilgilerini kullanarak cevapla.Not kullanıcı türkçe sorarsa türkçe cevap ver, ingilizce sorarsa ingilizce cevap ver. ");

            var prompt = $"Bağlam (Context):\n{contextJson}\n\nKullanıcının Sorusu: {request.Query}";
            chatHistory.AddUserMessage(prompt);

            var chatResult = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

            var response = new AiAdviceResponse
            {
                Answer = chatResult.Content ?? "Üzgünüm, bu soruya cevap üretemedim."
            };

            return ApiResponse<AiAdviceResponse>.Ok(response, "Başarılı");
        }
        catch (Exception ex)
        {
            return ApiResponse<AiAdviceResponse>.Fail($"Bir hata oluştu: {ex.Message}");
        }
    }

    private async Task<string> VectorSearch(List<string> vectorKeywords, List<string> vectorEntities, Guid? categoryId = null, string fallbackQuery = "", CancellationToken cancellationToken = default)
    {
        var searchTerms = vectorKeywords != null && vectorKeywords.Any() ? vectorKeywords : new List<string> { fallbackQuery };
        searchTerms = searchTerms.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        if (!searchTerms.Any()) return "{}";

        var allProducts = new List<ProductVectorChunk>();
        var allInventories = new List<WarehouseInventoryVectorChunk>();
        var allReviews = new List<ProductReviewVectorChunk>();

        foreach (var keyword in searchTerms)
        {
            var queryChunks = await _embeddingService.GenerateChunksAndEmbeddingsAsync(keyword, cancellationToken: cancellationToken);
            var queryEmbedding = queryChunks.FirstOrDefault()?.Embedding;
            if (queryEmbedding == null) continue;

            var queryVector = new Vector(queryEmbedding);

            if (vectorEntities.Contains("ProductVectorChunk"))
            {
                var productQuery = _context.ProductVectorChunks
                    .Include(x => x.Product)
                    .AsQueryable();
                if (categoryId.HasValue)
                    productQuery = productQuery.Where(x => x.Product.CategoryId == categoryId.Value);

                var topProductChunks = await productQuery
                    .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                    .Take(3)
                    .ToListAsync(cancellationToken);

                allProducts.AddRange(topProductChunks);
            }

            if (vectorEntities.Contains("WarehouseInventoryVectorChunk"))
            {
                var inventoryQuery = _context.WarehouseInventoryVectorChunks
                    .Include(x => x.WarehouseInventory)
                        .ThenInclude(wi => wi.Product)
                    .Include(x => x.WarehouseInventory)
                        .ThenInclude(wi => wi.Warehouse)
                    .AsQueryable();
                if (categoryId.HasValue)
                    inventoryQuery = inventoryQuery.Where(x => x.WarehouseInventory.Product.CategoryId == categoryId.Value);

                var topInventoryChunks = await inventoryQuery
                    .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                    .Take(3)
                    .ToListAsync(cancellationToken);

                allInventories.AddRange(topInventoryChunks);
            }

            if (vectorEntities.Contains("ProductReviewVectorChunk"))
            {
                var reviewQuery = _context.ProductReviewVectorChunks
                    .Include(x => x.ProductReview)
                        .ThenInclude(pr => pr.Product)
                    .Include(x => x.ProductReview)
                        .ThenInclude(pr => pr.Warehouse)
                    .AsQueryable();
                if (categoryId.HasValue)
                    reviewQuery = reviewQuery.Where(x => x.ProductReview.Product.CategoryId == categoryId.Value);

                var topReviewChunks = await reviewQuery
                    .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                    .Take(3)
                    .ToListAsync(cancellationToken);

                allReviews.AddRange(topReviewChunks);
            }
        }

        // Deduplicate using ID
        var uniqueProducts = allProducts.GroupBy(x => x.Id).Select(g => g.First()).ToList();
        var uniqueInventories = allInventories.GroupBy(x => x.Id).Select(g => g.First()).ToList();
        var uniqueReviews = allReviews.GroupBy(x => x.Id).Select(g => g.First()).ToList();

        var result = new
        {
            UrunBilgileri = uniqueProducts.Any() ? uniqueProducts.Select(x => new
            {
                Urun = x.Product.Name,
                SKU = x.Product.Sku,
                Aciklama = x.ChunkText
            }) : null,
            StokBilgileri = uniqueInventories.Any() ? uniqueInventories.Select(x => new
            {
                Depo = x.WarehouseInventory.Warehouse.Name,
                Sehir = x.WarehouseInventory.Warehouse.CityName,
                Urun = x.WarehouseInventory.Product.Name,
                SKU = x.WarehouseInventory.Product.Sku,
                StokDetayi = x.ChunkText
            }) : null,
            UrunYorumlari = uniqueReviews.Any() ? uniqueReviews.Select(x => new
            {
                Urun = x.ProductReview.Product.Name,
                SKU = x.ProductReview.Product.Sku,
                Depo = x.ProductReview.Warehouse?.Name,
                YorumDetayi = x.ChunkText
            }) : null
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    private async Task<QueryAnalysisDto?> QueryAnalyser(string prompt, CancellationToken cancellationToken = default)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(@"Sen gelişmiş bir Query Analiz (Sorgu Çözümleme) asistanısın. 
Kullanıcının girdiği e-ticaret arama veya soru sorgularını analiz edip JSON formatında dönmelisin.
Sistemdeki Domain Entitileri şunlardır: Brand, Product, Category, Warehouse, WarehouseInventory, ProductReview.
Vektör Arama için mevcut chunk tipleri (VectorEntity): ProductVectorChunk, WarehouseInventoryVectorChunk, ProductReviewVectorChunk.

Görevin:
1. Kullanıcının sorgusunu analiz ederek, arama yapılabilecek alt anlamlı parçalara ayır (vectorSearchList).
2. Bu sorgunun hangi vektör veri tabanlarında (vectorEntity) aranması gerektiğini belirle.
3. Sorgunun hangi domain entitileriyle (entitySearch) doğrudan ilişkili olduğunu belirle.

Not: Kullanıcının sorgusunda istenilen ürünleri aramak için kullanıcıların ürün hakkındaki yorumlarındanda faydalanabilirsin.

Sadece aşağıdaki JSON formatında cevap ver:
{
  ""vectorSearchList"": [""kargo problemi"", ""ürün hatası"", ""stok durumu""],
  ""vectorEntity"": [""ProductReviewVectorChunk"", ""ProductVectorChunk""],
  ""entitySearch"": [""Brand"", ""Product""]
}");

        chatHistory.AddUserMessage(prompt);

        var executionSettings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object> { { "response_mime_type", "application/json" } }
        };

        var result = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, cancellationToken: cancellationToken);

        var responseContent = result.Content ?? string.Empty;

        if (responseContent.StartsWith("```json"))
        {
            responseContent = responseContent.Substring(7);
            if (responseContent.EndsWith("```"))
                responseContent = responseContent.Substring(0, responseContent.Length - 3);
        }
        else if (responseContent.StartsWith("```"))
        {
            responseContent = responseContent.Substring(3);
            if (responseContent.EndsWith("```"))
                responseContent = responseContent.Substring(0, responseContent.Length - 3);
        }

        try
        {
            return JsonSerializer.Deserialize<QueryAnalysisDto>(responseContent.Trim(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }
}

