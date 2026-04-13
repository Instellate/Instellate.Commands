using System.Linq.Expressions;
using System.Reflection;
using DSharpPlus.Entities;
using Instellate.Commands.Attributes;
using Instellate.Commands.Attributes.Application;

namespace Instellate.Commands.Commands;

/// <summary>
/// A executable command
/// </summary>
public class Command : ICommand
{
    internal readonly Func<BaseController, IReadOnlyList<object?>, Task<IActionResult>>
        _executionLambda;

    internal readonly MethodInfo _method;

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string Description { get; }

    /// <summary>
    /// Options that this command requires
    /// </summary>
    public IReadOnlyList<CommandOption> Options { get; }

    /// <summary>
    /// Required permissions to execute this command
    /// </summary>
    public DiscordPermissions? RequirePermissions { get; }

    /// <summary>
    /// Integration type for this command group
    /// </summary>
    public IReadOnlyList<DiscordApplicationIntegrationType>? IntegrationTypes { get; }

    /// <summary>
    /// Context types for this command
    /// </summary>
    public IReadOnlyList<DiscordInteractionContextType>? Contexts { get; }

    /// <summary>
    /// If this command requires being executed in a guild or not
    /// </summary>
    public bool RequireGuild { get; }

    internal Command(
        string name,
        string description,
        IReadOnlyList<CommandOption> options,
        DiscordPermissions? requirePermissions,
        MethodInfo method
    )
    {
        this.Name = name;
        this.Description = description;
        this.Options = options;
        this.RequirePermissions = requirePermissions;
        this.IntegrationTypes
            = method.GetCustomAttribute<AppIntegrationAttribute>()?.IntegrationTypes;
        this.Contexts
            = method.GetCustomAttribute<AppContextsAttribute>()?.Contexts;
        this.RequireGuild = method.GetCustomAttribute<RequireGuildAttribute>() is not null;

        this._method = method;
        this._executionLambda = BuildExecutionLambda(method, options);
    }

    /// <inheritdoc/>
    public DiscordApplicationCommand ConstructApplicationCommand()
    {
        List<DiscordApplicationCommandOption> options = new(this.Options.Count);
        foreach (CommandOption option in this.Options)
        {
            options.Add(option.ConstructEntity());
        }

        return new DiscordApplicationCommand(
            this.Name,
            this.Description,
            options,
            allowDMUsage: !this.RequireGuild,
            defaultMemberPermissions: this.RequirePermissions,
            integrationTypes: this.IntegrationTypes,
            contexts: this.Contexts
        );
    }

    /// <inheritdoc/>
    public DiscordApplicationCommandOption ConstructApplicationCommandOption()
    {
        List<DiscordApplicationCommandOption> options = new(this.Options.Count);
        foreach (CommandOption option in this.Options)
        {
            options.Add(option.ConstructEntity());
        }

        return new DiscordApplicationCommandOption(
            this.Name,
            this.Description,
            DiscordApplicationCommandOptionType.SubCommand,
            options: options
        );
    }

    private static Func<BaseController, IReadOnlyList<object?>, Task<IActionResult>>
        BuildExecutionLambda(
            MethodInfo method,
            IReadOnlyList<CommandOption> options
        )
    {
        Type methodClass = method.DeclaringType!;

        ParameterExpression optionValues
            = Expression.Parameter(typeof(IReadOnlyList<object>));
        ParameterExpression controller = Expression.Parameter(typeof(BaseController));

        List<Expression> arguments = new(options.Count);
        for (int i = 0; i < options.Count; i++)
        {
            CommandOption option = options[i];
            arguments.Add(
                Expression.ConvertChecked(
                    Expression.Property(optionValues, "Item", Expression.Constant(i)),
                    option.ParameterType
                )
            );
        }

        MethodCallExpression callExpression = Expression.Call(
            Expression.ConvertChecked(controller, methodClass),
            method,
            arguments
        );

        Expression block
            = ExpressionHelper.HandleReturnValue(callExpression, method.ReturnType);

        Func<BaseController, IReadOnlyList<object?>, Task<IActionResult>> lambda =
            Expression.Lambda<Func<BaseController, IReadOnlyList<object?>, Task<IActionResult>>>(
                    block,
                    controller,
                    optionValues
                )
                .Compile();

        return lambda;
    }
}
