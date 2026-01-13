using System.Linq.Expressions;
using System.Reflection;
using Instellate.Commands.Actions;

namespace Instellate.Commands;

internal static class ExpressionHelper
{
    private static readonly MethodInfo _fromResult
        = typeof(Task).GetMethod("FromResult")!.MakeGenericMethod(typeof(IActionResult));

    public static Expression HandleReturnValue(Expression returnValue, Type returnType)
    {
        if (returnType == typeof(Task) || returnType == typeof(void))
        {
            return Expression.Block(
                returnValue,
                Expression.Call(
                    null,
                    _fromResult,
                    Expression.New(typeof(EmptyActionResult).GetConstructors()[0])
                )
            );
        }

        if (returnType == typeof(IActionResult) ||
            returnType.GetInterfaces().Contains(typeof(IActionResult)))
        {
            return Expression.Block(Expression.Call(null, _fromResult, returnValue));
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            Type innerReturnType = returnType.GenericTypeArguments[0];


            if (innerReturnType == typeof(IActionResult))
            {
                return Expression.Block(returnValue);
            }

            ParameterExpression task = Expression.Parameter(typeof(Task));
            MemberExpression value
                = Expression.Property(Expression.Convert(task, returnType), "Result");

            if (innerReturnType.GetInterfaces().Contains(typeof(IActionResult)))
            {
                return Expression.Block(Expression.Call(
                    null,
                    typeof(ExpressionHelper).GetMethod(nameof(AwaitTaskAsync))!,
                    Expression.Convert(returnValue, typeof(Task)),
                    Expression.Lambda<Func<Task, IActionContext>>(Expression.Block(value), task)
                ));
            }

            MethodCallExpression toString = Expression.Call(
                value,
                innerReturnType.GetMethod(nameof(ToString), Type.EmptyTypes)!
            );

            NewExpression textActionResult = Expression.New(
                typeof(TextActionResult).GetConstructors()[0],
                toString,
                Expression.Constant(false)
            );

            LambdaExpression lambda = Expression.Lambda<Func<Task, IActionResult>>(
                Expression.Block(textActionResult),
                task
            );

            return Expression.Block(Expression.Call(
                null,
                typeof(ExpressionHelper).GetMethod(nameof(AwaitTaskAsync))!,
                Expression.Convert(returnValue, typeof(Task)),
                lambda
            ));
        }
        else
        {
            MethodCallExpression toString = Expression.Call(
                returnValue,
                returnType.GetMethod(nameof(ToString), Type.EmptyTypes)!
            );

            NewExpression textActionResult = Expression.New(
                typeof(TextActionResult).GetConstructors()[0],
                toString,
                Expression.Constant(false)
            );

            return Expression.Block(Expression.Call(null, _fromResult, textActionResult));
        }
    }

    public static async Task<IActionResult> AwaitTaskAsync(Task task,
        Func<Task, IActionResult> lambda)
    {
        await task;
        return lambda(task);
    }
}
