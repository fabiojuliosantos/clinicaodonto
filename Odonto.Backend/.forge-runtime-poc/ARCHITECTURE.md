# Forge Architecture

## Purpose

Forge is a local AI engineering runtime.

It standardizes how project context, developer intent and source code input are composed and sent to an AI provider.

The goal is to make AI interactions consistent, reproducible and project-aware.

---

## Pipeline

```text
forge
  ↓
command
  ↓
context resolver
  ↓
task resolver
  ↓
input resolver
  ↓
request composer
  ↓
provider
  ↓
formatter
  ↓
stdout