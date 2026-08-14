/**
 * Wireframe frame definitions — aligned with inception/design/screens SCR-### specs.
 * Each entry becomes one Figma frame named SCR-###/ST-## — Title
 */

export type WireElement =
  | { type: 'title'; text: string; y?: number }
  | { type: 'logo' }
  | { type: 'subtitle'; text: string }
  | { type: 'label'; text: string }
  | { type: 'input'; placeholder: string; width?: number }
  | { type: 'button'; text: string; primary?: boolean; width?: number }
  | { type: 'alert'; text: string; variant?: 'error' | 'info' }
  | { type: 'header'; title: string; nav?: string[]; activeNav?: string; admin?: boolean }
  | { type: 'datePicker'; label: string }
  | { type: 'deskGrid'; desks: Array<{ id: string; available: boolean }> }
  | { type: 'skeletonGrid'; count: number }
  | { type: 'empty'; text: string }
  | { type: 'table'; headers: string[]; rows: string[][] }
  | { type: 'filters'; labels: string[] }
  | { type: 'chips'; items: string[] }
  | { type: 'modal'; title: string; body: string; actions: string[] }
  | { type: 'spacer'; height: number }
  | { type: 'note'; text: string };

export interface WireframeState {
  screenId: string;
  stateId: string;
  title: string;
  elements: WireElement[];
}

