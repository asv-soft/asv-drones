# Agent Instructions

These instructions apply to the entire repository.

## Comment and Documentation Language

- Write all code comments in English.
- Write all XML documentation, Markdown documentation, README content, and other developer-facing documentation in English.
- Do not use Russian or mixed-language comments or documentation.
- Keep terminology consistent across code, comments, and documentation.
- Use clear English names for types, members, variables, files, modules, and public APIs.

## Comment Quality

- Prefer self-explanatory code over excessive comments.
- Add comments only when they explain intent, constraints, assumptions, tradeoffs, or non-obvious behavior.
- Do not add comments that only restate what the code already makes obvious.
- Keep comments concise, accurate, and aligned with the current implementation.
- Update or remove comments when the code changes so documentation never becomes misleading.

## Architecture and Design

- Keep the architecture clean, modular, and easy to maintain.
- Follow SOLID principles in design and implementation.
- Give each class, service, and module a single, well-defined responsibility.
- Prefer composition over inheritance unless inheritance is clearly justified.
- Minimize coupling and keep related behavior cohesive.
- Separate domain logic from UI, infrastructure, persistence, and framework-specific concerns.
- Depend on abstractions at system boundaries when this improves testability, extensibility, or clarity.
- Keep public APIs explicit, stable, and easy to understand.
- Eliminate duplicated logic through extraction or refactoring instead of copying behavior.
- Avoid god objects, hidden side effects, and unclear ownership of responsibilities.

## Think Before Coding

- State assumptions explicitly before implementing. If uncertain, ask.
- If multiple interpretations exist, present them instead of picking silently.
- If a simpler approach exists, mention it.
- Push back when the requested approach appears risky or overcomplicated.
- If something is unclear, stop, name what is confusing, and ask.

## Simplicity First

- Write the minimum code that solves the problem.
- Do not add features beyond what was asked.
- Do not add abstractions for single-use code.
- Do not add flexibility or configurability that was not requested.
- Do not add error handling for impossible scenarios.
- If a solution is larger than it needs to be, simplify it before finishing.

## Surgical Changes

- Touch only what is necessary for the user's request.
- Do not improve adjacent code, comments, or formatting unless required.
- Do not refactor unrelated code.
- Match the existing style, even when another style might be preferable.
- If unrelated dead code is noticed, mention it instead of deleting it.
- Remove imports, variables, and functions only when the current change made them unused.
- Do not remove pre-existing dead code unless explicitly asked.
- Every changed line should trace directly to the user's request.

## Goal-Driven Execution

- Define success criteria for non-trivial tasks.
- Verify changes with restore, build, tests, or focused checks where applicable.
- For bug fixes, prefer a test or focused reproduction that fails before the fix and passes after it.
- For validation changes, cover invalid inputs where practical.
- For refactors, ensure relevant checks pass before and after when feasible.

For multi-step tasks, use a brief plan:

```text
1. [Step] -> verify: [check]
2. [Step] -> verify: [check]
3. [Step] -> verify: [check]
```
