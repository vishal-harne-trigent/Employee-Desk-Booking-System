# US-008 — decisions

| Date       | Decision | Rationale | Alternative rejected |
| ---------- | -------- | --------- | -------------------- |
| 2026-08-22 | `WebPush` NuGet for VAPID delivery | Matches `app-architecture.md` | Custom HTTP to push endpoints |
| 2026-08-22 | `InMemoryPushNotificationSender` in tests | Same pattern as US-007 `InMemoryEmailSender`; no browser in CI | Real WebPush in CI |
| 2026-08-22 | Booking commits even if push fails | Parity with AC-05 email behaviour; push is optional channel | Roll back booking on push failure |
| 2026-08-22 | Opt-in requires stored subscription JSON | Web Push needs endpoint + keys from browser | Opt-in flag without subscription |
| 2026-08-22 | MVC settings page + small `push-settings.js` | SCR-007; JWT API mirrors for contract tests | SPA-only settings |
| 2026-08-22 | Entry link from My Bookings | SCR-007 structural default | Top-level nav item |
| 2026-08-22 | Push body includes desk number and date | Consistent with email (V-13 pattern) | Title-only push |
