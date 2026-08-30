import { test, expect } from '@playwright/test';
import { adminEmail, employeeEmail } from '../src/credentials';
import { signInViaUi } from '../src/login.helpers';

test.beforeEach(async ({ page }, testInfo) => {
  if (testInfo.project.name === 'employee-chromium') {
    await signInViaUi(page, employeeEmail);
  } else if (testInfo.project.name === 'admin-chromium') {
    await signInViaUi(page, adminEmail);
  }
});

test('authenticated employee can open Desk Availability', async ({ page }) => {
  await page.goto('/Desks/Availability');
  await expect(page).toHaveURL(/\/Desks\/Availability/);
  await expect(page.getByRole('heading', { name: 'Desk Availability' })).toBeVisible();
});

test('authenticated admin can open All Bookings', async ({ page }) => {
  test.skip(
    test.info().project.name !== 'admin-chromium',
    'Admin session required',
  );
  await page.goto('/Admin/AdminBookings');
  await expect(page).toHaveURL(/\/Admin\/AdminBookings/);
});
