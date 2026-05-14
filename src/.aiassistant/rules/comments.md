---
apply: always
---

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

---
apply: always
---

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

---

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

