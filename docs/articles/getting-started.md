---
uid: articles.getting-started
title: Getting Started
---

> [!NOTE]
> This assumes you already know how to create a bot account and write a basic bot in DSharpPlus.
> If you don't know how to do this. Please read
> the [DSharpPlus documentation](xref:articles.basics.bot_account).

# Getting started

I'll assume you already have a basic setup for this.

```csharp
public static class Program 
{
    public static async Task Main() 
    {
        DiscordClientBuilder builder = 
            DiscordClientBuilder.CreateDefault("bot token", DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents);

        DiscordClient client = builder.Build();

        await client.ConnectAsync();
        await Task.Delay(-1);
    }
}
```

Something like this should work in the start. We'll come back and configure this later.

## Controllers

`Instellate.Commands` is built around the concepts of "Controllers". You should be familiar with
these
if you have used ASP.NET.

A controller is a class that contains a group of commands. Creating a controller is really simple.

```csharp
[BaseController]
public class BasicController : BaseController
{
}
```

A controller needs to derive @Instellate.Commands.BaseController and have the
@Instellate.Commands.Attributes.BaseControllerAttribute

### Commands

Commands are defined inside a controller. Commands are method that are public and non static.
The default return type used is @"Instellate.Commands.IActionResult", but you can return anything as
the library will
call `ToString` on types that doesn't implement @"Instellate.Commands.IActionResult".
Your method can also be async. But for this the only return type supported is `Task<T>`.

```csharp
[BaseController]
public class BasicController : BaseController
{
    // Example using IActionResult as return type
    [Command("action_ret", "IActionResult return type")]
    public IActionResult ActionRet() 
    {
        return Text("Hello, world!");
    }

    // Example using string as a return type
    [Command("string_ret", "String return type")]
    public string StringRet() 
    {
        return "Hello, world!";
    }
}
```

Both of these examples will return `Hello, world!` to the user. Simple, right?

Deferring a response is also really simple.

```csharp
[BaseController]
public class BasicController : BaseController
{
    public async Task<string> DeferringAsync() 
    {
        await DeferAsync();
        await Task.Delay(TimeSpan.FromSeconds(2));
        return "Waited 2 seconds.";
    }
}
```

`Text` isn't the only method that `BaseController` supports. See @Instellate.Commands.BaseController
if you want to know what more is supported. You can always implement `IActionResult` if these
options
are too limited for you.

`Instellate.Commads` also supports options. Options are defined as the method parameters.
To define a option you use @Instellate.Commands.Attributes.OptionAttribute

```csharp
[BaseController]
public class BasicController : BaseController
{
    [Command("echo", "Echoes the users input")]
    public string Echo([Option("input", "Input to echo")] string input) 
    {
        return input;
    }
}
```

For text based interactions options uses terminal style way of providing it. So to echo
`Hello, world!`
with the above command you would write `t!echo --input "Hello, world!`. If you want to have `input`
to be
positional for text instead you can also add the
@Instellate.Commands.Attributes.Text.PositionalAttribute
to the parameter.

Currently subcommands aren't fully implemented. You can currently only do a single iteration for
subcommands. To define a subcommand you only need to add the
@Instellate.Commands.Attributes.CommandAttribute
to the `BaseController`. Support for subcommand groups will be added later!

## Converters

Converters are used to convert the values for options to the type that a parameter uses. Converters
implement @"Instellate.Commands.Converters.IConverter`1". An example of a converter is the
StringConverter

```csharp
public class StringConverter : IConverter<string>
{
    public DiscordApplicationCommandOptionType Type => DiscordApplicationCommandOptionType.String;

    public CommandOption ConstructOption(CommandOptionMetadata metadata)
    {
        return new CommandOption(
            metadata.Name,
            metadata.Description,
            metadata.Optional,
            metadata.Positional
        )
        {
            Type = this.Type
        };
    }

    public ValueTask<object?> ConvertFromObject(object? obj, IActionContext context)
    {
        if (obj is not string)
        {
            throw new ArgumentException("Object is not string", nameof(obj));
        }

        return ValueTask.FromResult<object?>(obj);
    }

    public ValueTask<object?> ConvertFromString(Optional<string> input, IActionContext context)
    {
        if (input.TryGetValue(out string? value))
        {
            return ValueTask.FromResult<object?>(value);
        }

        throw new ArgumentNullException(nameof(input));
    }
}

```

Converters are added as a service to the `DiscordClient` service collection. For example you would
add
`StringConverter` as `collection.AddSingleton<IConverter<string>, StringConverter>`

## Various

There are some various other features in `Instellate.Commands` that aren't really too important to
know about.

@Instellate.Commands.Text.IPrefixResolver is used for resolving prefixes. This can be used to for
example
have guilds be able to set their own prefixes.

@Instellate.Commands.IErrorHandler can be implemented to add responses to mesages if a error occurs.

## Configuring `Instellate.Commands` for usage

Let's get back to the beginning code. `Instellate.Commands` uses dependency injection to function.
The library provides extension metohds to get most services added and working.

A basic configuration of `Instellate.Commands` can look like the following.

```csharp
public static class Program 
{
    public static async Task Main() 
    {
        DiscordClientBuilder builder = 
            DiscordClientBuilder.CreateDefault("bot token", DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents);
        builder
            .ConfigureServices(ConfigureServices)
            .ConfigureEventHandlers(e => e.HandleCommandEvents());

        DiscordClient client = builder.Build();
        client.MapCommandControllers();

        await client.ConnectAsync();
        await client.RegisterApplicationCommands(/* Optional guild ID for guild only registration */);

        await Task.Delay(-1);
    }
    
    public static void ConfigureServices(IServiceCollection services) 
    {
        services
            .AddCommands()
            .AddStaticPrefixResolver("t!");
    }
}
```

`AddCommands` adds all default converters, all controllers and
@Instellate.Commands.ControllerFactory
which is used for command execution.

`AddStaticPrefixResolver` adds a prefix resolver that always resolves to `t!`. This can be removed
if you don't want to support text commands or if you want to implement your own `IPrefixResolver`.

`HandleCommandEvents` registers handlers for all the events that is required to do command
execution.

`RegisterApplicationCommands` registers all commands as slash commands.

This is all that is required to get a bot running using `Instellate.Commands`,
have fun creating bots!