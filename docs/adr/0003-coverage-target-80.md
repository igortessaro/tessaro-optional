# ADR 0003 — Coverage target of 80 % using SonarCloud's default quality gate

**Status:** Accepted (2026-11-07) · **Date:** 2026-11-07

## Context

Constitution v1.0.0 required ≥ 95 % coverage enforced by the SonarCloud quality gate. The default
"Sonar way" gate requires 80 % on new code; a custom gate may require a paid SonarCloud plan, and
maintaining one adds effort for little gain on a small library whose acceptance criteria are each
mapped to tests anyway.

## Decision

- Minimum coverage is **80 % line and branch**.
- The test run in CI fails below that threshold (fail fast, independent of SonarCloud).
- SonarCloud uses its default quality gate; no custom gate for now.
- Constitution bumped to v1.1.0.

## Consequences

**Positive**

- No dependency on a paid plan or on maintaining a custom gate.
- The local and CI threshold and SonarCloud's gate agree on the same number.

**Negative / trade-offs**

- A lower floor. Mitigated by the spec's rule that every acceptance criterion has a test.

## Alternatives considered

- **Keep 95 % via coverlet only** — stricter, but the owner judged it unnecessary for now.
- **Custom 95 % SonarCloud gate** — possible cost and extra maintenance.
