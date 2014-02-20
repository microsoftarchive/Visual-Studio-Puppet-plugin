%using Microsoft.VisualStudio.TextManager.Interop
%using System.Diagnostics
%namespace Puppet.Parser
%valuetype LexValue
%partial

/* %expect 5 */


%union {
    public string str;
}


%{
    ErrorHandler handler = null;

    public void SetHandler(ErrorHandler hdlr) 
    { 
        handler = hdlr; 
    }

    internal void CallHdlr(string msg, LexLocation val)
    {
        handler.AddError(msg, val.sLin, val.sCol, val.eCol - val.sCol);
    }

    internal TextSpan MkTSpan(LexLocation s) 
    { 
        return TextSpan(s.sLin, s.sCol, s.eLin, s.eCol); 
    }

    internal void Match(LexLocation lh, LexLocation rh) 
    {
        DefineMatch(MkTSpan(lh), MkTSpan(rh)); 
    }
    
     public void dbgout(string s, params object[] a)
    {
        Debug.WriteLine("Dbg: " + string.Format(s, a));
    }

%}


%token STRING DQPRE DQMID DQPOST
%token LBRACK  RBRACK LBRACE RBRACE SYMBOL FARROW COMMA TRUE
%token FALSE EQUALS APPENDS DELETES LESSEQUAL NOTEQUAL DOT COLON LLCOLLECT RRCOLLECT
%token QMARK LPAREN RPAREN ISEQUAL GREATEREQUAL GREATERTHAN LESSTHAN
%token IF ELSE
%token DEFINE ELSIF VARIABLE CLASS INHERITS NODE BOOLEAN
%token NAME SEMIC CASE DEFAULT AT ATAT LCOLLECT RCOLLECT CLASSREF
%token NOT OR AND UNDEF PARROW PLUS MINUS TIMES DIV LSHIFT RSHIFT UMINUS
%token MATCH NOMATCH REGEX IN_EDGE OUT_EDGE IN_EDGE_SUB OUT_EDGE_SUB
%token IN UNLESS PIPE
%token LAMBDA SELBRACE
%token NUMBER
%token TILDE MODULO


%left  HIGH
%left  SEMIC
%left  PIPE
%left  LPAREN
%left  RPAREN
%left  AT ATAT
%left  DOT
%left  CALL
%left  LBRACK LISTSTART
%left  RBRACK
%left  QMARK
%left  LCOLLECT LLCOLLECT
%right NOT
%nonassoc UMINUS
%left  IN
%left  MATCH NOMATCH
%left  TIMES DIV MODULO
%left  MINUS PLUS
%left  LSHIFT RSHIFT
%left  NOTEQUAL ISEQUAL
%left  GREATEREQUAL GREATERTHAN LESSTHAN LESSEQUAL
%left  AND
%left  OR
%right APPENDS DELETES EQUALS
%left  LBRACE
%left  SELBRACE
%left  RBRACE
%left  IN_EDGE OUT_EDGE IN_EDGE_SUB OUT_EDGE_SUB
%left  TITLE_COLON
%left  CASE_COLON
%left  FARROW
%left  COMMA
%left  LOW

%token maxParseToken
%token LEX_WHITE LN_COMMENT BL_COMMENT LEX_ERROR
  
%%

program
    : statements
    ;

statements
    : syntactic_statements
    ;

syntactic_statements
    : syntactic_statement
    | syntactic_statements SEMIC syntactic_statement
    | syntactic_statements syntactic_statement
    ;

syntactic_statement
    : any_expression
    | syntactic_statement COMMA any_expression
    ;

any_expression
    : relationship_expression
    ;

relationship_expression
    : resource_expression
    | relationship_expression IN_EDGE      relationship_expression
    | relationship_expression IN_EDGE_SUB  relationship_expression
    | relationship_expression OUT_EDGE     relationship_expression
    | relationship_expression OUT_EDGE_SUB relationship_expression
    ;

