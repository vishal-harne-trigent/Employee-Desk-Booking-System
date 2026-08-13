const WIREFRAMES = [
  {
    "screenId": "SCR-001",
    "stateId": "ST-01",
    "title": "Default",
    "elements": [
      {
        "type": "title",
        "text": "Employee Desk Booking"
      },
      {
        "type": "subtitle",
        "text": "Sign in"
      },
      {
        "type": "label",
        "text": "Email"
      },
      {
        "type": "input",
        "placeholder": "you@company.com"
      },
      {
        "type": "label",
        "text": "Password"
      },
      {
        "type": "input",
        "placeholder": "••••••••"
      },
      {
        "type": "button",
        "text": "Sign in",
        "primary": true
      }
    ]
  },
  {
    "screenId": "SCR-001",
    "stateId": "ST-02",
    "title": "Loading",
    "elements": [
      {
        "type": "title",
        "text": "Employee Desk Booking"
      },
      {
        "type": "subtitle",
        "text": "Sign in"
      },
      {
        "type": "label",
        "text": "Email"
      },
      {
        "type": "input",
        "placeholder": "jane@company.com"
      },
      {
        "type": "label",
        "text": "Password"
      },
      {
        "type": "input",
        "placeholder": "••••••••"
      },
      {
        "type": "button",
        "text": "Signing in…",
        "primary": true
      },
      {
        "type": "note",
        "text": "(fields disabled)"
      }
    ]
  },
  {
    "screenId": "SCR-001",
    "stateId": "ST-03",
    "title": "Invalid credentials",
    "elements": [
      {
        "type": "title",
        "text": "Employee Desk Booking"
      },
      {
        "type": "alert",
        "text": "! Invalid email or password",
        "variant": "error"
      },
      {
        "type": "subtitle",
        "text": "Sign in"
      },
      {
        "type": "label",
        "text": "Email"
      },
      {
        "type": "input",
        "placeholder": "jane@company.com"
      },
      {
        "type": "label",
        "text": "Password"
      },
      {
        "type": "input",
        "placeholder": ""
      },
      {
        "type": "button",
        "text": "Sign in",
        "primary": true
      }
    ]
  },
  {
    "screenId": "SCR-001",
    "stateId": "ST-04",
    "title": "Deactivated account",
    "elements": [
      {
        "type": "title",
        "text": "Employee Desk Booking"
      },
      {
        "type": "alert",
        "text": "! Account deactivated — contact administrator",
        "variant": "error"
      },
      {
        "type": "subtitle",
        "text": "Sign in"
      },
      {
        "type": "label",
        "text": "Email"
      },
      {
        "type": "input",
        "placeholder": "former@company.com"
      },
      {
        "type": "label",
        "text": "Password"
      },
      {
        "type": "input",
        "placeholder": ""
      },
      {
        "type": "button",
        "text": "Sign in",
        "primary": true
      }
    ]
  },
  {
    "screenId": "SCR-002",
    "stateId": "ST-01",
    "title": "Default",
    "elements": [
      {
        "type": "header",
        "title": "EDBS",
        "nav": [
          "Book Desk",
          "My Bookings"
        ]
      },
      {
        "type": "datePicker",
        "label": "Book a desk for:"
      },
      {
        "type": "note",
        "text": "Select a date to view desk availability."
      }
    ]
  },
  {
    "screenId": "SCR-002",
    "stateId": "ST-02",
    "title": "Loading",
    "elements": [
      {
        "type": "header",
        "title": "EDBS",
        "nav": [
          "Book Desk",
          "My Bookings"
        ]
      },
      {
        "type": "datePicker",
        "label": "Book a desk for:"
      },
      {
        "type": "skeletonGrid",
        "count": 4
      }
    ]
  },
  {
    "screenId": "SCR-002",
    "stateId": "ST-03",
    "title": "Desks available",
    "elements": [
      {
        "type": "header",
        "title": "EDBS",
        "nav": [
          "Book Desk",
          "My Bookings"
        ]
      },
      {
        "type": "datePicker",
        "label": "Book a desk for:"
      },
      {
        "type": "deskGrid",
        "desks": [
          {
            "id": "A-01",
            "available": true
          },
          {
            "id": "A-02",
            "available": false
          },
          {
            "id": "B-01",
            "available": true
          },
          {
            "id": "B-02",
            "available": true
          }
        ]
      }
    ]
  },
  {
    "screenId": "SCR-002",
    "stateId": "ST-04",
    "title": "Empty",
    "elements": [
      {
        "type": "header",
        "title": "EDBS",
        "nav": [
          "Book Desk",
          "My Bookings"
        ]
      },
      {
        "type": "empty",
        "text": "All desks booked for this date. Try another day."
      }
    ]
  },
  {
    "screenId": "SCR-002",
    "stateId": "ST-05",
    "title": "Error",
    "elements": [
      {
        "type": "header",
        "title": "EDBS",
        "nav": [
          "Book Desk",
          "My Bookings"
        ]
      },
      {
        "type": "alert",
        "text": "! Could not load desks — Retry",
        "variant": "error"
      }
    ]
  },
  {
    "screenId": "SCR-002",
    "stateId": "ST-06",
    "title": "Already booked",
    "elements": [
      {
        "type": "header",
        "title": "EDBS",
        "nav": [
          "Book Desk",
          "My Bookings"
        ]
      },
      {
        "type": "alert",
        "text": "ℹ You already have desk A-01. Cancel in My Bookings first.",
        "variant": "info"
      }
    ]
  },
  {
    "screenId": "SCR-002",
    "stateId": "ST-07",
    "title": "Confirm booking",
    "elements": [
      {
        "type": "modal",
        "title": "Confirm booking",
        "body": "Desk B-01 on Thu 14 Aug 2026",
        "actions": [
          "Cancel",
          "Confirm"
        ]
      }
    ]
  },
  {
    "screenId": "SCR-003",
    "stateId": "ST-01",
    "title": "Default",
    "elements": [
      {
        "type": "header",
        "title": "EDBS",
        "nav": [
          "Book Desk",
          "My Bookings"
        ]
      },
      {
        "type": "subtitle",
        "text": "My bookings"
      },
      {
        "type": "table",
        "headers": [
          "Date",
          "Desk",
          "Status",
          "Action"
        ],
        "rows": [
          [
            "14 Aug 2026",
            "A-01",
            "● Confirmed",
            "Cancel"
          ],
          [
            "10 Aug 2026",
            "B-02",
            "○ Completed",
            "—"
          ]
        ]
      }
    ]
  },
  {
    "screenId": "SCR-003",
    "stateId": "ST-02",
    "title": "Loading",
    "elements": [
      {
        "type": "header",
        "title": "EDBS",
        "nav": [
          "Book Desk",
          "My Bookings"
        ]
      },
      {
        "type": "skeletonGrid",
        "count": 3
      }
    ]
  },
  {
    "screenId": "SCR-003",
    "stateId": "ST-03",
    "title": "Empty",
    "elements": [
      {
        "type": "header",
        "title": "EDBS",
        "nav": [
          "Book Desk",
          "My Bookings"
        ]
      },
      {
        "type": "empty",
        "text": "No bookings yet. → Book a desk"
      }
    ]
  },
  {
    "screenId": "SCR-003",
    "stateId": "ST-04",
    "title": "Error",
    "elements": [
      {
        "type": "header",
        "title": "EDBS",
        "nav": [
          "Book Desk",
          "My Bookings"
        ]
      },
      {
        "type": "alert",
        "text": "! Could not load bookings — Retry",
        "variant": "error"
      }
    ]
  },
  {
    "screenId": "SCR-003",
    "stateId": "ST-05",
    "title": "Cancel confirm",
    "elements": [
      {
        "type": "modal",
        "title": "Cancel booking?",
        "body": "Desk A-01 on 14 Aug 2026",
        "actions": [
          "Keep booking",
          "Confirm cancel"
        ]
      }
    ]
  },
  {
    "screenId": "SCR-004",
    "stateId": "ST-01",
    "title": "Default",
    "elements": [
      {
        "type": "header",
        "title": "EDBS Admin",
        "nav": [
          "All Bookings"
        ],
        "admin": true
      },
      {
        "type": "filters",
        "labels": [
          "Date: All",
          "Status: All",
          "Apply"
        ]
      },
      {
        "type": "table",
        "headers": [
          "Date",
          "Employee",
          "Desk",
          "Status",
          "Action"
        ],
        "rows": [
          [
            "14 Aug 2026",
            "jane@co.com",
            "A-01",
            "● Confirmed",
            "Cancel"
          ],
          [
            "13 Aug 2026",
            "bob@co.com",
            "B-02",
            "● Confirmed",
            "Cancel"
          ]
        ]
      }
    ]
  },
  {
    "screenId": "SCR-004",
    "stateId": "ST-02",
    "title": "Loading",
    "elements": [
      {
        "type": "header",
        "title": "EDBS Admin",
        "admin": true
      },
      {
        "type": "skeletonGrid",
        "count": 2
      }
    ]
  },
  {
    "screenId": "SCR-004",
    "stateId": "ST-03",
    "title": "Empty filter",
    "elements": [
      {
        "type": "header",
        "title": "EDBS Admin",
        "admin": true
      },
      {
        "type": "empty",
        "text": "No bookings match filters. Clear filters"
      }
    ]
  },
  {
    "screenId": "SCR-004",
    "stateId": "ST-04",
    "title": "Error",
    "elements": [
      {
        "type": "header",
        "title": "EDBS Admin",
        "admin": true
      },
      {
        "type": "alert",
        "text": "! Could not load bookings — Retry",
        "variant": "error"
      }
    ]
  },
  {
    "screenId": "SCR-004",
    "stateId": "ST-05",
    "title": "Filters applied",
    "elements": [
      {
        "type": "header",
        "title": "EDBS Admin",
        "admin": true
      },
      {
        "type": "chips",
        "items": [
          "Date: 14 Aug 2026",
          "Status: Confirmed"
        ]
      },
      {
        "type": "table",
        "headers": [
          "Date",
          "Employee",
          "Desk",
          "Status",
          "Action"
        ],
        "rows": [
          [
            "14 Aug 2026",
            "jane@co.com",
            "A-01",
            "● Confirmed",
            "Cancel"
          ]
        ]
      }
    ]
  },
  {
    "screenId": "SCR-004",
    "stateId": "ST-06",
    "title": "Cancel on behalf",
    "elements": [
      {
        "type": "modal",
        "title": "Cancel for employee?",
        "body": "jane@co.com — A-01 on 14 Aug 2026",
        "actions": [
          "Keep booking",
          "Confirm cancel"
        ]
      }
    ]
  }
];

