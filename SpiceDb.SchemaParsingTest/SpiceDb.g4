grammar SpiceDb;

// Parser Rules

schema: (definition | caveat | comment)*;

definition: 'definition' ID '{' definitionBody '}';

definitionBody: (relation | permission | comment)*;

relation: 'relation' ID ':' subject ( '|' subject )*;

permission: 'permission' ID '=' expression;

caveat: 'caveat' ID '(' parameters ')' '{' expression '}';

expression: expression ('+' | '&' | '-' | '->') expression
          | ID
          | '(' expression ')';

parameters: parameter (',' parameter)*;

parameter: ID type;

type: 'int' | 'string' | ID;  // You can add more types as needed.

subject: typeReference (subRelation | wildcard)?;

typeReference: ID;

subRelation: '#' ID;

wildcard: ':*';

comment: DOC_COMMENT | LINE_COMMENT | BLOCK_COMMENT;

// Lexer Rules

ID: [a-zA-Z_][a-zA-Z_0-9]*;

DOC_COMMENT: '/**' .*? '*/' -> channel(HIDDEN);

LINE_COMMENT: '//' ~[\r\n]* -> channel(HIDDEN);

BLOCK_COMMENT: '/*' .*? '*/' -> channel(HIDDEN);

WS: [ \t\r\n]+ -> channel(HIDDEN);