expression
    : higher_precedence
    | expression LBRACK       expressions RBRACK            { Match(@2, @4); }
    | expression IN           expression
    | expression MATCH        expression
    | expression NOMATCH      expression
    | expression PLUS         expression
    | expression MINUS        expression
    | expression DIV          expression
    | expression TIMES        expression
    | expression MODULO       expression
    | expression LSHIFT       expression
    | expression RSHIFT       expression
    |            MINUS        expression
    | expression NOTEQUAL     expression
    | expression ISEQUAL      expression
    | expression GREATERTHAN  expression
    | expression GREATEREQUAL expression
    | expression LESSTHAN     expression
    | expression LESSEQUAL    expression
    |            NOT          expression
    | expression AND          expression
    | expression OR           expression
    | expression EQUALS       expression
    | expression APPENDS      expression
    | expression DELETES      expression
    | expression QMARK selector_entries  
    |            LPAREN       expression RPAREN             { Match(@1, @3); }
    ;

//---EXPRESSIONS
// (e.g. argument list)
// This expression list can not contain function calls without parentheses around arguments

expressions
    : expression
    | expressions COMMA expression
    ;

// These go through a chain of left recursion, ending with primary_expression

higher_precedence
  : call_function_expression
  ;

primary_expression
    : literal_expression
    | variable
    | call_method_with_lambda_expression
    | collection_expression
    | case_expression
    | if_expression
    | unless_expression
    | definition_expression
    | hostclass_expression
    | node_definition_expression
    ;

// Aleways have the same value

literal_expression
    : array
    | boolean
    | default
    | hash
    | regex
    | text_or_name
    | number
    | type
    | undef
    ;

text_or_name
    : name
    | quotedtext
    ;

//---CALL FUNCTION

call_function_expression
    : primary_expression LPAREN expressions endcomma RPAREN           { Match(@2, @5);}
    | primary_expression LPAREN RPAREN                                { Match(@2, @3);}
    | primary_expression LPAREN expressions endcomma RPAREN lambda    { Match(@3, @5);}
    | primary_expression LPAREN RPAREN  lambda                        { Match(@2, @3);}
    | primary_expression
    ;

//---CALL METHOD

call_method_with_lambda_expression
  : call_method_expression
  | call_method_expression lambda
  ;

call_method_expression
    : named_access LPAREN expressions RPAREN    { Match(@2, @4);}
    | named_access LPAREN RPAREN                { Match(@2, @3);}
    | named_access
    ;

// TODO: It may be of value to access named elements of types too

named_access
    : expression DOT NAME
    ;

//---LAMBDA

// This is a temporary switch while experimenting with concrete syntax
// One should be picked for inclusion in puppet.

// Lambda with parameters to the left of the body

lambda
    : lambda_parameter_list lambda_rest
    ;

lambda_rest
    : LBRACE statements RBRACE    { Match(@1, @3);}
    | LBRACE RBRACE               { Match(@1, @2);}
    ;

// Produces Array<Model::Parameter>

lambda_parameter_list
    : PIPE PIPE
    | PIPE parameters endcomma PIPE
    ;

//---CONDITIONALS

//--IF

// Produces Model::IfExpression

if_expression
    : IF if_part
    ;

if_part
    : expression LBRACE statements RBRACE else      { Match(@2, @4);}
    | expression LBRACE RBRACE else                 { Match(@2, @3);}
    ;

else
    : 
    | ELSIF if_part 
    | ELSE LBRACE statements RBRACE                 { Match(@2, @4);}
    | ELSE LBRACE RBRACE                            { Match(@2, @3);}
    ;

//--UNLESS

// Changed from Puppet 3x where there is no else part on unless

unless_expression
    : UNLESS expression LBRACE statements RBRACE unless_else      { Match(@3, @5);}
    | UNLESS expression LBRACE RBRACE unless_else                 { Match(@3, @4);}
    ;

// Different from else part of if, since "elsif" is not supported, but 'else' is


unless_else
    :
    | ELSE LBRACE statements RBRACE     { Match(@2, @4);}
    | ELSE LBRACE RBRACE                { Match(@2, @3);}
    ;

// --- CASE EXPRESSION
// 
//  Produces Model::CaseExpression

case_expression
    : CASE expression LBRACE case_options RBRACE                { Match(@3, @5); }
    ;

//  Produces Array<Model::CaseOption>
case_options
    : case_option
    | case_options case_option
    ;

//  Produced Model::CaseOption (aka When)

case_option
    : expressions case_colon LBRACE statements RBRACE           { Match(@3, @5);}
    | expressions case_colon LBRACE RBRACE                      { Match(@3, @4);}
    ;

