using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Discord.Oauth;

public sealed class DiscordOauthConfig
{
	public ILoggerFactory? LoggerFactory { get; set; }

	public IHttpClientFactory? HttpClientFactory { get; set; }

	public LogLevel LogLevel { get; set; } = LogLevel.Information;
	
	public const string OauthUrl = "https://discord.com/api/oauth2/";

	public static readonly string Version = typeof(DiscordOauthConfig).GetTypeInfo().Assembly.GetName().Version?.ToString(3) ?? "Unknown";

	public static readonly string UserAgent = $"Discord.Net.Oauth (https://github.com/Misha-133/Discord.Net.Oauth, v{Version})";
}