export const WIREFRAMES: WireframeState[] = [
  {
    screenId: 'SCR-001',
    stateId: 'ST-01',
    title: 'Default',
    elements: [
      { type: 'logo' },
      { type: 'subtitle', text: 'Sign in' },
      { type: 'label', text: 'Email' },
      { type: 'input', placeholder: 'you@company.com' },
      { type: 'label', text: 'Password' },
      { type: 'input', placeholder: '••••••••' },
      { type: 'button', text: 'Sign in', primary: true },
    ],
  },
  {
    screenId: 'SCR-001',
    stateId: 'ST-02',
    title: 'Loading',
    elements: [
      { type: 'logo' },
      { type: 'subtitle', text: 'Sign in' },
      { type: 'label', text: 'Email' },
      { type: 'input', placeholder: 'jane@company.com' },
      { type: 'label', text: 'Password' },
      { type: 'input', placeholder: '••••••••' },
      { type: 'button', text: 'Signing in…', primary: true },
      { type: 'note', text: '(fields disabled)' },
    ],
  },
  {
    screenId: 'SCR-001',
    stateId: 'ST-03',
    title: 'Invalid credentials',
    elements: [
      { type: 'logo' },
      { type: 'alert', text: '! Invalid email or password', variant: 'error' },
      { type: 'subtitle', text: 'Sign in' },
      { type: 'label', text: 'Email' },
      { type: 'input', placeholder: 'jane@company.com' },
      { type: 'label', text: 'Password' },
      { type: 'input', placeholder: '' },
      { type: 'button', text: 'Sign in', primary: true },
    ],
  },
  {
    screenId: 'SCR-001',
    stateId: 'ST-04',
    title: 'Deactivated account',
    elements: [
      { type: 'logo' },
      {
        type: 'alert',
        text: '! Account deactivated — contact administrator',
        variant: 'error',
      },
      { type: 'subtitle', text: 'Sign in' },
      { type: 'label', text: 'Email' },
      { type: 'input', placeholder: 'former@company.com' },
      { type: 'label', text: 'Password' },
      { type: 'input', placeholder: '' },
      { type: 'button', text: 'Sign in', primary: true },
    ],
  },
  {
    screenId: 'SCR-002',
    stateId: 'ST-01',
    title: 'Default',
    elements: [
      {
        type: 'header',
        title: 'EDBS',
        nav: ['Book Desk', 'My Bookings'],
      },
      { type: 'datePicker', label: 'Book a desk for:' },
      { type: 'note', text: 'Select a date to view desk availability.' },
    ],
  },
  {
    screenId: 'SCR-002',
    stateId: 'ST-02',
    title: 'Loading',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Book Desk', 'My Bookings'] },
      { type: 'datePicker', label: 'Book a desk for:' },
      { type: 'skeletonGrid', count: 4 },
    ],
  },
  {
    screenId: 'SCR-002',
    stateId: 'ST-03',
    title: 'Desks available',
    elements: [
      {
        type: 'header',
        title: 'EDBS',
        nav: ['Book Desk', 'My Bookings'],
        activeNav: 'Book Desk',
      },
      { type: 'datePicker', label: 'Book a desk for:' },
      {
        type: 'table',
        headers: ['Desk', 'Status', 'Action'],
        rows: [
          ['A-01', '● Available', 'Book'],
          ['A-02', '✗ Booked', '—'],
          ['B-01', '● Available', 'Book'],
          ['B-02', '● Available', 'Book'],
        ],
      },
    ],
  },
  {
    screenId: 'SCR-002',
    stateId: 'ST-04',
    title: 'Empty',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Book Desk', 'My Bookings'] },
      { type: 'empty', text: 'All desks booked for this date. Try another day.' },
    ],
  },
  {
    screenId: 'SCR-002',
    stateId: 'ST-05',
    title: 'Error',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Book Desk', 'My Bookings'] },
      { type: 'alert', text: '! Could not load desks — Retry', variant: 'error' },
    ],
  },
  {
    screenId: 'SCR-002',
    stateId: 'ST-06',
    title: 'Already booked',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Book Desk', 'My Bookings'] },
      {
        type: 'alert',
        text: 'ℹ You already have desk A-01. Cancel in My Bookings first.',
        variant: 'info',
      },
    ],
  },
  {
    screenId: 'SCR-002',
    stateId: 'ST-07',
    title: 'Confirm booking',
    elements: [
      {
        type: 'modal',
        title: 'Confirm booking',
        body: 'Desk B-01 on Thu 14 Aug 2026',
        actions: ['Cancel', 'Confirm'],
      },
    ],
  },
  {
    screenId: 'SCR-003',
    stateId: 'ST-01',
    title: 'Default',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Book Desk', 'My Bookings'] },
      { type: 'subtitle', text: 'My bookings' },
      {
        type: 'table',
        headers: ['Date', 'Desk', 'Status', 'Action'],
        rows: [
          ['14 Aug 2026', 'A-01', '● Confirmed', 'Cancel'],
          ['10 Aug 2026', 'B-02', '○ Completed', '—'],
        ],
      },
    ],
  },
  {
    screenId: 'SCR-003',
    stateId: 'ST-02',
    title: 'Loading',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Book Desk', 'My Bookings'] },
      { type: 'skeletonGrid', count: 3 },
    ],
  },
  {
    screenId: 'SCR-003',
    stateId: 'ST-03',
    title: 'Empty',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Book Desk', 'My Bookings'] },
      { type: 'empty', text: 'No bookings yet. → Book a desk' },
    ],
  },
  {
    screenId: 'SCR-003',
    stateId: 'ST-04',
    title: 'Error',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Book Desk', 'My Bookings'] },
      { type: 'alert', text: '! Could not load bookings — Retry', variant: 'error' },
    ],
  },
  {
    screenId: 'SCR-003',
    stateId: 'ST-05',
    title: 'Cancel confirm',
    elements: [
      {
        type: 'modal',
        title: 'Cancel booking?',
        body: 'Desk A-01 on 14 Aug 2026',
        actions: ['Keep booking', 'Confirm cancel'],
      },
    ],
  },
  {
    screenId: 'SCR-004',
    stateId: 'ST-01',
    title: 'Default',
    elements: [
      { type: 'header', title: 'EDBS Admin', nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'All Bookings', admin: true },
      { type: 'filters', labels: ['Date: All', 'Status: All', 'Apply'] },
      {
        type: 'table',
        headers: ['Date', 'Employee', 'Desk', 'Status', 'Action'],
        rows: [
          ['14 Aug 2026', 'jane@co.com', 'A-01', '● Confirmed', 'Cancel'],
          ['13 Aug 2026', 'bob@co.com', 'B-02', '● Confirmed', 'Cancel'],
        ],
      },
    ],
  },
  {
    screenId: 'SCR-004',
    stateId: 'ST-02',
    title: 'Loading',
    elements: [
      { type: 'header', title: 'EDBS Admin', admin: true, nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'All Bookings' },
      { type: 'skeletonGrid', count: 2 },
    ],
  },
  {
    screenId: 'SCR-004',
    stateId: 'ST-03',
    title: 'Empty filter',
    elements: [
      { type: 'header', title: 'EDBS Admin', admin: true, nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'All Bookings' },
      { type: 'empty', text: 'No bookings match filters. Clear filters' },
    ],
  },
  {
    screenId: 'SCR-004',
    stateId: 'ST-04',
    title: 'Error',
    elements: [
      { type: 'header', title: 'EDBS Admin', admin: true, nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'All Bookings' },
      { type: 'alert', text: '! Could not load bookings — Retry', variant: 'error' },
    ],
  },
  {
    screenId: 'SCR-004',
    stateId: 'ST-05',
    title: 'Filters applied',
    elements: [
      { type: 'header', title: 'EDBS Admin', admin: true, nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'All Bookings' },
      { type: 'chips', items: ['Date: 14 Aug 2026', 'Status: Confirmed'] },
      {
        type: 'table',
        headers: ['Date', 'Employee', 'Desk', 'Status', 'Action'],
        rows: [['14 Aug 2026', 'jane@co.com', 'A-01', '● Confirmed', 'Cancel']],
      },
    ],
  },
  {
    screenId: 'SCR-004',
    stateId: 'ST-06',
    title: 'Cancel on behalf',
    elements: [
      {
        type: 'modal',
        title: 'Cancel for employee?',
        body: 'jane@co.com — A-01 on 14 Aug 2026',
        actions: ['Keep booking', 'Confirm cancel'],
      },
    ],
  },
  // SCR-005 Manage Desks
  {
    screenId: 'SCR-005',
    stateId: 'ST-01',
    title: 'Default',
    elements: [
      { type: 'header', title: 'EDBS Admin', nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'Desks', admin: true },
      { type: 'subtitle', text: 'Manage desks' },
      { type: 'button', text: '+ Add desk', primary: true, width: 140 },
      {
        type: 'table',
        headers: ['Desk', 'Status', 'Actions'],
        rows: [
          ['A-12', '● Active', 'Edit · Deactivate'],
          ['B-03', '○ Inactive', 'Edit · Activate'],
        ],
      },
    ],
  },
  {
    screenId: 'SCR-005',
    stateId: 'ST-02',
    title: 'Loading',
    elements: [
      { type: 'header', title: 'EDBS Admin', nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'Desks', admin: true },
      { type: 'skeletonGrid', count: 3 },
    ],
  },
  {
    screenId: 'SCR-005',
    stateId: 'ST-03',
    title: 'Empty',
    elements: [
      { type: 'header', title: 'EDBS Admin', nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'Desks', admin: true },
      { type: 'empty', text: 'No desks yet. + Add desk' },
    ],
  },
  {
    screenId: 'SCR-005',
    stateId: 'ST-04',
    title: 'Error',
    elements: [
      { type: 'header', title: 'EDBS Admin', nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'Desks', admin: true },
      { type: 'alert', text: '! Could not load desks — Retry', variant: 'error' },
    ],
  },
  {
    screenId: 'SCR-005',
    stateId: 'ST-05',
    title: 'Add desk',
    elements: [
      {
        type: 'modal',
        title: 'Add desk',
        body: 'Desk number (e.g. A-12)',
        actions: ['Cancel', 'Save'],
      },
    ],
  },
  {
    screenId: 'SCR-005',
    stateId: 'ST-06',
    title: 'Edit desk',
    elements: [
      {
        type: 'modal',
        title: 'Edit desk',
        body: 'Desk number: A-12',
        actions: ['Cancel', 'Save'],
      },
    ],
  },
  {
    screenId: 'SCR-005',
    stateId: 'ST-07',
    title: 'Deactivate confirm',
    elements: [
      {
        type: 'modal',
        title: 'Deactivate desk?',
        body: 'Desk A-12 will not appear for booking.',
        actions: ['Cancel', 'Deactivate'],
      },
    ],
  },
  {
    screenId: 'SCR-005',
    stateId: 'ST-08',
    title: 'Deactivate blocked',
    elements: [
      { type: 'header', title: 'EDBS Admin', nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'Desks', admin: true },
      {
        type: 'alert',
        text: '! B-03 has future bookings — cancel in All Bookings first',
        variant: 'error',
      },
    ],
  },
  // SCR-006 Manage Users
  {
    screenId: 'SCR-006',
    stateId: 'ST-01',
    title: 'Default',
    elements: [
      { type: 'header', title: 'EDBS Admin', nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'Users', admin: true },
      { type: 'subtitle', text: 'Manage users' },
      { type: 'button', text: '+ Add user', primary: true, width: 140 },
      {
        type: 'table',
        headers: ['Name', 'Email', 'Role', 'Status', 'Actions'],
        rows: [
          ['Jane Smith', 'jane@co.com', 'Employee', '● Active', 'Edit · Reset · Deactivate'],
          ['Marcus Lee', 'admin@co.com', 'Admin', '● Active', 'Edit · Reset'],
        ],
      },
    ],
  },
  {
    screenId: 'SCR-006',
    stateId: 'ST-02',
    title: 'Loading',
    elements: [
      { type: 'header', title: 'EDBS Admin', nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'Users', admin: true },
      { type: 'skeletonGrid', count: 3 },
    ],
  },
  {
    screenId: 'SCR-006',
    stateId: 'ST-03',
    title: 'Empty',
    elements: [
      { type: 'header', title: 'EDBS Admin', nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'Users', admin: true },
      { type: 'empty', text: 'No users found. + Add user' },
    ],
  },
  {
    screenId: 'SCR-006',
    stateId: 'ST-04',
    title: 'Error',
    elements: [
      { type: 'header', title: 'EDBS Admin', nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'Users', admin: true },
      { type: 'alert', text: '! Could not load users — Retry', variant: 'error' },
    ],
  },
  {
    screenId: 'SCR-006',
    stateId: 'ST-05',
    title: 'Add user',
    elements: [
      {
        type: 'modal',
        title: 'Add user',
        body: 'Name, email, role, initial password',
        actions: ['Cancel', 'Create'],
      },
    ],
  },
  {
    screenId: 'SCR-006',
    stateId: 'ST-06',
    title: 'Edit user',
    elements: [
      {
        type: 'modal',
        title: 'Edit user',
        body: 'Jane Smith — jane@co.com — Employee',
        actions: ['Cancel', 'Save'],
      },
    ],
  },
  {
    screenId: 'SCR-006',
    stateId: 'ST-07',
    title: 'Reset password',
    elements: [
      {
        type: 'modal',
        title: 'Password reset',
        body: 'New password (shown once): Kx9#mP2vLq — Copy now',
        actions: ['Done'],
      },
    ],
  },
  {
    screenId: 'SCR-006',
    stateId: 'ST-08',
    title: 'Deactivate confirm',
    elements: [
      {
        type: 'modal',
        title: 'Deactivate user?',
        body: 'jane@co.com will not be able to sign in.',
        actions: ['Cancel', 'Deactivate'],
      },
    ],
  },
  {
    screenId: 'SCR-006',
    stateId: 'ST-09',
    title: 'Last admin blocked',
    elements: [
      { type: 'header', title: 'EDBS Admin', nav: ['Desks', 'Users', 'All Bookings'], activeNav: 'Users', admin: true },
      { type: 'alert', text: '! Cannot deactivate the last active Admin', variant: 'error' },
    ],
  },
  // SCR-007 Notification Settings
  {
    screenId: 'SCR-007',
    stateId: 'ST-01',
    title: 'Default opt-out',
    elements: [
      {
        type: 'header',
        title: 'EDBS',
        nav: ['Desk Availability', 'My Bookings'],
        activeNav: 'My Bookings',
      },
      { type: 'subtitle', text: 'Notification settings' },
      { type: 'note', text: 'Email: confirmation, cancel, day-before reminder (automatic)' },
      { type: 'note', text: 'Browser push: Disabled' },
      { type: 'button', text: 'Enable push notifications', primary: true, width: 220 },
      { type: 'note', text: '← Back to my bookings' },
    ],
  },
  {
    screenId: 'SCR-007',
    stateId: 'ST-02',
    title: 'Push enabled',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Desk Availability', 'My Bookings'], activeNav: 'My Bookings' },
      { type: 'subtitle', text: 'Notification settings' },
      { type: 'note', text: 'Browser push: Enabled' },
      { type: 'button', text: 'Disable push notifications', width: 220 },
    ],
  },
  {
    screenId: 'SCR-007',
    stateId: 'ST-03',
    title: 'Browser unsupported',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Desk Availability', 'My Bookings'] },
      {
        type: 'alert',
        text: 'ℹ Browser push unavailable — email notifications still sent',
        variant: 'info',
      },
      { type: 'button', text: 'Enable push (disabled)', width: 220 },
    ],
  },
  {
    screenId: 'SCR-007',
    stateId: 'ST-04',
    title: 'Error saving',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Desk Availability', 'My Bookings'] },
      { type: 'alert', text: '! Could not save preference — Retry', variant: 'error' },
    ],
  },
  {
    screenId: 'SCR-007',
    stateId: 'ST-05',
    title: 'Loading',
    elements: [
      { type: 'header', title: 'EDBS', nav: ['Desk Availability', 'My Bookings'] },
      { type: 'skeletonGrid', count: 2 },
    ],
  },
];

export const FRAME_WIDTH = 1280;
export const FRAME_HEIGHT = 800;
export const COLUMNS = 3;
export const GAP = 48;
