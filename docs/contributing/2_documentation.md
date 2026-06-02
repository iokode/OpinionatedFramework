# Documentation Guidelines

Documentation should be written in English and stored as Markdown.

## Structure

Use these sections consistently:

- `concepts/`: mental model and theory.
- `guides/`: task-oriented steps.
- `decisions/`: rationale and trade-offs.
- `reference/`: lookup information.
- `contributing/`: internal contributor documentation.

## File Ordering

Markdown files in the same directory should start with a numeric prefix when order matters.

Examples:

```text
1_overview.md
2_philosophy.md
3_core_concepts.md
```

## Page Style

Each page should have a clear purpose.

For concept pages, prefer this structure:

```text
# Title

## Purpose
## How It Works
## When To Use It
## Related Pages
```

For guide pages, prefer this structure:

```text
# Title

## 1. First Step
## 2. Second Step
## 3. Verify The Result
```

For decision pages, prefer this structure:

```text
# Title

## Decision
## Why
## Trade-offs
```

`Trade-offs` is optional. Include it only when there are meaningful costs, risks, or boundaries to document.

## Writing Rules

- Explain why before how when introducing a concept.
- Keep consumer documentation focused on consumer concerns.
- Do not track planned work in `docs/`; use GitHub Issues and Milestones.
- Mark unstable or incomplete capabilities clearly.
- Prefer examples from tests or real framework usage.
- Avoid claiming generated behavior that is not implemented yet.