case_colon: COLON;

  //  This special construct is required or racc will produce the wrong result when the selector entry
  //  LHS is generalized to any expression (LBRACE looks like a hash). Thus it is not possible to write
  //  a selector with a single entry where the entry LHS is a hash.
  //  The SELBRACE token is a LBRACE that follows a QMARK, and this is produced by the lexer with a lookback
  //  Produces Array<Model::SelectorEntry>
  // 
  
selector_entries
    : selector_entry
    | LBRACE selector_entry_list endcomma RBRACE            { Match(@1, @4);}
    ;

//  Produces Array<Model::SelectorEntry>

selector_entry_list
    : selector_entry
    | selector_entry_list COMMA selector_entry
    ;

//  Produces a Model::SelectorEntry
//  This FARROW wins over FARROW in Hash

selector_entry
    : expression FARROW expression
    | expression FARROW error                           { CallHdlr("selector_entry: expression expected", @3); }
    ;

// ---RESOURCE
// 
//  Produces [Model::ResourceExpression, Model::ResourceDefaultsExpression]

//  The resource expression parses a generalized syntax and then selects the correct
//  resulting model based on the combinatoin of the LHS and what follows.
//  It also handled exported and virtual resources, and the class case

resource_expression
    : expression 
    | at expression LBRACE resourceinstances endsemi RBRACE             { Match(@3, @6);}
    | at expression LBRACE attribute_operations endcomma RBRACE         { Match(@3, @6);}
    | expression LBRACE resourceinstances endsemi RBRACE                { Match(@2, @5);}
    | expression LBRACE attribute_operations endcomma RBRACE            { Match(@2, @5);}
    | at CLASS LBRACE resourceinstances endsemi RBRACE                  { Match(@3, @6);}
    | CLASS LBRACE resourceinstances endsemi RBRACE                     { Match(@2, @5);}
    ;
  
  resourceinst
    : expression title_colon attribute_operations endcomma
    ;

title_colon 
    : 
    COLON
    ;

resourceinstances
    : resourceinst
    | resourceinstances SEMIC resourceinst
    ;

  //  Produces Symbol corresponding to resource form

at
    : AT
    | AT AT
    | ATAT
    ;

// ---COLLECTION

//  A Collection is a predicate applied to a set of objects with an implied context (used variables are
//  attributes of the object.
//  i.e. this is equivalent for source.select(QUERY).apply(ATTRIBUTE_OPERATIONS)
// 
//  Produces Model::CollectExpression
// 
collection_expression
    : expression collect_query LBRACE attribute_operations endcomma RBRACE  { Match(@3, @6);}
    | expression collect_query
    ;

collect_query
    : LCOLLECT  optional_query RCOLLECT             { Match(@1, @3);}
    | LLCOLLECT optional_query RRCOLLECT            { Match(@1, @3);}
    ;

optional_query
    : nil
    | expression
    ;

// ---ATTRIBUTE OPERATIONS
// 
//  (Not an expression)
// 
//  Produces Array<Model::AttributeOperation>
 
attribute_operations
    :
    | attribute_operation
    | attribute_operations COMMA attribute_operation
    ;

// Produces String
// QUESTION: Why is BOOLEAN valid as an attribute name?

attribute_name
    : NAME
    | keyword
    | BOOLEAN
    ;

// In this version, illegal combinations are validated instead of producing syntax errors
// (Can give nicer error message "+> is not applicable to...")
// Produces Model::AttributeOperation

attribute_operation
    : attribute_name FARROW expression
    | attribute_name PARROW expression
    ;

// ---DEFINE

definition_expression
    : DEFINE classname parameter_list LBRACE opt_statements RBRACE      { Match(@4, @6);}
    ;

// ---HOSTCLASS
// 
//  Produces Model::HostClassDefinition

hostclass_expression
    : CLASS stacked_classname parameter_list classparent LBRACE opt_statements RBRACE   { Match(@5, @7); dbgout("hostclass_expression: {0}'", $2.str);}
    ;

// Record the classname so nested classes gets a fully qualified name at parse-time
// This is a separate rule since racc does not support intermediate actions.

stacked_classname
    : classname
    ;

opt_statements
    : statements
    | nil
    ;

//  Produces String, name or nil result
classparent
    : nil
    | INHERITS classnameordefault
    ;

