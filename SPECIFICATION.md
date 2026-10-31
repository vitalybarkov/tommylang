---

# 📜 Tommy Language Specification (v18.3)

Tommy is designed to be a **monotonic**, strictly-typed language that acts as a blueprint for AI code generation. It removes ambiguity, forcing LLMs to output identical architectural patterns across all target languages.

---

## 🎨 1. Visual Palette (TommyColor)
Tommy uses semantic typography to reduce cognitive load. The official VS Code theme is **[TommyColor](https://marketplace.visualstudio.com/items?itemName=TommyStore.tommycolor)**.

| Entity | Color | Hex | Example |
| :--- | :--- | :--- | :--- |
| **Classes / Structs** | 🟢 Green | `#4EC9B0` | `@UserService` |
| **Interfaces** | 🟣 Purple | `#C586C0` | `#IPaymentGateway` |
| **Functions / Methods** | 🔵 Blue | `#569CD6` | `get_user()` |
| **Variables / Parameters** | ⚪ White | `#D4D4D4` | `user_id` |
| **Content (Strings/Numbers)** | 🟡 Sandy | `#CE9178` | `'active'`, `42` |
| **Attributes / Comments** | ⚫ Gray | `#6A9955` | `[REST]`, `// comment` |

---

## 🏗 2. Naming Conventions (STRICT)
To ensure AI never hallucinates, Tommy enforces strict casing:
*   **Interfaces (`#`):** `PascalCase` (no `I` prefix). Example: `#PaymentGateway`
*   **Classes/Structs (`@`):** `PascalCase`. Example: `@StripeGateway`
*   **Functions & Variables:** `snake_case`. Example: `process_payment`, `user_id`
*   **Constants:** `UPPER_SNAKE_CASE`. Example: `MAX_RETRIES`

---

## ⚙️ 3. Base Operators & Data

### The Golden Rule of Assignment
Tommy is **monotonic**. You use `=` for **ALL data** (variables, object fields, dictionary keys, attribute keys). 
You use `:` **ONLY for logic** (`else`, ternary operator). Using `:` for data is a fatal error.

```tommy
// CORRECT
user_map = @map<@i32, @s> { 1 = 'one', 2 = 'two' }
[Cache(ttl = 60s)]

// WRONG (Will cause a validation error)
user_map = @map<@i32, @s> { 1: 'one', 2: 'two' }
```

### Operators
*   `=` — Assignment.
*   `?` — `if` (at start of block) or ternary operator.
*   `:` — `else` (align with `?`) or ternary separator.
*   `?:` — `switch` (pattern matching).
*   `<=` — `return`.
*   `<==` — less than or equal to. `>==` — greater than or equal to.
*   `&` — Loop (`for`/`while`).
*   `=>` — Lambda expression.
*   `~` — `await` (before function call) OR bitwise NOT (inside parentheses).
*   `??` — Null-coalescing. `?.` — Null-conditional.

---

## 📦 4. Entities & Types

### Interfaces (`#`)
Contracts containing only method signatures. Return types MUST be specified in curly braces here.
```tommy
#IPaymentGateway {
    charge (@user_dto user, @f64 amount) {@bool}
}
```

### Classes (`@`) & Structs (`@struct`)
Implementations. **Return types are NEVER specified in implementations** (they are inferred from `<=`).
```tommy
@StripeGateway : #IPaymentGateway {
    api_key = @p @s // Property
    
    charge (@user_dto user, @f64 amount) {
        <= true // Return type inferred as @bool
    }
}
```

### Collections
*   `@c<T>` — List
*   `@map<K,V> { key = val }` — Dictionary
*   `@set<T> { val1, val2 }` — HashSet
*   `@queue<T> { val1, val2 }` — Queue

### Types
*   Primitives: `@i16`, `@i32`, `@i64`, `@f32`, `@f64`, `@s`, `@bool`.
*   Nullable: `@User?`.
*   Memory: `@span<T>`, `@stackalloc`, `@in`.
*   Unsafe: `unsafe { ... }`.

---

## 🛑 5. Built-in System Functions
Do not declare these. The AI compiler maps them to the target language's HTTP/gRPC/Result structures:
*   `ok(data)` -> HTTP 200 / Success
*   `created(data)` -> HTTP 201 / Created
*   `bad_request(msg)` -> HTTP 400
*   `not_found(msg)` -> HTTP 404
*   `conflict(msg)` -> HTTP 409
*   `server_error(msg)` -> HTTP 500

*Note: Infrastructure services like `@db`, `@cache`, `@channel`, and `@ptr` are treated as AI Ambient Intents. The AI compiler automatically maps them to the appropriate database drivers, caching clients, or concurrency primitives in the target language.*

---

## 🏛 6. Architecture Attributes (`[...]`)
Tommy uses `[Attributes]` (Gray) to define enterprise patterns:
*   **API:** `[REST]`, `[gRPC]`, `[WebSocket]`, `[GraphQL]`
*   **Security:** `[Auth]`, `[Encrypt]`, `[PII]`, `[GDPR]`, `[RateLimit]`
*   **Messaging:** `[Kafka]`, `[RabbitMQ]`, `[Outbox]`, `[Saga]`, `[Idempotent]`
*   **Resilience:** `[Timeout]`, `[Retry]`, `[CircuitBreaker]`, `[Bulkhead]`
*   **Observability:** `[Telemetry]`, `[Test]`, `[Migration]`
*   **Patterns:** `[Singleton]`, `[Scoped]`, `[Transient]`

---

## 💻 7. OS-Level & Bare-Metal
For kernel/driver development, use `[NoRuntime]` and `[Ring(0)]`. The AI will **ignore** Backend targets (C#, Python) and generate strictly Rust/C/C++ (`#![no_std]`).
```tommy
[NoRuntime]
[Ring(0)]
[Interrupt]
handle_hardware_irq (@i32 irq_number) {
    unsafe {
        ptr = @ptr<@i32> 0xFF0000
        *ptr = 1
    }
}
```

---

## 🤖 8. AI Directives & Control Flow
*   `// @ai: <instruction>` — Forces the AI to apply a specific logic (e.g., `// @ai: use Dapper instead of EF Core`).
*   `[Vibe]` — Enables rapid prototyping. AI auto-generates missing stubs and infrastructure.
*   `[UML]` — Instructs AI to generate Mermaid and PlantUML diagrams alongside the code.

### Control Flow Syntax
```tommy
?try {
    data = ~@db.query('SELECT ...')
}
:ex (@DatabaseError e) {
    log(e.message)
}
:fin {
    log('done')
}

& i = 0; i <== 10; i++ {
    // loop
}
```