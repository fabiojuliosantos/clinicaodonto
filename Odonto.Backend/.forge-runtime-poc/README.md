# Forge

## Purpose

AI Engineering CLI is a lightweight command-line tool designed to standardize how AI models interact with software projects.

The goal is not to replace the developer.

The goal is to provide consistent engineering context so every interaction with an AI model follows the same architectural principles, coding standards and project knowledge.

---

# Vision

The CLI should:

- Standardize AI interactions.
- Eliminate repetitive prompt engineering.
- Preserve project architecture.
- Centralize engineering knowledge.
- Keep the developer in control of technical decisions.
- Support different AI providers.
- Support different programming languages in the future.

---

# Architecture

The CLI is divided into four major areas.

```text
.ai/

bin/
core/
project/
scripts/
```

## bin

Contains the CLI entrypoint.

Responsible for:

- Parsing user commands.
- Delegating execution.

The entrypoint never contains business logic.

---

## core

Contains the permanent knowledge shared by every request.

Examples:

- System prompt
- Engineering philosophy
- Global configuration

Core never depends on the project.

---

## project

Contains project-specific knowledge.

Examples:

- Architecture
- Domain
- Coding style
- Workflow
- Quality standards
- Technology stack

This directory changes from project to project.

---

## scripts

Contains the CLI implementation.

Responsibilities include:

- Building requests
- Resolving context
- Resolving inputs
- Calling providers
- Executing commands

---

# Execution Flow

Current execution flow:

```text
Developer

↓

ai review

↓

review.sh

↓

resolve_context.sh

↓

build_request.sh

↓

provider

↓

AI Model

↓

Response
```

---

# Design Principles

The CLI follows a few fundamental principles.

## Single Responsibility

Each script performs exactly one responsibility.

---

## Context First

Every request is built from structured project context.

The model should never rely only on the user prompt.

---

## Provider Agnostic

The CLI does not depend on any AI provider.

Providers are interchangeable.

Examples:

- OpenAI
- Ollama
- Anthropic
- Gemini

---

## Project Agnostic

The Core does not know anything about the project.

Project-specific knowledge lives only inside the `project` directory.

---

## Simplicity

The framework should remain simple.

Avoid abstractions before they become necessary.

---

# Current Roadmap

## Version 1

- AI Review
- OpenAI Provider
- Ollama Provider
- Context Resolution
- Request Builder

---

## Future Versions

- Multiple programming language profiles
- Additional providers
- Automatic project discovery
- Interactive mode
- Conversation memory
- Project initialization command

---

# Directory Structure

```text
.ai/

bin/
    ai

core/
    config.sh
    prompts/

project/

scripts/

tmp/

_legacy/
```

---

# Development Philosophy

The framework evolves from real usage.

Features are added only after a practical need has been identified.

Premature abstraction should always be avoided.