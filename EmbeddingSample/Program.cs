using System.Text;
using EmbeddingSample;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;

var builder = Kernel.CreateBuilder();

builder.Services.AddLogging(builder => builder.AddConsole());
builder.Services.AddAzureOpenAIChatCompletion(Constants.ChatCompletionModel, Constants.Endpoint, Constants.ApiKey);
builder.Services.AddAzureOpenAITextEmbeddingGeneration(Constants.EmbeddingModel, Constants.Endpoint, Constants.ApiKey);

var kernel = builder.Build();

var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var embeddingService = new EmbeddingService(textEmbeddingGenerationService);

//await embeddingService.GenerateChunkedDocumentsAsync();
//await embeddingService.GenerateChunkedDocumentEmbeddingsAsync();

var chat = new ChatHistory("""
            You are an AI assistant that helps people find information.
            You can use only the information provided in this chat to answer questions.
            If you don't know the answer, reply suggesting to refine the question.
            Never answer to questions that are not related to this chat.
            You must answer in the same language of the user's question.
            """);

string question;
do
{
    Console.Write("Question: ");
    question = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(question))
    {
        break;
    }

    var answer = new StringBuilder();

    question = await CreateQuestionAsync(question);
    chat.AddUserMessage(question);

    await foreach (var response in chatCompletionService.GetStreamingChatMessageContentsAsync(chat, new OpenAIPromptExecutionSettings
    {
        MaxTokens = 400,
    }))
    {
        Console.Write(response);
        answer.Append(response);

        await Task.Delay(80);
    }

    Console.WriteLine();

    chat.AddAssistantMessage(answer.ToString());

    Console.WriteLine();

} while (!string.IsNullOrWhiteSpace(question));

async Task<string> CreateQuestionAsync(string question)
{
    // To be sure to keep the context of the chat when generating embeddings, we need to reformulate the question based on previous messages.
    var embeddingQuestion = $"""
        Reformulate the following question taking into account the context of the chat to perform keyword search and embeddings:
        ---
        {question}
        ---
        You must reformulate the question in the same language of the user's question.
        Never add "in this chat", "in the context of this chat", "in the context of our conversation" or something like that in your answer.
        """;

    chat.AddUserMessage(embeddingQuestion);

    var messageContent = await chatCompletionService.GetChatMessageContentAsync(chat);
    chat.AddAssistantMessage(messageContent.Content);

    var embeddings = await embeddingService.GetEmbeddingAsync(embeddingQuestion);

    var prompt = new StringBuilder("Using the following information:\r\n---\r\n");
    foreach (var result in embeddings.Where(r => r.CosineDistance > 0.73).Take(5))
    {
        prompt.AppendLine(result.Content);
        prompt.AppendLine("\r\n---\r\n");
    }

    prompt.AppendLine("Answer the following question:\r\n---\r\n");
    prompt.Append(question);

    var message = prompt.ToString();
    return message;
}

