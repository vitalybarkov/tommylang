# 📜 Tommy Language Grammar (EBNF)

This is the formal Extended Backus-Naur Form (EBNF) grammar for Tommy (`.ty`). It serves as the ground truth for parsers and LSP implementations.

```ebnf
(* Program Structure *)
Program         = { Statement } ;
Statement       = ImportStmt | AttributeStmt | InterfaceDecl | ClassDecl | StructDecl | FunctionDecl | MainDecl | ControlFlow | VarDecl ;

(* Imports and Packages *)
ImportStmt      = "use" ( StringLit | Dependency ) ;
Dependency      = "dependency" StringLit "{" "version" "=" StringLit "}" ;

(* Attributes *)
AttributeStmt   = "[" Identifier [ "(" { AttrArg } ")" ] "]" ;
AttrArg         = Identifier "=" ( StringLit | NumberLit | BoolLit | Identifier ) ;

(* Declarations *)
InterfaceDecl   = "#" Identifier "{" { Signature } "}" ;
ClassDecl       = "@" Identifier [ ":" { TypeRef } ] "{" { Member } "}" ;
StructDecl      = "@struct" Identifier "{" { Member } "}" ;
Member          = PropertyDecl | FunctionDecl | VarDecl ;

(* Signatures (Interfaces only) *)
Signature       = Identifier "(" [ ParamList ] ")" "{" TypeRef "}" ;

(* Implementations (Classes/Functions) *)
FunctionDecl    = Identifier "(" [ ParamList ] ")" Block ;
MainDecl        = "main" "(" [ ParamList ] ")" Block ;
ParamList       = Param { "," Param } ;
Param           = [ Modifier ] TypeRef Identifier ;
Modifier        = "out" | "in" | "ref" ;

(* Properties *)
PropertyDecl    = Identifier "=" "@p" ( TypeRef | "{" Getter Setter "}" ) ;
Getter          = "get" "(" ")" [ "{" TypeRef "}" ] ;
Setter          = "set" "(" TypeRef Identifier ")" Block ;

(* Variables and Types *)
VarDecl         = Identifier "=" TypeRef Expression ;
TypeRef         = "@" Identifier [ "<" { TypeRef } ">" ] [ "?" ] ;
CollectionType  = "@c" | "@map" | "@set" | "@queue" ;

(* Blocks and Statements *)
Block           = "{" { Statement } "}" ;
ControlFlow     = IfStmt | LoopStmt | SwitchStmt | TryStmt | ReturnStmt ;

IfStmt          = "?" Expression Block [ ":" ( Block | IfStmt ) ] ;
LoopStmt        = "&" ( VarDecl ";" Expression ";" Increment | Identifier ";" Expression ) Block ;
SwitchStmt      = "?:" Expression "{" { CaseStmt } [ DefaultStmt ] "}" ;
CaseStmt        = Pattern "=>" Block ;
DefaultStmt     = ": =>" Block ;
TryStmt         = "try" Block [ "ex" "(" TypeRef Identifier ")" Block ] [ "fin" Block ] ;
ReturnStmt      = "<=" Expression ;

(* Expressions *)
Expression      = Term { ( "==" | "!=" | "<== " | ">== " | "&&" | "||" | "??" ) Term } ;
Term            = Factor { ( "+" | "-" | "*" | "/" ) Factor } ;
Factor          = Identifier | Literal | "(" Expression ")" | Lambda | AwaitExpr | TernaryExpr | NullConditional ;
Lambda          = "(" [ ParamList ] ")" "=>" Expression ;
AwaitExpr       = "~" Identifier "(" [ { Expression } ] ")" ;
TernaryExpr     = Expression "?" Expression ":" Expression ;
NullConditional = Identifier "?." Identifier ;

(* Patterns (for SwitchStmt) *)
Pattern         = Identifier | Literal | TypeRef Identifier ;

(* Literals *)
Literal         = StringLit | NumberLit | BoolLit | NullLit | CollectionLit | MapLit ;
CollectionLit   = "{" [ Expression { "," Expression } ] "}" ;
MapLit          = "{" [ Expression "=" Expression { "," Expression "=" Expression } ] "}" ;

(* Lexical *)
Identifier      = ( "a".."z" | "A".."Z" | "_" ) { "a".."z" | "A".."Z" | "0".."9" | "_" } ;
StringLit       = "'" { AnyChar } "'" | "'''" { AnyChar } "'''" ;
NumberLit       = [ "-" ] Digit { Digit } [ "." Digit { Digit } ] [ ( "s" | "m" | "h" ) ] ;
BoolLit         = "true" | "false" ;
NullLit         = "null" ;
```