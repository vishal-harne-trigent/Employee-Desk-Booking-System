# EDBS_Figma — Colourful full hi-fi screens

Creates **full-colour hi-fi design frames** from `inception/design/tokens.css`. Does **not** generate wireframes.

Each screen uses a white **shell card** on a muted page background, pill badges, status chips, and drop shadows — aligned with the HTML component previews.

| Plugin | Folder | Purpose |
| ------ | ------ | ------- |
| **EDBS_Wireframes** | `tools/EDBS_Wireframes/plugin` | B&W wireframes |
| **EDBS_Figma** | `tools/EDBS_Figma/plugin` | Hi-fi screens only |

## Workflow

1. **EDBS_Wireframes** → Generate wireframes on the current page  
2. **EDBS_Figma** → Hi-fi beside wireframes **or** All 22 hi-fi screens (grid)

## Setup

```powershell
cd tools\EDBS_Figma
.\build.ps1
```

Import **`tools/EDBS_Figma/plugin`** in Figma (not the parent folder).

## Buttons

| Button | What it does |
| ------ | ------------ |
| **Colourful screens beside wireframes** | One full-colour frame to the right of each wireframe |
| **All 22 colourful screens (grid)** | Full hi-fi grid (no wireframes required) |

## Troubleshooting

**“No wireframes on this page”** — Run **EDBS_Wireframes** first, or use **All 22 hi-fi screens (grid)**.

**TypeScript stub error** — Import `plugin/` only; run `.\build.ps1` first.
