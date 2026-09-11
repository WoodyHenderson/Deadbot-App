# DeadBot

DeadBot is an open-source C# desktop chat GUI for asking questions about Deadlock
using the public [Deadlock knowledgebase](https://github.com/WoodyHenderson/Deadlock-Public-Data)
and models accessed through [OpenRouter](https://openrouter.ai/).

> **Status: early desktop prototype.** OpenRouter chat and local knowledgebase
> context are implemented. Retrieval coverage and model availability still need
> evaluation.

Each user supplies their own OpenRouter API key and pays for their own requests.
The application does not use a maintainer key or shared hosted backend. An API
key is sufficient for OpenRouter authentication; account passwords are not needed.
Keys are loaded locally and are not committed to the repository or included in
releases.

The current model selections are:

- `z-ai/glm-5.3-flash`
- `openai/gpt-5.6-luna`

## Knowledgebase setup

The application reads the independently maintained public data repository without
modifying it. Private research material is not used at runtime.

The expected development layout is:

```text
workspace/
├── DeadBot-App/
└── deadlock-data-public/
```

Knowledgebase licensing and attribution are separate from the application code;
review the data repository's [attribution](https://github.com/WoodyHenderson/Deadlock-Public-Data/blob/HEAD/ATTRIBUTION.md)
and [limitations](https://github.com/WoodyHenderson/Deadlock-Public-Data/blob/HEAD/LIMITATIONS.md)
before redistribution.

## Development

With the .NET 8 SDK and Git installed, run the application from this directory:

```bash
dotnet run
```

The app's **Download data** button downloads the public knowledgebase and stores
it in the local application data directory. On Windows this is:

```text
%LOCALAPPDATA%\DeadBot\knowledgebase
```

On macOS and Linux, the location is determined by the platform's local application
data directory.

The local `.env` file may contain the OpenRouter key for development:

```env
OPENROUTER_API_KEY=your-key-here
```

Never commit the populated `.env` file.

DeadBot is an unofficial project and is not affiliated with or endorsed by Valve.
