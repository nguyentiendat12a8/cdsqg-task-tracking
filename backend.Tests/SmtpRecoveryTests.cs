using System.Net;
using System.Net.Sockets;
using System.Text;
using Cdsqg.Application.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cdsqg.Tests;

public class SmtpRecoveryTests
{
    [Fact]
    public async Task SenderDeliversRecoveryLinkThroughSmtp()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var capture = Task.Run(async () => {
                using var connection = await listener.AcceptTcpClientAsync(timeout.Token);
                await using var stream = connection.GetStream();
                using var reader = new StreamReader(stream, Encoding.ASCII);
                await using var writer = new StreamWriter(stream, Encoding.ASCII) {NewLine="\r\n",AutoFlush=true};
                await writer.WriteLineAsync("220 localhost test SMTP");
                var body = new StringBuilder();
                bool data = false;
                while (await reader.ReadLineAsync(timeout.Token) is { } line)
                {
                    if (data) { if(line == ".") {data=false; await writer.WriteLineAsync("250 accepted");} else body.AppendLine(line); }
                    else if (line.StartsWith("EHLO") || line.StartsWith("HELO")) await writer.WriteLineAsync("250 localhost");
                    else if (line == "DATA") {data=true;await writer.WriteLineAsync("354 send message");}
                    else if (line == "QUIT") {await writer.WriteLineAsync("221 bye");break;}
                    else await writer.WriteLineAsync("250 OK");
                }
                return body.ToString();
            },timeout.Token);
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
                ["Smtp:Host"]="127.0.0.1", ["Smtp:Port"]=port.ToString(), ["Smtp:EnableSsl"]="false",["Smtp:From"]="noreply@example.test"
            }).Build();
            await new SmtpRecoveryEmailSender(config).SendAsync("recipient@example.test", "https://example.test/#reset-password?token=test-token");
            var message = await capture;
            Assert.Contains("recipient@example.test",message);
            var parts = message.Replace("\r\n", "\n").Split("\n\n", 2);
            var body = message.Contains("Content-Transfer-Encoding: base64", StringComparison.OrdinalIgnoreCase)
                ? Encoding.UTF8.GetString(Convert.FromBase64String(parts[1])) : parts[1];
            Assert.Contains("reset-password",body);
        }
        finally {listener.Stop();}
    }
}
