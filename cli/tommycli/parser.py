from lark import Lark, UnexpectedInput
import os

# Extended grammar (close to GRAMMAR.md)
GRAMMAR = r"""
start: statement+

?statement: import_stmt | attribute_stmt | var_decl | function_decl | if_stmt | loop_stmt | return_stmt | block | try_stmt

// Imports and Packages
import_stmt: "use" (STRING | IDENTIFIER)

// Attributes
attribute_stmt: "[" IDENTIFIER [ "(" [attr_args] ")" ] "]"
attr_args: attr_arg ("," attr_arg)*
attr_arg: IDENTIFIER "=" (STRING | NUMBER | IDENTIFIER | "true" | "false")

// Variables and Types
var_decl: IDENTIFIER "=" "@" IDENTIFIER ( "<" [IDENTIFIER ("," IDENTIFIER)*] ">" )? (STRING | NUMBER | IDENTIFIER | "true" | "false" | "null" | expr)
expr: STRING | NUMBER | IDENTIFIER | "true" | "false" | "null"

// Functions
function_decl: IDENTIFIER "(" [param_list] ")" block
param_list: param ("," param)*
param: [MODIFIER] "@" IDENTIFIER IDENTIFIER

// Control Flow
if_stmt: "?" expr block [ ":" (block | if_stmt) ]
loop_stmt: "&" expr ";" expr ";" expr block
return_stmt: "<=" expr

// Exceptions
try_stmt: "?try" block [":ex" "(" "@" IDENTIFIER IDENTIFIER ")" block] [":fin" block]

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
        parser = Lark(GRAMMAR, start='start', parser='lalr')
        parser.parse(code)
        return True, "Syntax OK"
    except UnexpectedInput as e:
        # Return a detailed error with context
        return False, f"Syntax Error at line {e.line}, column {e.column}: {e.get_context(code).strip()}"
    except Exception as e:
        return False, f"Parser Error: {str(e)}"