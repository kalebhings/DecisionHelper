# DecisionHelper

DecisionHelper is a personal Discord bot written in C# to help make everyday decisions easier.

The project currently focuses on maintaining a shared movie list, tracking who added each movie, and randomly choosing something to watch. The long-term goal is to expand the same idea to board games, card games, video games, date-night ideas, and other activities.

This project is both personally useful and intended as a portfolio project for practicing C#, Discord bot development, Entity Framework Core, and relational database design.

## Current Features

- Discord slash commands
- Add movies to a shared list
- Store the person who added each movie
- Optional release year
- Persist data using SQLite and Entity Framework Core
- List movies grouped by who added them
- Pick a random movie
- Set a custom nickname
- Initial support for per-person movie watch status

Current command structure:

```text
/movie add
/movie list
/movie pick
/movie watched

/setnickname
/ping
```

## Planned Features

- Mark movies as watched or unwatched per person
- Filter random selections by watched status
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
- Better command responses and embeds
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

## Project Structure

```text
DecisionHelper/
├── Commands/       # Discord command handlers
├── Data/           # EF Core DbContext and database configuration
├── Models/         # Domain/database models
├── Services/       # Application and database logic
├── Migrations/     # Entity Framework Core migrations
├── Bot.cs          # Discord client setup and command registration
├── CommandHandler.cs
└── Program.cs      # Application startup and dependency configuration
```

The project follows a simple layered structure:

```text
Discord
   ↓
Commands
   ↓
Services
   ↓
Entity Framework Core
   ↓
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

Apply the included Entity Framework Core migrations:

```bash
dotnet ef database update
```

If `dotnet ef` is not available:

```bash
dotnet tool install --global dotnet-ef
```

### Run the bot

```bash
dotnet run
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

## Development Status

This project is actively under development.

The current focus is completing the movie workflow before expanding into other activity types. The goal is to build one complete vertical slice first rather than prematurely generalizing movies, games, and date ideas into one abstraction.

## License

This project is currently intended for personal and portfolio use.
