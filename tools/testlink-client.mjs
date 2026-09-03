import xmlrpc from 'xmlrpc';
import { promisify } from 'node:util';

/**
 * Minimal TestLink XML-RPC client (TestLink 1.9.x).
 * API: https://github.com/TestLinkOpenSourceTRMS/testlink-code/tree/testlink_1_9/lib/api/xmlrpc/v1
 */
export function createTestLinkClient({ apiUrl, devKey }) {
  if (!apiUrl || !devKey) {
    throw new Error('TestLink apiUrl and devKey are required.');
  }

  const isSecure = apiUrl.startsWith('https://');
  const client = isSecure
    ? xmlrpc.createSecureClient({ url: apiUrl, rejectUnauthorized: true })
    : xmlrpc.createClient({ url: apiUrl });
  const methodCall = promisify(client.methodCall.bind(client));

  async function call(method, params = []) {
    const response = await methodCall(method, [devKey, ...params]);
    if (response?.status != null && Number(response.status) !== true) {
      const message = response.message ?? JSON.stringify(response);
      throw new Error(`TestLink ${method} failed: ${message}`);
    }
    return response;
  }

  return { call };
}

export async function resolveProjectId(client, projectName) {
  const projects = await client.call('tl.getProjects');
  const list = Array.isArray(projects) ? projects : projects ? [projects] : [];
  const hit = list.find(
    (p) =>
      p.name?.toLowerCase() === projectName.toLowerCase()
      || p.prefix?.toLowerCase() === projectName.toLowerCase(),
  );
  if (!hit) {
    throw new Error(
      `TestLink project "${projectName}" not found. Available: ${list.map((p) => p.name).join(', ') || '(none)'}`,
    );
  }
  return Number(hit.id);
}

export async function resolveTestPlanId(client, projectName, planName) {
  const plan = await client.call('tl.getTestPlanByName', [planName, projectName]);
  const id = plan?.[0]?.id ?? plan?.id;
  if (!id) {
    throw new Error(`TestLink test plan "${planName}" not found in project "${projectName}".`);
  }
  return Number(id);
}

export async function resolveTestSuiteId(client, projectName, suiteName) {
  const suite = await client.call('tl.getTestSuiteByName', [suiteName, projectName]);
  const id = suite?.[0]?.id ?? suite?.id;
  if (!id) {
    throw new Error(`TestLink test suite "${suiteName}" not found in project "${projectName}".`);
  }
  return Number(id);
}

export async function ensureBuild(client, planId, buildName) {
  const builds = await client.call('tl.getBuildsForTestPlan', [planId]);
  const list = Array.isArray(builds) ? builds : builds ? [builds] : [];
  const existing = list.find((b) => b.name === buildName);
  if (existing?.id) {
    return Number(existing.id);
  }
  const created = await client.call('tl.createBuild', [planId, buildName, 'Playwright e2e run']);
  return Number(created.id ?? created[0]?.id);
}

export async function findTestCaseIdByName(client, name) {
  try {
    const tc = await client.call('tl.getTestCaseIDByName', [name]);
    const id = tc?.[0]?.id ?? tc?.id ?? tc?.[0]?.testcase_id;
    return id ? Number(id) : null;
  } catch {
    return null;
  }
}

export async function upsertTestCase(client, {
  projectId,
  suiteId,
  name,
  summary,
  authorLogin,
  externalId,
}) {
  const existingId = await findTestCaseIdByName(client, name);
  if (existingId) {
    return existingId;
  }

  const created = await client.call('tl.createTestCase', [
    name,
    suiteId,
    projectId,
    authorLogin,
    summary,
    null,
    null,
    null,
    2,
    null,
    null,
    externalId,
  ]);
  const id = created?.[0]?.id ?? created?.id;
  if (!id) {
    throw new Error(`createTestCase returned no id for ${name}`);
  }
  return Number(id);
}

export async function addCaseToPlan(client, planId, caseId) {
  try {
    await client.call('tl.addTestCaseToTestPlan', [planId, caseId]);
  } catch (error) {
    if (!String(error.message).toLowerCase().includes('already')) {
      throw error;
    }
  }
}

/** status: p = pass, f = fail, b = blocked */
export async function reportResult(client, {
  caseId,
  planId,
  buildId,
  status,
  notes,
  platformId,
}) {
  const args = [caseId, planId, status];
  if (platformId) {
    args.push(platformId, true, notes ?? '', true, 0, buildId);
  } else {
    args.push('', true, notes ?? '', true, 0, buildId);
  }
  await client.call('tl.reportTCResult', args);
}
