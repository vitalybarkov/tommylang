import os
import sys
import argparse
from openai import OpenAI

# Determine the path to the COMPILER_PROMPT.md 
# We assume the user runs it from the root of the tommylang repo
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
    parser.add_argument("--model", default="gpt-4o", help="OpenAI model to use (default: gpt-4o)")
    
    args = parser.parse_args()

    if not os.path.exists(args.file):
        print(f"❌ File '{args.file}' not found.")
        sys.exit(1)

    api_key = os.getenv("OPENAI_API_KEY")
    if not api_key:
        print("❌ Error: OPENAI_API_KEY environment variable is missing.")
        print("Set it using: export OPENAI_API_KEY='your-key-here'")
        sys.exit(1)

    print(f"🏛  tommylang compiler v0.1.0")
    print(f"📄 Blueprint: {args.file}")
    print(f"🎯 Target: {args.target.upper()}")
    print(f"🤖 Engine: {args.model}")
    print("-" * 50)

    with open(args.file, "r", encoding="utf-8") as f:
        code = f.read()

    system_prompt = load_system_prompt()
    client = OpenAI(api_key=api_key)

    print("⏳ Transpiling... (this may take 10-20 seconds)")

    try:
        response = client.chat.completions.create(
            model=args.model,
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": f"Target: {args.target}\n\n{code}"}
            ],
            temperature=0.1
        )

        output = response.choices[0].message.content

        # Create output directory
        os.makedirs("output", exist_ok=True)
        out_filename = os.path.join("output", f"{os.path.basename(args.file).split('.')[0]}.{args.target}.md")
        
        with open(out_filename, "w", encoding="utf-8") as f:
            f.write(output)

        print(f"✅ Success! Output saved to: {out_filename}")

    except Exception as e:
        print(f"❌ API Error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()