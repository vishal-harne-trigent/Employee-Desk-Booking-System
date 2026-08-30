import { chromium, type FullConfig } from '@playwright/test';
import fs from 'node:fs';
import path from 'node:path';

const authDir = path.join(__dirname, '..', '.auth');

async function signIn(
  baseURL: string,
  email: string,
  password: string,
  storagePath: string,
) {
  fs.mkdirSync(authDir, { recursive: true });
  const browser = await chromium.launch();
  const page = await browser.newPage();
  await page.goto(`${baseURL}/Account/Login`);
  await page.getByLabel('Email address').fill(email);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Sign in' }).click();
  await page.waitForURL(/\/(Desks\/Availability|Admin\/AdminBookings)/);
  await page.context().storageState({ path: storagePath });
  await browser.close();
}

export default async function globalSetup(config: FullConfig) {
  const baseURL = config.projects[0]?.use?.baseURL ?? 'http://localhost:5198';
  const password = process.env.E2E_PASSWORD ?? 'Password1!';

  await signIn(
    baseURL,
    process.env.E2E_EMPLOYEE_EMAIL ?? 'vishal_h@trigent.com',
    password,
    path.join(authDir, 'employee.json'),
  );

  await signIn(
    baseURL,
    process.env.E2E_ADMIN_EMAIL ?? 'admin@trigent.com',
    password,
    path.join(authDir, 'admin.json'),
  );
}
