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

test('employee lands on Desk Availability after sign-in (US-001/AC-01)', async ({ page }) => {
  await page.goto('/Desks/Availability');
  await expect(page).toHaveURL(/\/Desks\/Availability/);
  await expect(page.getByRole('heading', { name: 'Desk Availability' })).toBeVisible();
});

test('admin can open All Bookings (US-001/AC-02)', async ({ page }) => {
  await page.goto('/Admin/AdminBookings');
  await expect(page).toHaveURL(/\/Admin\/AdminBookings/);
});
