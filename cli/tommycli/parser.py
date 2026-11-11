from lark import Lark, UnexpectedInput
import os

# Extended grammar (close to GRAMMAR.md)
# We use Earley parser for flexibility with nested objects and expressions.
GRAMMAR = r"""
start: statement+

?statement: import_stmt | attribute_stmt | var_decl | function_decl | if_stmt | loop_stmt | return_stmt | block | try_stmt | expr_stmt | attribute_block

// Imports and Packages
import_stmt: "use" (STRING | IDENTIFIER)

// Attributes
attribute_stmt: "[" IDENTIFIER [ "(" [attr_args] ")" ] "]"
attribute_block: attribute_stmt+

attr_args: attr_arg ("," attr_arg)*
attr_arg: IDENTIFIER "=" (STRING | NUMBER | IDENTIFIER | "true" | "false" | "@" IDENTIFIER "(" [func_args] ")")

// Variables and Types
var_decl: IDENTIFIER "=" (var_type | func_call | object_init | expr)
var_type: "@" IDENTIFIER ( "<" [IDENTIFIER ("," IDENTIFIER)*] ">" )? (STRING | NUMBER | IDENTIFIER | "true" | "false" | "null" | expr)
expr_stmt: IDENTIFIER "=" (func_call | object_init | expr)
expr: STRING | NUMBER | IDENTIFIER | "true" | "false" | "null" | "@" IDENTIFIER | "~" func_call
object_init: "@" IDENTIFIER "{" [object_body] "}"
object_body: (IDENTIFIER "=" (STRING | NUMBER | IDENTIFIER | "true" | "false" | func_call | object_init) [","])* 

// Functions
function_decl: IDENTIFIER "(" [param_list] ")" block
param_list: param ("," param)*
param: [MODIFIER] "@" IDENTIFIER IDENTIFIER

// Function Calls & Fluent API (Resilience)
func_call: (IDENTIFIER | "@" IDENTIFIER) "." IDENTIFIER "(" [func_args] ")" [ { method_chain } ]
method_chain: "." IDENTIFIER "(" [func_args] ")"
func_args: func_arg ("," func_arg)*
func_arg: IDENTIFIER | STRING | NUMBER | "@" IDENTIFIER | object_init | IDENTIFIER "=" (STRING | NUMBER | IDENTIFIER | "@" IDENTIFIER | func_call)

// Control Flow
if_stmt: "?" expr block [ ":" (block | if_stmt) ]
loop_stmt: "&" expr ";" expr ";" expr block
return_stmt: "<=" (expr | func_call | object_init)

// Exceptions
try_stmt: "try" block [ "ex" "(" "@" IDENTIFIER IDENTIFIER ")" block ] [ "fin" block ]

// Blocks
block: "{" statement* "}"

// Tokens
MODIFIER: "in" | "out" | "ref"
IDENTIFIER: /[a-zA-Z_][a-zA-Z0-9_]*/
STRING: /'[^']*'/
NUMBER: /-?\d+(\.\d+)?/

%import common.WS
%ignore WS
"""

def validate_syntax(file_path):
    """Validates the syntax of a .ty file using the Lark parser."""
    if not os.path.exists(file_path):
        return False, f"File {file_path} not found."
        
    with open(file_path, "r", encoding="utf-8") as f:
        code = f.read()

    try:
        # We use 'earley' parser for maximum flexibility with nested rules.
        # It's a bit slower than 'lalr' but handles complex grammars much better.
        parser = Lark(GRAMMAR, start='start', parser='earley')
        parser.parse(code)
        return True, "Syntax OK"
    except UnexpectedInput as e:
        # Return a detailed error with context
        return False, f"Syntax Error at line {e.line}, column {e.column}: {e.get_context(code).strip()}"
    except Exception as e:
        return False, f"Parser Error: {str(e)}"