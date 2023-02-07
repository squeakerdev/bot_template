using Discord;
using Discord.WebSocket;
using SampleBot.Helpers.Common;

namespace SampleBot.Helpers.Extensions;

/// <summary>
///   Extensions to the <see cref="SocketInteraction"/> class. Accessible in commands through Context.Interaction.
/// </summary>
public static class DiscordInteractionExtensions
{
    private static readonly Embed SuccessEmbed;
    private static readonly Embed ErrorEmbed;
    private static readonly Embed WarningEmbed;

    static DiscordInteractionExtensions()
    {
        // Create the base embeds.
        SuccessEmbed = new EmbedBuilder()
            .WithColor(BotColours.Green)
            .WithDescription($"{BotEmojis.Success} ")
            .Build();

        ErrorEmbed = new EmbedBuilder()
            .WithColor(BotColours.Red)
            .WithDescription($"{BotEmojis.Error} ")
            .Build();

        WarningEmbed = new EmbedBuilder()
            .WithColor(BotColours.Yellow)
            .WithDescription($"{BotEmojis.Warning} ")
            .Build();
    }

    /// <summary>
    ///   Responds to an interaction with a success message.
    /// </summary>
    /// <param name="interaction">The interaction to respond to.</param>
    /// <param name="message">The text to show in the embed.</param>
    /// <param name="ephemeral">Whether or not to send the message privately.</param>
    public static async Task SendSuccessAsync(this IDiscordInteraction interaction, string message, bool ephemeral = false)
    {
        Embed embed = SuccessEmbed.ToEmbedBuilder()
            .WithDescription(string.Concat(SuccessEmbed.Description, message))
            .Build();
        await RespondOnRightAvenueAsync(interaction: interaction, embed: embed, ephemeral: ephemeral);
    }
    
    /// <summary>
    ///   Responds to an interaction with an error message.
    /// </summary>
    /// <param name="interaction">The interaction to respond to.</param>
    /// <param name="message">The text to show in the embed.</param>
    /// <param name="ephemeral">Whether or not to send the message privately.</param>
    public static async Task SendErrorAsync(this IDiscordInteraction interaction, string message, bool ephemeral = false)
    {
        Embed embed = ErrorEmbed.ToEmbedBuilder()
            .WithDescription(string.Concat(ErrorEmbed.Description, message))
            .Build();
        await RespondOnRightAvenueAsync(interaction: interaction, embed: embed, ephemeral: ephemeral);
    }
    
    /// <summary>
    ///   Responds to an interaction with a warning message.
    /// </summary>
    /// <param name="interaction">The interaction to respond to.</param>
    /// <param name="message">The text to show in the embed.</param>
    /// <param name="ephemeral">Whether or not to send the message privately.</param>
    public static async Task SendWarningAsync(this IDiscordInteraction interaction, string message, bool ephemeral = false)
    {
        Embed embed = WarningEmbed.ToEmbedBuilder()
            .WithDescription(string.Concat(WarningEmbed.Description, message))
            .Build();
        await RespondOnRightAvenueAsync(interaction: interaction, embed: embed, ephemeral: ephemeral);
    }
    
    /// <summary>
    ///   Responds to an interaction with a message, checking whether the response should be direct or a followup.
    /// </summary>
    /// <param name="interaction">The interaction to respond to.</param>
    /// <param name="text">The text to attach to the message.</param>
    /// <param name="embed">The embed to attach to the message.</param>
    /// <param name="ephemeral">Whether or not to send the message privately.</param>
    private static async Task RespondOnRightAvenueAsync(this IDiscordInteraction interaction, string? text = null, Embed? embed = null, bool ephemeral = false)
    {
        if (interaction.HasResponded) await interaction.FollowupAsync(text: text, embed: embed, ephemeral: ephemeral);
        else await interaction.RespondAsync(text: text, embed: embed, ephemeral: ephemeral);
    }
}