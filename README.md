# SUGCON 2025 OpenTelemetry with JSS head and .NET Core

...

<!--

TODO:

- powerpoint
    - hvad vil vi løse? composable, architecture, multiple components, sitecore stack
    - hvad er open telemetry
    - standardiseret, mange teknologier, ens koncepter
    - hvilke dele er der
    - SDK og zero-code
    - Lightweight dev setup such as aspire dashboard
    - Production ready stacks such as signoz, grafana, cloud offerings such as SigNoz cloud, Application Insights, New Relic, Datadog
    - demo
    - what did we see...
      - traces across applications and technologies
      - aspire dashboard (simple dev focused)
      - signoz
      - application insights (vercel)
    - metrics
      - other: grafana/tempo, new relic, data dog, etc.
      - application level
      - OS level
    - perspektiv
      - client instrumentation
      - cloud infrastructure
- SQL tracing, måske det en bedre demo med https://opentelemetry.io/docs/collector/configuration/#processors så man dropper det man IKK vil have UDEN for applicationen?
- en tidlig processor der kan igore activites, fx ashx/media samt statiske filer
- ....

-->

## Pre-requisites and Dependencies

- Windows 11 24H2 or later
- Docker Desktop >= **v4.39.0**
- Valid Sitecore license at `C:\license\license.xml`

## Running

First run, then:

1. `.\init.ps1`
1. `.\up.ps1 -RebuildIndexes`

Else just `.\up.ps1`

## Service urls

- <https://aspire-dashboard.sugcon2025otel.localhost
- <https://cm-sc-platform.sugcon2025otel.localhost
- <https://id-sc-platform.sugcon2025otel.localhost
- <https://solr-sc-platform.sugcon2025otel.localhost
