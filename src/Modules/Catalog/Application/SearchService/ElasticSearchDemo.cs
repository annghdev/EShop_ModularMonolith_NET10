using Elastic.Clients.Elasticsearch;
using Elastic.Transport.Products.Elasticsearch;

namespace Catalog.Application;

public class ElasticSearchDemoService(ElasticsearchClient _client)
{
    private const string IndexName = "demo";

    // ➕ Insert / Index
    public async Task IndexAsync(DemoDocument doc, CancellationToken ct = default)
    {
        var response = await _client.IndexAsync(
            doc,
            i => i
                .Index(IndexName)
                .Id(doc.Id),
            ct
        );

        EnsureSuccess(response);
    }
    // 🔄 Update (partial)
    public async Task UpdateAsync(
        string id,
        object partialDoc,
        CancellationToken ct = default)
    {
        var response = await _client.UpdateAsync<DemoDocument, object>(
            IndexName,
            id,
            u => u.Doc(partialDoc),
            ct
        );

        EnsureSuccess(response);
    }

    // ❌ Delete
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync(
            IndexName,
            id,
            ct
        );

        EnsureSuccess(response);
    }

    // 🔍 Get by Id
    public async Task<DemoDocument?> GetByIdAsync(
        string id,
        CancellationToken ct = default)
    {
        var response = await _client.GetAsync<DemoDocument>(
            IndexName,
            id,
            ct
        );

        return response.Found ? response.Source : null;
    }

    // 🔎 Search (full-text + filter)
    public async Task<IReadOnlyList<DemoDocument>> SearchAsync(
    string keyword,
    decimal? minPrice = null,
    CancellationToken ct = default)
    {
        var response = await _client.SearchAsync<DemoDocument>(s => s
            .Indices(IndexName)
            .Query(q => q
                .Bool(b =>
                {
                    // MUST: match name
                    b.Must(m => m
                        .Match(mm => mm
                            .Field("name")
                            .Query(keyword)
                        )
                    );

                    // FILTER: price range (chỉ add khi có value)
                    if (minPrice.HasValue)
                    {
                        b.Filter(f => f
                            .Range(r => r
                                .Number(n => n
                                    .Field("price")
                                    .Gte((double)minPrice.Value)
                                )
                            )
                        );
                    }
                })
            ),
            ct
        );

        return response.Documents.ToList();
    }

    private static void EnsureSuccess(ElasticsearchResponse response)
    {
        if (!response.IsSuccess())
            throw new InvalidOperationException(response.DebugInformation);
    }
}


public sealed class DemoDocument
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string Category { get; init; } = default!;
    public decimal Price { get; init; }
    public DateTime CreatedAt { get; init; }
}