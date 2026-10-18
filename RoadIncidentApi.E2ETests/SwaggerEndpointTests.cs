using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;

namespace RoadIncidentApi.E2ETests;

public class SwaggerEndpointTests : IAsyncLifetime
{
    private Process? _appProcess;
    private readonly string _baseUrl = "http://127.0.0.1:5099";
    private IPlaywright? _playwright;

    public async Task InitializeAsync()
    {
        var appProjectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "RoadIncidentApi", "RoadIncidentApi.csproj"));

        _appProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{appProjectPath}\" --urls \"{_baseUrl}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };

        _appProcess.Start();

        await WaitForAppToBeReadyAsync();
        _playwright = await Playwright.CreateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_playwright is not null)
        {
            _playwright.Dispose();
        }

        if (_appProcess is not null && !_appProcess.HasExited)
        {
            _appProcess.Kill(entireProcessTree: true);
            await _appProcess.WaitForExitAsync();
        }
    }

    [Fact]
    public async Task SwaggerEndpoint_ShouldReturnOpenApiDocument()
    {
        var request = await _playwright!.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = _baseUrl
        });

        var response = await request.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal((int)System.Net.HttpStatusCode.OK, response.Status);

        var content = await response.TextAsync();
        Assert.Contains("openapi", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("System zgłoszeń zdarzeń drogowych", content, StringComparison.OrdinalIgnoreCase);
    }

    private async Task WaitForAppToBeReadyAsync()
    {
        using var client = new HttpClient();

        var timeout = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < timeout)
        {
            try
            {
                var response = await client.GetAsync(_baseUrl + "/swagger/v1/swagger.json");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // app is still starting
            }

            await Task.Delay(500);
        }

        throw new TimeoutException("Aplikacja nie wystartowała w przewidzianym czasie.");
    }
}
