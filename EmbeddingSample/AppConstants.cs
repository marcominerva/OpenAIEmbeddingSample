namespace EmbeddingSample;

internal class AppConstants
{
    public class ChatCompletion
    {
        public const string Endpoint = "https://<my-resource-name>.openai.azure.com";
        public const string ApiKey = "";
        public const string Deployment = "gpt-4";
        public const int MaxTokens = 8_192;   // The max number of tokens supported by the model.
    }

    public class Embedding
    {
        public const string Endpoint = "https://<my-resource-name>.openai.azure.com";
        public const string ApiKey = "";
        public const string Deployment = "text-embedding-ada-002";
        public const int MaxTokens = 8_191;   // The max number of tokens supported by the model.
    }

    public class Memory
    {
        public const string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Embeddings;Integrated Security=True;";

        // If you want to store imported document in a persistent file system storage, set this value and uncomment the corresponding line in Program.cs
        public const string ContentStoragePath = @"";
    }
}
