using DSharpPlus.Entities;
using Instellate.Commands.Commands;

namespace Instellate.Commands;

public interface IErrorHandler
{
    public Task<IActionResult> HandleConverterErrorAsync(
        ICommand command,
        CommandOption option,
        object? value,
        Exception exception
    );

    public Task<IActionResult> HandleCommandErrorAsync(ICommand command, Exception exception);

    public Task<IActionResult> HandleAuthorMissingPermissionsAsync(
        ICommand command,
        DiscordPermissions requiredPermissions
    );
}
