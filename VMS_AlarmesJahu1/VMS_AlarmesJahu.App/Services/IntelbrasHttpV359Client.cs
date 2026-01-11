using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VMS_AlarmesJahu.App.Services;

/// <summary>
/// Cliente leve inspirado na API HTTP Intelbras "V3.59" (cgi-bin/*.cgi).
/// Serve para melhorar diagnóstico/gerenciamento quando o DVR está acessível por IP/DDNS.
/// 
/// Observação: muitos DVRs usam autenticação Basic ou Digest. Este cliente tenta ambos via HttpClientHandler.
/// </summary>
public sealed class IntelbrasHttpV359Client
{
    public record ProbeResult(bool Success, string Host, int Port, string Summary, string Raw);

    private static readonly int[] DefaultPorts = { 80, 8080, 37778, 37810, 8000, 9000, 9002 };

    public async Task<ProbeResult> ProbeAsync(
        string host,
        string user,
        string password,
        int[]? ports = null,
        int timeoutMs = 3500,
        CancellationToken ct = default)
    {
        ports ??= DefaultPorts;
        host = (host ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(host))
            return new ProbeResult(false, host, 0, "Host vazio", "");

        foreach (var port in ports.Distinct())
        {
            var url = $"http://{host}:{port}/cgi-bin/magicBox.cgi?action=getSystemInfo";

            try
            {
                using var handler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential(user ?? string.Empty, password ?? string.Empty),
                    PreAuthenticate = true,
                    UseProxy = false,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using var http = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromMilliseconds(timeoutMs)
                };

                using var resp = await http.GetAsync(url, ct).ConfigureAwait(false);
                var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    // 401 é comum quando precisa Digest/Basic; handler normalmente resolve,
                    // mas alguns firmwares exigem primeiro 401. Vamos só seguir para a próxima porta.
                    continue;
                }

                var summary = SummarizeSystemInfo(raw);
                return new ProbeResult(true, host, port, summary, raw);
            }
            catch
            {
                // tenta próxima porta
            }
        }

        return new ProbeResult(false, host, 0, "HTTP API não respondeu (portas comuns testadas)", "");
    }

    private static string SummarizeSystemInfo(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "Sem resposta";

        // O magicBox.cgi costuma retornar pares "key=value" em texto.
        // Vamos extrair alguns campos comuns se existirem.
        string Find(string key)
        {
            var line = raw.Split('\n')
                .Select(l => l.Trim())
                .FirstOrDefault(l => l.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase));
            return line == null ? "" : line.Substring(key.Length + 1).Trim();
        }

        var deviceType = Find("deviceType");
        var serial = Find("serialNumber");
        var build = Find("buildDate");
        var version = Find("version");

        var parts = new[]
        {
            string.IsNullOrWhiteSpace(deviceType) ? null : $"Tipo: {deviceType}",
            string.IsNullOrWhiteSpace(serial) ? null : $"Serial: {serial}",
            string.IsNullOrWhiteSpace(version) ? null : $"Versão: {version}",
            string.IsNullOrWhiteSpace(build) ? null : $"Build: {build}",
        }.Where(p => p != null);

        var summary = string.Join(" | ", parts!);
        if (!string.IsNullOrWhiteSpace(summary))
            return summary;

        // fallback: primeiras linhas
        var first = string.Join(" ", raw.Split('\n').Take(3).Select(s => s.Trim()).Where(s => s.Length > 0));
        return first.Length > 180 ? first[..180] + "..." : first;
    }
}
