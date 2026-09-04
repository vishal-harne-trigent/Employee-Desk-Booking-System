// Screenshot extraction + optional Jira attachment upload for failed Playwright tests.

const fs = require('node:fs');
const path = require('node:path');

const MAX_SCREENSHOT_BYTES = 900_000;

/**
 * @param {import('@playwright/test/reporter').TestResult | undefined} outcome
 * @returns {{ path?: string, body?: Buffer, contentType: string, name: string } | null}
 */
function extractScreenshot(outcome) {
  if (!outcome?.attachments?.length) return null;

  const shot = outcome.attachments.find(
    (a) =>
      a.contentType?.startsWith('image/')
      || a.name === 'screenshot'
      || /\.(png|jpe?g|webp)$/i.test(a.path ?? a.name ?? ''),
  );
  if (!shot) return null;

  const contentType = shot.contentType ?? 'image/png';
  const name = shot.name ?? 'screenshot.png';

  if (shot.path && fs.existsSync(shot.path)) {
    return { path: shot.path, contentType, name };
  }
  if (shot.body?.length) {
    return { body: shot.body, contentType, name };
  }
  return null;
}

/**
 * @param {{ path?: string, body?: Buffer, contentType: string }} shot
 */
function screenshotToBase64(shot) {
  let buf;
  if (shot.path) {
    buf = fs.readFileSync(shot.path);
  } else if (shot.body) {
    buf = shot.body;
  } else {
    return null;
  }

  if (buf.length > MAX_SCREENSHOT_BYTES) {
    console.warn(
      `[Jira Reporter] Screenshot ${shot.path ?? shot.name} is ${buf.length} bytes — omitting base64 (limit ${MAX_SCREENSHOT_BYTES}). Will still upload to Jira if configured.`,
    );
    return null;
  }

  return `data:${shot.contentType};base64,${buf.toString('base64')}`;
}

function safeFilename(title) {
  return `${title
    .replace(/[^\w.-]+/g, '-')
    .replace(/-+/g, '-')
    .slice(0, 80)}-failure.png`;
}

function jiraAuth(env) {
  if (!env.JIRA_EMAIL || !env.JIRA_API_TOKEN) return null;
  return `Basic ${Buffer.from(`${env.JIRA_EMAIL}:${env.JIRA_API_TOKEN}`).toString('base64')}`;
}

/**
 * Upload screenshot to a Jira issue (Attachments tab). Returns attachment metadata.
 * @param {object} opts
 * @param {Record<string, string | undefined>} opts.env
 * @param {string} opts.issueKey e.g. EDBS-38
 * @param {{ path?: string, body?: Buffer, contentType: string, name: string }} opts.shot
 * @param {string} opts.testTitle
 */
async function uploadScreenshotToJira({ env, issueKey, shot, testTitle }) {
  const auth = jiraAuth(env);
  const base = env.JIRA_BASE_URL?.replace(/\/+$/, '');
  if (!auth || !base || !issueKey) return null;

  const filename = safeFilename(testTitle);
  let buf;
  if (shot.path) buf = fs.readFileSync(shot.path);
  else if (shot.body) buf = shot.body;
  else return null;

  const form = new FormData();
  form.append(
    'file',
    new Blob([buf], { type: shot.contentType }),
    filename,
  );

  try {
    const res = await fetch(`${base}/rest/api/3/issue/${issueKey}/attachments`, {
      method: 'POST',
      headers: {
        Authorization: auth,
        'X-Atlassian-Authorization': 'no-check',
      },
      body: form,
    });
    const text = await res.text();
    if (!res.ok) {
      console.warn(
        `[Jira Reporter] Screenshot upload failed (${res.status}): ${text.slice(0, 200)}`,
      );
      return null;
    }
    const attachments = JSON.parse(text);
    const att = attachments[0];
    if (!att) return null;
    return {
      id: att.id,
      filename: att.filename,
      mimeType: att.mimeType,
      content: att.content,
      thumbnail: att.thumbnail,
    };
  } catch (error) {
    console.warn('[Jira Reporter] Screenshot upload error:', error);
    return null;
  }
}

module.exports = {
  extractScreenshot,
  screenshotToBase64,
  uploadScreenshotToJira,
  safeFilename,
};
