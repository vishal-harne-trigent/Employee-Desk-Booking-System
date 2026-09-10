figma.showUI(__html__, { width: 360, height: 300 });

figma.ui.onmessage = async (msg) => {
  if (msg.type === 'generate') {
    try {
      const count = await generateWireframes();
      figma.ui.postMessage({ type: 'done', count });
      figma.notify(`Created ${count} wireframe frames on "${figma.currentPage.name}"`);
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err);
      figma.ui.postMessage({ type: 'error', message });
      figma.notify(`Error: ${message}`, { error: true });
    }
  }
  if (msg.type === 'cancel') {
    figma.closePlugin();
  }
};
