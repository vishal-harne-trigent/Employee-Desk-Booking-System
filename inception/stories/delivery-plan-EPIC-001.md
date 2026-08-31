# Delivery plan — EPIC-001 Employee Desk Booking

> **Baseline revised** by delivery manager review on **2026-08-27**; **single-sprint grouping** agreed in conversation (supersedes Sprints 0–4). All numbers measure **agent completion** — not human effort, person-days, or traditional sprint calendars. One sprint = EPIC-001 MVP; it is done when the listed personas finish their agent runtime (Composer 2.5 Fast wall-clock while a human operator drives Cursor). Human D1/D2 approval, CI queue wait, and manual exploratory testing are **outside** these totals.

| | |
| --- | --- |
| **Epic** | EPIC-001 |
| **Traces to** | BRD-001, SRS-001, US-001 … US-009 |
| **Locked by** | Delivery manager (human approval in conversation, 2026-08-27; single sprint) |
| **Operator** | 1 human drives agents in Cursor (`/dev`, `/architect`, `/qa`, `/devops`) — not a staffed dev/QA calendar team |
| **AI runtime** | Cursor · Composer 2.5 Fast · AIDLC personas |
| **Estimate basis** | Agent active runtime until Sprint 1 (full epic) is complete |
| **/qa total** | 7–10 h agent (test derivation + AC verification across US-001 … US-009) |

## What we measure

| Included | Excluded |
| -------- | -------- |
| `/dev` spec package (plan, impact, traceability seed) | Unit / integration test authoring |
| `/dev` feature implementation (API, services, UI) | Test execution (`dotnet test`, CI test runs) |
| `/architect` and `/devops` agent sessions | Human D1/D2 review wait |
| Agent fix loops for lint/build (non-test) failures | Manual exploratory testing |
| Re-prompting agent when output needs correction | |

**One story = one PR** (`feat/US-###-<slug>`). Rows below are `/dev` agent runtime per story — `/qa` is **7–10 h epic total**, not repeated per phase.

**Note on existing code:** Scaffold implementation (API, Web, domain) is already in the repo. Agent time below assumes retrofit to match story ACs, not greenfield — roughly **30–40 % less** than building from scratch.

## Agent session model (per story — implementation only)

| Agent session | Persona | Composer 2.5 Fast runtime |
| ------------- | ------- | ------------------------- |
| Spec package (plan, impact, traceability seed) | `/dev` | 20–35 min |
| Feature implementation (no tests) | `/dev` | 30–90 min |
| Lint/build fix loop (1 round, non-test) | `/dev` | 10–20 min |
| Advisory design note (Complex tier only) | `/architect` | 15–25 min |
| Test derivation + AC verification (epic total) | `/qa` | 7–10 h |

## Story point → agent time mapping (implementation only)

| Story Points | Spec | Implementation | Fix loop | Total agent |
| ------------ | ---- | -------------- | -------- | ----------- |
| **3 SP** | 20 min | 25–40 min | 10 min | ~1–1.25 h |
| **5 SP** | 25 min | 40–65 min | 15 min | ~1.25–1.75 h |
| **8 SP** | 30 min | 60–90 min | 20 min | ~1.75–2.5 h |

## Per-story agent effort (implementation only)

| Story | Title | SP | Spec | Code | Fix | Total agent | Jira |
| ----- | ----- | -- | ---- | ---- | --- | ----------- | ---- |
| US-001 | Sign in / sign out | 5 | 25 min | 40–65 min | 15 min | ~1.25–1.75 h | EDBS-38 |
| US-002 | Book a desk | 8 | 30 min | 60–90 min | 20 min | ~1.75–2.5 h | EDBS-39 |
| US-003 | My bookings | 5 | 25 min | 40–65 min | 15 min | ~1.25–1.75 h | EDBS-40 |
| US-004 | Admin all bookings | 5 | 25 min | 40–65 min | 15 min | ~1.25–1.75 h | EDBS-41 |
| US-005 | Admin manage desks | 5 | 25 min | 40–65 min | 15 min | ~1.25–1.75 h | EDBS-42 |
| US-006 | Admin manage users | 8 | 30 min | 60–90 min | 20 min | ~1.75–2.5 h | EDBS-43 |
| US-007 | Booking email notifications | 8 | 30 min | 70–100 min | 20 min | ~2–2.5 h | EDBS-44 |
| US-008 | Browser push preferences | 5 | 25 min | 40–65 min | 15 min | ~1.25–1.75 h | EDBS-45 |
| US-009 | Auto-complete past bookings | 3 | 20 min | 25–40 min | 10 min | ~1–1.25 h | EDBS-46 |
| **Stories subtotal** | | **52 SP** | | | | **~12–18 h** | |

## Sprint 1 — EPIC-001 MVP (single sprint)

**Scope:** Foundation + US-001 … US-009 in one sprint. Sprint 1 is complete when agents have consumed **~21–31 h cumulative runtime** for the whole epic — not when a human team would finish the same scope on a calendar.

### Execution phases (order within Sprint 1)

