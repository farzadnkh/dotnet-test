# .NET QA Sandbox (Backend + Real-Time Validation)

Collection of .NET-focused QA samples and backend testing artifacts related to fintech-oriented systems, including API integration and real-time data scenarios.

## Contents

- Exchange-rate provider related testing assets
- KYC/client integration samples
- Contract and integration-test style examples

## QA Focus

- Backend correctness and API validation
- Real-time behavior checks (streaming assumptions, failover scenarios)
- Security-aware testing around auth/token flows
- CI suitability for release-time quality gates

## Notes

- This repo is intentionally sanitized for public sharing.
- Any credential-like values are placeholders only.

## Usage

Each subproject under this repository has its own build/run commands and dependencies. Open the target folder and run standard .NET commands:

```bash
dotnet restore
dotnet build
dotnet test
```
