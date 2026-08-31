import { test, expect } from '@playwright/test';

test('employee lands on Desk Availability after sign-in (US-001/AC-01)', async ({
  page,
}) => {
  await page.goto('/Desks/Availability');
  await expect(page).toHaveURL(/\/Desks\/Availability/);
  await expect(page.getByRole('heading', { name: 'Desk Availability' })).toBeVisible();
});

test('admin can open All Bookings (US-001/AC-02)', async ({ page }) => {
  await page.goto('/Admin/AdminBookings');
  await expect(page).toHaveURL(/\/Admin\/AdminBookings/);
});
