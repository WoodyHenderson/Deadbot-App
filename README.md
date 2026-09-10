# DeadBot

An open-source C# desktop chat GUI for asking questions about Deadlock using the
public [Deadlock knowledgebase](https://github.com/WoodyHenderson/Deadlock-Public-Data)
and models accessed through [OpenRouter](https://openrouter.ai/).

> **Status: early desktop prototype.** OpenRouter chat and phase-one knowledgebase context are implemented; retrieval coverage and model availability still need evaluation.

## Planned MVP

- Local desktop GUI built with C# and Avalonia UI.
- Model selector, prompt box, and conversation history.
- Local retrieval from the public Deadlock knowledgebase.
- Grounded answers with source references and snapshot information.
- User-supplied OpenRouter API key and account.
- Loading, timeout, provider-error, and insufficient-evidence states.
- Self-contained executables for supported platforms after the core application works.

The application will not use a maintainer API key or shared hosted backend. Each
user will enter their own OpenRouter API key and pay for their own requests. An API
key is sufficient for OpenRouter authentication; account passwords are not needed.
Keys will not be committed to the repository or included in releases.

The initial model selections are currently:

- `z-ai/glm-5.3-flash`
- `openai/gpt-5.6-luna`

Availability and pricing will be checked again during implementation.

## Knowledgebase setup

The planned development layout is:

```text
workspace/
├── DeadBot-App/
└── deadlock-data-public/
```

The application will read the independently maintained public data repository
without modifying it. Private research material will not be used at runtime.
Knowledgebase licensing and attribution are separate from the application code;
review the data repository's [attribution](https://github.com/WoodyHenderson/Deadlock-Public-Data/blob/HEAD/ATTRIBUTION.md)
and [limitations](https://github.com/WoodyHenderson/Deadlock-Public-Data/blob/HEAD/LIMITATIONS.md)
before redistribution.

## Development

With the .NET 8 SDK installed, run `dotnet run` from this directory.
Git must be installed for the app's **Download data** button. Data is stored under
`%LOCALAPPDATA%\DeadBot\knowledgebase` on Windows.

### Phase-one context

Each chat request includes all Markdown/YAML documents under `general/` and `data/`,
plus complete Markdown/YAML files for explicitly named hero/item folders. Names
are case-insensitive with punctuation normalized; aliases, typos, ability-name
lookup, build-candidate discovery, and patch retrieval are not implemented.
Entities from successful prior user turns remain included until **New chat**.
The expandable sources panel lists the files selected for the latest attempt.
Selected evidence and conversation text are sent to OpenRouter and its provider.

Missing core folders and evidence above 250,000 characters block the request rather
than silently dropping sources. This is a local character guard, not a model token
budget; model context limits can still reject requests. Citations are requested
using relative paths but are not yet automatically verified. Data freshness is
not verified against the live game.

Run offline retrieval checks with `dotnet run --project tests/ContextChecks`.
These checks do not contact OpenRouter or require an API key.
The immediate development order is:

1. Desktop application shell and secure local key entry.
2. Public-knowledgebase retrieval and grounding context.
3. OpenRouter client and model selection.
4. Chat interface, citations, snapshot display, and error handling.
5. Tests and platform-specific release builds.

See [`docs/initial-plan.md`](docs/initial-plan.md) and
[`docs/implementation-decisions.md`](docs/implementation-decisions.md) for the
current planning details. A Discord bot is deferred until the desktop application
is working.

DeadBot is an unofficial project and is not affiliated with or endorsed by Valve.
