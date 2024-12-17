using EmbeddingSample;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var kernelMemory = new KernelMemoryBuilder()
    .With(new KernelMemoryConfig
    {
        DataIngestion = new()
        {
            DefaultSteps = [.. Constants.DefaultPipeline, Constants.PipelineStepsDeleteGeneratedFiles]
        }
    })
    .WithAzureOpenAITextEmbeddingGeneration(new()
    {
        APIKey = AppConstants.Embedding.ApiKey,
        Auth = AzureOpenAIConfig.AuthTypes.APIKey,
        Deployment = AppConstants.Embedding.Deployment,
        Endpoint = AppConstants.Embedding.Endpoint,
        APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
        MaxTokenTotal = AppConstants.Embedding.MaxTokens
    })
    .WithAzureOpenAITextGeneration(new()
    {
        APIKey = AppConstants.ChatCompletion.ApiKey,
        Auth = AzureOpenAIConfig.AuthTypes.APIKey,
        Deployment = AppConstants.ChatCompletion.Deployment,
        Endpoint = AppConstants.ChatCompletion.Endpoint,
        APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
        MaxTokenTotal = AppConstants.ChatCompletion.MaxTokens
    })
    .WithSearchClientConfig(new()
    {
        EmptyAnswer = "I'm sorry, I haven't found any relevant information that can be used to answer your question",
        MaxMatchesCount = 25,
        AnswerTokens = 800
    })
    .WithCustomTextPartitioningOptions(new()
    {
        // Defines the properties that are used to split the documents in chunks.
        MaxTokensPerParagraph = 1000,
        MaxTokensPerLine = 300,
        OverlappingTokens = 100
    })
    //.WithSimpleFileStorage(AppConstants.Memory.ContentStoragePath)  // Uncomment to use persistent Content Storage oh file system.    
    .WithSqlServerMemoryDb(AppConstants.Memory.ConnectionString)    // Use SQL Server as Vector Storage for embeddings.
    .Build<MemoryServerless>();

var builder = Kernel.CreateBuilder();

builder.Services.AddLogging(builder => builder.AddConsole());
builder.Services
    .AddAzureOpenAIChatCompletion(AppConstants.ChatCompletion.Deployment, AppConstants.ChatCompletion.Endpoint, AppConstants.ChatCompletion.ApiKey);

var kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

//var tags = new TagCollection
//{
//    { "userId", "42" },
//    { "category", "cities" }
//};

//await kernelMemory.ImportDocumentAsync(@"D:\Taggia.pdf");

//var tokenizer = new GPT4oTokenizer();
//var tokenCount = tokenizer.CountTokens("Oggi è una bella giornata, domani chissà come sara?!");

var chat = new ChatHistory();

string question;
do
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("\n> Question: ");
    Console.ResetColor();

    question = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(question))
    {
        break;
    }

    question = await CreateQuestionAsync(question);

    // Asks using the embedding search via Kernel Memory and the reformulated question.
    //var context = new RequestContext();
    //context.SetArg(Constants.CustomContext.Rag.EmptyAnswer, "I haven't found the answer");

    var answer = await kernelMemory.AskAsync(question);

    if (answer.NoResult == false)
    {
        // The answer has been found. Adds it to the chat so that it can be used to reformulate next questions.
        chat.AddUserMessage(question);
        chat.AddAssistantMessage(answer.Result);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("> Answer: ");
        Console.ResetColor();

        Console.WriteLine(answer.Result);
        Console.WriteLine("Sources:");
        foreach (var source in answer.RelevantSources)
        {
            Console.WriteLine($"- {source.SourceName}");
        }
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(answer.Result);
        Console.ResetColor();
    }

    Console.WriteLine();

} while (!string.IsNullOrWhiteSpace(question));

async Task<string> CreateQuestionAsync(string question)
{
    // To be sure to keep the context of the chat when generating embeddings, we need to reformulate the question based on previous messages.
    var embeddingQuestion = $"""
        Reformulate the following question taking into account the context of the chat to perform embeddings search:
        ---
        {question}
        ---
        You must reformulate the question in the same language of the user's question.
        Never add "in this chat", "in the context of this chat", "in the context of our conversation", "search for" or something like that in your answer.
        """;

    chat.AddUserMessage(embeddingQuestion);

    var reformulatedQuestion = await chatCompletionService.GetChatMessageContentAsync(chat);
    chat.AddAssistantMessage(reformulatedQuestion.Content);

    return reformulatedQuestion.Content;
}
