# Quality Standards

## General

- Code must compile.
- Avoid dead code.
- Avoid duplicated logic.
- Avoid magic strings.
- Prefer explicit code.

## Architecture

- Business rules stay in Domain.
- Controllers stay thin.
- Infrastructure never contains business rules.

## Error Handling

- Exceptions should be meaningful.
- Validation errors should not use exceptions.
- Log unexpected failures.

## Performance

- Avoid N+1 queries.
- Use pagination when appropriate.
- Avoid unnecessary allocations.

## Security

- Validate user input.
- Never expose secrets.
- Follow least privilege.