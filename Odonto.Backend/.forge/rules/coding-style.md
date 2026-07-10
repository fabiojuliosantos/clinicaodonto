# Coding Style

## Naming

- Use PascalCase for types, methods and properties.
- Use camelCase for local variables and parameters.
- Use meaningful names.
- Avoid abbreviations unless widely accepted.

---

## Organization

- One responsibility per class.
- Keep files focused on a single concern.
- Prefer small methods.

---

## Architecture

- Controllers should remain thin.
- Business rules belong to the Domain whenever possible.
- Application coordinates use cases.
- Infrastructure implements external concerns.

---

## Dependencies

- Depend on abstractions.
- Avoid circular dependencies.
- Keep coupling low.

---

## Exceptions

- Throw exceptions only for exceptional situations.
- Validation should not rely on exceptions.

---

## Asynchronous Code

- Prefer async/await for I/O operations.
- Avoid blocking asynchronous code.

---

## General

- Prefer readability over cleverness.
- Keep consistency with the existing codebase.
- Remove unused code.
- Avoid premature optimization.