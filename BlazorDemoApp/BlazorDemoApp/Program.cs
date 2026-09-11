using BlazorDemoApp.Client.Pages;
using BlazorDemoApp.Components;
using BlazorDemoApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;
using MongoDB.Driver;
using System.Globalization;

// Standardowy "builder pattern" w ASP.NET Core (od .NET 6+ minimal hosting model).
// Zastępuje starą parę Startup.cs + Program.cs znaną z .NET Core 3.1 / .NET 5.
var builder = WebApplication.CreateBuilder(args);

// Ustawienie polskiej kultury dla serwera Linux na Azure
// UWAGA: to ustawia kulturę globalnie dla WĄTKU DOMYŚLNEGO procesu w momencie startu.
// W ASP.NET Core każde żądanie może być obsługiwane przez inny wątek z puli (thread pool),
// więc dla żądań HTTP lepszym / bardziej niezawodnym mechanizmem jest RequestLocalizationMiddleware
// (app.UseRequestLocalization(...)) - on gwarantuje kulturę per-request, niezależnie od wątku.
// To co masz teraz zadziała dla większości przypadków (bo CultureInfo.DefaultThreadCurrentCulture
// faktycznie propaguje się na nowe wątki), ale to raczej "działa w praktyce", a nie w 100% best practice.
var cultureInfo = new CultureInfo("pl-PL");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Rejestracja komponentów Razor + włączenie DWÓCH interaktywnych render modes jednocześnie:
// - Server (SignalR, logika po stronie serwera)
// - WebAssembly (kod .NET kompilowany do WASM i wykonywany w przeglądarce)
// To jest charakterystyczne dla nowego modelu "Blazor Web App" wprowadzonego w .NET 8,
// który pozwala MIESZAĆ render mode per-komponent/per-strona (Auto/Server/WASM).
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// AddDbContextFactory zamiast zwykłego AddDbContext.
// Dobra decyzja w kontekście Blazor Server: DbContext NIE jest thread-safe,
// a komponenty Blazor Server mogą żyć długo (cały czas trwania połączenia SignalR).
// Factory pozwala tworzyć krótkożyjące instancje DbContext na żądanie (np. per operacja),
// zamiast trzymać jeden scoped DbContext przez cały cykl życia komponentu - to częsty
// błąd początkujących (i częste pytanie na rozmowie: "dlaczego DbContext w Blazor Server
// bywa problematyczny?").
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AzureSqlDb")));

// MongoClient jako Singleton - poprawnie. Oficjalna dokumentacja MongoDB rekomenduje
// dokładnie to: MongoClient zarządza wewnętrznie connection poolingiem i powinien
// być jednym, długożyjącym obiektem w całej aplikacji, a nie tworzony za każdym razem.
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
    return new MongoClient(connectionString);
});

// Rejestracja klienta Azure Cosmos DB
// CosmosClient jako Singleton - podobnie jak Mongo, tak zaleca Microsoft:
// CosmosClient jest "expensive to create" i thread-safe, więc nadaje się do współdzielenia.
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("CosmosDb");
    return new CosmosClient(connectionString, new CosmosClientOptions
    {
        // Automatyczna konwersja PascalCase (C#) <-> camelCase (JSON/Cosmos) bez ręcznych
        // atrybutów [JsonProperty] na modelach - wygodne.
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    });
});

// Rejestracja bazy PostgreSQL (Neon Serverless)
// Ponownie AddDbContextFactory - konsekwentnie z podejściem do AppDbContext powyżej. Dobrze.
builder.Services.AddDbContextFactory<PostgresDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb")));

// Rejestracja HttpClient do pobierania danych z zewnętrznych API
// AddHttpClient() bez nazwy/typu rejestruje "domyślny" HttpClient poprzez IHttpClientFactory.
// To zapobiega klasycznemu problemowi "socket exhaustion" (gdy ktoś robi `new HttpClient()`
// za każdym razem). Plusem byłoby użycie NAZWANYCH lub TYPOWANYCH klientów
// (AddHttpClient<IWeatherApiClient, WeatherApiClient>()), jeśli korzystasz z kilku różnych API -
// łatwiej wtedy skonfigurować BaseAddress, nagłówki, Polly retry policy itd. per-klient.
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Debugowanie kodu WASM bezpośrednio w przeglądarce (breakpointy w VS/VS Code) - działa
    // tylko w trybie deweloperskim, stąd warunek.
    app.UseWebAssemblyDebugging();
}
else
{
    // W produkcji: globalny handler wyjątków przekierowujący na stronę /Error,
    // zamiast pokazywać stack trace użytkownikowi.
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // HSTS - wymusza HTTPS w przeglądarce przez zadeklarowany czas (domyślnie 30 dni).
    app.UseHsts();
}
// Obsługa kodów statusu (np. 404) przez re-execute pipeline'u na dedykowaną stronę,
// zamiast zwracać goły kod błędu bez treści.
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Ochrona przed atakami CSRF (Cross-Site Request Forgery) - istotne przy formularzach
// (EditForm) w Blazor.
app.UseAntiforgery();

// Nowe API z .NET 8/9 do serwowania plików statycznych z automatycznym
// fingerprintingiem/cache busting (zastępuje częściowo app.UseStaticFiles()).
app.MapStaticAssets();

// Mapowanie komponentów Razor + włączenie OBU render modes na poziomie hosta,
// oraz dołączenie dodatkowego assembly klienckiego (BlazorDemoApp.Client),
// żeby routing widział też strony/komponenty zdefiniowane w projekcie WASM.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorDemoApp.Client._Imports).Assembly);

app.Run();