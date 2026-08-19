using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BlazorDemo.Functions;

public class DailyTaskCleanUpFunction
{
    private readonly ILogger<DailyTaskCleanUpFunction> _logger;

    public DailyTaskCleanUpFunction(ILogger<DailyTaskCleanUpFunction> logger)
    {
        _logger = logger;
    }

    // CRON: "0 0 0 * * *" = Codziennie o 00:00:00 (o północy czasu UTC)
    [Function("DailyTaskCleanUp")]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("🧹 [Azure Function] Rozpoczynam codzienne czyszczenie bazy zadań: {Time}", DateTime.UtcNow);

        string? connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogError("Brak zmiennej 'SqlConnectionString' w konfiguracji!");
            return;
        }

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string sqlQuery = "DELETE FROM DemoTasks";

            await using var command = new SqlCommand(sqlQuery, connection);
            int rowsDeleted = await command.ExecuteNonQueryAsync();

            _logger.LogInformation("[Azure Function] Czyszczenie zakończone sukcesem! Usunięto {Count} ukończonych zadań.", rowsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wystąpił błąd podczas czyszczenia bazy");
        }
    }
}