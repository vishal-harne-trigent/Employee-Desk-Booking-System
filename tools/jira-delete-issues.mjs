// Delete Jira issues by key — refuses keys recorded in the traceability manifest.
//   node tools/jira-delete-issues.mjs EDBS-97 EDBS-98 --apply
// Dry run (default): prints keys that would be deleted.

import { readFileSync } from 'node:fs';
import { join } from 'node:path';

function loadEnvFile(filePath) {
  try {
    for (const line of readFileSync(filePath, 'utf8').split('\n')) {
      const trimmed = line.trim();
      if (!trimmed || trimmed.startsWith('#')) continue;
      const eq = trimmed.indexOf('=');
      if (eq === -1) continue;
      const key = trimmed.slice(0, eq).trim();
      const value = trimmed.slice(eq + 1).trim();
      if (process.env[key] === undefined) process.env[key] = value;
    }
  } catch {
    // no .env
  }
}

loadEnvFile(join(process.cwd(), '.env'));

const args = process.argv.slice(2);
const APPLY = args.includes('--apply');
const keys = args.filter((a) => !a.startsWith('--'));

const die = (m) => {
  console.error(`jira-delete-issues: ${m}`);
  process.exit(1);
};

if (!keys.length) {
  die('usage: node tools/jira-delete-issues.mjs EDBS-97 EDBS-98 [--apply]');
}

function creds() {
  const missing = ['JIRA_BASE_URL', 'JIRA_EMAIL', 'JIRA_API_TOKEN'].filter(
    (k) => !process.env[k],
  );
  if (missing.length) die(`needs ${missing.join(', ')} in .env`);
  return {
    base: process.env.JIRA_BASE_URL.replace(/\/+$/, ''),
    auth:
      'Basic ' +
      Buffer.from(
        `${process.env.JIRA_EMAIL}:${process.env.JIRA_API_TOKEN}`,
      ).toString('base64'),
  };
}

function protectedKeys() {
  const mfPath = join(process.cwd(), 'knowledge', 'traceability', 'manifest.json');
  const mf = JSON.parse(readFileSync(mfPath, 'utf8'));
  const blocked = new Set();
  for (const entry of Object.values(mf.stories ?? {})) {
    if (entry.jira) blocked.add(entry.jira);
    if (entry.testJira) blocked.add(entry.testJira);
  }
  for (const entry of Object.values(mf.epics ?? {})) {
    if (entry.jira) blocked.add(entry.jira);
  }
  return blocked;
}

async function deleteIssue(key) {
  const { base, auth } = creds();
  const res = await fetch(`${base}/rest/api/3/issue/${key}`, {
    method: 'DELETE',
    headers: { Authorization: auth, Accept: 'application/json' },
  });
  if (res.status === 204 || res.status === 200) return;
  const text = await res.text();
  die(`DELETE ${key} → ${res.status}\n${text.slice(0, 400)}`);
}

const blocked = protectedKeys();
for (const key of keys) {
  if (blocked.has(key)) {
    die(`refusing to delete ${key} — recorded in traceability manifest`);
  }
}

if (!APPLY) {
  console.log(`DRY RUN — would delete: ${keys.join(', ')}\nRe-run with --apply`);
  process.exit(0);
}

for (const key of keys) {
  await deleteIssue(key);
  console.log(`deleted ${key}`);
}
