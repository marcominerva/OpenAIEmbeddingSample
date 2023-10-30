using System.Text;
using System.Text.RegularExpressions;
using SharpToken;
using UglyToad.PdfPig;

namespace EmbeddingSample;

public static partial class DocumentUtility
{
    [GeneratedRegex("[\\s]+")]
    private static partial Regex TrimTextRegex();

    public static Task<string> ExtractTextFromPdfAsync(Stream stream)
    {
        var content = new StringBuilder();
        using var document = PdfDocument.Open(stream);

        foreach (var page in document.GetPages())
        {
            var words = string.Join(" ", page.GetWords());
            content.Append(words);

            content.Append(' ');
        }

        var text = TrimTextRegex().Replace(content.ToString(), " ").Trim();
        return Task.FromResult(text);
    }

    public static async Task<int> GetTokenCountAsync(string fileName)
    {
        using var stream = File.OpenRead(fileName);
        var content = await ExtractTextFromPdfAsync(stream);

        var encoding = GptEncoding.GetEncoding("cl100k_base");
        var tokenCount = encoding.Encode(content).Count;

        return tokenCount;
    }
}
