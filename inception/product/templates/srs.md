# SRS-### — <Product / feature name>

> Software Requirements Specification derived from an approved BRD. Approval = PO/BA human reviewing + merging this document's PR (Gate 1 companion artifact). GitHub records who approved what.

> **ID rule:** REQ/NFR/RISK IDs are defined only in the BRD. In SRS tables, reference them as `BRD-### / REQ-###` in the first column — never `| REQ-001 |` alone (aidlc-check treats that as a duplicate definition).

|                  |                                                                                  |
| ---------------- | -------------------------------------------------------------------------------- |
| **Author**       | BA persona (AI draft) with <human name>                                          |
| **Derived from** | `inception/product/requirements/BRD-###-<slug>.md` (approved baseline)          |
| **Source input** | Same inputs as parent BRD (see BRD § header)                                     |
| **Related**      | EPIC-###, screen specs SCR-### (when design exists)                              |

## 1. Introduction

### 1.1 Purpose

<Who reads this document and why — developers, QA, architects.>

### 1.2 Scope

<What the software product will and will not do; reference BRD out-of-scope.>

### 1.3 Definitions and acronyms

| Term | Definition |
| ---- | ---------- |

### 1.4 References

| Document | Location |
| -------- | -------- |

### 1.5 Document overview

<Brief map of remaining sections.>

## 2. Overall description

### 2.1 Product perspective

<System context: web app, single office, actors.>

### 2.2 Product functions

<High-level capability list.>

### 2.3 User classes and characteristics

| User class | Characteristics | System access |
| ---------- | --------------- | ------------- |

### 2.4 Operating environment

<Browser, HTTPS, timezone, email service — or TBD.>

### 2.5 Design and implementation constraints

<From BRD constraints; no invented tech stack unless Architect ADR exists.>

### 2.6 Assumptions and dependencies

<Assumptions; external dependencies (SMTP, etc.).>

## 3. System features

> Use SRS-F-### IDs for software requirements. Trace to BRD REQ IDs in the last column (not as table row IDs).

### 3.1 <Feature area>

| ID | Software requirement | Priority | Traces to |
| -- | -------------------- | -------- | --------- |
| SRS-F-001 | | Must | BRD-001 / REQ-### |

## 4. External interface requirements

### 4.1 User interfaces

| Screen | Purpose | Primary user | Spec |
| ------ | ------- | ------------ | ---- |

### 4.2 Software interfaces

<APIs, email, push — or TBD.>

### 4.3 Communications interfaces

<Notification channels, protocols.>

## 5. Non-functional requirements

> Point to BRD §5 for canonical NFR definitions. Use `BRD-### / NFR-###` in the first column.

| BRD reference | Category | Implementation note | Priority |
| ------------- | -------- | ------------------- | -------- |

## 6. Data requirements

### 6.1 Logical data model

<Entities, key attributes, relationships — high level.>

### 6.2 Enumerations and lifecycles

<Status values, state transitions.>

## 7. Business rules

> Canonical rules remain in the parent BRD; this section indexes them for implementers.

| Rule ID | Summary | Affects (REQ) |
| ------- | ------- | ------------- |

## 8. Validation rules

| ID | Validation | Related |
| -- | ---------- | ------- |

## 9. Open items

| # | Item | Owner | Status |
| - | ---- | ----- | ------ |

## Appendix A — Requirement traceability

| BRD reference | SRS section | UI screen (if any) |
| ------------- | ----------- | ------------------ |
| BRD-001 / REQ-001 | | |
