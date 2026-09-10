function hex(h) {
  const n = h.replace('#', '');
  return {
    r: parseInt(n.slice(0, 2), 16) / 255,
    g: parseInt(n.slice(2, 4), 16) / 255,
    b: parseInt(n.slice(4, 6), 16) / 255,
  };
}

/** Grayscale — low-fidelity wireframes */
const WIRE_PALETTE = {
  bg: { r: 0.96, g: 0.96, b: 0.96 },
  surface: { r: 1, g: 1, b: 1 },
  border: { r: 0.75, g: 0.75, b: 0.75 },
  borderStrong: { r: 0.2, g: 0.2, b: 0.2 },
  text: { r: 0.1, g: 0.1, b: 0.1 },
  muted: { r: 0.45, g: 0.45, b: 0.45 },
  fillDark: { r: 0.15, g: 0.15, b: 0.15 },
  onDark: { r: 1, g: 1, b: 1 },
  alertBg: { r: 0.94, g: 0.94, b: 0.94 },
  skeleton: { r: 0.88, g: 0.88, b: 0.88 },
  overlay: { r: 0, g: 0, b: 0 },
  primary: { r: 0.15, g: 0.15, b: 0.15 },
  onPrimary: { r: 1, g: 1, b: 1 },
  success: { r: 0.45, g: 0.45, b: 0.45 },
  successBg: { r: 0.92, g: 0.92, b: 0.92 },
  error: { r: 0.1, g: 0.1, b: 0.1 },
  errorBg: { r: 0.94, g: 0.94, b: 0.94 },
  errorBorder: { r: 0.2, g: 0.2, b: 0.2 },
  warning: { r: 0.1, g: 0.1, b: 0.1 },
  warningBg: { r: 0.94, g: 0.94, b: 0.94 },
  unavailableBg: { r: 0.92, g: 0.92, b: 0.92 },
};

/** From inception/design/tokens.css — client hi-fi (navy + green) */
const HIFI_PALETTE = {
  bg: hex('#ffffff'),
  bgMuted: hex('#f0f2f5'),
  surface: hex('#ffffff'),
  border: hex('#e2e8f0'),
  borderStrong: hex('#cbd5e1'),
  text: hex('#0d1b2a'),
  muted: hex('#64748b'),
  fillDark: hex('#0d1b2a'),
  onDark: hex('#ffffff'),
  nav: hex('#0d1b2a'),
  navLight: hex('#1b2d45'),
  primary: hex('#00e676'),
  onPrimary: hex('#0d1b2a'),
  success: hex('#047857'),
  successBg: hex('#ecfdf5'),
  successBorder: hex('#a7f3d0'),
  error: hex('#ef4444'),
  errorBg: hex('#fef2f2'),
  errorBorder: hex('#fecaca'),
  warning: hex('#a16207'),
  warningBg: hex('#fef9c3'),
  unavailableBg: hex('#f1f5f9'),
  skeleton: hex('#e2e8f0'),
  overlay: { r: 0.051, g: 0.106, b: 0.165 },
  alertBg: hex('#fef2f2'),
};
