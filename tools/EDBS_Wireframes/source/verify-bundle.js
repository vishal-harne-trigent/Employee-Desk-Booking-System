const STUB_MARKERS = [
  'This plugin template uses TypeScript',
  'Follow the instructions in `README.md` to generate `code.js`',
];

function assertValidPluginBundle(bundle, label) {
  if (bundle.length < 10000) {
    throw new Error(`${label}: code.js too small (${bundle.length} bytes). Build failed.`);
  }
  for (const marker of STUB_MARKERS) {
    if (bundle.includes(marker)) {
      throw new Error(`${label}: code.js is still the Figma TypeScript stub. Run build.ps1.`);
    }
  }
  if (!bundle.includes('const WIREFRAMES = [')) {
    throw new Error(`${label}: code.js missing WIREFRAMES bundle. Run build.ps1.`);
  }
}

module.exports = { assertValidPluginBundle, STUB_MARKERS };
