function createGenerator(config) {
  const C = config.palette;
  const hifi = config.hifi;
  const nameTag = config.nameTag;
  let yCursor = 0;
  let padX = 32;
  let contentW = FRAME_WIDTH - 64;
  let layoutAuth = false;
  let logoImage = null;
  const PAD = 32;

  async function loadFonts() {
    await figma.loadFontAsync({ family: 'Inter', style: 'Regular' });
    await figma.loadFontAsync({ family: 'Inter', style: 'Medium' });
    await figma.loadFontAsync({ family: 'Inter', style: 'Bold' });
  }

  async function loadLogo() {
    if (!hifi || typeof LOGO_BASE64 === 'undefined') return;
    const binary = atob(LOGO_BASE64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
    logoImage = figma.createImage(bytes);
  }

  function placeLogo(parent, x, y, height) {
    const aspect = typeof LOGO_ASPECT === 'number' ? LOGO_ASPECT : 4.93;
    const w = height * aspect;
    if (hifi && logoImage) {
      const rect = figma.createRectangle();
      rect.name = 'Logo';
      rect.x = x;
      rect.y = y;
      rect.resize(w, height);
      rect.fills = [{ type: 'IMAGE', scaleMode: 'FIT', imageHash: logoImage.hash }];
      parent.appendChild(rect);
      return w;
    }
    const rect = figma.createRectangle();
    rect.name = 'Logo';
    rect.x = x;
    rect.y = y;
    rect.resize(w, height);
    rect.fills = [solid(C.surface)];
    rect.strokes = [solid(C.border)];
    rect.strokeWeight = 1;
    rect.cornerRadius = 4;
    parent.appendChild(rect);
    const label = figma.createText();
    label.fontName = { family: 'Inter', style: 'Medium' };
    label.characters = 'Desk Booking';
    label.fontSize = 11;
    label.fills = [solid(C.muted)];
    label.x = x + 8;
    label.y = y + Math.max(4, (height - 14) / 2);
    parent.appendChild(label);
    return w;
  }

  function solid(color, opacity = 1) {
    return { type: 'SOLID', color, opacity };
  }

  function parentSize(parent) {
    return {
      w: 'width' in parent ? parent.width : FRAME_WIDTH,
      h: 'height' in parent ? parent.height : FRAME_HEIGHT,
    };
  }

  function addShadow(node) {
    if (!hifi) return;
    node.effects = [
      {
        type: 'DROP_SHADOW',
        color: { r: 0, g: 0, b: 0, a: 0.07 },
        offset: { x: 0, y: 4 },
        radius: 6,
        spread: 0,
        visible: true,
        blendMode: 'NORMAL',
      },
    ];
  }

  function createShell(parent, width, height, x, y) {
    const shell = figma.createFrame();
    shell.name = 'Shell';
    shell.x = x;
    shell.y = y;
    shell.resize(width, height);
    shell.fills = [solid(C.surface)];
    shell.strokes = [solid(C.border)];
    shell.strokeWeight = 1;
    shell.cornerRadius = 12;
    shell.clipsContent = true;
    addShadow(shell);
    parent.appendChild(shell);
    return shell;
  }

  function text(parent, content, size, weight = 'Regular', color = C.text, x = padX, width = contentW) {
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

  function box(parent, w, h, fill = C.surface, stroke = true, x = padX) {
    const rect = figma.createRectangle();
    rect.x = x;
    rect.y = yCursor;
    rect.resize(w, h);
    rect.fills = [solid(fill)];
    if (stroke) {
      rect.strokes = [solid(C.border)];
      rect.strokeWeight = 1;
    }
    rect.cornerRadius = hifi ? 8 : 6;
    parent.appendChild(rect);
    yCursor += h + 12;
    return rect;
  }

  function primaryButton(parent, label, width = 200) {
    const h = 40;
    const fill = hifi ? C.primary : C.fillDark;
    const fg = hifi ? C.onPrimary : C.onDark;
    const rect = box(parent, width, h, fill, !hifi);
    if (!hifi) {
      rect.strokes = [solid(C.fillDark)];
    }
    const labelNode = figma.createText();
    labelNode.fontName = { family: 'Inter', style: 'Medium' };
    labelNode.characters = label;
    labelNode.fontSize = 14;
    labelNode.fills = [solid(fg)];
    labelNode.x = rect.x + 16;
    labelNode.y = rect.y + 11;
    parent.appendChild(labelNode);
  }

  function pill(parent, x, y, label, bg, fg, border) {
    const pillW = Math.max(label.length * 6.5 + 16, 72);
    const pillH = 22;
    const rect = figma.createRectangle();
    rect.x = x;
    rect.y = y;
    rect.resize(pillW, pillH);
    rect.fills = [solid(bg)];
    rect.cornerRadius = pillH / 2;
    if (border) {
      rect.strokes = [solid(border)];
      rect.strokeWeight = 1;
    }
    parent.appendChild(rect);
    const t = figma.createText();
    t.fontName = { family: 'Inter', style: 'Medium' };
    t.characters = label;
    t.fontSize = 10;
    t.fills = [solid(fg)];
    t.x = x + 8;
    t.y = y + 5;
    parent.appendChild(t);
    return pillW;
  }

  function renderElement(parent, el) {
    switch (el.type) {
      case 'logo': {
        const logoH = hifi ? 56 : 32;
        const logoW = logoH * (typeof LOGO_ASPECT === 'number' ? LOGO_ASPECT : 4.93);
        const lx = hifi && layoutAuth ? padX + (contentW - logoW) / 2 : padX;
        placeLogo(parent, lx, yCursor, logoH);
        yCursor += logoH + (hifi ? 24 : 16);
        break;
      }
      case 'title': {
        const node = text(parent, el.text, hifi ? 20 : 22, 'Bold');
        if (hifi && layoutAuth) {
          node.textAlignHorizontal = 'CENTER';
          node.x = padX;
          node.resize(contentW, 40);
        }
        break;
      }
      case 'subtitle':
        text(parent, el.text, 16, 'Medium');
        break;
      case 'label':
        text(parent, el.text, 12, 'Medium', C.muted);
        yCursor -= 4;
        break;
      case 'input':
        box(parent, el.width ?? contentW, 40);
        if (el.placeholder) {
          const ph = figma.createText();
          ph.fontName = { family: 'Inter', style: 'Regular' };
          ph.characters = el.placeholder;
          ph.fontSize = 14;
          ph.fills = [solid(C.muted)];
          ph.x = padX + 12;
          ph.y = yCursor - 40 + 11;
          parent.appendChild(ph);
        }
        break;
      case 'button':
        if (el.primary) {
          primaryButton(parent, el.text, el.width ?? contentW);
        } else {
          box(parent, el.width ?? contentW, 40, C.surface, true);
          const t = figma.createText();
          t.fontName = { family: 'Inter', style: 'Medium' };
          t.characters = el.text;
          t.fontSize = 14;
          t.x = padX + 16;
          t.y = yCursor - 40 + 11;
          parent.appendChild(t);
        }
        break;
      case 'alert': {
        const isInfo = el.variant === 'info';
        const bg = hifi ? (isInfo ? C.warningBg : C.errorBg) : C.alertBg;
        const stroke = hifi ? (isInfo ? C.warning : C.errorBorder) : C.borderStrong;
        const fg = hifi ? (isInfo ? C.warning : C.error) : C.text;
        const alertBox = box(parent, contentW, 48, bg);
        alertBox.strokes = [solid(stroke)];
        alertBox.strokeWeight = 1;
        text(parent, el.text, 13, 'Regular', fg, padX + 8, contentW - 16);
        yCursor -= 12;
        break;
      }
      case 'header': {
        const { w: barW } = parentSize(parent);
        const bar = figma.createFrame();
        bar.name = 'Header';
        bar.x = 0;
        bar.y = 0;
        bar.resize(barW, hifi ? 68 : 56);
        bar.fills = [solid(hifi ? C.nav : C.surface)];
        bar.strokes = [solid(hifi ? C.nav : C.border)];
        bar.strokeWeight = 1;
        bar.strokeAlign = 'INSIDE';
        parent.appendChild(bar);
        const logoH = hifi ? 36 : 28;
        const logoY = hifi ? 16 : 14;
        const logoW = placeLogo(bar, padX, logoY, logoH);
        if (el.nav?.length) {
          let nx = padX + logoW + 24;
          el.nav.forEach((item, i) => {
            const isActive = el.activeNav ? item === el.activeNav : i === 0;
            const navItem = figma.createText();
            navItem.fontName = { family: 'Inter', style: isActive && hifi ? 'Bold' : isActive ? 'Medium' : 'Regular' };
            navItem.characters = item;
            navItem.fontSize = 13;
            if (hifi && isActive) {
              const pill = figma.createRectangle();
              pill.x = nx - 8;
              pill.y = 18;
              pill.resize(item.length * 7 + 16, 32);
              pill.fills = [solid(C.primary)];
              pill.cornerRadius = 8;
              bar.appendChild(pill);
              navItem.fills = [solid(C.onPrimary)];
            } else {
              navItem.fills = [solid(hifi ? C.onDark : isActive ? C.text : C.muted, hifi ? (isActive ? 1 : 0.75) : 1)];
            }
            navItem.x = nx;
            navItem.y = hifi ? 26 : 20;
            bar.appendChild(navItem);
            nx += item.length * 7 + 24;
          });
        }
        const signOut = figma.createText();
        signOut.fontName = { family: 'Inter', style: 'Medium' };
        signOut.characters = 'Sign out';
        signOut.fontSize = 12;
        signOut.fills = [solid(hifi ? C.onDark : C.text, hifi ? 0.6 : 1)];
        signOut.x = barW - padX - 60;
        signOut.y = hifi ? 26 : 20;
        bar.appendChild(signOut);
        yCursor = hifi ? 84 : 72;
        break;
      }
      case 'datePicker':
        text(parent, el.label, 13, 'Regular');
        box(parent, 280, 36);
        break;
      case 'deskGrid': {
        const cols = 4;
        const cardW = (contentW - 36) / cols;
        const startY = yCursor;
        el.desks.forEach((desk, i) => {
          const col = i % cols;
          const row = Math.floor(i / cols);
          const card = figma.createFrame();
          card.x = padX + col * (cardW + 12);
          card.y = startY + row * 118;
          card.resize(cardW, 104);
          card.fills = [solid(desk.available || !hifi ? C.surface : C.unavailableBg)];
          card.strokes = [solid(C.border)];
          card.strokeWeight = 1;
          card.cornerRadius = 8;
          parent.appendChild(card);
          const num = figma.createText();
          num.fontName = { family: 'Inter', style: 'Bold' };
          num.characters = desk.id;
          num.fontSize = 16;
          num.fills = [solid(C.text)];
          num.x = card.x + 12;
          num.y = card.y + 10;
          num.textAlignHorizontal = 'CENTER';
          num.resize(cardW - 24, 20);
          parent.appendChild(num);
          const badgeLabel = desk.available ? '✓ Available' : '✗ Booked';
          if (hifi) {
            const badgeW = pill(
              parent,
              card.x + (cardW - 80) / 2,
              card.y + 34,
              badgeLabel,
              desk.available ? C.successBg : C.unavailableBg,
              desk.available ? C.success : C.muted,
              desk.available ? C.successBorder : null,
            );
            void badgeW;
          } else {
            const badge = figma.createText();
            badge.fontName = { family: 'Inter', style: 'Regular' };
            badge.characters = badgeLabel;
            badge.fontSize = 11;
            badge.fills = [solid(C.muted)];
            badge.x = card.x + 12;
            badge.y = card.y + 36;
            parent.appendChild(badge);
          }
          if (desk.available) {
            const btn = figma.createRectangle();
            btn.x = card.x + 12;
            btn.y = card.y + 64;
            btn.resize(cardW - 24, 28);
            btn.fills = [solid(hifi ? C.primary : C.fillDark)];
            btn.cornerRadius = 6;
            parent.appendChild(btn);
            const bt = figma.createText();
            bt.fontName = { family: 'Inter', style: 'Medium' };
            bt.characters = 'Book';
            bt.fontSize = 12;
            bt.fills = [solid(hifi ? C.onPrimary : C.onDark)];
            bt.x = btn.x + (cardW - 24) / 2 - 14;
            bt.y = btn.y + 6;
            parent.appendChild(bt);
          }
        });
        yCursor = startY + Math.ceil(el.desks.length / cols) * 118 + 12;
        break;
      }
      case 'skeletonGrid':
        for (let i = 0; i < el.count; i++) {
          box(parent, contentW, 100, hifi ? C.unavailableBg : C.skeleton, false);
        }
        break;
      case 'empty': {
        const node = text(parent, el.text, 14, 'Regular', C.muted);
        if (hifi) {
          node.textAlignHorizontal = 'CENTER';
          node.x = padX;
          node.resize(contentW, 40);
        }
        break;
      }
      case 'table': {
        text(parent, '', 1);
        yCursor -= 12;
        const rowH = 32;
        el.headers.forEach((h, i) => {
          const cell = figma.createText();
          cell.fontName = { family: 'Inter', style: 'Medium' };
          cell.characters = h;
          cell.fontSize = 11;
          cell.fills = [solid(C.muted)];
          cell.x = padX + i * 140;
          cell.y = yCursor;
          parent.appendChild(cell);
        });
        yCursor += rowH;
        el.rows.forEach((row) => {
          row.forEach((cellText, i) => {
            if (hifi && (cellText === 'Confirmed' || cellText === 'Completed' || cellText === 'Cancelled')) {
              const statusBg =
                cellText === 'Confirmed' ? C.successBg : cellText === 'Completed' ? C.unavailableBg : C.errorBg;
              const statusFg =
                cellText === 'Confirmed' ? C.success : cellText === 'Completed' ? C.muted : C.error;
              const statusBorder =
                cellText === 'Confirmed' ? C.successBorder : cellText === 'Cancelled' ? C.errorBorder : null;
              pill(parent, padX + i * 140, yCursor, cellText, statusBg, statusFg, statusBorder);
              return;
            }
            const cell = figma.createText();
            cell.fontName = { family: 'Inter', style: 'Regular' };
            cell.characters = cellText;
            cell.fontSize = 12;
            cell.fills = [solid(cellText === 'Cancel' && hifi ? C.primary : C.text)];
            cell.x = padX + i * 140;
            cell.y = yCursor + (hifi && cellText === 'Cancel' ? 4 : 0);
            parent.appendChild(cell);
          });
          yCursor += rowH;
          box(parent, contentW, 1, C.border, false);
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
          t.fills = [solid(C.text)];
          t.x = padX + i * 160;
          t.y = yCursor;
          if (lbl === 'Apply') {
            const btn = figma.createRectangle();
            btn.x = padX + i * 160;
            btn.y = yCursor;
            btn.resize(72, 32);
            btn.fills = [solid(hifi ? C.primary : C.fillDark)];
            btn.cornerRadius = 6;
            parent.appendChild(btn);
            t.fills = [solid(hifi ? C.onPrimary : C.onDark)];
            t.y = yCursor + 8;
            t.x = padX + i * 160 + 16;
          }
          parent.appendChild(t);
        });
        yCursor += 44;
        break;
      case 'chips':
        el.items.forEach((item, i) => {
          const chip = box(parent, 140, 24, hifi ? C.successBg : C.bg, true, padX + i * 150);
          if (hifi) {
            chip.strokes = [solid(C.successBorder)];
          }
          const t = figma.createText();
          t.fontName = { family: 'Inter', style: 'Regular' };
          t.characters = item;
          t.fontSize = 10;
          t.fills = [solid(hifi ? C.success : C.text)];
          t.x = padX + i * 150 + 8;
          t.y = yCursor - 24 + 6;
          parent.appendChild(t);
        });
        break;
      case 'modal': {
        const { w: pw, h: ph } = parentSize(parent);
        const overlay = figma.createRectangle();
        overlay.x = 0;
        overlay.y = 0;
        overlay.resize(pw, ph);
        overlay.fills = [solid(C.overlay, hifi ? 0.4 : 0.35)];
        parent.appendChild(overlay);
        const modalW = 400;
        const modalH = 180;
        const modal = figma.createFrame();
        modal.x = (pw - modalW) / 2;
        modal.y = (ph - modalH) / 2;
        modal.resize(modalW, modalH);
        modal.fills = [solid(C.surface)];
        modal.cornerRadius = 12;
        modal.strokes = [solid(C.border)];
        modal.strokeWeight = 1;
        addShadow(modal);
        parent.appendChild(modal);
        const mt = figma.createText();
        mt.fontName = { family: 'Inter', style: 'Bold' };
        mt.characters = el.title;
        mt.fontSize = 16;
        mt.fills = [solid(C.text)];
        mt.x = modal.x + 24;
        mt.y = modal.y + 24;
        parent.appendChild(mt);
        const mb = figma.createText();
        mb.fontName = { family: 'Inter', style: 'Regular' };
        mb.characters = el.body;
        mb.fontSize = 13;
        mb.fills = [solid(C.text)];
        mb.x = modal.x + 24;
        mb.y = modal.y + 56;
        parent.appendChild(mb);
        el.actions.forEach((action, i) => {
          const isPrimary = i === el.actions.length - 1;
          const bw = 160;
          const bx = modal.x + 24 + i * (bw + 12);
          const by = modal.y + modalH - 52;
          const rect = figma.createRectangle();
          rect.x = bx;
          rect.y = by;
          rect.resize(bw, 36);
          rect.fills = [solid(isPrimary ? (hifi ? C.primary : C.fillDark) : C.surface)];
          rect.cornerRadius = 6;
          rect.strokes = [solid(isPrimary ? (hifi ? C.primary : C.fillDark) : C.border)];
          rect.strokeWeight = 1;
          parent.appendChild(rect);
          const bt = figma.createText();
          bt.fontName = { family: 'Inter', style: 'Medium' };
          bt.characters = action;
          bt.fontSize = 12;
          bt.fills = [solid(isPrimary ? (hifi ? C.onPrimary : C.onDark) : C.text)];
          bt.x = bx + 16;
          bt.y = by + 10;
          parent.appendChild(bt);
        });
        yCursor = ph;
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
    padX = PAD;
    contentW = FRAME_WIDTH - PAD * 2;
    layoutAuth = false;
    yCursor = PAD;

    const frame = figma.createFrame();
    frame.name = `${state.screenId}/${state.stateId} — ${state.title}${nameTag}`;
    frame.resize(FRAME_WIDTH, FRAME_HEIGHT);
    frame.clipsContent = true;

    const badge = figma.createText();
    badge.fontName = { family: 'Inter', style: 'Regular' };
    badge.characters = hifi ? `${state.screenId} / ${state.stateId} · Design` : `${state.screenId} / ${state.stateId}`;
    badge.fontSize = 10;
    badge.fills = [solid(C.muted)];
    badge.x = FRAME_WIDTH - 160;
    badge.y = 8;
    frame.appendChild(badge);

    let contentParent = frame;
    const hasHeader = state.elements.some((e) => e.type === 'header');
    frame.fills = [solid(hifi ? (hasHeader ? C.bgMuted : C.nav) : C.bg)];

    if (hifi) {
      if (hasHeader) {
        const shellW = FRAME_WIDTH - PAD * 2;
        const shellH = FRAME_HEIGHT - PAD * 2;
        contentParent = createShell(frame, shellW, shellH, PAD, PAD);
        padX = 24;
        contentW = shellW - padX * 2;
        yCursor = 0;
      } else {
        layoutAuth = true;
        const cardW = 420;
        const cardH = 520;
        contentParent = createShell(
          frame,
          cardW,
          cardH,
          (FRAME_WIDTH - cardW) / 2,
          (FRAME_HEIGHT - cardH) / 2,
        );
        padX = 32;
        contentW = cardW - padX * 2;
        yCursor = 32;
      }
    }

    for (const el of state.elements) {
      renderElement(contentParent, el);
    }
    return frame;
  }

  function isWireframeNode(n) {
    return n.type === 'FRAME' && n.name.startsWith('SCR-') && !n.name.includes('(Design)');
  }

  function isDesignNode(n) {
    return n.type === 'FRAME' && n.name.includes('(Design)');
  }

  async function generate() {
    await loadFonts();
    await loadLogo();
    const page = figma.currentPage;

    if (hifi) {
      page.findAll(isDesignNode).forEach((n) => n.remove());
    } else {
      page.findAll(isWireframeNode).forEach((n) => n.remove());
    }

    let startX = 0;
    let startY = 0;

    if (hifi) {
      const wireframes = page.findAll(isWireframeNode);
      if (wireframes.length > 0) {
        let maxX = 0;
        let minY = Infinity;
        wireframes.forEach((f) => {
          maxX = Math.max(maxX, f.x + f.width);
          minY = Math.min(minY, f.y);
        });
        startX = maxX + GAP;
        startY = minY;
      } else {
        for (const child of page.children) {
          if (child.type === 'FRAME' || child.type === 'GROUP') {
            startY = Math.max(startY, child.y + child.height + GAP);
          }
        }
      }
    } else {
      for (const child of page.children) {
        if (child.type === 'FRAME' || child.type === 'GROUP') {
          const bottom = child.y + ('height' in child ? child.height : 0);
          startY = Math.max(startY, bottom + GAP);
        }
      }
    }

    const frames = [];
    for (const state of WIREFRAMES) {
      frames.push(buildFrame(state));
    }

    frames.forEach((frame, index) => {
      const col = index % COLUMNS;
      const row = Math.floor(index / COLUMNS);
      frame.x = startX + col * (FRAME_WIDTH + GAP);
      frame.y = startY + row * (FRAME_HEIGHT + GAP);
      page.appendChild(frame);
    });

    figma.viewport.scrollAndZoomIntoView(frames);
    return frames.length;
  }

  return { generate, buildFrame, loadFonts };
}

const wireframeApi = createGenerator({
  palette: WIRE_PALETTE,
  hifi: false,
  nameTag: '',
});

const hiFiApi = createGenerator({
  palette: HIFI_PALETTE,
  hifi: true,
  nameTag: ' (Design)',
});

const generateWireframes = wireframeApi.generate;
const generateHiFiDesigns = hiFiApi.generate;
const buildHiFiFrame = hiFiApi.buildFrame;
const loadHiFiFonts = hiFiApi.loadFonts;
