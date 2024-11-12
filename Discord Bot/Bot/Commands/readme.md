# Command Flow

``` mermaid
graph TD
    A[CreateDynamicCommands] -->|BuildCommandAsync| B[DynamicCommandBuilder]
    B -->|CreateCommand| C[SlashCommandBuilder]
    B -->|For each option| D[DynamicCommandOptionBuilder]
    D -->|CreateOption| E[SlashCommandOptionBuilder]
    E -->|Returns| C
    C -->|Build| F[SlashCommandProperties]
```

Flow Description
The process starts in CreateDynamicCommands which:

Takes a command name, description, and endpoint
Makes HTTP request to get command configuration
Iterates through command configurations
For each command, DynamicCommandBuilder:

Creates a new SlashCommandBuilder
Sets basic properties (name, description)
Handles optional command options
For each option, DynamicCommandOptionBuilder:

Creates SlashCommandOptionBuilder
Configures option properties
Handles nested options recursively
Finally builds into Discord.NET SlashCommandProperties that can be registered with Discord's API