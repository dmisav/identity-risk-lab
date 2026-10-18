var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/ingest", () => Results.StatusCode(StatusCodes.Status501NotImplemented));
app.MapGet("/findings", () => Results.StatusCode(StatusCodes.Status501NotImplemented));

app.Run();

public partial class Program;
