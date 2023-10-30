namespace EmbeddingSample.Models;

public class DocumentChunk
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public int Sequence { get; set; }

    public string Content { get; set; }
}