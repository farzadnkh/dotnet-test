# .NET QA Engineering Portfolio

![QA Focus](https://img.shields.io/badge/QA-Backend%20%26%20Real--Time-1f6feb)
![Platform](https://img.shields.io/badge/Platform-.NET-512BD4)
![Domain](https://img.shields.io/badge/Domain-Fintech-informational)
![Security](https://img.shields.io/badge/Public%20Data-Sanitized-success)

Backend-focused QA samples for fintech and real-time systems.

## Architecture Overview

```mermaid
flowchart LR
  C[Client / Consumer] --> API[API Services]
  API --> AUTH[Token/Auth Layer]
  API --> WS[Streaming/WebSocket Layer]
  WS --> PROV[Multi-Provider Sources]
  API --> VAL[Validation & Business Rules]
  VAL --> REP[Test Reports / CI Signals]
```

## Test Strategy

- **Contract validation:** request/response schema and version safety
- **Business-rule checks:** pricing and provider consistency controls
- **Real-time checks:** reconnection, heartbeat, stale-data, failover behavior
- **Security-negative checks:** invalid token/access patterns and exposure prevention
- **Release-readiness:** repeatable tests for CI quality gates

## Repository Map

- `CoreTestSample/` - exchange-rate provider QA-related assets
- `NT KYC Jibit_/` - KYC client testing and integration samples

## Run Basics

```bash
dotnet restore
dotnet build
dotnet test
```

## Security Note

All public values are placeholders. Do not commit credentials, private feeds, or runtime secrets.
