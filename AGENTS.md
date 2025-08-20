# Agent Instructions for Sharp2D

This file outlines conventions and practices for AI agents working in this repository.

## Repository Structure
- The repository root contains a nested `Sharp2D/` directory holding the solution file `Sharp2D.sln` and project folders.
- The core library code lives under `Sharp2D/Sharp2D`.
- Example projects reside in `Sharp2D/AnimationPreview`, `Sharp2D/Fireflies`, `Sharp2D/SomeGame`, and `Sharp2D/TestGame`.
- Use the .NET SDK version specified in `Sharp2D/global.json` (currently 8.0).

## Coding Conventions
- Use four spaces for indentation; no tabs.
- Place opening braces on a new line (Allman style).
- Use PascalCase for class, method, and property names; use camelCase for local variables and private fields.
- Keep `using` directives at the top of files, grouped with `System` namespaces first and sorted alphabetically.
- End every file with a newline.

## Build and Test
- Before committing, run:
  ```bash
  cd Sharp2D
  dotnet build
  dotnet test
  ```
  Run these commands even for documentation-only changes to ensure the solution builds and tests (if any) pass.
- If tests are absent, `dotnet test` will report that no tests were found; this is acceptable.

## Git and PR Guidelines
- Work directly on the default branch (no new branches).
- Commit messages should be concise and descriptive.
- Leave the working tree clean after each commit.
- Pull requests and final responses should summarize changes and list the commands run for verification.

## Utility Tips
- Prefer `rg` for searching the codebase instead of `grep -R` or `ls -R`.
- Follow any additional instructions in nested `AGENTS.md` files when working in subdirectories.
