# EDBS Wireframes — Figma plugin

Generates **low-fidelity wireframes** for the Employee Desk Booking System from the approved screen specs (`SCR-001` … `SCR-004`).

## Import into Figma (3 steps)

1. **Build once** (if `code.js` is missing or small):

   ```powershell
   cd tools\EDBS_Wireframes
   .\build.ps1
   ```

   Or: `node source\build-code.js`

   `plugin/code.js` must be **~30 KB** and start with `const WIREFRAMES = [` — not a one-line error.

2. **Remove old plugin** in Figma: **Plugins → Development →** remove **all** previous “EDBS” entries.

3. **Import the `plugin` subfolder only** (not this parent folder):

   **Plugins → Development → Import plugin from manifest…**  
   → select **`tools/EDBS_Wireframes/plugin`** (contains `IMPORT-ME.txt`)

   | ✅ Import | ❌ Do not import |
   | --------- | ---------------- |
   | `…/EDBS_Wireframes/plugin` | `…/EDBS_Wireframes` (parent) |
   | | `…/EDBS_Wireframes/source` |

4. Open the Figma **page** where you want wireframes (any page — free plans are limited to 3 pages total).

5. Run the plugin:
   - **1. Generate wireframes** — black & white structural frames
   - **2. Generate hi-fi designs** — coloured frames from `inception/design/tokens.css`, placed **beside** the wireframes

6. **Zoom out** to see both grids. Hi-fi frame names end with `(Design)`.

## Troubleshooting

### “This plugin template uses TypeScript… generate code.js”

Figma is **not** loading the real `code.js`. Common causes:

| Cause | Fix |
| ----- | --- |
| Imported wrong folder | Import **`plugin/`** subfolder only |
| Old dev plugin cached | Remove plugin in Development menu, re-import |
| `code.js` missing / stub | Run `.\build.ps1` |
| Imported before build | Re-import after `code.js` exists |
| Free Figma page limit | Plugin uses **current page** only — does not create new pages |

Open **`plugin/code.js`** — if the whole file is one `throw new Error(...)`, run `.\build.ps1`.

### Free (Starter) Figma account

The plugin **does not create a new page** (Starter allows only 3 pages). Before running:

1. Switch to the page where you want wireframes (or use an empty page).
2. Click **Generate wireframes** — all 22 frames are placed on that page in a grid.
3. **Zoom out** (`Shift + 1` or scroll) to see the full grid.
4. Re-run replaces only frames whose names start with `SCR-`.

## What it creates

| Screen | Frames |
| ------ | ------ |
| Sign In | 4 |
| Book a Desk | 7 |
| My Bookings | 5 |
| Admin Bookings | 6 |

Frame names: `SCR-002/ST-03 — Desks available` (wireframe) or `… (Design)` (hi-fi).

## Hi-fi designs from wireframes

1. Generate **wireframes** first (step 1 in the plugin).
2. Click **Generate hi-fi designs** — 22 styled frames appear in a second grid **to the right** of the wireframes.
3. Colours match `inception/design/tokens.css` (primary blue, success green, error red, etc.).
4. Refine in Figma — add your brand polish, components, and auto-layout.

Re-running step 2 replaces only `(Design)` frames; wireframes are untouched.

## Folder layout

| Path | Purpose |
| ---- | ------- |
| **`plugin/`** | **Import this folder into Figma** (`manifest.json`, `ui.html`, `code.js`) |
| `source/` | Build sources (never import into Figma) |

## Regenerate after spec changes

```powershell
node source\build-code.js
```

Then re-run the plugin in Figma (re-import only if you changed `manifest.json`).

## Related repo artifacts

- Screen specs: `inception/design/screens/SCR-*.md`
- Browser previews: `inception/design/components/*/preview.html`
- Design tokens: `inception/design/tokens.json`
