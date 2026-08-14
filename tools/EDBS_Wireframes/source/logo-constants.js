const fs = require('fs');
const path = require('path');

/** @param {string} repoRoot */
function buildLogoConstants(repoRoot) {
  const logoPath = path.join(repoRoot, 'inception', 'design', 'assets', 'desk-booking-logo.png');
  const logoBuf = fs.readFileSync(logoPath);
  const width = logoBuf.readUInt32BE(16);
  const height = logoBuf.readUInt32BE(20);
  return [
    'const LOGO_BASE64 = ' + JSON.stringify(logoBuf.toString('base64')) + ';',
    'const LOGO_ASPECT = ' + width / height + ';',
    '',
  ].join('\n');
}

module.exports = { buildLogoConstants };
