using System.Text.Json.Serialization;

namespace Discord.Oauth.Api;

public sealed class AccessTokenModel
{
	[JsonPropertyName("access_token")]
	public required string AccessToken { get; set; }

	[JsonPropertyName("token_type")]
	public required string TokenType { get; set; }

	[JsonPropertyName("expires_in")]
	public required int ExpiresIn { get; set; }

	[JsonPropertyName("refresh_token")]
	public required string RefreshToken { get; set; }

	[JsonPropertyName("scope")]
	public required string Scope { get; set; }
}
