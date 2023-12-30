using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Discord.Oauth;

public class DiscordOauthClient : IAsyncDisposable, IDisposable
{
	private readonly ILogger _logger;
	private readonly HttpClient _httpClient;

	public DiscordOauthClient()
	{
		_httpClient = new HttpClient
		{
			BaseAddress = new Uri(DiscordOauthConfig.OauthUrl)
		};
		_httpClient.DefaultRequestHeaders.Add("user-agent", DiscordOauthConfig.UserAgent);

		_logger = new NullLogger<DiscordOauthClient>();
	}

	public DiscordOauthClient(DiscordOauthConfig config)
	{
		_logger = config.LoggerFactory?.CreateLogger<DiscordOauthClient>() ?? new NullLogger<DiscordOauthClient>();
		_httpClient = config.HttpClientFactory?.CreateClient() ?? new HttpClient();
		_httpClient.BaseAddress = new Uri(DiscordOauthConfig.OauthUrl);
		_httpClient.DefaultRequestHeaders.Add("user-agent", DiscordOauthConfig.UserAgent);

		_logger.LogDebug("Discord.Net.Oauth v{Version}", DiscordOauthConfig.Version);
	}

	public void Dispose()
	{
		_httpClient.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		if (_httpClient is IAsyncDisposable httpClientAsyncDisposable)
			await httpClientAsyncDisposable.DisposeAsync();
		else
			_httpClient.Dispose();
	}
}
