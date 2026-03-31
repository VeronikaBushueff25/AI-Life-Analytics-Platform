using MediatR;

namespace AILifeAnalytics.Application.Commands.Settings;

/// <summary>
/// Команда проверки HTTP-прокси
/// </summary>
public record TestProxyCommand(string Host, int Port, string Username, string Password) : IRequest<string>;

/// <summary>
/// Обработчик проверки HTTP-прокси
/// </summary>
public class TestProxyHandler : IRequestHandler<TestProxyCommand, string>
{
    public async Task<string> Handle(TestProxyCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Host))
            return "Укажите хост прокси.";

        var isSocks = command.Port is 1080 or 9050 or 9150 or 1081;
        if (isSocks)
            return $"Порт {command.Port} — SOCKS5. .NET поддерживает только HTTP-прокси.";

        try
        {
            var webProxy = new System.Net.WebProxy($"http://{command.Host}:{command.Port}", false);

            if (!string.IsNullOrWhiteSpace(command.Username))
                webProxy.Credentials = new System.Net.NetworkCredential(command.Username, command.Password);

            var handler = new HttpClientHandler { Proxy = webProxy, UseProxy = true };
            using var client = new HttpClient(handler)
            { Timeout = TimeSpan.FromSeconds(10) };

            var resp = await client.GetAsync("https://httpbin.org/ip", cancellationToken);
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);

            string ip;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(body);
                ip = doc.RootElement.GetProperty("origin").GetString() ?? body;
            }
            catch { ip = body.Trim(); }

            return $"Прокси работает. Внешний IP: {ip}";
        }
        catch (TaskCanceledException)
        {
            return "Тайм-аут. Прокси не отвечает.";
        }
        catch (HttpRequestException ex)
        {
            return $"Ошибка соединения: {ex.Message}";
        }
    }
}