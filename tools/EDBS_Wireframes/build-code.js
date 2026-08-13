const fs = require('fs');
const path = require('path');

const dir = __dirname;
const data = fs.readFileSync(path.join(dir, 'wireframe-data.ts'), 'utf8');
const match = data.match(/export const WIREFRAMES[\s\S]*?=\s*(\[[\s\S]*?\n\]);/);
if (!match) throw new Error('Could not parse WIREFRAMES array');
// eslint-disable-next-line no-eval
const WIREFRAMES = eval(match[1]);

const renderer = fs.readFileSync(path.join(dir, 'code-runtime.js'), 'utf8');
const out =
  'const WIREFRAMES = ' +
  JSON.stringify(WIREFRAMES, null, 2) +
  ';\n\nconst FRAME_WIDTH = 1280;\nconst FRAME_HEIGHT = 800;\nconst COLUMNS = 3;\nconst GAP = 48;\n\n' +
  renderer;

fs.writeFileSync(path.join(dir, 'code.js'), out);
console.log('Generated code.js (' + WIREFRAMES.length + ' wireframes)');
