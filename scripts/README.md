# Scripts

| Script | Purpose |
|--------|---------|
| [seed.sh](seed.sh) | Seeds three test users (`keki`, `irena`, `branko` — password `Password123!`), wallets ($500 each), sample videos and chats. Run after `./dev.sh`. |
| [run_integration_tests.ps1](run_integration_tests.ps1) | PowerShell test runner. `-Project Account|Video|Donation|Chat`, `-Filter "FullyQualifiedName~Auth"`. |
| [run_all_tests.py](run_all_tests.py) | Cross-platform Python test runner (alternative to the PowerShell one). |
| [setup-account-service.sh](setup-account-service.sh) | Bootstrap just the Account service (legacy single-service workflow). |
| [setup-pp.sh](setup-pp.sh) | Configure git so personal branches default to a `pp/<yourname>/` prefix. Usage: `./setup-pp.sh yourname`. |
| [setup-hooks.sh](setup-hooks.sh) | Install the pre-commit hook that runs `dotnet format`. |

Top-level [`dev.sh`](../dev.sh) is the main entry point for local development.