| Phase | Work | SP | /dev | /architect | /qa | /devops | Phase agent |
| ----- | ---- | -- | ---- | ---------- | --- | ------- | ----------- |
| **A — Foundation** | Architecture, CI, exit criteria | — | — | 0.4–0.7 h | — | 0.75–1.5 h | 1.25–2.25 h |
| **B — Auth + booking** | US-001, US-002 | 13 | 3–4.25 h | 0.25–0.4 h | — | — | 4.75–6.65 h |
| **C — Employee** | US-003, US-009 | 8 | 2.25–3 h | — | — | — | 3.5–4.75 h |
| **D — Admin** | US-004, US-005, US-006 | 18 | 4.25–6 h | 0.25–0.4 h | — | — | 6.5–9.4 h |
| **E — Notifications** | US-007, US-008 | 13 | 3.25–4.25 h | 0.25–0.4 h | — | — | 5.25–7.15 h |
| **Sprint 1 total** | Full MVP | **52** | **12–18 h** | **1.15–1.9 h** | **7–10 h** | **0.75–1.5 h** | **~21–31 h agent** |

**/qa:** 7–10 h agent after each story (phases B–E), per `ai/gates/delivery.md` — not split per phase in the table above.

Test project scaffold and AC test naming for `/qa` are part of **Phase A exit** (CI + test shell).

**Exit criteria (human/PO — not agent time):** architecture current; BRD Q #1 / #5 decided; CI runs `aidlc-check` and test runner shell.

## Operator calendar (Sprint 1 complete)

Maps **~21–31 h total agent runtime** → working days the operator runs Cursor. **Not** human person-days.

| Operator throughput | Days until agents finish Sprint 1 | Meaning |
| ------------------- | --------------------------------- | ------- |
| **Aggressive** — ~8 h agent runtime / day | ~3–4 working days | Operator runs persona chains until ~21–31 h consumed |
| **Steady** — ~4 h agent runtime / day | ~5–8 working days | One story chain at a time |
| **Conservative** — ~2 h agent runtime / day | ~10–15 working days | Part-time agent driving |

Human wall calendar (PO review, D1/D2, merge, SMTP Q #7) runs **after** Sprint 1 agent completion and is not counted above.

## Sequential execution pattern (Sprint 1)

```
Phase A (~1.25–2.25 h agent):  [/architect] → [/devops CI]

Phase B–E (stories, dependency order):
  US-001 → US-002 → … → US-007 ∥ US-008
  (/qa — 7–10 h total — after each story merge)
```

## Story dependencies (within Sprint 1)

| Rule | Detail |
| ---- | ------ |
| US-001 before US-002 | Auth before booking |
| US-001+002 before US-003, US-009 | Employee lists need auth + bookings |
| US-001 before US-004–006 | Admin authz |
| BRD Q #6 before US-005 | Desk deactivate UX |
| US-002–004 before US-007 | Booking events for email |
| US-008 | Can run parallel with US-007 after US-002 |

**Sprint 1 agent-complete** = Phase E agents finished (~21–31 h cumulative). Human gates, merges, and exploratory QA are after that agent runtime.

## Agent session playbook (agent completes each step)

| Order | Prompt | Agent completes in |
| ----- | ------ | ------------------ |
| 1 | `/dev` → spec package for US-### | 20–35 min agent |
| 2 | `/dev` → implement US-### (no tests) | 30–90 min agent |
| 3 | `/dev` → fix lint/build findings | 10–20 min agent |
| 4 | `/qa` → tests from AC (epic 7–10 h total) | after each story merge |

Human D1/D2 gates sit **outside** agent runtime.

## Risks (agent-specific)

| Risk | Agent-time impact | Mitigation |
| ---- | ----------------- | ---------- |
| Complex-tier story misclassified as Simple | +45–90 min rework | Escalate to thinking model for US-002, US-006, US-007 |
| Build red on first push (non-test) | +10–20 min fix loop | Build locally before ending agent session |
| Existing code drift vs story ACs | +20–40 min reconciliation | Story AC is source of truth |
| SMTP undecided (BRD Q #7) | Blocks US-007 merge, not draft | Agent drafts with dev mail catcher |
| Multi-story PR (scope creep) | +30–60 min untangle | One story per `/dev` prompt |

## Shareable summary

> **Employee Desk Booking — EPIC-001** · baseline **2026-08-27** · one sprint · Cursor Composer 2.5 Fast + AIDLC personas  
> **Scope:** 9 stories · 52 SP · 44 ACs · 7 screens · one story = one PR (`feat/US-###-<slug>`)  
> **Measure:** Sprint 1 complete when agents finish **~21–31 h** — not human calendar or FTE estimates.

### Sprint 1 — agent completion by role

| Sprint | Phases / stories | SP | /dev | /architect | /qa | /devops | Sprint complete (agents) |
| ------ | ---------------- | -- | ---- | ---------- | --- | ------- | ------------------------ |
| **1 — EPIC-001 MVP** | A foundation → B–E US-001…US-009 | 52 | 12–18 h | 1.15–1.9 h | 7–10 h | 0.75–1.5 h | **~21–31 h agent** |

**Operator calendar (Sprint 1):** ~3–4 days (aggressive) · ~5–8 days (steady) · ~10–15 days (conservative) — all agent-runtime days, not human team sprints.

**Next (agents):** Phase A — `/architect` + `/devops` ~1.25–2.25 h agent, then story agents US-001 onward; `/qa` 7–10 h across merges.

## Related

- Epic: [`inception/stories/epics/EPIC-001-desk-booking.md`](epics/EPIC-001-desk-booking.md)
- Requirements: [`inception/product/requirements/BRD-001-desk-booking.md`](../product/requirements/BRD-001-desk-booking.md), [`SRS-001-desk-booking.md`](../product/requirements/SRS-001-desk-booking.md)
- Traceability: [`knowledge/traceability/manifest.json`](../../knowledge/traceability/manifest.json)
- Delivery gate: [`ai/gates/delivery.md`](../../ai/gates/delivery.md)