const FRAME_WIDTH = 1280;
const FRAME_HEIGHT = 800;
const COLUMNS = 3;
const GAP = 48;

const C = {
  bg: { r: 0.96, g: 0.96, b: 0.96 },
  surface: { r: 1, g: 1, b: 1 },
  border: { r: 0.8, g: 0.8, b: 0.8 },
  text: { r: 0.15, g: 0.15, b: 0.15 },
  muted: { r: 0.45, g: 0.45, b: 0.45 },
  primary: { r: 0.15, g: 0.39, b: 0.92 },
  primaryText: { r: 1, g: 1, b: 1 },
  errorBg: { r: 1, g: 0.95, b: 0.95 },
  errorBorder: { r: 0.99, g: 0.8, b: 0.8 },
  infoBg: { r: 1, g: 0.98, b: 0.88 },
  skeleton: { r: 0.9, g: 0.9, b: 0.9 },
};

let yCursor = 0;
const PAD = 32;
const CONTENT_W = FRAME_WIDTH - PAD * 2;

async function loadFonts() {
  await figma.loadFontAsync({ family: 'Inter', style: 'Regular' });
  await figma.loadFontAsync({ family: 'Inter', style: 'Medium' });
  await figma.loadFontAsync({ family: 'Inter', style: 'Bold' });
}

