using SatReportAI.Services;

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