//var question = "Cosa c'è nelle valli della zona?";
//string[] data =
//{
//    "Il centro storico di Taggia è situato nell'immediato entroterra della valle Argentina, mentre l'abitato di Arma è una località balneare. Tra i due centri vi è la zona denominata Levà (il toponimo deriva dalla denominazione romana per indicare un'area rialzata). Il territorio comunale è tuttavia molto esteso, perché coincide con la bassa valle del torrente Argentina, dalla confluenza del torrente Oxentina, presso la località San Giorgio, fino al mare. Si tratta di un ampio settore di entroterra caratterizzato da estese colture - soprattutto oliveti - nella fascia collinare e da estesi boschi nella sua porzione montana, che raggiunge il monte Faudo, massima elevazione del comune con i suoi 1149 metri. Altre vette del territorio il monte Follia (1031 m), il monte Neveia (835 m), il monte Santa Maria (462 m), il monte Giamanassa (405 m).",
//    "La città di Sanremo è stata fondata a ridosso di due dorsali montuose, che si originano nel monte Bignone (1300 m circa) e procedono fino al mare: a est, verso il promontorio di Capo Verde (sormontato dal faro di Capo dell'Arma della Marina Militare), e a ovest fino a Capo Nero. L'originario nucleo abitativo, la Pigna, è adagiato sulle pareti di una collina-promontorio sovrastata dal santuario della Madonna della Costa, e lambito dai torrenti San Francesco e San Romolo. A levante, il torrente San Martino e a ponente il torrente Foce danno il nome alle relative zone est e ovest della città. La prima fascia dell'entroterra, a ridosso della città, è ricca di serre e coltivazioni di fiori, stanti a ricordare il ruolo importante della floricoltura nell'economia cittadina. Alcuni oliveti e resti di fasce (coltivazioni a terrazza), oramai quasi completamente abbandonate, costellano le campagne e le scoscese colline circostanti. Le frazioni dell'entroterra boschivo distano pochi chilometri dal centro della città, tanto che durante l'estate è consuetudine, da parte dei sanremesi, fare le \"gite fuori porta\" tra i boschi di castagno nella frazione di San Romolo, edificata intorno a un prato che da anni è meta di giochi di bimbi, o tra i pascoli della arieggiata vetta del Bignone, da cui è possibile godere della vista da Saint-Tropez ad Albenga, e in giornate terse, fino alla Corsica. La fascia boschiva dell'entroterra è stata recentemente inserita nel parco naturale di San Romolo-Monte Bignone: un'area di circa 700 ettari, ricca di fauna e vegetazione, nella quale si intrecciano fittamente sentieri e mulattiere. I due promontori a est e a ovest di Sanremo ospitano rispettivamente le frazioni di Poggio e di Coldirodi: la prima, nota per rappresentare l'ultimo tratto in salita della Milano-Sanremo, la seconda sede della Pinacoteca Rambaldi. Dal punto di vista amministrativo, la città si estende a est oltre il Capo Verde. Qui si trova la frazione di Bussana, la più popolosa del comune, che è stata edificata ex novo dopo che il terremoto del 1887 distrusse il centro abitato originario, posto sulle colline retrostanti e oggi noto come Bussana Vecchia. Tale diroccato paesino rimase abbandonato fino all'inizio degli anni sessanta del Novecento, quando artisti da tutto il mondo decisero di ripopolarlo, riportando le costruzioni in pietra alla loro struttura originale.",
//    "Bajardo è un paese montano della valle Armea situato, in linea d'aria, 11 km a nord del comune di Ospedaletti, circa 10 km da Sanremo e posizionato su uno dei crinali delle montagne delle Alpi Marittime dell'entroterra all'apice della val Nervia. Il territorio del comune risulta compreso tra i 332 e i 1.627 metri sul livello del mare. L'escursione altimetrica complessiva risulta essere pari a 1.295 metri. Vette del territorio sono il monte Ceppo (1627 m), il monte Oliveto (1510 m), il monte Gavanelle (1447 m), il monte Bignone (1299 m), il monte Alpicella (1238 m), il monte Collettazzo (1230 m), la Punta Lodiro (1085 m), il monte Garbinee (1065 m), il monte Doa (716 m), il monte Campi (702 m). Il territorio è composto principalmente da coltivazioni di olivi, qualità taggiasca di montagna[5], da seminativi ormai incolti, vigne, castagneti e bosco misto. Oggi una parte dei terreni incolti (quelli facilmente irrigabili) è stata adibita a floricoltura. La recente sensibilità nazionale sui prodotti biologici ha impresso nuovo interesse per la coltivazione dell'ulivo da olio. Da qualche anno si sta estendendo la coltivazione della lavanda per la successiva distillazione dell'estratto. Il borgo ha carattere tipicamente rurale, a struttura sia lineare, sia anulare, e presenta molti aspetti ambientali ed architettonici tipici dei villaggi liguri: pietra, stretti vicoli, con alte case collegate tra loro da archi di controspinta. Bajardo è il municipio della provincia di Imperia con la maggiore altitudine: 910 m s.l.m..",
//    "L'odierno centro urbano di Imperia comprende gli abitati di Porto Maurizio e Oneglia (che a sua volta comprende Castelvecchio), storicamente e geograficamente distinti. Oneglia, a levante, è la parte più estesa della città, si estende nella breve piana alluvionale sulla sinistra della foce del torrente Impero, raccogliendosi intorno a piazza Dante, dalla quale si aprono alcune delle principali strade moderne della città. Ha costituito storicamente il centro industriale della città, legata principalmente alla produzione di olio di oliva.[4] Era famosa per la produzione di pasta. Subito a nord di Oneglia si trova il borgo di Castelvecchio di Santa Maria Maggiore. Porto Maurizio, a ponente del torrente Impero (che dà nome alla città), è raccolto su un promontorio proteso nel mare sulla sinistra della foce del torrente Caramagna e si espande sulla cimosa costiera; ha una vocazione prevalentemente residenziale e turistica. È intricata e pittoresca, ricca di caruggi (vicoli), piccole creuze (viottoli) e palazzi di pregio. Il territorio retrostante la città, al centro della Riviera dei Fiori, ha un andamento orografico caratterizzato da brevi valli, poste perpendicolarmente alla costa e uniformemente digradanti, nelle quali si sono sviluppati molti insediamenti che sono riusciti a conservare intatta o quasi la loro struttura originaria. I punto più alto del territorio comunale imperiese si trova 534 m s.l.m. La coltura dell'olivo, introdotta intorno al XII secolo, ha segnato profondamente la storia del territorio imperiese, così come, secoli più tardi, hanno fatto i fiori e il turismo. Olivi coltivati su colline terrazzate (dette in dialetto locale fasce) con i caratteristici muretti a secco sono l'elemento dominante del paesaggio. Classificazione: zona 3 (sismicità bassa), Ordinanza PCM n. 3274 del 20/03/2003",
//    "Badalucco è situato nella media valle Argentina, adiacente alla Rocca di San Nicolò. L'antico borgo medievale, sorto lungo il torrente omonimo, presenta tipiche case in pietra a vista site in stretti vicoli, caruggi e piazzette. Caratteristici sono i due ponti del tardo Medioevo, costruiti con forma a schiena d'asino, situati all'entrata e all'uscita del borgo. Sul versante opposto i terrazzamenti coltivati ad uliveti sono sostituiti, verso l'alto, dai boschi e dai pascoli che ricoprono il monte Faudo (1149 m), già luogo di un'importante frequentazione umana risalente al neolitico. Altra vetta del territorio è il monte Pallarea (1076 m), al confine amministrativo con Montalto Carpasio.",
//    "Il territorio comunale di Montalto Carpasio, nato dall'unione dei due precedenti e distinti enti comunali, è ubicato geograficamente tra la media valle Argentina, zona a meridione intorno a Montalto Ligure, e l'alta valle del torrente Carpasina, a ridosso delle valli del Maro, di Prelà e di Rezzo, verso la parte settentrionale (Carpasio e località viciniore). Tra le vette del territorio montaltese e carpasino il monte Grande (1418 m), il monte Carpasina (1385 m), il Croce Alpe di Baudo (1270 m), il monte Moro (1184 m), il Poggio Amandolini (1137 m), il monte Arbozzaro (1129 m), il monte Albaspino (1099 m), il monte le Ciazze (1088 m), il Pizzo dei Grossi (1081 m), il monte delle More (1080 m), il monte Pallarea (1076 m), il passo del Maro (1064 m), il passo di Carpasio (1023 m), il monte Crocetta (1050 m), il Rocca Castè (970 m), il passo di Vena (969 m), il monte Colletto (738 m).",
//    "Il territorio comunale di Triora - il più esteso della provincia imperiese - è situato quasi interamente nella valle Argentina e in minima parte nella vallata del torrente Tanarello (ramo sorgentifero del fiume del Tanaro), nel cui bacino sorge la frazione di Monesi di Triora. Il centro principale (Triora) sorge a 780 m s.l.m. sulle estreme pendici meridionali di un costone montuoso che digrada dal massiccio del Saccarello (2201 m), verso la stretta conca di fondovalle percorsa dal torrente Argentina. Tra le vette del territorio triorese il monte Fronté (2151 m), la Cima Garlenda (2141 m), la Punta di Santa Maria (2138 m), il monte Cimonasso (2085 m), il monte Grai (2013 m), la Cima dell'Ortica (1840 m), la Rocca Rossa (1804 m), il monte Collardente (1777 m), il Carmo Ciaberta (1768 m), il monte Gerbonte (1768 m), la Rocca Barbone (1627 m), il Carmo Gerbontina (1582 m), il monte Giaire (1525 m), la Cima Ubago di Medan (1500 m), la Rocca Penna (1471 m), il monte Croce dei Campi (1425 m), la Rocca Goina (1425 m), la Testa delle Collette (1422 m), il Carmo delle Strade (1402 m), la Cima Bareghi (1390 m), il Colle Belenda (1382 m), il monte Grimperto (1369 m), il monte Croce Castagna (1343 m), il Carmo Binelli (1329 m), il monte Croce di Cetta (1311 m), la Rocca Mea (1307 m), la Rocca Castellaccio (1275 m), il monte Gorda (1268 m), il Bric dei Corvi (1260 m), il Carmo Langan (1204 m), la Cima del Corvo (1185 m), il monte Trono (1182 m). Nel comune è compreso quasi interamente il lago di Tenarda, che è artificiale."
//};

//var questionEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(question);

//var dataEmbedding = new List<ReadOnlyMemory<float>>();
//foreach (var text in data)
//{
//    var embedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(text);
//    dataEmbedding.Add(embedding);
//}

//var similarity = dataEmbedding.Select(x => CosineSimilarity(questionEmbedding.Span, x.Span)).ToArray();
//similarity.AsSpan().Sort(data.AsSpan(), (a, b) => b.CompareTo(a));

//for (var i = 0; i < data.Length; i++)
//{
//    Console.WriteLine($"{similarity[i]} - {data[i][..100]}");
//}

////var similarity = CosineSimilarity(embedding.Span, embedding.Span);
////Console.WriteLine(similarity);

//Console.ReadLine();

//static float CosineSimilarity(ReadOnlySpan<float> x, ReadOnlySpan<float> y)
//{
//    float dot = 0, xSumSquared = 0, ySumSquared = 0;

//    for (var i = 0; i < x.Length; i++)
//    {
//        dot += x[i] * y[i];
//        xSumSquared += x[i] * x[i];
//        ySumSquared += y[i] * y[i];
//    }

//    return dot / (MathF.Sqrt(xSumSquared) * MathF.Sqrt(ySumSquared));
//}
