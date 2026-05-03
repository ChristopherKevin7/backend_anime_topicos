using AnimeGraphApi.Config;
using AnimeGraphApi.Repositories;
using AnimeGraphApi.Services;
using Microsoft.OpenApi.Models;
using Neo4j.Driver;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// ── Neo4j Settings ────────────────────────────────────────────────────────────
builder.Services.Configure<Neo4jSettings>(builder.Configuration.GetSection("Neo4j"));

var neo4j = builder.Configuration.GetSection("Neo4j").Get<Neo4jSettings>()!;
builder.Services.AddSingleton<IDriver>(_ =>
    GraphDatabase.Driver(
        neo4j.Uri,
        AuthTokens.Basic(neo4j.Username, neo4j.Password)));

// ── Repositories ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAnimeRepository, AnimeRepository>();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IGraphRepository, GraphRepository>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAnimeService, AnimeService>();
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IGraphService, GraphService>();

// ── Controllers + JSON ───────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Preserve UTF-8 characters (Japanese, etc.)
        options.JsonSerializerOptions.Encoder =
            JavaScriptEncoder.Create(UnicodeRanges.All);
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Anime Graph API",
        Version = "v1",
        Description = "API REST para exploração de relações entre animes, personagens e profissionais via Neo4j."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anime Graph API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
