# Coding Style

These preferences are intended to be shared across projects. Extend this document as conventions are agreed upon.

## Readability

- Write one operation per line. Put separate assignments, calls, and control-flow actions on separate lines.
- Prefer straightforward statements and control flow. Avoid clever tricks, dense expressions, nested conditional expressions, and code compressed to save space.
- Use braces for control-flow blocks. Make the execution order easy to follow.
- Use intermediate variables when they clarify intent or separate meaningful steps. Do not introduce temporary variables that merely add noise.
- Keep methods focused on a clearly named responsibility.

## Names

- Use complete English words in variable and method names.
- Choose self-documenting names that describe the value, purpose, or action in the current context.
- Prefer `databasePath`, `configurationManager`, `exception`, and `minimumSalary` over `dbPath`, `configManager`, `ex`, and `min`.
- Name predicate and query parameters descriptively: `job => job.IsArchived` rather than `j => j.IsArchived`.
- Avoid numbered names such as `job2` when a role such as `archivedJob` explains the distinction.
- Keep names accurate when behavior changes. Preserve names required by external APIs; apply these preferences to names the project controls.

## Database Names

- Use PascalCase for table names, such as `Jobs` and `SearchFilters`.
- Use camelCase for column names, such as `title`, `sourceJobId`, and `datePosted`.
- Prefer `"id" INTEGER PRIMARY KEY AUTOINCREMENT` for SQLite integer primary keys.
- Keep C# public property names in PascalCase and map them to the database convention.

## Line Length and Layout

- Limit source code to 80 columns. Break long signatures and expressions at natural boundaries and indent continuation lines consistently.
- The 80-column limit does not apply to Markdown or plain-text prose. Allow complete sentences and paragraphs without artificial hard wrapping.
- In scripts, including Bash, PowerShell, and batch files, keep each command on one physical line even when it exceeds 80 columns. Put separate commands on separate lines.
- Script commands embedded in documentation follow the same one-line convention.
- Follow the repository's indentation and line-ending settings. Keep formatting consistent within the file.

## Comments and Documentation

- Prefer clear names and simple code over comments explaining complicated expressions.
- Use comments to explain intent, constraints, and non-obvious decisions.
- Update or remove misleading comments, obsolete examples, and abandoned commented-out implementations when changing the surrounding code.
- Reserve repository documentation for tasks, plans, important findings, and architectural decisions. Omit minor implementation details; use an occasional focused code comment where helpful.

## Applying These Preferences

Apply these rules to new and edited code. Keep broad naming or formatting cleanup separate from behavior changes so each change remains easy to review. Treat generated files separately; avoid manual formatting that their generator will immediately undo.
