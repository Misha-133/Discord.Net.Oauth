using Discord.Oauth.Api;

namespace Discord.Oauth;

public sealed class AccessTokenResponse
{
	internal AccessTokenResponse(AccessTokenModel model)
	{
		AccessToken = model.AccessToken;
		TokenType = model.TokenType;
		ExpiresIn = model.ExpiresIn;
		RefreshToken = model.RefreshToken;
		Scopes = model.Scope.Split();
	}

	public string AccessToken { get; }

	public string TokenType { get; }

	public int ExpiresIn { get; }

	/// <summary>
	///		Gets the refresh token. <see langword="null" /> if the response is a client credentials access token.
	/// </summary>
	public string? RefreshToken { get; }

	public string[] Scopes { get; }
}