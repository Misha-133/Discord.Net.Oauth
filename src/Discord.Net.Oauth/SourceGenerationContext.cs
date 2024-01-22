using System.Text.Json.Serialization;
using Discord.Oauth.Api;

namespace Discord.Oauth;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AccessTokenModel))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
	
}