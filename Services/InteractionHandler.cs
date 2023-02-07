using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using SampleBot.Helpers.Extensions;
using Microsoft.Extensions.Configuration;

namespace SampleBot.Services;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _handler;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ulong _devGuildId = 560509387626643477;

    public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, IConfiguration config)
    {
        _client = client;
        _handler = handler;
        _services = services;
        _config = config;
    }

    /// <summary>
    ///   Start up async processes.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Process when the client is ready, so we can register our commands.
        _client.Ready += ReadyAsync;
        _handler.InteractionExecuted += HandleInteractionExecutedAsync;

        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        // Process the InteractionCreated payloads to execute Interactions commands
        _client.InteractionCreated += HandleInteraction;
    }
    
    private async Task HandleInteractionExecutedAsync(ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            // Error 
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    Console.WriteLine(result.ErrorReason);
                    break;
                case InteractionCommandError.UnknownCommand:
                    // implement
                    break;
                case null:
                case InteractionCommandError.ConvertFailed:
                case InteractionCommandError.BadArgs:
                case InteractionCommandError.Exception:
                case InteractionCommandError.Unsuccessful:
                case InteractionCommandError.ParseFailed:
                default: break;
            }
    
            string msg = "No message provided.";
            IUser user = context.Interaction.User;
            string cmd = command.Name ?? "_";
            string? interactionType = Enum.GetName(typeof(InteractionType), context.Interaction.Type);
            
            switch (context.Interaction.Type)
            {
                case InteractionType.Ping:
                    msg = $"{user.Id}'s ping: {result.Error}: {result.ErrorReason}";
                    break;
                case InteractionType.ApplicationCommand:
                    msg = $"{user.Id}'s `/{cmd}` command failed: {result.Error}: {result.ErrorReason}";
                    break;
                case InteractionType.MessageComponent:
                    msg = $"{user.Id}'s component interaction failed: {result.Error}: {result.ErrorReason}";
                    break;
                case InteractionType.ApplicationCommandAutocomplete:
                    msg = $"{user.Id}'s autocomplete failed: {result.Error}: {result.ErrorReason}";
                    break;
                case InteractionType.ModalSubmit:
                    msg = $"{user.Id}'s modal submit failed: {result.Error}: {result.ErrorReason}";
                    break;
                default: 
                    msg = $"{user.Id}'s interaction failed: {result.Error}: {result.ErrorReason}";
                    break;
            }
            
            await Bot.LogAsync(new LogMessage(LogSeverity.Error, interactionType!, msg));
            await context.Interaction.SendErrorAsync(result.ErrorReason);
        }
        else
        {
            // Success
        }
    }

    /// <summary>
    ///   Called when the bot is ready. Register the commands to guilds.
    /// </summary>
    private async Task ReadyAsync()
    {
        await _handler.RegisterCommandsToGuildAsync(_devGuildId);
    }

    /// <summary>
    ///   Handle an incoming interaction.
    /// </summary>
    /// <param name="interaction">The interaction to handle.</param>
    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            SocketInteractionContext context = new(_client, interaction);

            // Execute the incoming command.
            await _handler.ExecuteCommandAsync(context, _services);
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }
}

