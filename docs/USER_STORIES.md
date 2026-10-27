---
codex: 1
project: GTFO.SkipDrop
code: G.SD
layer: stories
status: living
updated: 2026-10-16
---
<!-- GENERATED from MindAttic.Tickets/data/Tickets.db by `tickets db render`. Do not edit: this file is rewritten from the database; change stories and tickets with the tickets CLI. -->

# GTFO.SkipDrop — User Stories
> ✅ done (shipped & tested) · 🟡 partial · ⬜ planned. Every ✅ cites the test.

## Priority backlog
_Nothing recorded yet._

## Tickets
> Permanent log of every change and fix — this repo's ticket system. Rows are append-only: never
> delete or reword one; only move its Status (open → closed / wont-do) and fill in Ref. Status is
> a word, not a glyph, so tickets don't count toward the story index. Keep this section last.

| ID | Date | Type | Status | Summary | Ref |
|---|---|---|---|---|---|
| G.SD-T1 | 2026-10-16 | docs | closed | Start the permanent ticket log in the workflow database | — |
| G.SD-T2 | 2026-10-16 | chore | closed | install.ps1 adds this repo's 15 .prose/commands stubs, AGENTS.md pointer for the shared global commands, so any LLM client finds them | mindattic-agent-standard/install.ps1 run 2026-10-16 |
| G.SD-T3 | 2026-10-16 | chore | closed | Bug hunt: run an exhaustive /bughunt over the whole repo and fix every real bug found (each fix under its own fix ticket), then hunt again; close this only when a full pass finds nothing | bughunt full pass 2026-10-19 on fed6270: 0 bugs; dotnet test 16/16 pass, 0 warnings |
| G.SD-T4 | 2026-10-16 | chore | closed | Run the full unit test suite and fix every failure (each under its own fix ticket) until it passes; put the passing run in --ref. No test project: close this wont-do saying so | dotnet test tests/GTFO.SkipDrop.Tests: Passed 16/16, 0 failed, at origin/main fed6270 |
| G.SD-T5 | 2026-10-16 | chore | closed | Clean up code and docs: remove dead code, stale comments and outdated or duplicated docs, and simplify where behaviour doesn't change (/simplify); tests stay green | 3a9239d |
| G.SD-T6 | 2026-10-16 | docs | closed | Bring README.md up to date with the code as it is now (what it is, setup, usage, commands, current state), following codex-standard/README-STYLE.md; README.htm regenerated if the repo has one | 28919ff |
| G.SD-T7 | 2026-10-16 | chore | closed | Add logo.png, icon.png, tests; Update README.md, USER_STORIES.md, CHANGELOG.md, manifest.json, Plugin.cs | — |
| G.SD-T8 | 2026-10-18 | feature | closed | Master switch: [0. Mod] Enabled as the cfg's first entry; off means the mod loads nothing (user request) | — |
| G.SD-T9 | 2026-10-18 | fix | closed | A drop cut short (leaving or the expedition ending mid-drop) restores the fast-elevator dev flag when the elevator is cleaned up, instead of leaving it on until the next drop | cd683c2 |
| G.SD-T10 | 2026-10-19 | chore | wont-do | Convert tracked source files to LF line endings via .gitattributes, per HOUSE-LAW-13 (low priority) | — |
| G.SD-T11 | 2026-10-20 | chore | closed | Bump GTFO.SkipDrop to v5.0.0 for release: changelog for the corrected Thunderstore description (user request) | 123ffa0 |
| G.SD-T12 | 2026-10-20 | docs | open | Publish GTFO.SkipDrop docs to GitHub Pages via the shared Agency DocFX pipeline: run adopt-docs-site.ps1 (README.md as home page, docs/*.md as articles, API reference only where the code builds in CI), enable Pages with build_type=workflow, set the repo homepage URL, retire tools/build-readme.ps1 and README.htm, confirm the docs Actions run is green and https://mindattic.github.io/GTFO.SkipDrop/ returns 200. Depends on MAT-T8. | — |
| G.SD-T13 | 2026-10-22 | docs | closed | Update USER_STORIES.md | — |
| G.SD-T14 | 2026-10-24 | docs | closed | Update USER_STORIES.md | — |
| G.SD-T15 | 2026-10-24 | chore | closed | Joins the GTFO.EZ family's v17 release (previously numbered independently); no functional change | package/manifest.json |
| G.SD-T16 | 2026-10-26 | fix | closed | Bughunt: RestoreAfterDrop now only clears the saved FastElevator flag after a successful restore, so a failed SetValue no longer permanently discards the real original value | Plugin.cs |
