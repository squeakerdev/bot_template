using Discord.Interactions;
using SampleBot.Helpers.Common.Preconditions;

namespace SampleBot.Modules;

/// <summary>
///   Admin-only commands. 
/// </summary>
[IsBotOwner]
public class Admin : InteractionModuleBase<SocketInteractionContext>
{
}