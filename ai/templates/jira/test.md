---
issuetype: Subtask
summary: '${STORY_ID}/Tests — Test evidence'
labels: [aidlc, test]
parent: '${STORY_KEY}'
---

## Test results for ${STORY_ID}

Latest CI run: ${CI_RUN_URL}

${AC_RESULTS_TABLE}

## How this is verified

Each row maps to an automated test whose name carries the criterion identifier (`US-###/AC-##`), so the test and the criterion cannot drift apart without the build noticing. Outcomes are filled from a real pipeline run only.

**Not yet automated** means exactly that — no test exists for that criterion yet. It is never a substitute for a pass.

---

### Covers

[${STORY_ID}](${ARTIFACT_URL})

_Tracked automatically from the team's repository. Results are never set by hand._
