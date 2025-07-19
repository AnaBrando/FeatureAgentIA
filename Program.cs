using System.Text.Json;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

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

    var content = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    var markdown = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

    // 🔽 Generate safe file name based on feature name
    var safeTitle = Regex.Replace(request.Business.ToLower(), @"[^\w\-]+", "-");
    var fileName = $"feature-{safeTitle}.md";
    var filePath = Path.Combine("GeneratedSpecs", fileName);

    // 🔽 Create folder and write file
    Directory.CreateDirectory("GeneratedSpecs");
    await File.WriteAllTextAsync(filePath, markdown);
    
    return Results.Ok(content);
});

app.Run();

record FeatureRequest(string Business, string Implementation, string Quality);