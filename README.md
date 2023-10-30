# OpenAI Embeddings Sample
An example that shows how to using embeddings in a .NET application with SQL Server as Vector Database.

The embeddings are stored in a SQL Server database. The repository contains a couple of books with the corresponding embeddings (in Italian):
- The strange case of Dr Jekyll and Mr Hyde by Robert Louis Stevenson
- Treasure Island by Robert Louis Stevenson

To execute the application:
- Create a database in SQL Server
- Execute the [Scripts.sql](https://github.com/marcominerva/OpenAIEmbeddingSample/blob/master/Scripts.sql) file to create and populate the tables that will contain the embeddings
- Open the [Costants.cs](https://github.com/marcominerva/OpenAIEmbeddingSample/blob/master/EmbeddingSample/Constants.cs) file and set the connection string to the database and the other required parameters

Refer to the [EmbeddingService.cs](https://github.com/marcominerva/OpenAIEmbeddingSample/blob/master/EmbeddingSample/EmbeddingService.cs) file to see how document chunking is performed and how embeddings are calculated, stored and retrieved from the database.
