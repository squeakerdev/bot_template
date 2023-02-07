using Discord;
using Discord.WebSocket;

namespace SampleBot.Helpers.Extensions;

public static class DiscordSocketClientExtensions
{
    /// <summary>
    ///   Set the bot's status with validity checking.
    /// </summary>
    /// <param name="client">The DiscordSocketClient to extend.</param>
    /// <param name="statusType">The type of status (listening, watching, etc.)</param>
    /// <param name="statusText">The text to include after the type.</param>
    /// <param name="url">The stream url to attach.</param>
    public static async Task SafeSetStatusAsync(this DiscordSocketClient client, string? statusType, string? statusText, string? url = null)
    {
        // Set the bot's status.
        if (!string.IsNullOrEmpty(statusType) && !string.IsNullOrEmpty(statusText))
        {
            // Capitalize the status type; user may provide "watching" instead of "Watching".
            string statusTypeCapitalized = string.Concat(statusType[0].ToString().ToUpper(), statusType[1..]);
                
            string[] statusTypes = Enum.GetNames(typeof(ActivityType));
            if (!statusTypes.Contains(statusTypeCapitalized))
            {
                // Invalid status type.
                await Bot.LogAsync(new LogMessage(LogSeverity.Warning, "Bot", $"Invalid status type `{statusType}`. Defaulting to `Playing`."));
                statusTypeCapitalized = "Playing";
            }
            
            if (url != null && !string.IsNullOrEmpty(url))
                await client.SetGameAsync(statusText, url, (ActivityType)Enum.Parse(typeof(ActivityType), statusTypeCapitalized));
            else
                await client.SetGameAsync(statusText, type: (ActivityType)Enum.Parse(typeof(ActivityType), statusTypeCapitalized));
        }
    }
}