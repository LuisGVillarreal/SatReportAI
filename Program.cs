using MongoDB.Driver;
using SatReportAI.Services.IA;
using SatReportAI.Services.MongoDB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddScoped<GeminiService>();

builder.Services.AddScoped<ILLMService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var provider = config["AppSettings:LLM:Provider"];

    return provider switch
    {
        "Gemini" => sp.GetRequiredService<GeminiService>(),
        _ => throw new NotImplementedException("Proveedor no soportado")
    };
});

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["AppSettings:Mongo:ConnectionString"];
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = config["AppSettings:Mongo:Database"];
    return client.GetDatabase(databaseName);
});

builder.Services.AddScoped<IMongoQueryService, MongoQueryService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
