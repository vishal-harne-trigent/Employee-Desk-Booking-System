export const defaultPassword = process.env.E2E_PASSWORD ?? 'Password1!';

export const employeeEmail =
  process.env.E2E_EMPLOYEE_EMAIL ?? 'vishal_h@trigent.com';

export const adminEmail = process.env.E2E_ADMIN_EMAIL ?? 'admin@trigent.com';

export const deactivatedEmail =
  process.env.E2E_DEACTIVATED_EMAIL ?? 'deactivated@trigent.com';

export const authMessages = {
  invalidCredentials: 'Invalid email or password. Please try again.',
  deactivatedAccount:
    'Your account has been deactivated. Contact your administrator.',
} as const;
