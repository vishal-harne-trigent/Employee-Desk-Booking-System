import { test, expect } from '@playwright/test';
import {
  adminEmail,
  authMessages,
  deactivatedEmail,
  defaultPassword,
  employeeEmail,
} from '../src/credentials';
import {
  expectLoginPage,
  expectUnauthenticated,
  signInViaUi,
} from '../src/login.helpers';
import { assertNotForceFailed } from '../src/force-fail';

/**
 * US-001 — Sign in and sign out (SCR-001)
 * Browser-level proof for every acceptance criterion (AC-01..AC-05).
 */
test.describe('US-001 — Sign in and sign out', () => {
  test('employee lands on Desk Availability after sign-in (US-001/AC-01)', async ({
    page,
  }) => {
    await signInViaUi(page, employeeEmail);

    await expect(page).toHaveURL(/\/Desks\/Availability/);
    await expect(
      page.getByRole('heading', { name: 'Desk Availability' }),
    ).toBeVisible();
  });

  test('admin lands on All Bookings after sign-in (US-001/AC-02)', async ({
    page,
  }) => {
    await signInViaUi(page, adminEmail);

    await expect(page).toHaveURL(/\/Admin\/AdminBookings/);
    await expect(
      page.getByRole('heading', { name: 'All Bookings' }),
    ).toBeVisible();
  });

  test('invalid credentials rejected with generic error (US-001/AC-03)', async ({
    page,
  }) => {
    await signInViaUi(page, employeeEmail, 'WrongPassword!', {
      expectSuccess: false,
    });

    await expectLoginPage(page);
    await expect(page.getByRole('alert')).toContainText(
      authMessages.invalidCredentials,
    );
    await expect(page.getByLabel('Email address')).toHaveValue(employeeEmail);
    await expect(page.getByLabel('Password')).toHaveValue('');

    await expectUnauthenticated(page);
    assertNotForceFailed('AC-03');
  });

  test('deactivated account rejected with deactivated message (US-001/AC-04)', async ({
    page,
  }) => {
    await signInViaUi(page, deactivatedEmail, defaultPassword, {
      expectSuccess: false,
    });

    await expectLoginPage(page);
    await expect(page.getByRole('alert')).toContainText(
      authMessages.deactivatedAccount,
    );

    await expectUnauthenticated(page);
  });

  test('sign out ends session and returns to sign-in (US-001/AC-05)', async ({
    page,
  }) => {
    await signInViaUi(page, employeeEmail);
    await expect(page).toHaveURL(/\/Desks\/Availability/);

    await page.getByRole('button', { name: 'Sign out' }).click();

    await expectLoginPage(page);
    await expectUnauthenticated(page);
    assertNotForceFailed('AC-05');
  });
});
