import os
import sys
import argparse
import re
import json
from openai import OpenAI
from .parser import validate_syntax

# Determine the path to the COMPILER_PROMPT.md 
PROMPT_FILE = "COMPILER_PROMPT.md"

def load_system_prompt():
    if not os.path.exists(PROMPT_FILE):
        print(f"❌ Error: {PROMPT_FILE} not found in the current directory.")
        print("Please run tommycli from the root of the tommylang repository.")
        sys.exit(1)
    with open(PROMPT_FILE, "r", encoding="utf-8") as f:
        return f.read()

def main():
    parser = argparse.ArgumentParser(
        description="tommycli — The official Tommy Compiler",
        formatter_class=argparse.RawTextHelpFormatter
    )
    parser.add_argument("file", help="Path to the .ty blueprint file")
    parser.add_argument("--target", default="csharp", help="Target language (csharp, python, rust, go, php)")
    parser.add_argument("--provider", default="openai", choices=["openai", "ollama"], help="LLM Provider (openai, ollama)")
    parser.add_argument("--model", default=None, help="Model name. Defaults: gpt-4o (openai), llama3 (ollama)")
    parser.add_argument("--trace", action="store_true", help="Generate a traceability JSON mapping annotations to code lines")
    
    args = parser.parse_args()

    # Set default models based on provider
    if args.model is None:
        args.model = "gpt-4o" if args.provider == "openai" else "llama3"

    if not os.path.exists(args.file):
        print(f"❌ File '{args.file}' not found.")
        sys.exit(1)

    # --- API CONFIGURATION ---
    if args.provider == "openai":
        api_key = os.getenv("OPENAI_API_KEY")
        if not api_key:
            print("❌ Error: OPENAI_API_KEY environment variable is missing.")
            print("Set it using: export OPENAI_API_KEY='your-key-here'")
            sys.exit(1)
        client = OpenAI(api_key=api_key)
    elif args.provider == "ollama":
        # Ollama uses an OpenAI-compatible API on localhost
        client = OpenAI(base_url="http://localhost:11434/v1", api_key="ollama")
        print("🦙 Using local Ollama instance (http://localhost:11434)")

    print(f"🏛  tommylang compiler v0.4.0 (AST & Infra Edition)")
    print(f"📄 Blueprint: {args.file}")
    print(f"🎯 Target: {args.target.upper()}")
    print(f"🤖 Provider: {args.provider.upper()} | Model: {args.model}")
    if args.trace:
        print(f"🔍 Traceability: Enabled")
    print("-" * 50)

    # --- V1: AST VALIDATION ---
    print("🔍 Validating syntax (AST)...")
    is_valid, error_msg = validate_syntax(args.file)
    if not is_valid:
        print(f"❌ SYNTAX ERROR:\n{error_msg}")
        print("Transpilation aborted. LLM was not called.")
        sys.exit(1)
    print("✅ Syntax OK. Proceeding to LLM...")

    with open(args.file, "r", encoding="utf-8") as f:
        code = f.read()

    system_prompt = load_system_prompt()

    print("⏳ Transpiling... (this may take 10-20 seconds)")

    trace_instruction = ""
    if args.trace:
        trace_instruction = "\nIMPORTANT: Generate the trace JSON block as instructed in the system prompt."

    try:
        response = client.chat.completions.create(
            model=args.model,
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": f"Target: {args.target}\n{trace_instruction}\n\n{code}"}
            ],
            temperature=0.1
        )

        output = response.choices[0].message.content

        # Create output directory
        os.makedirs("output", exist_ok=True)
        
        # Extract JSON trace if requested
        if args.trace and "```json" in output:
            json_match = re.search(r"```json\n(.*?)\n```", output, re.DOTALL)
            if json_match:
                trace_json_str = json_match.group(1)
                trace_filename = os.path.join("output", f"{os.path.basename(args.file).split('.')[0]}.trace.json")
                
                with open(trace_filename, "w", encoding="utf-8") as f:
                    f.write(trace_json_str)
                
                print(f"🔍 Trace file saved to: {trace_filename}")
                
                # --- COMPLIANCE CHECK ---
                try:
                    trace_data = json.loads(trace_json_str)
                    trace_array = trace_data.get("trace", [])
                    
                    if not isinstance(trace_array, list):
                        trace_array = []

                    missing = [t for t in trace_array if t.get("verification_result") == "MISSING"]
                    
                    if missing:
                        print("\n⚠️  ARCHITECTURE COMPLIANCE VIOLATION DETECTED!")
                        for m in missing:
                            c_id = m.get('constraint_id', 'UNKNOWN')
                            ann = m.get('annotation', 'UNKNOWN')
                            print(f"   - Constraint [{c_id}] '{ann}' is MISSING in the generated code.")
                        print("   Please review the output carefully.\n")
                    else:
                        print("✅ Architecture Compliance: All constraints verified.\n")
                        
                        # Print evidence links
                        for t in trace_array:
                            if t.get("target_file") and t.get("target_file") != "none":
                                file_path = os.path.abspath(os.path.join("output", t['target_file']))
                                print(f"   ↳ Evidence for {t.get('constraint_id', 'UNKNOWN')}: {file_path}:{t.get('target_line', 0)}")
                        print("\n")
                        
                except json.JSONDecodeError:
                    print("⚠️ Warning: Could not parse trace JSON for compliance check.")
                except Exception as e:
                    print(f"⚠️ Warning: Unexpected error during compliance check: {e}")

                # Remove JSON from the main output
                output = output.replace(json_match.group(0), "")
            else:
                print("⚠️ Warning: --trace was requested but no JSON block was found in the response.")

        # Save the main code
        out_filename = os.path.join("output", f"{os.path.basename(args.file).split('.')[0]}.{args.target}.md")
        
        with open(out_filename, "w", encoding="utf-8") as f:
            f.write(output.strip())

        print(f"✅ Success! Output saved to: {out_filename}")

    except Exception as e:
        print(f"❌ API Error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()