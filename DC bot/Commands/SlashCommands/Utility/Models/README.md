# Models

Contains the language choice model used by utility slash commands.

## Overview

- `SlashLanguage.cs` supplies English (`Eng`) and Hungarian (`Hu`) choices.
- DSharpPlus displays these choices as `en` and `hu`.
- The existing `DC_bot.Commands.SlashCommands.Utility` namespace is preserved.

## Related Components

- `../LanguageSlashCommand.cs` - Consumes the language choice when updating the guild language.
- `../README.md` - Documents the utility slash command adapters.
