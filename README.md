# DeadBot

DeadBot is an open-source C# desktop chat GUI for asking questions about Deadlock
using the public [Deadlock knowledgebase](https://github.com/WoodyHenderson/Deadlock-Public-Data)
and models accessed through [OpenRouter](https://openrouter.ai/).

This is mainly just an experimental test to see how accurate I can actually get the responses to be, how niche the questions/interactions the bot can answer are and how much manual interference I need to give during the process.

Each user supplies their own OpenRouter API key and pays for their own requests.
The application does not use a maintainer key or shared hosted backend. An API
key is sufficient for OpenRouter authentication; account passwords are not needed.
Keys are loaded locally and are not committed to the repository or included in
releases.

The current model selections are:

- `z-ai/glm-5.3-flash`
- `openai/gpt-5.6-luna`

After testing GPT 5.6 Luna seems to be significantly better than 5.3 flash in terms not only of prompt quality but response speed and the cost isn't all that terrible. I've just defaulted to using Luna for everything at this point.

## Knowledgebase setup

The Knowledgebase is installed from a GitHub Repo that I am (maybe) maintaining, https://github.com/WoodyHenderson/Deadlock-Public-Data it simply installs the zip file then unpacks it and is then used in a read only capacity to help with sending the appropriate context to OpenRouter to answer questions.

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

Can put your Openrouter API Key in `.env` to preload using format.

```env
OPENROUTER_API_KEY=your-key-here
```
