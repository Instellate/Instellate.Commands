using System.Linq.Expressions;
using System.Reflection;
using DSharpPlus.Entities;
using Instellate.Commands.Attributes;
using Instellate.Commands.Attributes.Application;

namespace Instellate.Commands.Commands;

public class Command : ICommand
{
    internal readonly Func<BaseController, IReadOnlyList<object?>, Task<IActionResult>>
        _executionLambda;

    internal readonly MethodInfo _method;

    public string Name { get; }
    public string Description { get; }
    public IReadOnlyList<CommandOption> Options { get; }
    public DiscordPermissions? RequirePermissions { get; }
    public IReadOnlyList<DiscordApplicationIntegrationType>? IntegrationTypes { get; }
    public bool RequireGuildAttribute { get; }

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
        this.RequireGuildAttribute = method.GetCustomAttribute<RequireGuildAttribute>() is not null;

        this._method = method;
        this._executionLambda = BuildExecutionLambda(method, options);
    }


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
            allowDMUsage: !this.RequireGuildAttribute,
            defaultMemberPermissions: this.RequirePermissions,
            integrationTypes: this.IntegrationTypes
        );
    }

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
