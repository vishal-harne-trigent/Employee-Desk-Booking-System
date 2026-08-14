/**
 * Wireframe frame definitions — aligned with inception/design/screens SCR-### specs.
 * Each entry becomes one Figma frame named SCR-###/ST-## — Title
 */

export type WireElement =
  | { type: 'title'; text: string; y?: number }
  | { type: 'subtitle'; text: string }
  | { type: 'label'; text: string }
  | { type: 'input'; placeholder: string; width?: number }
  | { type: 'button'; text: string; primary?: boolean; width?: number }
  | { type: 'alert'; text: string; variant?: 'error' | 'info' }
  | { type: 'header'; title: string; nav?: string[]; admin?: boolean }
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
      { type: 'title', text: 'Employee Desk Booking' },
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
      { type: 'title', text: 'Employee Desk Booking' },
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
      { type: 'title', text: 'Employee Desk Booking' },
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
      { type: 'title', text: 'Employee Desk Booking' },
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
      { type: 'header', title: 'EDBS', nav: ['Book Desk', 'My Bookings'] },
      { type: 'datePicker', label: 'Book a desk for:' },
      {
        type: 'deskGrid',
        desks: [
          { id: 'A-01', available: true },
          { id: 'A-02', available: false },
          { id: 'B-01', available: true },
          { id: 'B-02', available: true },
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
      { type: 'header', title: 'EDBS Admin', nav: ['All Bookings'], admin: true },
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
      { type: 'header', title: 'EDBS Admin', admin: true },
      { type: 'skeletonGrid', count: 2 },
    ],
  },
  {
    screenId: 'SCR-004',
    stateId: 'ST-03',
    title: 'Empty filter',
    elements: [
      { type: 'header', title: 'EDBS Admin', admin: true },
      { type: 'empty', text: 'No bookings match filters. Clear filters' },
    ],
  },
  {
    screenId: 'SCR-004',
    stateId: 'ST-04',
    title: 'Error',
    elements: [
      { type: 'header', title: 'EDBS Admin', admin: true },
      { type: 'alert', text: '! Could not load bookings — Retry', variant: 'error' },
    ],
  },
  {
    screenId: 'SCR-004',
    stateId: 'ST-05',
    title: 'Filters applied',
    elements: [
      { type: 'header', title: 'EDBS Admin', admin: true },
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
];

export const FRAME_WIDTH = 1280;
export const FRAME_HEIGHT = 800;
export const COLUMNS = 3;
export const GAP = 48;
