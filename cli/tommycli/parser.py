from lark import Lark, UnexpectedInput
import os

# Простая грамматика для проверки базового синтаксиса Tommy
GRAMMAR = """
start: statement+
?statement: import_stmt | attribute_stmt | var_decl | function_decl | block
import_stmt: "use" (STRING | IDENTIFIER)
attribute_stmt: "[" IDENTIFIER [ "(" [attr_args] ")" ] "]"
attr_args: attr_arg ("," attr_arg)*
attr_arg: IDENTIFIER "=" (STRING | NUMBER | IDENTIFIER | "true" | "false")
var_decl: IDENTIFIER "=" "@" IDENTIFIER ( "<" [IDENTIFIER ("," IDENTIFIER)*] ">" )? (STRING | NUMBER | IDENTIFIER | "true" | "false" | "null")
function_decl: IDENTIFIER "(" [param_list] ")" block
param_list: param ("," param)*
param: [MODIFIER] "@" IDENTIFIER IDENTIFIER
block: "{" statement* "}"

MODIFIER: "in" | "out" | "ref"
IDENTIFIER: /[a-zA-Z_][a-zA-Z0-9_]*/
STRING: /'[^']*'/
NUMBER: /-?\d+(\.\d+)?/

%import common.WS
%ignore WS
"""

def validate_syntax(file_path):
    if not os.path.exists(file_path):
        return False, f"File {file_path} not found."
        
    with open(file_path, "r", encoding="utf-8") as f:
        code = f.read()

    try:
        parser = Lark(GRAMMAR, start='start', parser='lalr')
        parser.parse(code)
        return True, "Syntax OK"
    except UnexpectedInput as e:
        # Возвращаем понятную ошибку
        return False, f"Syntax Error at line {e.line}, column {e.column}: {e.get_context(code).strip()}"
    except Exception as e:
        return False, str(e)