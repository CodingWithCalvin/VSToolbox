# Contributing to VSToolbox

Thank you for your interest in contributing to VSToolbox! This document provides guidelines and instructions for contributing.

## Development Environment Setup

### Prerequisites

- Windows 10/11
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) (Preview)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with the following workloads:
  - .NET Desktop Development
  - Windows App SDK C# Templates
- Git

### Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/VSToolbox.git
   cd VSToolbox
   ```
3. Add the upstream remote:
   ```bash
   git remote add upstream https://github.com/CodingWithCalvin/VSToolbox.git
   ```
4. Build the project:
   ```bash
   dotnet build src/CodingWithCalvin.VSToolbox/CodingWithCalvin.VSToolbox.csproj
   ```
5. Run the application:
   ```bash
   dotnet run --project src/CodingWithCalvin.VSToolbox/CodingWithCalvin.VSToolbox.csproj
   ```

## Code Style and Conventions

### General Guidelines

- Follow standard C# coding conventions
- Use meaningful variable and method names
- Keep methods focused and concise
- All UI must use WinUI 3/XAML with Fluent Design
- Use theme resources for colors (no hardcoded colors except brand purple `#68217A`)

### Project Structure

- **CodingWithCalvin.VSToolbox** - Main WinUI 3 application
- **CodingWithCalvin.VSToolbox.Core** - Core library with models and services

## Branch Naming Conventions

Use the format: `type/short-description`

Examples:
- `feat/settings-dialog`
- `fix/tray-icon-crash`
- `docs/update-readme`

## Commit Message Format

We use [Conventional Commits](https://www.conventionalcommits.org/). Format:

```
type(scope): description
```

### Commit Types

| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `test` | Adding or updating tests |
| `chore` | Maintenance tasks |
| `ci` | CI/CD changes |

### Examples

```
feat(tray): add context menu for quick actions
fix(detection): handle missing vswhere gracefully
docs(readme): update installation instructions
```

## Submitting Pull Requests

### Before You Start

1. Check existing issues and PRs to avoid duplicate work
2. For significant changes, open an issue first to discuss your approach
3. Create a new branch from an updated `main` branch

### Pull Request Process

1. Update your fork:
   ```bash
   git checkout main
   git pull upstream main
   ```

2. Create a feature branch:
   ```bash
   git checkout -b feat/your-feature-name
   ```

3. Make your changes and commit using conventional commit format

4. Push to your fork:
   ```bash
   git push origin feat/your-feature-name
   ```

5. Open a pull request against `main`

### Pull Request Guidelines

- Use conventional commit format for the PR title (e.g., `feat(scope): description`)
- Provide a clear description of the changes
- Reference any related issues (e.g., "Closes #123")
- Ensure the build passes
- Keep PRs focused - one feature or fix per PR

## Testing Requirements

- Ensure the application builds without errors
- Test your changes manually before submitting
- Verify the application runs correctly on Windows 10/11

## Getting Help

- Open an issue for bugs or feature requests
- Use discussions for questions and general help

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.
