/** Hi-fi screens only — wireframes come from tools/EDBS_Wireframes. */

function parseWireframeName(name) {
  if (name.includes('(Design)')) return null;
  const m = name.match(/^(SCR-\d{3})\/(ST-\d{2})\s*[—\-]\s*(.+)$/);
  if (!m) return null;
  return { screenId: m[1], stateId: m[2] };
}

function findState(screenId, stateId) {
  return WIREFRAMES.find((s) => s.screenId === screenId && s.stateId === stateId);
}

function isDesignFrame(n) {
  return n.type === 'FRAME' && n.name.includes('(Design)');
}

function isWireframeFrame(n) {
  return n.type === 'FRAME' && n.name.startsWith('SCR-') && !n.name.includes('(Design)');
}

function listWireframes(page) {
  return page.findAll(isWireframeFrame).filter((n) => parseWireframeName(n.name));
}

async function generateHiFiBesideWireframes() {
  const page = figma.currentPage;
  const wireframes = listWireframes(page);

  if (wireframes.length === 0) {
    throw new Error(
      'No wireframes on this page. Run the EDBS_Wireframes plugin first, then run EDBS_Figma again.',
    );
  }

  await loadHiFiFonts();
  page.findAll(isDesignFrame).forEach((n) => n.remove());

  wireframes.sort((a, b) => {
    if (a.y !== b.y) return a.y - b.y;
    return a.x - b.x;
  });

  const created = [];

  for (const wf of wireframes) {
    const parsed = parseWireframeName(wf.name);
    if (!parsed) continue;
    const state = findState(parsed.screenId, parsed.stateId);
    if (!state) continue;
    const design = buildHiFiFrame(state);
    design.x = wf.x + wf.width + GAP;
    design.y = wf.y;
    page.appendChild(design);
    created.push(design);
  }

  if (created.length === 0) {
    throw new Error(
      'Wireframes found but names did not match screen specs (expected SCR-###/ST-## — Title).',
    );
  }

  figma.viewport.scrollAndZoomIntoView(created);
  return created.length;
}

async function generateAllHiFiScreens() {
  await loadHiFiFonts();
  return generateHiFiDesigns();
}
