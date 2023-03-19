using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api;
using X;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
typeof(JsonSerializerOptions).GetRuntimeFields()
    .Single(f => f.Name == "_defaultIgnoreCondition")
    .SetValue(JsonSerializerOptions.Default, JsonIgnoreCondition.WhenWritingNull);
typeof(JsonSerializerOptions).GetRuntimeFields()
    .Single(f => f.Name == "_dictionaryKeyPolicy")
    .SetValue(JsonSerializerOptions.Default, JsonNamingPolicy.CamelCase);
typeof(JsonSerializerOptions).GetRuntimeFields()
    .Single(f => f.Name == "_jsonPropertyNamingPolicy")
    .SetValue(JsonSerializerOptions.Default, JsonNamingPolicy.CamelCase);

Console.WriteLine(typeof(JsonSerializerOptions).GetRuntimeFields());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRabbitMQ(new XrmqProperties());
// builder.Services.AddHostedService<CreateProgramConsumer>();
// builder.Services.AddHostedService<TesteConsumer>();
builder.Services.AddSingleton<IHostedService, TesteConsumer>();
builder.Services.AddSingleton<IHostedService, ProgramRawConsumer>();
builder.Services.AddSingleton<IHostedService, ProgramRawConsumer>();

var app = builder.Build();

var xrmq = app.Services.GetService<IXrmq>();
xrmq?.QueueDeclare("teste");
xrmq?.QueueDeclare("program");

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
