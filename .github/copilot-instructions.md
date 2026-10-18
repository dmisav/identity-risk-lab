# Identity Risk Lab Project Guidelines

## Project Structure

- This is a C# .NET 8 solution using xUnit.
- `Core` contains the domain model, interfaces, rules, and ingest handler.
- `Data` contains the in-memory store implementation.
- `Api` contains the single ASP.NET Core minimal API host.
- `Tests` contains the automated tests.
- Keep dependencies flowing toward `Core`; do not add projects or packages unless the request requires them.

## Change Workflow

- For any change larger than one method, propose a short plan of no more than five bullets and wait for approval before editing.
- Ask at most two clarifying questions. If requirements remain unclear, state the assumption and continue.
- Write the simplest code that satisfies the request. Do not add speculative abstractions, extra features, unused code, or projects.
- For new behavior, write tests first.
- After every change, build and run the tests with `dotnet test` from the solution root, then report the result.
- Do not invent APIs or packages. If an API or package is uncertain, say so and verify it before proceeding.

## Architecture and Domain Rules

- Tenant identity comes only from the authenticated context. Never accept tenant identity from request bodies, headers, or query strings.
- Every store method must receive `tenantId` and filter all reads and writes by that tenant.
- Version checking and writing must be one atomic operation.
- Never store or log secret values.
- Preserve the interface boundary between `Core` and `Data`; domain and application logic must not depend directly on the in-memory store implementation.

## Testing

- Prefer focused xUnit tests for the affected behavior.
- Include tests for tenant isolation, authenticated-context tenant resolution, and atomic version behavior when those paths are changed.
- Keep tests deterministic and independent; use the in-memory store through its interface where practical.
