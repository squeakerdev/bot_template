using Discord;
using Discord.Interactions;

namespace SampleBot.Helpers.Common.Preconditions;

/// <summary>
///   A precondition that checks if the user is a bot owner.
/// </summary>
public class IsBotOwner : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
    {
        ulong[] ownerIds = Bot.GetOwnerIds();
        await Bot.LogAsync(new LogMessage(LogSeverity.Debug, "Precondition", $"Checking if {context.User.Id} is a bot owner."));
        
        return ownerIds.Contains(context.User.Id) 
            ? PreconditionResult.FromSuccess() 
            : PreconditionResult.FromError("You must be a bot owner to use this command.");
    }
}