function solid(color, opacity = 1) {
  return { type: 'SOLID', color, opacity };
}

function text(
  parent,
  content,
  size,
  weight = 'Regular',
  color = C.text,
  x = PAD,
  width = CONTENT_W,
) {
  const node = figma.createText();
  node.fontName = { family: 'Inter', style: weight };
  node.characters = content;
  node.fontSize = size;
  node.fills = [solid(color)];
  node.x = x;
  node.y = yCursor;
  node.resize(width, 40);
  node.textAutoResize = 'HEIGHT';
  parent.appendChild(node);
  yCursor += node.height + 12;
  return node;
}

function box(
  parent,
  w,
  h,
  fill = C.surface,
  stroke = true,
  x = PAD,
) {
  const rect = figma.createRectangle();
  rect.x = x;
  rect.y = yCursor;
  rect.resize(w, h);
  rect.fills = [solid(fill)];
  if (stroke) {
    rect.strokes = [solid(C.border)];
    rect.strokeWeight = 1;
  }
  rect.cornerRadius = 6;
  parent.appendChild(rect);
  yCursor += h + 12;
  return rect;
}

function button(
  parent,
  label,
  primary = false,
  width = 200,
) {
  const h = 40;
  const rect = box(parent, width, h, primary ? C.primary : C.surface, true);
  rect.strokes = primary ? [] : [solid(C.border)];
  const labelNode = figma.createText();
  labelNode.fontName = { family: 'Inter', style: 'Medium' };
  labelNode.characters = label;
  labelNode.fontSize = 14;
  labelNode.fills = [solid(primary ? C.primaryText : C.text)];
  labelNode.x = rect.x + 16;
  labelNode.y = rect.y + 11;
  parent.appendChild(labelNode);
}

