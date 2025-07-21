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
app.MapPost("/generate-feature", async (HttpRequest request) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("Invalid content type");
    
    var form = await request.ReadFormAsync();

    string? business = form["business"];
    string? implementation = form["implementation"];
    string? quality = form["quality"];

    // 👇 Handle file if uploaded
    if (form.Files.Count > 0)
    {
        var file = form.Files[0];
        using var reader = new StreamReader(file.OpenReadStream());
        var content = await reader.ReadToEndAsync();

        // Parse file content into prompt sections (optional logic)
        business = content; // or parse `business`, `implementation`, `quality` if format is known
        implementation = "To be defined...";
        quality = "To be verified...";
    }

    var openAiKey = Environment.GetEnvironmentVariable("OPENAI_KEY");
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiKey);

    var prompt = $"""
            [1] Contexto de Negócio:
            {business}

            [2] Implementação:
            {implementation}

            [3] Qualidade:
            {quality}

            Por favor, gere uma feature de backlog escrita em formato Markdown.
            Utilize a seguinte estrutura e adicione um emoji representativo no início de cada seção:

            # 💡 Feature: Título da funcionalidade

            ## 🧠 Contexto de Negócio
            - Explique o motivo da demanda, os impactos esperados e o objetivo da funcionalidade.

            ## 🧱 Plano de Implementação
            - Liste os passos técnicos principais para a implementação.
            - Inclua detalhes como endpoints, handlers, integrações e validações.

            ## ✅ DoR (Definição de Pronto)
            - Liste os critérios para considerar essa feature pronta para desenvolvimento.
            - Inclua dependências, definições de massa de teste e validações obrigatórias.

            ## 🧪 Cenários BDD
            - Escreva os critérios de aceitação no formato Gherkin.
            - Cubra fluxos positivos (happy path) e fluxos alternativos.

            ## 🧰 Cenários de Qualidade
            - Liste os testes manuais e automatizados recomendados.
            - Inclua testes negativos, casos de borda e validações obrigatórias.
            - Detalhe as massas de dados necessárias e quando utilizar mocks.
            - A funcionalidade **não deve retornar erro 500 (5xx)** em nenhuma hipótese. Todos os erros devem retornar **400 com mensagem tratada**.

            ## 🧩 Exemplos de Código
            - Gere também exemplos de classes em C# quando necessário, com `public class`, `get; set;` e nomes coerentes.
            - Inclua os objetos usados nos handlers e nos endpoints, formatados como blocos de código.

            - Se houver múltiplos endpoints que consumam APIs externas (como FN8), gere um exemplo de requisição `curl` para **cada um deles**.
            - Preserve todos os exemplos de chamadas HTTP mencionados, mesmo que sejam semelhantes.
            - Gere o conteúdo completo, sem omitir nenhuma parte que esteja no texto original.
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

    var fileName = $"feature_{DateTime.UtcNow:yyyyMMddHHmmss}.md";
    var filePath = Path.Combine("wwwroot", "downloads", fileName);

    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    await File.WriteAllTextAsync(filePath, markdown!, Encoding.UTF8);

    var downloadUrl = $"http://localhost:5000/downloads/{fileName}";
    return Results.Ok(new { message = "Markdown generated", download = downloadUrl });
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