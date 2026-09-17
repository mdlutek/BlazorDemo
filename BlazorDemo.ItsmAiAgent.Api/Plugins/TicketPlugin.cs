#pragma warning disable CS0618 // Wyciszenie ostrzeżenia o przestarzałym interfejsie Semantic Kernel

using System.ComponentModel;
using BlazorDemo.ItsmAiAgent.Api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace BlazorDemo.ItsmAiAgent.Api.Plugins;

public class TicketPlugin
{
    private readonly Microsoft.Azure.Cosmos.Container _container;
    private readonly ITextEmbeddingGenerationService _embeddingService;

    public TicketPlugin(CosmosClient cosmosClient, IConfiguration config, ITextEmbeddingGenerationService embeddingService)
    {
        _container = cosmosClient.GetContainer(config["CosmosDb:DatabaseName"], config["CosmosDb:ContainerName"]);
        _embeddingService = embeddingService;
    }

    // --- 1. RAG (Wyszukiwanie wektorowe w historii) ---
    [KernelFunction, Description("Przeszukuje historię rozwiązanych problemów IT (baza wiedzy/tickety), aby znaleźć gotowe rozwiązanie.")]
    public async Task<string> SearchResolvedIssuesAsync(
        [Description("Krótki opis problemu, np. 'błąd 800 vpn'")] string problemQuery)
    {
        // Generujemy wektor dla zapytania użytkownika
        var embedding = await _embeddingService.GenerateEmbeddingAsync(problemQuery);

        // Natywne zapytanie wektorowe w Azure Cosmos DB
        var sql = @"
            SELECT TOP 2 c.id, c.title, c.description, c.resolution, VectorDistance(c.embedding, @vector) AS Score 
            FROM c 
            WHERE c.status = 'Resolved' 
            ORDER BY VectorDistance(c.embedding, @vector)";

        var queryDef = new QueryDefinition(sql)
            .WithParameter("@vector", embedding.ToArray());

        var iterator = _container.GetItemQueryIterator<dynamic>(queryDef);
        var results = new List<string>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var item in response)
            {
                results.Add($"[Ticket ID: {item.id}] Tytuł: {item.title} | Rozwiązanie: {item.resolution}");
            }
        }

        if (results.Count == 0)
            return "Nie znaleziono żadnych podobnych rozwiązanych problemów w bazie wiedzy.";

        return string.Join("\n---\n", results);
    }

    // --- 2. TOOL CALLING (Wykonanie akcji w systemie) ---
    [KernelFunction, Description("Tworzy nowe zgłoszenie (ticket) w systemie ITSM, gdy nie udało się pomóc użytkownikowi.")]
    public async Task<string> CreateTicketAsync(
        [Description("Krótki tytuł zgłoszenia")] string title,
        [Description("Szczegółowy opis problemu użytkownika")] string description,
        [Description("Kategoria: Network, Software lub Hardware")] string category,
        [Description("Priorytet: Low, Medium, High")] string priority)
    {
        var newTicket = new TicketModel
        {
            Id = $"TICK-{Random.Shared.Next(1000, 9999)}",
            Title = title,
            Description = description,
            Category = category,
            Priority = priority,
            Status = "Open"
        };

        await _container.CreateItemAsync(newTicket, new PartitionKey(newTicket.Category));
        return $"Pomyślnie utworzono nowe zgłoszenie o numerze: {newTicket.Id} z priorytetem {newTicket.Priority}.";
    }
}