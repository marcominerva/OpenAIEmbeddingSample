# OpenAI Embeddings Sample

An example that shows how to use [Semantic Kernel](https://github.com/microsoft/semantic-kernel) and [Kernel Memory](https://github.com/microsoft/kernel-memory) to work with embeddings in a .NET application using [SQL Server as Vector Database](https://github.com/kbeaugrand/SemanticKernel.Connectors.Memory.SqlServer).

The embeddings are stored in a SQL Server database and the Vector Search is efficiently performed thanks to COLUMNSTORE indexes.

To execute the application:
- Create a database in SQL Server
- Open the [AppCostants.cs](https://github.com/marcominerva/OpenAIEmbeddingSample/blob/master/EmbeddingSample/AppConstants.cs) file and set the connection string to the database and the other required parameters
- Import some documents in the memory (search for `await kernelMemory.ImportDocumentAsync` in the [Program.cs](https://github.com/marcominerva/OpenAIEmbeddingSample/blob/master/Program.cs) file

Refer to the [Program.cs](https://github.com/marcominerva/OpenAIEmbeddingSample/blob/master/Program.cs) file to see how document chunking is performed and how embeddings are calculated, stored and retrieved from the database using Kernel Memory.
