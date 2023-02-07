using Discord.Interactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SampleBot.Helpers.Extensions;

namespace SampleBot.Modules;

/// <summary>
///   Miscellaneous commands. 
/// </summary>
public class Misc : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly HttpClient HttpClient = new HttpClient();
    private const string ApiKey = "1b25bc1fe3ba44cc9a17a03a1b47cb41";
    private const string ApiEndpoint = "https://api.opensea.io/api/v1/asset/";
    
    /// <summary>
    ///   Addresses of the GENE_SIS contract and its sub-contracts.
    /// </summary>
    private static class Addresses
    {
        public static readonly string Genesis = "0xd8a5d498ab43ed060cb6629b97a19e3e4276dd9f";
        public static readonly string TgoaFnd = "";
        public static readonly string TgoaOs = "";
    }
    
    static Misc()
    {
        HttpClient.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
    }

    /// <summary>
    ///   Fetch a GENE_SIS token.
    /// </summary>
    [SlashCommand("genesis", "Fetch a GENE_SIS token.")]
    public async Task Genesis(int tokenId)
    {
        string requestUri = $"{ApiEndpoint}{Addresses.Genesis}/{tokenId}";
        string responseString = await HttpClient.GetStringAsync(requestUri);
        var tokenInfo = JsonConvert.DeserializeObject<JObject>(responseString);

        if (tokenInfo == null)
        {
            await Context.Interaction.SendErrorAsync("Couldn't find that token.");
            return;
        }
        
        string? imageUrl = tokenInfo["image_url"]!.Value<string>();

        var embed = new Discord.EmbedBuilder()
            .WithTitle($"Token #{tokenId}")
            .WithImageUrl(imageUrl)
            .Build();

        await ReplyAsync(embed: embed);
    }
}
