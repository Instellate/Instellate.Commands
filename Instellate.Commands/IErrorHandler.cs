using DSharpPlus.Entities;
using Instellate.Commands.Commands;

namespace Instellate.Commands;

/// <summary>
/// A error handler for handling errors created by converters or controller exceptions
/// </summary>
public interface IErrorHandler
{
    /// <summary>
    /// Called when a converter creates an error
    /// </summary>
    /// <param name="command">The command related for the current error</param>
    /// <param name="option">The option that got the error</param>
    /// <param name="value">The value that caused the error</param>
    /// <param name="exception">The exception</param>
    /// <returns></returns>
    public Task<IActionResult> HandleConverterErrorAsync(
        ICommand command,
        CommandOption option,
        object? value,
        Exception exception
    );

    /// <summary>
    /// Handle an error caused by a command
    /// </summary>
    /// <param name="command">The command that caused an error</param>
    /// <param name="exception">The error itself</param>
    /// <returns></returns>
    public Task<IActionResult> HandleCommandErrorAsync(ICommand command, Exception exception);

    public Task<IActionResult> HandleAuthorMissingPermissionsAsync(
        ICommand command,
        DiscordPermissions requiredPermissions
    );
}
