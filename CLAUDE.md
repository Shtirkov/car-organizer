# CLAUDE.md

Guidance for agents working in this repo. Keep it short — it is read on every session.

`car-organizer` is a vehicle maintenance tracker. The repo is currently **backend-only**: a .NET
solution under `backend/` (`Domain` / `Application` / `Infrastructure` / `API`, with `tests/`).

## No frontend yet

`frontend/` was deleted deliberately and stays deleted until the backend is complete. Its absence
is not an oversight. Do not recreate it, scaffold client code, or add frontend tooling — the
frontend gets its own setup session, with its own dedicated `CLAUDE.md`, when work on it starts.

## Agent skills

Per-repo configuration for Matt Pocock's engineering skills, installed globally in
`~/.claude/skills/`. Edit the files below directly; re-running `/setup-matt-pocock-skills` is only
for switching trackers or starting over.

### Issue tracker

GitHub Issues in `Shtirkov/car-organizer`, via the `gh` CLI. PRs are not a request surface. See
[`docs/agents/issue-tracker.md`](./docs/agents/issue-tracker.md).

### Triage labels

The five canonical roles, label strings unchanged. Only `wontfix` exists in the repo so far; the
other four need creating. See [`docs/agents/triage-labels.md`](./docs/agents/triage-labels.md).

### Domain docs

Single-context: [`CONTEXT.md`](./CONTEXT.md) is the glossary and [`docs/adr/`](./docs/adr/) holds
decisions. Neither exists yet — `/domain-modeling` creates them lazily. See
[`docs/agents/domain.md`](./docs/agents/domain.md).
