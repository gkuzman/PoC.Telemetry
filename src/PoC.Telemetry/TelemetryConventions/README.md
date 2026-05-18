# Generating Semantic conventions

## csharp

### Bash / Linux / macOS

```shell
podman run --rm \
        -v $(pwd)/Shared:/output \
        -v $(pwd)/TelemetryConventions:/conventions \
        -v $(pwd)/templates:/templates \
        otel/weaver:latest \
        registry generate csharp \
        --registry=/conventions \
        --templates=/templates \
        /output/
```

### PowerShell (Windows)

```powershell
podman run --rm `
        -v "${PWD}/Shared:/output" `
        -v "${PWD}/TelemetryConventions:/conventions" `
        -v "${PWD}/templates:/templates" `
        otel/weaver:latest `
        registry generate csharp `
        --registry=/conventions `
        --templates=/templates `
        /output/
```

## markdown

### Bash / Linux / macOS

```shell
podman run --rm \
        -v $(pwd)/TelemetryDocs:/output \
        -v $(pwd)/TelemetryConventions:/conventions \
        otel/weaver:latest \
        registry generate markdown \
        --registry=/conventions \
        --templates=https://github.com/open-telemetry/semantic-conventions/archive/refs/tags/v1.33.0.zip\[templates\] \
        /output/
```

### PowerShell (Windows)

```powershell
podman run --rm `
        -v "${PWD}/TelemetryDocs:/output" `
        -v "${PWD}/TelemetryConventions:/conventions" `
        otel/weaver:latest `
        registry generate markdown `
        --registry=/conventions `
        "--templates=https://github.com/open-telemetry/semantic-conventions/archive/refs/tags/v1.33.0.zip[templates]" `
        /output/
```
