import { expect, type Page } from '@playwright/test';
import { defaultPassword } from './credentials';

export async function expectLoginPage(page: Page) {
  await expect(page.getByRole('heading', { name: 'Welcome back' })).toBeVisible();
}

export async function expectUnauthenticated(page: Page) {
  await page.goto('/Desks/Availability');
  await expectLoginPage(page);
}

export async function gotoLogin(page: Page) {
  await page.goto('/Account/Login');
  await expectLoginPage(page);
}

export async function submitLogin(page: Page, email: string, password: string) {
  await page.getByLabel('Email address').fill(email);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Sign in' }).click();
}

/** Sign in through the login form (SCR-001). Waits for post-login redirect on success. */
export async function signInViaUi(
  page: Page,
  email: string,
  password: string = defaultPassword,
  options?: { expectSuccess?: boolean },
) {
  await gotoLogin(page);
  await submitLogin(page, email, password);

  if (options?.expectSuccess !== false) {
    await page.waitForURL(/\/(Desks\/Availability|Admin\/AdminBookings)/);
  }
}
