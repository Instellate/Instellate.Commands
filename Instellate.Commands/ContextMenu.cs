using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using DSharpPlus.Entities;
using Instellate.Commands.Actions;

namespace Instellate.Commands;

public class ContextMenu
{
    internal readonly Type _controllerType;
    internal readonly Func<BaseController, Task<IActionResult>> _executionLambda;

    public string Name { get; }
    public DiscordApplicationCommandType Type { get; }

    public ContextMenu(string name, MethodInfo method)
    {
        this.Name = name;
        this.Type = GetCommandType(method);
        this._executionLambda = BuildExecutionLambda(method, this.Type);
        this._controllerType = method.DeclaringType!;
    }

    public DiscordApplicationCommand ConstructApplicationCommand()
    {
        return new DiscordApplicationCommand(this.Name, null!, type: this.Type);
    }

    private static DiscordApplicationCommandType GetCommandType(MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length != 1)
        {
            throw new ArgumentException(
                $"Method {method.Name} is required to have only a single parameter to be a context menu",
                nameof(method)
            );
        }

        ParameterInfo parameter = parameters[0];

        if (parameter.ParameterType == typeof(DiscordUser))
        {
            return DiscordApplicationCommandType.UserContextMenu;
        }

        if (parameter.ParameterType == typeof(DiscordMessage))
        {
            return DiscordApplicationCommandType.MessageContextMenu;
        }

        throw new ArgumentException(
            $"Parameter {parameter.Name} of method {method.Name} has unsupported context menu value"
        );
    }

    private static Func<BaseController, Task<IActionResult>> BuildExecutionLambda(
        MethodInfo method,
        DiscordApplicationCommandType commandType
    )
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length != 1)
        {
            throw new ArgumentException(
                $"Method {method.Name} is required to have only a single parameter",
                nameof(method)
            );
        }

        ParameterExpression baseController = Expression.Parameter(typeof(BaseController));
        UnaryExpression interactionContext = Expression.ConvertChecked(
            Expression.Property(baseController, nameof(BaseController.ActionContext)),
            typeof(InteractionActionContext)
        );

        MemberExpression interactionData =
            Expression.Property(
                Expression.Property(
                    interactionContext,
                    nameof(InteractionActionContext.Interaction)
                ),
                nameof(DiscordInteraction.Data)
            );
        MemberExpression resolvedData
            = Expression.Property(interactionData, nameof(DiscordInteractionData.Resolved));

        MethodInfo genericFirst = typeof(Enumerable)
            .GetMethods()
            .First(m =>
                m.Name == nameof(Enumerable.First) && m.GetParameters().Length == 1
            );

        MethodInfo first;
        MemberExpression dictionary;
        switch (commandType)
        {
            case DiscordApplicationCommandType.UserContextMenu:
                dictionary = Expression.Property(
                    resolvedData,
                    nameof(DiscordInteractionResolvedCollection.Users)
                );
                first = genericFirst.MakeGenericMethod(typeof(DiscordUser));
                break;

            case DiscordApplicationCommandType.MessageContextMenu:
                dictionary = Expression.Property(
                    resolvedData,
                    nameof(DiscordInteractionResolvedCollection.Messages)
                );
                first = genericFirst.MakeGenericMethod(typeof(DiscordMessage));
                break;

            default:
                throw new UnreachableException();
        }

        MemberExpression values
            = Expression.Property(dictionary, nameof(IReadOnlyDictionary<,>.Values));
        MethodCallExpression firstValue = Expression.Call(null, first, values);

        MethodCallExpression contextMenuCall = Expression.Call(
            Expression.ConvertChecked(baseController, method.DeclaringType!),
            method,
            firstValue
        );

        Expression value = ExpressionHelper.HandleReturnValue(contextMenuCall, method.ReturnType);

        Func<BaseController, Task<IActionResult>> lambda =
            Expression.Lambda<Func<BaseController, Task<IActionResult>>>(
                value,
                baseController
            ).Compile();
        return lambda;
    }
}
