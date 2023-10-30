using System.Globalization;
using Dapper;
using EmbeddingSample.Models;
using Microsoft.Data.SqlClient;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Text;
using TinyHelpers.Extensions;

namespace EmbeddingSample;

public class EmbeddingService
{
    private readonly ITextEmbeddingGeneration textEmbeddingGeneration;

    public EmbeddingService(ITextEmbeddingGeneration textEmbeddingGeneration)
    {
        this.textEmbeddingGeneration = textEmbeddingGeneration;
    }

    public async Task GenerateChunkedDocumentsAsync()
    {
        using var sqlConnection = new SqlConnection(Constants.ConnectionString);

        await sqlConnection.ExecuteAsync("DELETE FROM DocumentChunkEmbeddings");
        await sqlConnection.ExecuteAsync("DELETE FROM DocumentChunks");

        var documents = await sqlConnection.QueryAsync<Document>("SELECT * FROM Documents");

        foreach (var document in documents)
        {
            var paragraphs = TextChunker.SplitPlainTextParagraphs(TextChunker.SplitPlainTextLines(document.Content, 128), 1024, 128,
                $"{document.Title} by {document.Author}\r\n\r\n");

            foreach (var (paragraph, index) in paragraphs.WithIndex())
            {
                await sqlConnection.ExecuteAsync("INSERT INTO DocumentChunks(DocumentId, Sequence, Content) VALUES(@documentId, @sequence, @content)",
                    new
                    {
                        DocumentId = document.Id,
                        Sequence = index,
                        Content = paragraph
                    });
            }
        }
    }

    public async Task GenerateChunkedDocumentEmbeddingsAsync()
    {
        using var sqlConnection = new SqlConnection(Constants.ConnectionString);

        await sqlConnection.ExecuteAsync("DELETE FROM DocumentChunkEmbeddings");

        var documentChunks = await sqlConnection.QueryAsync<DocumentChunk>("SELECT * FROM DocumentChunks ORDER BY DocumentId, Sequence");

        foreach (var documentChunk in documentChunks)
        {
            var embedding = await textEmbeddingGeneration.GenerateEmbeddingAsync(documentChunk.Content);

            for (var i = 0; i < embedding.Length; i++)
            {
                await sqlConnection.ExecuteAsync(
                    "INSERT INTO DocumentChunkEmbeddings(DocumentChunkId, vector_value_id, vector_value) VALUES(@documentChunkId, @vectorId, @vectorValue)",
                    new
                    {
                        DocumentChunkId = documentChunk.Id,
                        VectorId = i,
                        VectorValue = embedding.Span[i]
                    });
            }
        }
    }

    public async Task<IEnumerable<DocumentVectorSearchResult>> GetEmbeddingAsync(string question)
    {
        var embedding = await textEmbeddingGeneration.GenerateEmbeddingAsync(question);

        using var sqlConnection = new SqlConnection(Constants.ConnectionString);

        var embeddingString = $"[{string.Join(",", embedding.ToArray().Select(v => v.ToString(CultureInfo.InvariantCulture)))}]";

        // Uses a SQL Server function to calculate the cosine distance between the question and each document chunk.
        // Thanks to a COLUMNSTORE index on the DocumentChunkEmbeddings table, this query is very fast.
        var results = await sqlConnection.QueryAsync<DocumentVectorSearchResult>(
            "SELECT * FROM SimilarDocumentChunks(@embeddings) ORDER BY CosineDistance DESC",
            new
            {
                Embeddings = embeddingString
            });

        return results;
    }
}