//  Produces String (this construct allows a class to be named "default" and to be referenced as
//  the parent class.
//  TODO: Investigate the validity
//  Produces a String (classname), or a token (DEFAULT).

classnameordefault
    : classname
    | DEFAULT
    ;

// ---NODE
// 

node_definition_expression
    : NODE hostnames nodeparent LBRACE statements RBRACE    { Match(@4, @6);}
    | NODE hostnames nodeparent LBRACE RBRACE               { Match(@4, @5);}
    ;

//  Hostnames is not a list of names, it is a list of name matchers (including a Regexp).
//  (The old implementation had a special "Hostname" object with some minimal validation)
// 

hostnames
    : hostname
    | hostnames COMMA hostname
    ;

//  Produces a LiteralExpression (string, :default, or regexp)
//  String with interpolation is validated for better error message
hostname
    : dotted_name
    | quotedtext
    | DEFAULT
    | regex
    ;

dotted_name
    : NAME
    | dotted_name DOT NAME
    ;

// Produces Expression, since hostname is an Expression

nodeparent
    : nil
    | INHERITS hostname
    ;

// ---NAMES AND PARAMETERS COMMON TO SEVERAL RULES
//    Produces String
// 
classname
    : NAME
    | CLASS     { CallHdlr("Bad class name", @1); }
    ;

parameter_list
    : nil
    | LPAREN  RPAREN
    | LPAREN parameters endcomma RPAREN
    ;

parameters
    : parameter
    | parameters COMMA parameter
    ;

parameter
    : VARIABLE EQUALS expression
    | VARIABLE
    ;

// --RESTRICTED EXPRESSIONS
//   i.e. where one could have expected an expression, but the set is limited

// //  What is allowed RHS of match operators (see expression)
// match_rvalue
//   : regex
//   | text_or_name

// --VARIABLE
// 
variable
    : VARIABLE
    ;

// ---LITERALS (dynamic and static)
// 





array
    : LBRACK expressions            RBRACK          { Match(@1, @3);}
    | LBRACK expressions            error           { CallHdlr("unmatched bracket", @3); }
    | LBRACK expressions COMMA      RBRACK          { Match(@1, @4);}
    | LBRACK expressions COMMA      error           { CallHdlr("unmatched bracket", @4); }
    | LBRACK                        RBRACK          { Match(@1, @2);}
    | LBRACK                        error           { CallHdlr("unmatched bracket", @2); }
    ;
    
hash
    : LBRACE hashpairs RBRACE           { Match(@1, @3);}
    | LBRACE hashpairs error            { CallHdlr("unmatched brace", @3);}
    | LBRACE hashpairs COMMA RBRACE     { Match(@1, @4);}
    | LBRACE hashpairs COMMA error      { CallHdlr("unmatched brace", @4);}
    | LBRACE RBRACE                     { Match(@1, @2);}
    | LBRACE RBRACE                     { CallHdlr("unmatched brace", @2);}
    ;

hashpairs
    : hashpair
    | hashpairs COMMA hashpair
    ;

hashpair
    : expression FARROW expression
    ;

quotedtext
    : string
    | dq_string
    ;

string
    : STRING
    ;

dq_string
    : dqpre dqrval
    ;
dqpre
    : DQPRE
    ;
dqpost
    :
    DQPOST
    ;
dqmid
    :
    DQMID
    ;
dqrval
    :
    text_expression dqtail
    ;
text_expression
    :
    expression
    ;

dqtail
    : dqpost
    | dqmid dqrval
    ;

number   : NUMBER;
name     : NAME;
type     : CLASSREF;
undef    : UNDEF;
default  : DEFAULT;

// Assumes lexer produces a Boolean value for booleans, or this will go wrong and produce a literal string
// with the text 'true'.
// TODO: could be changed to a specific boolean literal factory method to prevent this possible glitch.

boolean  : BOOLEAN;

regex
  : REGEX
  ;

//---MARKERS, SPECIAL TOKENS, SYNTACTIC SUGAR, etc.

endcomma
  : 
  | COMMA
  ;

endsemi
  : 
  | SEMIC
  ;

keyword
 : AND
 | CASE
 | CLASS
 | DEFAULT
 | DEFINE
 | ELSE
 | ELSIF
 | IF
 | IN
 | INHERITS
 | NODE
 | OR
 | UNDEF
 | UNLESS
 ;
  
nil
    :
    ;
   
    
%%



