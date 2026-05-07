# Contributing

## Branch naming

Personal feature branches use a `pp/<yourname>/...` prefix. Set it up once:

```bash
./scripts/setup-pp.sh yourname
```

This configures git so new branches default to your prefix. See [scripts/README.md](../scripts/README.md).

## Pre-commit hook

C# formatting is enforced by `dotnet format` via a pre-commit hook:

```bash
./scripts/setup-hooks.sh
```

Run formatting manually with:

```bash
dotnet format Glense.sln
```

## Pull requests

- One feature/fix per PR.
- Run `dotnet test` and `npm run lint` (in `glense.client/`) before pushing.
- At least one approval required before merge.
- Update the relevant doc(s) in this repo when behavior or API surface changes:
  - New endpoint → update the service's file under [api/](api/).
  - New event/queue → update [ARCHITECTURE.md](ARCHITECTURE.md) and add a flow under [flows/](flows/).
  - New env var → update [CONFIGURATION.md](CONFIGURATION.md) and [.env.example](../.env.example).

## Local sanity check

```bash
./dev.sh                   # full stack + seed
dotnet test                # all integration tests
cd glense.client && npm run dev   # frontend
```
