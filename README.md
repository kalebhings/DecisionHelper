# DecisionHelper

DecisionHelper is a personal Discord bot written in C# to help make everyday decisions easier.

The project currently focuses on maintaining a movie list for each configured Discord server, tracking who added and watched each movie, and randomly choosing something to watch. The long-term goal is to expand the same idea to board games, card games, video games, date-night ideas, and other activities.

This project is both personally useful and intended as a portfolio project for practicing C#, Discord bot development, Entity Framework Core, and relational database design.

## Current Features

- Discord slash commands synchronized to each configured server
- Independent movie lists, nicknames, and watch history per server
- Add movies with an optional release year and duplicate prevention
- List movies grouped by the nickname of the person who added them
- Filter lists and random picks by bot nickname
- Filter lists and random picks by the caller's watched or unwatched status
- Mark movies as watched per person
- Disambiguate releases that share a title by release year
- Set short bot-specific nicknames without depending on Discord presence
- Paginate long movie lists to stay within Discord message limits
- Validate stored input, escape Discord Markdown, and suppress mentions
- Persist data with SQLite and Entity Framework Core migrations
- Apply migrations automatically at startup
- Cover service behavior and database migrations with automated tests

Current command structure and options:

```text
/movie add name:<title> [year:<release-year>]
/movie list [added-by:<bot-nickname>] [status:watched|unwatched]
/movie pick [added-by:<bot-nickname>] [status:watched|unwatched]
/movie watched name:<title> [year:<release-year>]

/setnickname nickname:<nickname>
/ping
```

The `added-by` option uses nicknames stored by `/setnickname`. Matching is
case-insensitive and does not require the Discord member to be online.

## Planned Features

- Mark movies as unwatched again
- Movie genres and custom tags
  - Sci-Fi
  - Comedy
  - Disney
  - Christmas
  - Franchise
  - Mood
  - etc.
- Movie metadata and improved movie selection
- Mood-based filtering
- A sadness scale for avoiding emotionally heavy movies when desired
- Discord autocomplete for selecting existing movies
- Nickname autocomplete for movie filters
- Richer command responses and embeds
- Board games
- Card games
- Video games
- Date-night ideas
- Generalized random activity selection

## Tech Stack

- C#
- .NET 10
- Discord.Net
- Entity Framework Core
- SQLite
- Microsoft.Extensions.DependencyInjection
- DotNetEnv
- xUnit

## Project Structure

```text
DecisionHelper/
├── Commands/          # Slash-command handlers
├── Configuration/     # Environment configuration parsing
├── Data/              # EF Core context, factory, and legacy data migration
├── Discord/           # Command registration and safe interaction responses
├── Migrations/        # EF Core schema migrations and model snapshot
├── Models/            # Database entities, filters, and operation results
├── Services/          # Validation and database-backed application logic
├── Tests/             # xUnit service and migration tests
├── Bot.cs             # Discord client lifecycle
├── CommandHandler.cs  # Interaction routing and error boundary
├── DecisionHelper.csproj
└── Program.cs         # Startup, migrations, and dependency wiring
```

The project follows a simple layered structure:

```text
Discord interactions
   |
   v
Commands
   |
   v
Services
   |
   v
Entity Framework Core
   |
   v
SQLite
```

## Setup

### Requirements

- .NET 10 SDK
- A Discord application and bot token

### Clone the repository

```bash
git clone https://github.com/kalebhings/DecisionHelper
cd DecisionHelper
```

### Restore dependencies

```bash
dotnet restore
```

### Configure environment variables

Create a `.env` file in the project root:

```dotenv
DISCORD_TOKEN=your-discord-bot-token
DISCORD_SERVER_IDS=123456789012345678,987654321098765432
DATABASE_CONNECTION_STRING=Data Source=decision-helper.db
```

Do not commit your real `.env` file or Discord token.

You may also commit an `.env.example` file with placeholder values so other developers know which variables are required.

### Restore the database

Migrations are applied automatically when the bot starts. To apply them
without connecting the bot to Discord:

```bash
dotnet run -- --migrate-only
```

When upgrading from the original shared database, existing records are
assigned to the first configured server and copied to every additional
configured server. New records are isolated per server.

### Run the bot

```bash
dotnet run
```

Commands are bulk-synchronized in each configured server when the bot starts,
which also removes stale command definitions. The existing clear flag remains
available for an explicit command refresh:

```bash
dotnet run -- --clear-commands
```

### Run tests

```bash
dotnet test Tests/DecisionHelper.Tests.csproj
```

## Database

DecisionHelper currently uses SQLite for local persistent storage.

The database includes models for:

- People
- Movies
- Movie watch statuses
- Tags
- Movie/tag relationships

SQLite keeps development and deployment simple while still allowing the project to demonstrate relational modeling, migrations, foreign keys, and asynchronous database operations through Entity Framework Core.

The generated database file is intentionally excluded from source control.

All application queries use parameterized EF Core LINQ expressions. User input
is validated before persistence and is never concatenated into SQL. Raw SQL is
limited to static migration statements that do not contain user-provided data.

## Development Status

This project is actively under development.

The current focus is completing the movie workflow before expanding into other activity types. The goal is to build one complete vertical slice first rather than prematurely generalizing movies, games, and date ideas into one abstraction.

## License

This project is currently intended for personal and portfolio use.
