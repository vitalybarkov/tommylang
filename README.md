<table border="0" cellspacing="0" cellpadding="10">
  <tr>
    <td valign="middle">
      <img src="TommyLangAvatar.png" width="256" alt="Tommy Logo">
    </td>
    <td valign="middle">
      <h1>🏛 tommylang: The AI Architecture Compiler</h1>
    </td>
  </tr>
</table>

[![VS Code Theme](https://img.shields.io/badge/VS%20Code-TommyColor-blue?style=flat-square&logo=visual-studio-code)](https://marketplace.visualstudio.com/items?itemName=TommyStore.tommycolor)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Hashnode](https://img.shields.io/badge/Blog-Hashnode-blue?logo=hashnode)](https://tommylang.hashnode.dev/)
[![Dev.to](https://img.shields.io/badge/Blog-Dev.to-black?logo=devdotto)](https://dev.to/vitalybark/i-got-tired-of-ai-writing-500-lines-of-c-boilerplate-so-i-built-a-language-that-forces-it-to-376j)

**Are you tired of AI generating 500 lines of C# with missing dependencies and broken architecture?**
**Are you spending hours reviewing AI-generated boilerplate?**

**Tommy solves this.** It is an **Architecture-as-Code** tool designed for the era of Large Language Models. It does not replace C#, Python, or Rust. It **orchestrates** them.

Instead of writing code, you write **Architectural Blueprints** (`.ty` files). One blueprint generates production-ready microservices, database migrations, and infrastructure configs across multiple languages.

---

## 📖 Table of Contents
1. [Why Tommy?](#-why-tommy)
2. [Quick Start](#-quick-start)
3. [How it Works](#-how-it-works)
4. [Core Features](#-core-features)
5. [The "Killer" Example](#-the-killer-example)
6. [Legacy Modernization](#-legacy-modernization)
7. [Repository Structure](#-repository-structure)
8. [Roadmap](#-roadmap)

---

## 🚀 Why Tommy?

1. **Strict-API Compiler:** Our prompt forces LLMs (GPT-4o, Claude 3.5) to act as strict AST compilers. While LLMs are probabilistic, the Strict-API approach minimizes hallucinations.
2. **Polyglot by Design:** Write once in `.ty`, transpile to C#, Python, Rust, Go, PHP, TypeScript, and more.
3. **Enterprise-Ready:** Built-in annotations for `[Saga]`, `[Outbox]`, `[GDPR]`, `[Kafka]`, and `[OpenAPI]`.
4. **Semantic Typography:** The official `TommyColor` VS Code theme uses color to separate Interfaces (Purple), Classes (Green), and Functions (Blue).

---

## 🛠 Quick Start

### Step 1: Install the Extension
1. Open VS Code.
2. Go to Extensions (`Ctrl+Shift+X`).
3. Search for **[TommyColor](https://marketplace.visualstudio.com/items?itemName=TommyStore.tommycolor)** and install it.

### Step 2: Write the Blueprint (`.ty`)
Create a file named `main.ty` and write your architecture:

```tommy
@struct user_dto {
    id = @i32 0
    name = @s ''
}

[REST('/api/hello')]
[Cache(ttl = 60s)]
[Test]
say_hello (@user_dto user) {
    ? user.name == '' {
        <= bad_request('Name is required')
    }
    <= ok('Hello, ' + user.name)
}
```

### Step 3: Transpile to Target Language

**Option A: Using the CLI (Recommended)**
1. Clone the repo and navigate to `cd cli`.
2. Install the compiler: `pip install -e .`
3. Set your API key (or use Ollama locally): 
   - OpenAI: `export OPENAI_API_KEY="your-key"`
   - Ollama: `ollama run llama3`
4. Run the compiler:
   ```bash
   tommycli main.ty --target csharp --trace
   ```

**Option B: Manual AI Use**
1. Open ChatGPT or Claude.
2. Copy **[`COMPILER_PROMPT.md`](COMPILER_PROMPT.md)** as a System Prompt.
3. Paste your `main.ty` code.

---

## 🧠 How it works

Tommy is an AI Architecture Compiler built on top of a strictly crafted **System Prompt** and formal **EBNF Grammar**.

```text
[Your .ty code] -> [AST Parser] -> [LLM System Prompt] -> [Code + Infra + Tests + UML]
```

Because the prompt enforces strict rules (e.g., `snake_case` for functions, `=` for all data, no return types in implementations), the AI is forced to generate code that is identical in architecture across all target languages.

---

## 💎 Core Features

*   **Enterprise Architecture:** `[Saga]`, `[Outbox]`, `[Idempotent]`, `[CircuitBreaker]`, `[RateLimit]`.
*   **Security & Audit-Ready:** `[Auth]`, `[Encrypt]`, `[PII]`, `[GDPR]`, `[AuditLog]`.
*   **Infrastructure as Code:** Generates Dockerfiles, Kubernetes manifests, and Terraform from the same blueprint.
*   **Testing:** Full xUnit (with Moq) and pytest generation with mocks.
*   **UML Generation:** `[UML]` -> generates Mermaid & PlantUML diagrams on the fly.
*   **Traceability & Auditing:** Use the `--trace` flag to generate a JSON map linking every `.ty` annotation directly to its generated implementation file and line number.
*   **Multi-LLM Support:** Run with OpenAI (GPT-4o) or locally with Ollama.

---

## 🚀 The "Killer" Example

Look at the [`examples/enterprise_payment.ty`](examples/enterprise_payment.ty) blueprint. It's about 80 lines of code. 

When you run `tommycli enterprise_payment.ty --target csharp --trace`, the AI generates:

1.  **C# Web API** with OAuth2, Rate Limiting, and OpenAPI (Swagger).
2.  **MassTransit Saga** with a Kafka Outbox pattern.
3.  **Stripe Integration** with Polly (Circuit Breaker + Retry).
4.  **PII Masking** and Audit Logs.
5.  **PostgreSQL Migrations** for the DB.
6.  **Kubernetes Manifests** (`replicas: 3`, resource limits).
7.  **Dockerfile** for deployment.
8.  **xUnit Tests** with real mocks for dependencies.
9.  **Mermaid UML Sequence Diagram** for documentation.

*(See the actual generated outputs in the [`examples/output/`](examples/output/) folder).*

---

## 🔄 Legacy Modernization

Tommy is not just for new projects. It acts as an **Architectural Filter** for legacy code.

Use the `--decompile` flag to reverse-engineer a 10-year-old spaghetti C# or PHP file into a clean `.ty` blueprint:

```bash
tommycli legacy_monolith.cs --decompile --target csharp
```

The AI will strip away all language-specific boilerplate and generate a clean architecture file. You can then transpile this blueprint into modern Rust, Go, or .NET 8 code.

---

## 📂 Repository Structure

*   **[COMPILER_PROMPT.md](COMPILER_PROMPT.md)** — The core logic (System Prompt v20.1).
*   **[SPECIFICATION.md](SPECIFICATION.md)** — The strict syntax and architectural rules.
*   **[GRAMMAR.md](GRAMMAR.md)** — Formal EBNF grammar.
*   `examples/` — Production-ready `.ty` blueprints.
*   `examples/output/` — Real generated outputs (C#, trace.json, Dockerfile).
*   `cli/` — The `tommycli` compiler (Python). [Read the CLI Docs here](cli/README.md).

---

## 🗺 Roadmap

Tommy is in early alpha. Here is what's coming next:
* [x] **Formal Grammar (BNF):** A strict grammar specification for `.ty` files.
* [x] **Multi-LLM Support:** Native support for OpenAI and local Ollama.
* [x] **Traceability:** JSON Evidence map for audits.
* [x] **AST Validator:** A native Python parser to catch syntax errors before hitting the LLM.
* [x] **Infrastructure Generation:** Built-in Docker and Kubernetes generation.
* [x] **Legacy Decompiler:** Reverse-engineer old C#/PHP into clean `.ty` blueprints.
* [ ] **Semantic AST Validation:** Extend parser to validate semantics (e.g., annotations match function types).
* [ ] **LSP & Validator (V5):** A Language Server for real-time IDE syntax checking.

---

## 🤝 Contributing
Tommy is open for architects, AI researchers, and developers. We believe that in the future, humans will design software, and AI will write the implementation. Tommy is the bridge between the two.

Read the **[SPECIFICATION.md](SPECIFICATION.md)** to learn the syntax and start writing blueprints.