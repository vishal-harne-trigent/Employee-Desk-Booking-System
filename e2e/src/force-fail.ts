/** Opt-in intentional failure for Jira screenshot verification (E2E_FORCE_FAIL=AC-03,AC-05). */
export function assertNotForceFailed(acId: string) {
  const raw = process.env.E2E_FORCE_FAIL ?? '';
  const ids = raw.split(/[,;\s]+/).map((s) => s.trim()).filter(Boolean);
  if (ids.includes(acId)) {
    throw new Error(
      `Intentional failure (${acId}) — E2E_FORCE_FAIL set for Jira screenshot verification`,
    );
  }
}
