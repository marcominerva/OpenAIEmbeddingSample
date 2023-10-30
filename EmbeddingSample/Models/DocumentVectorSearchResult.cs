namespace EmbeddingSample.Models;

public class DocumentVectorSearchResult
{
    public Guid Id { get; set; }    // This is the DocumentChunkId.

    public Guid DocumentId { get; set; }

    public int Sequence { get; set; }

    public string Content { get; set; }

    public float CosineDistance { get; set; }
}