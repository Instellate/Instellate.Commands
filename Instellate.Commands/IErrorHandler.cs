using Instellate.Commands.Commands;

namespace Instellate.Commands;

public interface IErrorHandler
{
    public Task<IActionResult> HandleConverterError(
        ICommand command,
        CommandOption option,
        object? value,
        Exception exception
    );

    public Task<IActionResult> HandleCommandError(ICommand command, Exception exception);
}
