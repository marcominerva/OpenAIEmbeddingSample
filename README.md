# OpenAI Embeddings Sample

An example that shows how to use [Semantic Kernel](https://github.com/microsoft/semantic-kernel) and [Kernel Memory](https://github.com/microsoft/kernel-memory) to work with embeddings in a .NET application using [SQL Server as Vector Database](https://github.com/kbeaugrand/SemanticKernel.Connectors.Memory.SqlServer).

The embeddings are stored in a SQL Server database and the Vector Search is efficiently performed thanks to COLUMNSTORE indexes.

To execute the application:
- Create a database in SQL Server
- Open the [AppCostants.cs](https://github.com/marcominerva/OpenAIEmbeddingSample/blob/master/EmbeddingSample/AppConstants.cs) file and set the connection string to the database and the other required parameters. This example assumes you're using Azure OpenAI, but you can easily update it to use OpenAI or whatever LLM you want. Take a look to **Kernel** and **KernelMemoryBuilder** configurations in the [Program.cs](https://github.com/marcominerva/OpenAIEmbeddingSample/blob/master/EmbeddingSample/Program.cs) file
- Import some documents into the memory (search for `await kernelMemory.ImportDocumentAsync` in the [Program.cs](https://github.com/marcominerva/OpenAIEmbeddingSample/blob/master/EmbeddingSample/Program.cs) file

Refer to [Program.cs](https://github.com/marcominerva/OpenAIEmbeddingSample/blob/master/EmbeddingSample/Program.cs) to see how document chunking is performed and how embeddings are calculated, stored and retrieved from the database using Kernel Memory.

If you want to see a manual (explicit) approach to embedding and Vector Search using SQL Server, refer to the [manual-approach branch](https://github.com/marcominerva/OpenAIEmbeddingSample/tree/manual-approach).
