using System.Net.Http.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Discord.Oauth;

public class DiscordOauthClient : IAsyncDisposable, IDisposable
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    public DiscordOauthClient(DiscordOauthConfig config)
    {
        _logger = config.LoggerFactory?.CreateLogger<DiscordOauthClient>() ?? new NullLogger<DiscordOauthClient>();

        _httpClient = config.HttpClientFactory?.CreateClient() ?? new HttpClient();
        _httpClient.BaseAddress = new Uri(DiscordOauthConfig.ApiUrlBase);
        _httpClient.DefaultRequestHeaders.Add("user-agent", DiscordOauthConfig.UserAgent);

        _logger.LogDebug("Discord.Net.Oauth v{Version}", DiscordOauthConfig.Version);
    }

    public DiscordOauthClient()
    {
        _logger = new NullLogger<DiscordOauthClient>();

        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(DiscordOauthConfig.ApiUrlBase);
        _httpClient.DefaultRequestHeaders.Add("user-agent", DiscordOauthConfig.UserAgent);

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

    /// <summary>
    ///		Exchange a code for an access token.
    /// </summary>
    /// <returns>
    ///		The access token. <see langword="null"/> if the exchange failed.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="HttpRequestException"></exception>
    public async Task<AccessTokenResponse?> ExchangeCodeAsync(string code, string redirectUri, ulong clientId, string clientSecret, CancellationToken? cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentNullException(nameof(code));

        if (string.IsNullOrWhiteSpace(redirectUri))
            throw new ArgumentNullException(nameof(redirectUri));

        if (clientId == default)
            throw new ArgumentNullException(nameof(clientId));

        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentNullException(nameof(clientSecret));

        var body = new FormUrlEncodedContent([
            new ("grant_type", "authorization_code"),
            new ("code", code),
            new ("redirect_uri", redirectUri),
            new ("client_id", clientId.ToString()),
            new ("client_secret", clientSecret),
        ]);

        var res = await SendFormBodyAsync<Api.AccessTokenModel>(body, "oauth2/token", cancellationToken);

        return res is not null ? new AccessTokenResponse(res) : default;
    }

    /// <summary>
    ///		
    /// </summary>
    /// <returns>
    ///		The refreshed access token. <see langword="null"/> if the refresh failed.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<AccessTokenResponse?> RefreshTokenAsync(string refreshToken, ulong clientId, string clientSecret, CancellationToken? cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentNullException(nameof(refreshToken));

        var body = new FormUrlEncodedContent([
            new ("grant_type", "refresh_token"),
            new ("refresh_token", refreshToken),
            new ("client_id", clientId.ToString()),
            new ("client_secret", clientSecret),
        ]);

        var response = await SendFormBodyAsync<Api.AccessTokenModel>(body, "oauth2/token", cancellationToken).ConfigureAwait(false);

        return response is null
            ? default
            : new AccessTokenResponse(response);
    }

    /// <summary>
    ///		Gets an access token for the owner of the application. Scopes are limited to <c>identify</c> and <c>applications.commands.update</c>
    ///		in case the application is owned by a team.
    /// </summary>
    /// <remarks>
    ///		Client credentials access tokens do not have a refresh token.
    /// </remarks>
	/// <returns>
	///		The access token. <see langword="null"/> if the refresh failed.
	/// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<AccessTokenResponse?> GetClientCredentialsAsync(string[] scopes, ulong clientId, string clientSecret, CancellationToken? cancellationToken = default)
    {
        if (clientId == default)
            throw new ArgumentNullException(nameof(clientId));

        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentNullException(nameof(clientSecret));

        if (scopes.Length == 0)
            throw new ArgumentNullException(nameof(scopes));

        var body = new FormUrlEncodedContent([
            new ("grant_type", "client_credentials"),
            new ("scope", string.Join(' ', scopes)),
            new ("client_id", clientId.ToString()),
            new ("client_secret", clientSecret),
        ]);

        var response = await SendFormBodyAsync<Api.AccessTokenModel>(body, "oauth2/token", cancellationToken).ConfigureAwait(false);

        return response is null
            ? default
            : new AccessTokenResponse(response);
    }

    /// <summary>
    ///		Revokes a token.
    /// </summary>
    /// <returns>
    ///		<see langword="true"/> if token was successfully revoked, <see langword="false"/> otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="HttpRequestException"></exception>
    public Task<bool> RevokeTokenAsync(string token, ulong clientId, string clientSecret, CancellationToken? cancellationToken = default)
    {
        if (clientId == default)
            throw new ArgumentNullException(nameof(clientId));

        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentNullException(nameof(clientSecret));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentNullException(nameof(token));

        var body = new FormUrlEncodedContent([
            new ("token", token),
            new ("client_id", clientId.ToString()),
            new ("client_secret", clientSecret),
        ]);

        return SendAsync(body, "oauth2/token/revoke", cancellationToken);
    }

    internal async Task<T?> SendFormBodyAsync<T>(FormUrlEncodedContent body, string route, CancellationToken? cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(route, body, cancellationToken: cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Server responded with {Code}", response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<T>(DiscordOauthConfig.SerializerOptions, cancellationToken ?? CancellationToken.None);
    }

    internal async Task<bool> SendAsync(FormUrlEncodedContent body, string route, CancellationToken? cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(route, body, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Server responded with {Code}", response.StatusCode);
            return false;
        }

        return true;
    }
}
