# SUGCON 2025 OpenTelemetry with JSS head and .NET Core

...

<!--

TODO:

- powerpoint
    - hvad vil vi løse? composable, architecture, multiple components, sitecore stack
    - hvad er open telemetry
    - standardiseret, mange teknologier, ens koncepter
    - hvilke dele er der
      - No collector node, collector agent and collector gateway modes
    - SDK og zero-code
    - Lightweight dev setup such as aspire dashboard
    - Production ready stacks such as signoz, grafana, cloud offerings such as SigNoz cloud, Application Insights, New Relic, Datadog
    - demo
        - show trace from traefik -> cm -> sql & solr
        - show trace from traefik -> jss -> .NET api -> cm -> sql & solr
        - show trace tags
        - show pipeline instrumentation
        - show sql instrumentation and why NOT use it in Sitecore 10.4
        - show logs
        - show metrics
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
