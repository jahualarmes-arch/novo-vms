using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace VMS_AlarmesJahu.App.IA;

public class IaClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;

    public IaClient(string baseUrl, int timeoutMs = 5000)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeoutMs) };
    }

    public async Task<IaDetectionResult?> DetectAsync(byte[] jpegData, double confidence = 0.35, CancellationToken ct = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(jpegData), "image", "frame.jpg");
            content.Add(new StringContent(confidence.ToString("F2")), "conf");

            var response = await _client.PostAsync($"{_baseUrl}/detect", content, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("IA retornou status {Status}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<IaDetectionResult>(json);
        }
        catch (TaskCanceledException) { return null; }
        catch (HttpRequestException ex)
        {
            Log.Debug("Erro de conexão com IA: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao chamar IA");
            return null;
        }
    }

    public async Task<IaHealthResult?> HealthCheckAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetAsync($"{_baseUrl}/health", ct);
            if (!response.IsSuccessStatusCode) return null;
            
            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<IaHealthResult>(json);
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}

public class IaDetectionResult
{
    public bool ok { get; set; }
    public string? error { get; set; }
    public int w { get; set; }
    public int h { get; set; }
    public IaDetection[] detections { get; set; } = Array.Empty<IaDetection>();
}

public class IaDetection
{
    public int class_id { get; set; }
    public string class_name { get; set; } = "";
    public double conf { get; set; }
    public double x1 { get; set; }
    public double y1 { get; set; }
    public double x2 { get; set; }
    public double y2 { get; set; }

    public double Cx => (x1 + x2) / 2;
    public double Cy => (y1 + y2) / 2;
    public double Width => x2 - x1;
    public double Height => y2 - y1;
}

public class IaHealthResult
{
    public string status { get; set; } = "";
    public string model { get; set; } = "";
    public bool gpu { get; set; }
    public double? fps { get; set; }
}
