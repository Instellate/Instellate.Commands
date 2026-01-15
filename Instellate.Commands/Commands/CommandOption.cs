using DSharpPlus.Entities;

namespace Instellate.Commands.Commands;

public class CommandOption
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DiscordApplicationCommandOptionType Type { get; set; }
    public object? MaxValue { get; set; }
    public object? MinValue { get; set; }
    public bool Optional { get; set; }
    public int? Positional { get; set; }

    // These should always be set
    public Type ConverterType { get; internal set; } = null!;
    public Type ParameterType { get; internal set; } = null!;
    public object? DefaultValue { get; internal set; }

    public CommandOption(string name, string description, bool optional, int? positional)
    {
        this.Name = name;
        this.Description = description;
        this.Optional = optional;
        this.Positional = positional;
    }

    public DiscordApplicationCommandOption ConstructEntity()
    {
        return new DiscordApplicationCommandOption(
            this.Name,
            this.Description,
            this.Type,
            !this.Optional,
            minValue: this.MinValue!,
            maxValue: this.MaxValue!
        );
    }
}
