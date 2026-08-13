# EDBS Wireframes — Figma plugin

Generates **low-fidelity wireframes** for the Employee Desk Booking System from the approved screen specs (`SCR-001` … `SCR-004`). Each frame is named `SCR-###/ST-## — Title` so it lines up with the design brief in `inception/design/screens/`.

## What it creates

| Screen | Frames |
| ------ | ------ |
| Sign In | 4 |
| Book a Desk | 7 |
| My Bookings | 5 |
| Admin Bookings | 6 |

**22 frames total** on a Figma page named **EDBS Wireframes**.

## Setup

1. Install dependencies:

   ```bash
   cd tools/EDBS_Wireframes
   npm install
   ```

2. Build the plugin (no npm install required if `code.js` is committed):

   ```bash
   npm run build
   ```

   This runs `node build-code.js` from `wireframe-data.ts` + `code-runtime.js`. Optional: `npm install` then `npm run build:tsc` if you prefer TypeScript compilation.

3. In Figma: **Plugins → Development → Import plugin from manifest…** and select this folder (`manifest.json`).

## Run

1. Open a Figma file (new or existing).
2. **Plugins → Development → EDBS_Wireframes**.
3. Click **Generate wireframes**.

Re-running replaces existing `SCR-*` frames on the **EDBS Wireframes** page.

## Relationship to the repo design artifacts

| Location | Purpose |
| -------- | ------- |
| `inception/design/screens/` | Screen specs — every state, a11y, traceability |
| `inception/design/components/*/preview.html` | Browser previews with design tokens |
| `tools/EDBS_Wireframes/` | **This plugin** — Figma wireframe frames for hi-fi work in your design tool |

Import colours and type from `inception/design/tokens.json` (Tokens Studio / Penpot) when moving from wireframe to visual design.

## Source of truth

Frame content is driven by `wireframe-data.ts`, kept in sync with the screen specs. If requirements change via a BA change request, update the specs first, then this data file.
