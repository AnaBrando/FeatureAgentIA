using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

var app = builder.Build();
app.UseCors(policy =>
    policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
app.UseStaticFiles();
app.MapPost("/generate-feature", async (FeatureRequest request) =>
{
    var openAiKey = Environment.GetEnvironmentVariable("OPENAI_KEY");

    using var http = new HttpClient();
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiKey);

    var prompt = $"""
            [1] Business Context:
            {request.Business}

            [2] Implementation:
            {request.Implementation}

            [3] Quality:
            {request.Quality}

            Please generate a backlog feature written in markdown format.
            Use the following sections and prepend each with a suitable emoji:

            # 💡 Feature: Title

            ## 🧠 Business Context

            ## 🧱 Implementation Plan

            ## ✅ DoR

            ## 🧪 BDD Scenarios

            Use bullet points, clean formatting, and include Gherkin code blocks when needed.
            """;
            
    var body = new
    {
        model = "gpt-4o",
        messages = new[] {
            new { role = "system", content = "You are a senior product manager writing user stories" },
            new { role = "user", content = prompt }
        }
    };

    var response = await http.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", body);
    var resultJson = await response.Content.ReadAsStringAsync();
    var result = JsonDocument.Parse(resultJson).RootElement;
    
    var markdown = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

    // 🔽 Generate safe file name based on feature name
    var fileName = $"feature_{DateTime.UtcNow:yyyyMMddHHmmss}.md";
    var filePath = Path.Combine("wwwroot", "downloads", fileName);

    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    await File.WriteAllTextAsync(filePath, markdown, Encoding.UTF8);

    var downloadUrl = $"http://localhost:5000/downloads/{fileName}";
    return Results.Ok(new { message = "Markdown generated", Download = downloadUrl });
    
});
app.MapGet("/download/{file}", async (string file) =>
{
    var path = Path.Combine("wwwroot", "downloads", file);
    if (!System.IO.File.Exists(path))
        return Results.NotFound();

    var content = await System.IO.File.ReadAllBytesAsync(path);
    return Results.File(content, "text/markdown", file);
});

app.Run();

record FeatureRequest(string Business, string Implementation, string Quality);