function renderElement(parent, el) {
  switch (el.type) {
    case 'title':
      text(parent, el.text, 22, 'Bold');
      break;
    case 'subtitle':
      text(parent, el.text, 16, 'Medium');
      break;
    case 'label':
      text(parent, el.text, 12, 'Medium', C.muted);
      yCursor -= 4;
      break;
    case 'input':
      box(parent, el.width ?? 360, 40);
      {
        const ph = figma.createText();
        ph.fontName = { family: 'Inter', style: 'Regular' };
        ph.characters = el.placeholder;
        ph.fontSize = 14;
        ph.fills = [solid(C.muted)];
        ph.x = PAD + 12;
        ph.y = yCursor - 40 + 11;
        parent.appendChild(ph);
      }
      break;
    case 'button':
      button(parent, el.text, el.primary, el.width ?? 360);
      break;
    case 'alert': {
      const h = 48;
      const bg = el.variant === 'info' ? C.infoBg : C.errorBg;
      box(parent, CONTENT_W, h, bg);
      text(parent, el.text, 13, 'Regular', C.text, PAD + 8, CONTENT_W - 16);
      yCursor -= 12;
      break;
    }
    case 'header': {
      const bar = figma.createFrame();
      bar.name = 'Header';
      bar.x = 0;
      bar.y = 0;
      bar.resize(FRAME_WIDTH, 56);
      bar.fills = [solid(C.surface)];
      bar.strokes = [solid(C.border)];
      bar.strokeWeight = 1;
      bar.strokeAlign = 'INSIDE';
      parent.appendChild(bar);
      const title = figma.createText();
      title.fontName = { family: 'Inter', style: 'Bold' };
      title.characters = el.title;
      title.fontSize = 16;
      title.x = PAD;
      title.y = 18;
      bar.appendChild(title);
      if (el.nav?.length) {
        const navText = figma.createText();
        navText.fontName = { family: 'Inter', style: 'Regular' };
        navText.characters = el.nav.join('  ·  ');
        navText.fontSize = 13;
        navText.fills = [solid(C.muted)];
        navText.x = 200;
        navText.y = 20;
        bar.appendChild(navText);
      }
      const signOut = figma.createText();
      signOut.fontName = { family: 'Inter', style: 'Medium' };
      signOut.characters = 'Sign out';
      signOut.fontSize = 12;
      signOut.fills = [solid(C.primary)];
      signOut.x = FRAME_WIDTH - PAD - 60;
      signOut.y = 20;
      bar.appendChild(signOut);
      yCursor = 72;
      break;
    }
    case 'datePicker':
      text(parent, el.label, 13, 'Regular');
      box(parent, 280, 36);
      break;
    case 'deskGrid': {
      const cols = 4;
      const cardW = (CONTENT_W - 36) / cols;
      const startY = yCursor;
      el.desks.forEach((desk, i) => {
        const col = i % cols;
        const row = Math.floor(i / cols);
        const card = figma.createFrame();
        card.x = PAD + col * (cardW + 12);
        card.y = startY + row * 110;
        card.resize(cardW, 96);
        card.fills = [solid(C.surface)];
        card.strokes = [solid(C.border)];
        card.strokeWeight = 1;
        card.cornerRadius = 6;
        parent.appendChild(card);
        const num = figma.createText();
        num.fontName = { family: 'Inter', style: 'Bold' };
        num.characters = desk.id;
        num.fontSize = 16;
        num.x = card.x + 12;
        num.y = card.y + 12;
        parent.appendChild(num);
        const badge = figma.createText();
        badge.fontName = { family: 'Inter', style: 'Regular' };
        badge.characters = desk.available ? '✓ Available' : '✗ Booked';
        badge.fontSize = 11;
        badge.fills = [solid(C.muted)];
        badge.x = card.x + 12;
        badge.y = card.y + 36;
        parent.appendChild(badge);
        if (desk.available) {
          const btn = figma.createRectangle();
          btn.x = card.x + 12;
          btn.y = card.y + 58;
          btn.resize(cardW - 24, 28);
          btn.fills = [solid(C.primary)];
          btn.cornerRadius = 4;
          parent.appendChild(btn);
          const bt = figma.createText();
          bt.fontName = { family: 'Inter', style: 'Medium' };
          bt.characters = 'Book';
          bt.fontSize = 12;
          bt.fills = [solid(C.primaryText)];
          bt.x = btn.x + (cardW - 24) / 2 - 14;
          bt.y = btn.y + 6;
          parent.appendChild(bt);
        }
      });
      yCursor = startY + Math.ceil(el.desks.length / cols) * 110 + 12;
      break;
    }
    case 'skeletonGrid':
      for (let i = 0; i < el.count; i++) {
        box(parent, CONTENT_W, 32, C.skeleton, false);
      }
      break;
    case 'empty':
      text(parent, el.text, 14, 'Regular', C.muted);
      break;
    case 'table': {
      text(parent, '', 1);
      yCursor -= 12;
      const rowH = 28;
      el.headers.forEach((h, i) => {
        const cell = figma.createText();
        cell.fontName = { family: 'Inter', style: 'Medium' };
        cell.characters = h;
        cell.fontSize = 11;
        cell.fills = [solid(C.muted)];
        cell.x = PAD + i * 140;
        cell.y = yCursor;
        parent.appendChild(cell);
      });
      yCursor += rowH;
      el.rows.forEach((row) => {
        row.forEach((cellText, i) => {
          const cell = figma.createText();
          cell.fontName = { family: 'Inter', style: 'Regular' };
          cell.characters = cellText;
          cell.fontSize = 12;
          cell.x = PAD + i * 140;
          cell.y = yCursor;
          parent.appendChild(cell);
        });
        yCursor += rowH;
        box(parent, CONTENT_W, 1, C.border, false);
        yCursor -= 12;
      });
      break;
    }
    case 'filters':
      el.labels.forEach((lbl, i) => {
        const t = figma.createText();
        t.fontName = { family: 'Inter', style: 'Regular' };
        t.characters = lbl;
        t.fontSize = 12;
        t.x = PAD + i * 160;
        t.y = yCursor;
        if (lbl === 'Apply') {
          box(parent, 72, 32, C.primary, false, PAD + i * 160);
          t.fills = [solid(C.primaryText)];
          t.y = yCursor + 8;
        }
        parent.appendChild(t);
      });
      yCursor += 44;
      break;
    case 'chips':
      el.items.forEach((item, i) => {
        box(parent, 140, 24, C.bg, true, PAD + i * 150);
        const t = figma.createText();
        t.fontName = { family: 'Inter', style: 'Regular' };
        t.characters = item;
        t.fontSize = 10;
        t.x = PAD + i * 150 + 8;
        t.y = yCursor - 24 + 6;
        parent.appendChild(t);
      });
      break;
    case 'modal': {
      const overlay = figma.createRectangle();
      overlay.x = 0;
      overlay.y = 0;
      overlay.resize(FRAME_WIDTH, FRAME_HEIGHT);
      overlay.fills = [solid({ r: 0, g: 0, b: 0 }, 0.35)];
      parent.appendChild(overlay);
      const modalW = 400;
      const modalH = 180;
      const modal = figma.createFrame();
      modal.x = (FRAME_WIDTH - modalW) / 2;
      modal.y = (FRAME_HEIGHT - modalH) / 2;
      modal.resize(modalW, modalH);
      modal.fills = [solid(C.surface)];
      modal.cornerRadius = 8;
      modal.strokes = [solid(C.border)];
      modal.strokeWeight = 1;
      parent.appendChild(modal);
      const mt = figma.createText();
      mt.fontName = { family: 'Inter', style: 'Bold' };
      mt.characters = el.title;
      mt.fontSize = 16;
      mt.x = modal.x + 24;
      mt.y = modal.y + 24;
      parent.appendChild(mt);
      const mb = figma.createText();
      mb.fontName = { family: 'Inter', style: 'Regular' };
      mb.characters = el.body;
      mb.fontSize = 13;
      mb.x = modal.x + 24;
      mb.y = modal.y + 56;
      parent.appendChild(mb);
      el.actions.forEach((action, i) => {
        const bw = 160;
        const bx = modal.x + 24 + i * (bw + 12);
        const by = modal.y + modalH - 52;
        const rect = figma.createRectangle();
        rect.x = bx;
        rect.y = by;
        rect.resize(bw, 36);
        rect.fills = [solid(i === el.actions.length - 1 ? C.primary : C.bg)];
        rect.cornerRadius = 6;
        if (i !== el.actions.length - 1) {
          rect.strokes = [solid(C.border)];
          rect.strokeWeight = 1;
        }
        parent.appendChild(rect);
        const bt = figma.createText();
        bt.fontName = { family: 'Inter', style: 'Medium' };
        bt.characters = action;
        bt.fontSize = 12;
        bt.fills = [solid(i === el.actions.length - 1 ? C.primaryText : C.text)];
        bt.x = bx + 16;
        bt.y = by + 10;
        parent.appendChild(bt);
      });
      yCursor = FRAME_HEIGHT;
      break;
    }
    case 'spacer':
      yCursor += el.height;
      break;
    case 'note':
      text(parent, el.text, 12, 'Regular', C.muted);
      break;
  }
}

