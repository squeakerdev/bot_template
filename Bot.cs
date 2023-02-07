using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleBot.Helpers.Common;
using SampleBot.Helpers.Extensions;
using SampleBot.Services;

namespace SampleBot;

public class Bot
{
    private readonly string? _token;
    private readonly IServiceProvider _services;
    private readonly DiscordSocketConfig _gatewayConfig = 
        new()
        {
            AlwaysDownloadUsers = true,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            LogGatewayIntentWarnings = false,
        };
    
    
    // config.json fields.
    /// <summary>
    ///   The bot's configuration, sourced from config.json.
    /// </summary>
    private readonly IConfiguration _botConfig = new ConfigurationBuilder()
        .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "config.json"), optional: false)
        .Build();
    /// <summary>
    ///   The type of bot status to use ("Listening", "Playing", Competing", etc.).
    /// </summary>
    private readonly string? _statusType;
    /// <summary>
    ///   The bot's status text (e.g. "/help").
    /// </summary>
    private readonly string? _statusText;
    /// <summary>
    ///   A list of bot owners by numeric ID.
    ///   <para>These users can use admin commands.</para>
    /// </summary>
    /// <remarks>
    ///   Used in preconditions such as <see cref="Helpers.Common.Preconditions.IsBotOwner"/>.
    /// </remarks>
    private static ulong[]? _ownerIds;

    private static void Main()
        => new Bot()
            .MainAsync()
            .GetAwaiter()
            .GetResult();

    /// <summary>
    ///   The constructor for the bot.
    /// </summary>
    /// <exception cref="Exception">config.json is missing.</exception>
    /// <exception cref="MissingFieldException">A required field in config.json is missing.</exception>
    private Bot()
    {
        // Get the config.json fields.
        string tokenEnvironmentVariable = _botConfig["tokenEnvironmentVariableName"] ?? "";
        string dbConnectionString = _botConfig["dbConnectionString"] ?? "";
        string dbName = _botConfig["dbName"] ?? "";
        _statusType = _botConfig["statusType"];
        _statusText = _botConfig["statusText"];
        string? ownerIdsString = _botConfig["ownerIds"];

        // Parse owner IDs.
        if (string.IsNullOrEmpty(ownerIdsString)) throw new MissingFieldException("ownerIds not found in config.json.");
        _ownerIds = ownerIdsString.Split(", ").Select(ulong.Parse).ToArray();

        // Check if the required string fields are missing.
        CheckIfStringFieldsMissing(dbConnectionString, dbName, tokenEnvironmentVariable);
        
        // Fetch bot token from environment variables.
        _token = Environment.GetEnvironmentVariable(tokenEnvironmentVariable, EnvironmentVariableTarget.Machine);
        if (string.IsNullOrEmpty(_token)) throw new Exception($"No token found. Add `{tokenEnvironmentVariable}` to system environment vars.");
        
        // Initialize the database.
        Mongo.InitMongo(dbConnectionString, dbName);

        // Build the service provider.
        _services = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddSingleton(_gatewayConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .BuildServiceProvider();
    }
    
    /// <summary>
    ///   The main entry point for the bot.
    /// </summary>
    private async Task MainAsync()
    {
        var client = _services.GetRequiredService<DiscordSocketClient>();
        
        // Add handlers for events.
        client.Ready += ReadyAsync;
        client.Log += LogAsync;

        // Initialize the interaction handler.
        await _services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();
        
        // Login and start the bot.
        await client.LoginAsync(TokenType.Bot, _token);
        await client.StartAsync();

        // Set the bot's status based on config.json.
        await client.SafeSetStatusAsync(_statusType, _statusText);

        // Block the program until it is closed.
        await Task.Delay(Timeout.Infinite);
    }

    /// <summary>
    ///    Called when the bot is ready. Linked to client.Ready.
    /// </summary>
    /// <returns>The successfully completed task.</returns>
    private static Task ReadyAsync()
    {
        Console.WriteLine("Bot connected and ready!");
        return Task.CompletedTask;
    }
    
    /// <summary>
    ///    Called when the bot logs something. Linked to client.Log.
    /// </summary>
    /// <param name="log">The message to log.</param>
    /// <returns>The successfully completed task.</returns>
    public static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    /// <summary>
    ///   Throw an error if any values in a given list are missing.
    /// </summary>
    /// <param name="fields">The fields to check.</param>
    /// <exception cref="MissingFieldException">A field is null or empty.</exception>
    private static void CheckIfStringFieldsMissing(params string?[] fields)
    {
        if (fields.Any(string.IsNullOrEmpty)) throw new MissingFieldException("A required field in config.json is missing.");
    }
    
    /// <summary>
    ///   Get the bot owner IDs.
    /// </summary>
    /// <returns>An array of the owner IDs.</returns>
    public static ulong[] GetOwnerIds()
    {
        return _ownerIds ?? Array.Empty<ulong>();
    }
}
