using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Discord.Oauth;

public sealed class DiscordOauthConfig
{
	/// <summary>
	///		Gets or sets the <see cref="ILoggerFactory"/> to use for logging.
	/// </summary>
	public ILoggerFactory? LoggerFactory { get; set; }

	/// <summary>
	///		Gets or sets the <see cref="IHttpClientFactory"/> to use for making HTTP requests.
	/// </summary>
	public IHttpClientFactory? HttpClientFactory { get; set; }
	
	/// <summary>
	///		Gets the base URL for Discord's API.
	/// </summary>
	public const string ApiUrlBase = "https://discord.com/api/v10/";

	/// <summary>
	///		Gets the version of Discord.Net.Oauth.
	/// </summary>
	public static readonly string Version = typeof(DiscordOauthConfig).GetTypeInfo().Assembly.GetName().Version?.ToString(3) ?? "Unknown";

	/// <summary>
	///		Gets the user agent for Discord.Net.Oauth.
	/// </summary>
	public static readonly string UserAgent = $"Discord.Net.Oauth (https://github.com/Misha-133/Discord.Net.Oauth, v{Version})";

	/// <summary>
	///		Gets the default <see cref="JsonSerializerOptions"/> to use.
	/// </summary>
	public static readonly JsonSerializerOptions SerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true,
	};
}