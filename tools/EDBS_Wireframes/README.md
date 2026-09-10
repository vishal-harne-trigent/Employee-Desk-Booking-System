# EDBS Wireframes — Figma plugin

Generates **low-fidelity wireframes** (44 screen states) for the Employee Desk Booking System from approved screen specs (`SCR-001` … `SCR-007`).

## Import into Figma

1. **Build once** (if `plugin/code.js` is missing or small):

   ```powershell
   cd tools\EDBS_Wireframes
   .\build.ps1
   ```

   `plugin/code.js` must be **~47 KB** and start with `const WIREFRAMES = [` — not a one-line error.

2. **Remove old plugin** in Figma: **Plugins → Development →** remove previous “EDBS” entries.

3. **Import the `plugin` subfolder only**:

   **Plugins → Development → Import plugin from manifest…**  
   → select **`tools/EDBS_Wireframes/plugin`** (contains `IMPORT-ME.txt`)

   | ✅ Import | ❌ Do not import |
   | --------- | ---------------- |
   | `…/EDBS_Wireframes/plugin` | `…/EDBS_Wireframes` (parent — no manifest) |
   | | `…/EDBS_Wireframes/source` |

4. Open the Figma **page** where you want wireframes.

5. Run the plugin → **Generate wireframes**.

6. **Zoom out** to see the full grid.

For **colourful hi-fi screens**, use the separate **`tools/EDBS_Figma/plugin`** plugin.

## Troubleshooting

### “This plugin template uses TypeScript… generate code.js”

Figma is loading the **wrong folder** or a **cached old import**.

| Cause | Fix |
| ----- | --- |
| Imported parent folder | Import **`plugin/`** only |
| Imported `source/` | Import **`plugin/`** only |
| Old dev plugin cached | Remove plugin in Development menu, re-import |
| `code.js` missing | Run `.\build.ps1`, then re-import |

Open **`plugin/code.js`** — it should start with `const WIREFRAMES = [`.

### Free (Starter) Figma account

The plugin **does not create a new page**. It places all frames on the **current page**. Re-run replaces only frames whose names start with `SCR-`.

## Regenerate after spec changes

```powershell
cd tools\EDBS_Wireframes
.\build.ps1
```

Re-import in Figma only if `manifest.json` changed.

## Folder layout

| Path | Purpose |
| ---- | ------- |
| **`plugin/`** | **Import this folder into Figma** |
| `source/` | Build sources — never import |
| `ui.html` | Plugin UI source (copied to `plugin/` on build) |

## Related

- Hi-fi plugin: `tools/EDBS_Figma/plugin`
- Screen specs: `inception/design/screens/SCR-*.md`
- HTML previews: `inception/design/components/*/preview.html`
