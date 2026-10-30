# 🛠 tommycli — The Official Tommy Compiler

`tommycli` is a command-line tool that automates the transpilation of `.ty` blueprints into production-ready code using the OpenAI API.

It reads your `.ty` file, pairs it with the strict `COMPILER_PROMPT.md` (the Tommy System Prompt), and generates clean code, `<pre-flight>` architecture plans, and UML diagrams.

## 📋 Prerequisites

Before installing, ensure you have:
*   **Python 3.9+** installed on your system.
*   An **OpenAI API Key** (You can get one at [platform.openai.com](https://platform.openai.com/)).

## 📦 Installation

1. Open your terminal and navigate to the `cli` directory inside the Tommy repository:
   ```bash
   cd cli
   ```

2. Install the package locally in editable mode. This will also install the `openai` dependency:
   ```bash
   pip install -e .
   ```

3. Verify the installation by checking the version:
   ```bash
   tommycli --help
   ```

## 🔑 Setup API Key

`tommycli` requires an OpenAI API key to function. You must set it as an environment variable in your terminal.

**macOS / Linux:**
```bash
export OPENAI_API_KEY="sk-your-api-key-here"
```

**Windows (Command Prompt):**
```cmd
set OPENAI_API_KEY=sk-your-api-key-here
```

**Windows (PowerShell):**
```powershell
$env:OPENAI_API_KEY="sk-your-api-key-here"
```

*(Note: The API key is only used locally by the script and is never sent anywhere except the official OpenAI API).*

## 🚀 Usage

**Important:** You must run `tommycli` from the **root** of the repository (where `COMPILER_PROMPT.md` is located), not from inside the `cli/` folder.

### Basic Command
```bash
tommycli path/to/your/file.ty --target csharp
```

### Arguments

| Argument | Description | Default |
| :--- | :--- | :--- |
| `file` | **(Required)** Path to your `.ty` blueprint file. | None |
| `--target` | The target language to generate (`csharp`, `python`, `rust`, `go`, `php`, `typescript`, `java`, `swift`, `kotlin`). | `csharp` |
| `--model` | The OpenAI model to use (`gpt-4o`, `gpt-4-turbo`, etc.). | `gpt-4o` |

### Examples

**Generate C# from the API example:**
```bash
tommycli examples/api.ty --target csharp
```

**Generate Rust (useful for OS-level code):**
```bash
tommycli examples/os_kernel.ty --target rust
```

**Generate Python using a different model:**
```bash
tommycli examples/ai_agent.ty --target python --model gpt-4-turbo
```

## 📁 Output

The compiler does not print the code directly to the terminal (to prevent clutter). 

Instead, it creates an `output/` directory in the root of your project and saves the result as a Markdown file. 
For example, running `tommycli main.ty --target rust` will create:
`output/main.rust.md`

Inside this file, you will find:
1.  The `<pre-flight>` Architecture Plan.
2.  Mermaid and PlantUML diagrams.
3.  The clean, production-ready target code.

## 🛠 Troubleshooting

*   **`Error: COMPILER_PROMPT.md not found`** — You are running the command from the wrong directory. Make sure you are in the root of the `tommylang` repository.
*   **`Error: OPENAI_API_KEY environment variable is missing`** — You need to export your API key in the terminal session you are using.
*   **`ModuleNotFoundError: No module named 'openai'`** — You forgot to run `pip install -e .` inside the `cli` folder.
```