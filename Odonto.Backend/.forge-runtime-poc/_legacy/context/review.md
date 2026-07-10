# Review Context

## Purpose

Used to review source code, pull requests, commits or code changes.

The objective is to identify confirmed issues, technical risks and improvement opportunities while ensuring consistency with the project's architecture, engineering philosophy and coding standards.

---

## Required Context

Always load:

- system.md
- engineering-philosophy.md
- architecture.md
- coding-style.md
- quality-standards.md

---

## Optional Context

Load only when relevant:

- architecture-decisions.md
- domain.md
- stack.md
- workflow.md
- glossary.md

---

## User Input

The following inputs are valid:

- Pull Request
- Git Diff
- One or more source files
- Entire project
- Commit
- Feature implementation

---

## Execution

Evaluate the supplied code in the following order:

1. Correctness
2. Architecture
3. Maintainability
4. Security
5. Performance
6. Readability
7. Consistency

Only evaluate information supported by the provided context.

Ignore personal preferences that are not defined by the project's coding standards or engineering philosophy.

---

## Deliverables

The review may produce:

- Confirmed Issues
- Technical Risks
- Improvement Suggestions
- Architectural Observations
- Positive Observations
- Learning Notes (when valuable)

If no relevant issues are found, explicitly state that.