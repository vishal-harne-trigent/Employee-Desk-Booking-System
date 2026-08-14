figma.showUI(__html__, { width: 360, height: 300 });

figma.ui.onmessage = async (msg) => {
  if (msg.type === 'beside-wireframes') {
    try {
      const count = await generateHiFiBesideWireframes();
      figma.ui.postMessage({ type: 'done', count, mode: 'beside' });
      figma.notify(`Created ${count} hi-fi screens beside wireframes`);
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err);
      figma.ui.postMessage({ type: 'error', message });
      figma.notify(message, { error: true });
    }
  }
  if (msg.type === 'generate-all') {
    try {
      const count = await generateAllHiFiScreens();
      figma.ui.postMessage({ type: 'done', count, mode: 'grid' });
      figma.notify(`Created ${count} hi-fi screens`);
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err);
      figma.ui.postMessage({ type: 'error', message });
      figma.notify(message, { error: true });
    }
  }
  if (msg.type === 'cancel') {
    figma.closePlugin();
  }
};
