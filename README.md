# Bank API 🏦

[![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa]
![GitHub commit activity](https://img.shields.io/github/commit-activity/m/erwinkramer/bank-api)

![Scalar landing page](.images/scalar-landingpage.png)

![Aspire](.images/aspire.png)

The Bank API is a design reference project suitable to bootstrap development for a compliant and modern API.

## Technology stack

- [ASP.NET Core 9.0 - Minimal API](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview?view=aspnetcore-9.0) for API development, with following base services:

  - [Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=package-reference) for resilience when calling downstream APIs

  - [Compliance](https://andrewlock.net/redacting-sensitive-data-with-microsoft-extensions-compliance/) for redacting sensitive data

  - [Health checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-9.0) for checking health status

  - [Service Discovery](https://learn.microsoft.com/en-us/dotnet/core/extensions/service-discovery?tabs=package-reference) for resolving endpoints from config

  - [Hybrid Cache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0) for caching

  - [Rate limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-9.0) for rate limiting

  - [API Key, JWT bearer and OpenID Connection-based authentication](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-9.0#configuring-authentication-strategy) for security

  - [OpenApi](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-9.0&tabs=visual-studio) for generating OpenAPI specifications

- [OpenTelemetry (OTel)](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel) for observability

- [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview) for development bootstrapping and client integrations

- [Kiota API client generation](https://learn.microsoft.com/en-us/openapi/kiota/using#client-generation) for calling downstream APIs

- [Gridify](https://alirezanet.github.io/Gridify) for filtering, ordering and paging

- [Scalar](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/using-openapi-documents?view=aspnetcore-9.0#use-scalar-for-interactive-api-documentation) for interactive API documentation

- [Spectral](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/using-openapi-documents?view=aspnetcore-9.0#lint-generated-openapi-documents-with-spectral) for linting

- [OpenApiAnyFactory](./BankApi.Service/Defaults/Helper.OpenApiAnyFactory.cs) from [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) for parsing JSON to `IOpenApiAny` types

- [TUnit](https://thomhurst.github.io/TUnit/docs/intro) for unit tests

- [REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) in Visual Studio Code for quick local tests via `.http` files

## Prerequisites

If not using the [Dev Container](.devcontainer/devcontainer.json), install:

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

- All [recommended Visual Studio Code extensions](.vscode/extensions.json)

## Quick start

- (Optionally) regenerate the GitHub downstream API client by going to the [Kiota workspace](.kiota/workspace.json) and clicking `Re-generate` under `clients`.

- Generate a new JWT-token for secured endpoints:

    ```powershell
    dotnet user-jwts create --scope "bank_api" --role "banker"
    ```

- Run `dotnet build` to output the OpenAPI definition

- Validate the OpenAPI definition by going to the [generated.json](/Specs.Generated/openapi.json) definition and check for problems via the Spectral extension.

### Run in Aspire minimal mode

This mode just runs the ASP.NET Core API.

1. Start the standalone Aspire Dashboard for developer visualization:

    ```powershell
    docker run --rm -it `
    -p 18888:18888 `
    -p 4317:18889 `
    --name aspire-dashboard `
    mcr.microsoft.com/dotnet/aspire-dashboard:latest
    ```

    Copy the url shown in the resulting output when running the container, and replace `0.0.0.0` with `localhost`, eg <http://localhost:18888/login?t=123456780abcdef123456780> and open that in your browser, or you can also paste the key after `/login?t=` when the login dialog is shown. The token will change each time you start the container.

2. Run the [launch config](.vscode/launch.json) `C#: Api Debug (with standalone Aspire)`.

### Run in Aspire mode

This mode starts the API in the context of .NET Aspire.

1. Make sure the docker runtime is started.

2. Run the [launch config](.vscode/launch.json) `C#: App Host Debug (via Aspire)`.

## Considerations

1. [OpenID Connect isn't fully supported in Scalar](https://github.com/scalar/scalar/issues/3656).

2. Running tests works in VSCode. However, [debugging tests doesn't work with TUnit in VSCode yet](https://github.com/microsoft/vscode-dotnettools/issues/1616#issue-2669360822).

3. To extend OpenTelemetry logging to Application Insights, [expand the OpenTelemetry exporter](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-applicationinsights).

4. [The Aspire dashboard doesn't start the first time inside the Dev Container](https://github.com/dotnet/aspire/issues/6829), open a new tab and paste the same URL, then it works.

5. [The compliance NullRedactor doesn't seem to work](https://github.com/dotnet/extensions/issues/5691), the redactor is already defined at [Builder.Compliance.cs](/BankApi.Service/Defaults/Builder.Compliance.cs) but not used because of the issue.

6. Dev Containers with the `docker-outside-of-docker` feature instead of `docker-in-docker` [do not work](https://github.com/dotnet/aspire/issues/6830), for now we're using `docker-in-docker`.

Please see the Reddit [r/dotnet post](https://www.reddit.com/r/dotnet/comments/1gyql5a/bank_api_modern_api_reference_project/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button) about this project for more considerations and information.

## Troubleshooting

- If debugging isn't working properly inside a Dev Container, please clear the Extension Host Cache at `%AppData%\Code\CachedData` (on Windows) and restart VS Code.

## License

This work is licensed under a
[Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License][cc-by-nc-sa].

[![CC BY-NC-SA 4.0][cc-by-nc-sa-image]][cc-by-nc-sa]

[cc-by-nc-sa]: http://creativecommons.org/licenses/by-nc-sa/4.0/
[cc-by-nc-sa-image]: https://licensebuttons.net/l/by-nc-sa/4.0/88x31.png
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg
