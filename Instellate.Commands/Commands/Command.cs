using System.Linq.Expressions;
using System.Reflection;
using DSharpPlus.Entities;
using Instellate.Commands.Attributes.Application;
using Instellate.Commands.Controllers;

namespace Instellate.Commands.Commands;

public class Command : ICommand
{
    public string Name { get; }
    public string? Description { get; }
    public IReadOnlyList<CommandOption> Options { get; }
    public DiscordApplicationCommandType? ApplicationCommandType { get; }
    internal MethodInfo Method { get; }
    internal Func<BaseController, IReadOnlyList<object?>, object> ExecutionLambda { get; }

    internal Command(string name,
        string? description,
        IReadOnlyList<CommandOption> options,
        MethodInfo method)
    {
        this.Name = name;
        this.Description = description;
        this.Options = options;
        this.Method = method;
        this.ApplicationCommandType = method.GetCustomAttribute<ApplicationTypeAttribute>()?.Type;
        this.ExecutionLambda = BuildExecutionLambda(method, options);
    }

    public DiscordApplicationCommand ConstructApplicationCommand()
    {
        List<DiscordApplicationCommandOption> options = new(this.Options.Count);
        foreach (CommandOption option in this.Options)
        {
            options.Add(option.ConstructEntity());
        }

        return new DiscordApplicationCommand(this.Name,
            this.Description!,
            options: options,
            type: this.ApplicationCommandType ?? DiscordApplicationCommandType.SlashCommand);
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
            this.Description!,
            type: DiscordApplicationCommandOptionType.SubCommand,
            options: options);
    }

    private static Func<BaseController, IReadOnlyList<object?>, object> BuildExecutionLambda(
        MethodInfo method,
        IReadOnlyList<CommandOption> options)
    {
        Type methodClass = method.DeclaringType!;

        ParameterExpression optionValues
            = Expression.Parameter(typeof(IReadOnlyList<>).MakeGenericType(typeof(object)));
        ParameterExpression controller = Expression.Parameter(typeof(BaseController));

        List<Expression> arguments = new(options.Count);
        for (int i = 0; i < options.Count; i++)
        {
            CommandOption option = options[i];
            arguments.Add(Expression.ConvertChecked(
                Expression.Property(optionValues, "Item", Expression.Constant(i)),
                option.ParameterType
            ));
        }

        BlockExpression block = Expression.Block(Expression.Call(
            Expression.ConvertChecked(controller, methodClass),
            method,
            arguments
        ));

        Func<BaseController, IReadOnlyList<object?>, object> lambda =
            Expression.Lambda<Func<BaseController, IReadOnlyList<object?>, object>>(
                    block,
                    controller,
                    optionValues
                )
                .Compile();

        return lambda;
    }
}
