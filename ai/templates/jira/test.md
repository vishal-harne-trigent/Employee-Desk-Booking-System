---
issuetype: Subtask
summary: '${STORY_ID}/Tests — Test evidence'
labels: [aidlc, test]
parent: '${STORY_KEY}'
---

## Test results for ${STORY_ID}

${RUN_SUMMARY}

---

## Test cases

From approved browser test plan — [${STORY_ID} test plan](${PLAN_URL}).

${TEST_CASES_TABLE}

**Automated spec:** `${SPEC_PATH}`

---

## Test data (dev seed)

${TEST_DATA_TABLE}

---

## Test trials (execution log)

Each row is one Playwright execution from the latest run (`playwright-report.json`).

${TEST_TRIALS_TABLE}

---

## Failure screenshots

${FAILURE_SCREENSHOTS}

---

## AC summary

${AC_SUMMARY_TABLE}

---

## Criterion ↔ automation map

${AC_RESULTS_TABLE}

**Latest CI / run link:** ${CI_RUN_URL}

---

## How this is verified

Test case IDs map to the approved browser test plan. Trial rows and AC outcomes are filled from a real Playwright run only — never typed by hand.

### Covers

[${STORY_ID}](${ARTIFACT_URL}) · [Test plan](${PLAN_URL})

_Tracked automatically from the team's repository._