function buildFrame(state) {
  yCursor = PAD;
  const frame = figma.createFrame();
  frame.name = `${state.screenId}/${state.stateId} — ${state.title}`;
  frame.resize(FRAME_WIDTH, FRAME_HEIGHT);
  frame.fills = [solid(C.bg)];
  frame.clipsContent = true;

  const badge = figma.createText();
  badge.fontName = { family: 'Inter', style: 'Regular' };
  badge.characters = `${state.screenId} / ${state.stateId}`;
  badge.fontSize = 10;
  badge.fills = [solid(C.muted)];
  badge.x = FRAME_WIDTH - 120;
  badge.y = 8;
  frame.appendChild(badge);

  for (const el of state.elements) {
    renderElement(frame, el);
  }
  return frame;
}

async function generateWireframes() {
  await loadFonts();

  let page = figma.currentPage;
  if (page.name !== 'EDBS Wireframes') {
    page = figma.createPage();
    page.name = 'EDBS Wireframes';
    figma.currentPage = page;
  }

  const existing = page.findAll(
    (n) => n.type === 'FRAME' && n.name.startsWith('SCR-'),
  );
  for (const node of existing) {
    node.remove();
  }

  const frames = [];
  for (const state of WIREFRAMES) {
    frames.push(buildFrame(state));
  }

  frames.forEach((frame, index) => {
    const col = index % COLUMNS;
    const row = Math.floor(index / COLUMNS);
    frame.x = col * (FRAME_WIDTH + GAP);
    frame.y = row * (FRAME_HEIGHT + GAP);
    page.appendChild(frame);
  });

  figma.viewport.scrollAndZoomIntoView(frames);
  return frames.length;
}

figma.showUI(__html__, { width: 360, height: 280 });

figma.ui.onmessage = async (msg) => {
  if (msg.type === 'generate') {
    try {
      const count = await generateWireframes();
      figma.ui.postMessage({ type: 'done', count });
      figma.notify(`Created ${count} wireframe frames on "EDBS Wireframes" page`);
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err);
      figma.ui.postMessage({ type: 'error', message });
      figma.notify(`Error: ${message}`, { error: true });
    }
  }
  if (msg.type === 'cancel') {
    figma.closePlugin();
  }
};
