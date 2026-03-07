# .NET 9 Upgrade Plan

## Overview

This plan outlines the steps to upgrade the TorrentScrapper backend from .NET 8 to .NET 9.

## Current State Analysis

### Projects Currently on .NET 8
1. [`TorrentScrapper.Api`](../backend/src/TorrentScrapper.Api/TorrentScrapper.Api.csproj:4) - ASP.NET Core Web API
2. [`TorrentScrapper.Application`](../backend/src/TorrentScrapper.Application/TorrentScrapper.Application.csproj:5) - Application layer
3. [`TorrentScrapper.Infrastructure`](../backend/src/TorrentScrapper.Infrastructure/TorrentScrapper.Infrastructure.csproj:4) - Infrastructure layer
4. [`TorrentScrapper.Parsing`](../backend/src/TorrentScrapper.Parsing/TorrentScrapper.Parsing.csproj:4) - Parsing logic
5. [`TorrentScrapper.Tests.Integration`](../backend/tests/TorrentScrapper.Tests.Integration/TorrentScrapper.Tests.Integration.csproj:4) - Integration tests
6. [`TorrentScrapper.Tests.Unit`](../backend/tests/TorrentScrapper.Tests.Unit/TorrentScrapper.Tests.Unit.csproj:4) - Unit tests

### Package References to Update

#### TorrentScrapper.Api
- `Microsoft.AspNetCore.OpenApi`: 8.0.24 → 9.0.x
- `Swashbuckle.AspNetCore`: 6.6.2 (check for updates)

#### TorrentScrapper.Application
- `Microsoft.Extensions.Caching.Abstractions`: 8.0.0 → 9.0.x
- `Microsoft.Extensions.Configuration.Abstractions`: 8.0.0 → 9.0.x
- `Microsoft.Extensions.Configuration.Binder`: 8.0.0 → 9.0.x
- `Microsoft.Extensions.DependencyInjection.Abstractions`: 8.0.0 → 9.0.x
- `Microsoft.Extensions.Logging.Abstractions`: 8.0.0 → 9.0.x

#### TorrentScrapper.Infrastructure
- `Microsoft.Extensions.Configuration.Abstractions`: 8.0.0 → 9.0.x
- `Microsoft.Extensions.DependencyInjection.Abstractions`: 8.0.0 → 9.0.x
- `Microsoft.Extensions.Http`: 8.0.0 → 9.0.x
- `Microsoft.Extensions.Logging.Abstractions`: 8.0.0 → 9.0.x

#### TorrentScrapper.Parsing
- `AngleSharp`: 1.1.2 (check for updates)
- `Microsoft.Extensions.Http`: 8.0.0 → 9.0.x
- `Microsoft.Extensions.Logging.Abstractions`: 8.0.0 → 9.0.x

#### Test Projects
- `Microsoft.AspNetCore.Mvc.Testing`: 8.0.0 → 9.0.x
- `Microsoft.NET.Test.Sdk`: 17.8.0 → 17.11.1 (latest)
- `coverlet.collector`: 6.0.0 → 6.0.2 (latest)
- `xunit`: 2.5.3 (already latest)
- `xunit.runner.visualstudio`: 2.5.3 (already latest)
- `Moq`: 4.20.70 (check for updates)

## Migration Strategy

### Phase 1: Update Target Framework
Update the `<TargetFramework>` element in all 6 `.csproj` files from `net8.0` to `net9.0`.

### Phase 2: Update Package References
Update all Microsoft package references to their .NET 9 versions (9.0.x).

### Phase 3: Verification
1. Clean the solution (delete `bin` and `obj` directories)
2. Restore NuGet packages
3. Build the solution
4. Run all tests (unit and integration)
5. Verify the application runs correctly

### Phase 4: Documentation Update
Update [`specify-rules.md`](../.roo/rules/specify-rules.md:7) to reflect .NET 9 instead of .NET 10.

## Breaking Changes to Review

### .NET 9 Notable Changes
- No major breaking changes expected for this project
- Performance improvements in runtime and libraries
- Enhanced C# 13 language features support
- Improved garbage collection

## Rollback Plan

If issues arise:
1. Revert all `.csproj` files to .NET 8
2. Restore original package versions
3. Clean and rebuild

## Post-Upgrade Validation

- [ ] All projects build successfully
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] API endpoints respond correctly
- [ ] No runtime errors in logs
- [ ] Performance is maintained or improved

## Notes

- The project rules mention ".NET 10" but the current implementation is on .NET 8
- This upgrade to .NET 9 will align with the latest LTS release
- Consider updating to .NET 10 when it becomes available (projected November 2026)
