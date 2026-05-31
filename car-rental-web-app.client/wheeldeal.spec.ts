/**
 * WheelDeal – Full Application Screenshot Suite
 * ------------------------------------------------
 * Acoperire completă: fiecare pagină + fiecare acțiune importantă.
 *
 * Rulare:
 *   npx playwright test wheeldeal.spec.ts --headed
 *
 * Output: screenshots/
 */

import { test, Page } from '@playwright/test';
// @ts-ignore
const fs = require('fs');

// ── Config ────────────────────────────────────────────────────────────────────
const BASE  = 'http://localhost:4200';
const OUT   = 'screenshots';
const WAIT  = 500;
const CREDS = { email: 'admin@wheeldeal.ro', password: 'Admin@123!' };

// Crează folderele de output dacă nu există
['home','about','cars','offers','contact','auth','dashboard','bookings','mobile']
  .forEach(dir => fs.mkdirSync(`${OUT}/${dir}`, { recursive: true }));

// ── Helpers ───────────────────────────────────────────────────────────────────

async function shot(page: Page, path: string) {
  await page.waitForTimeout(WAIT);
  await page.screenshot({ path: `${OUT}/${path}.png`, fullPage: true });
  console.log(`  ✓ ${path}`);
}

async function scrollReveal(page: Page) {
  await page.evaluate(async () => {
    await new Promise<void>(resolve => {
      let pos = 0;
      const step = () => {
        pos += 300;
        window.scrollTo(0, pos);
        if (pos < document.body.scrollHeight) requestAnimationFrame(step);
        else resolve();
      };
      requestAnimationFrame(step);
    });
  });
  await page.waitForTimeout(600);
  await page.evaluate(() => window.scrollTo(0, 0));
}

async function login(page: Page) {
  await page.goto(`${BASE}/login`, { waitUntil: 'domcontentloaded' });
  await page.waitForTimeout(WAIT);
  await page.fill('[type="email"]', CREDS.email);
  await page.fill('[type="password"]', CREDS.password);

  // Click buton submit
  await page.locator('[type="submit"]').click();

  // Așteptăm redirect după login — max 8s
  await Promise.race([
    page.waitForURL(`${BASE}/dashboard`, { timeout: 8000 }),
    page.waitForURL(`${BASE}/`, { timeout: 8000 }),
  ]).catch(() => {});

  // Dacă tot pe login, încearcă selector alternativ
  if (page.url().includes('/login')) {
    await page.locator('button[type="submit"], form button').last().click();
    await page.waitForTimeout(3000);
  }

  console.log(`  → logged in, URL: ${page.url()}`);
}

// ═════════════════════════════════════════════════════════════════════════════
// 1. HOME
// ═════════════════════════════════════════════════════════════════════════════
test('01 – Home page', async ({ page }) => {
  await page.goto(`${BASE}/`);
  await page.waitForLoadState('networkidle');

  await shot(page, 'home/01-hero');
  await scrollReveal(page);
  await shot(page, 'home/02-full-page');
  await page.locator('.btn--primary').first().hover();
  await shot(page, 'home/03-cta-hover');
});

// ═════════════════════════════════════════════════════════════════════════════
// 2. ABOUT
// ═════════════════════════════════════════════════════════════════════════════
test('02 – About page', async ({ page }) => {
  await page.goto(`${BASE}/about`);
  await page.waitForLoadState('networkidle');

  await shot(page, 'about/01-hero');

  await page.evaluate(() => window.scrollTo(0, 600));
  await page.waitForTimeout(WAIT);
  await shot(page, 'about/02-mission');

  await page.evaluate(() => window.scrollTo(0, 1400));
  await page.waitForTimeout(WAIT);
  await shot(page, 'about/03-team');

  await page.evaluate(() => window.scrollTo(0, 2200));
  await page.waitForTimeout(WAIT);
  await shot(page, 'about/04-timeline');

  await page.evaluate(() => window.scrollTo(0, 3000));
  await page.waitForTimeout(WAIT);
  await shot(page, 'about/05-perks');

  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await page.waitForTimeout(WAIT);
  await shot(page, 'about/06-footer-cta');
});

// ═════════════════════════════════════════════════════════════════════════════
// 3. CARS
// ═════════════════════════════════════════════════════════════════════════════
test('03 – Cars page', async ({ page }) => {
  await page.goto(`${BASE}/cars`);
  await page.waitForLoadState('networkidle');

  await shot(page, 'cars/01-all-cars');

  const filters = page.locator('.filter-btn, .tab-btn, [class*="filter"]');
  const filterCount = await filters.count();

  if (filterCount > 1) {
    await filters.nth(1).click();
    await page.waitForTimeout(WAIT);
    await shot(page, 'cars/02-filtered');

    if (filterCount > 2) {
      await filters.nth(2).click();
      await page.waitForTimeout(WAIT);
      await shot(page, 'cars/03-filtered-2');
    }

    await filters.first().click();
    await page.waitForTimeout(WAIT);
  }

  const firstCard = page.locator('.car-card, [class*="car-card"]').first();
  if (await firstCard.isVisible()) {
    await firstCard.hover();
    await page.waitForTimeout(300);
    await shot(page, 'cars/04-card-hover');
  }

  const detailBtn = page.locator('.btn--primary, [class*="detail"], [class*="rent"]').first();
  if (await detailBtn.isVisible()) {
    await detailBtn.click();
    await page.waitForTimeout(WAIT);
    await shot(page, 'cars/05-detail-or-modal');
  }

  await page.goto(`${BASE}/cars`, { waitUntil: 'domcontentloaded', timeout: 10000 });
  await page.waitForTimeout(WAIT);
  await scrollReveal(page);
  await shot(page, 'cars/06-full-page');
});

// ═════════════════════════════════════════════════════════════════════════════
// 4. OFFERS
// ═════════════════════════════════════════════════════════════════════════════
test('04 – Offers page', async ({ page }) => {
  await page.goto(`${BASE}/offers`);
  await page.waitForLoadState('networkidle');

  await shot(page, 'offers/01-initial');
  await scrollReveal(page);
  await shot(page, 'offers/02-full-page');

  const offerCard = page.locator('[class*="offer-card"], [class*="promo"]').first();
  if (await offerCard.isVisible()) {
    await offerCard.hover();
    await page.waitForTimeout(300);
    await shot(page, 'offers/03-card-hover');
  }
});

// ═════════════════════════════════════════════════════════════════════════════
// 5. CONTACT
// ═════════════════════════════════════════════════════════════════════════════
test('05 – Contact page', async ({ page }) => {
  await page.goto(`${BASE}/contact`);
  await page.waitForLoadState('networkidle');

  await shot(page, 'contact/01-initial');

  await page.fill('[placeholder="John"]', 'Ion');
  await shot(page, 'contact/02-firstname-filled');

  await page.fill('[placeholder="Doe"]', 'Popescu');
  await page.fill('[placeholder="john@email.com"]', 'ion@test.ro');
  await page.fill('[placeholder="+40 7xx xxx xxx"]', '+40721000000');
  await shot(page, 'contact/03-personal-data-filled');

  await page.selectOption('select', { index: 1 });
  await shot(page, 'contact/04-subject-selected');

  await page.fill('textarea', 'Bună ziua, aș dori informații suplimentare despre serviciile dvs.');
  await shot(page, 'contact/05-form-complete');

  await page.locator('.c-form__submit').click();
  await page.waitForSelector('.c-form-success--visible', { timeout: 5000 });
  await shot(page, 'contact/06-success-message');

  await page.goto(`${BASE}/contact`);
  await page.waitForLoadState('networkidle');

  const tabs = page.locator('.branches__tab');
  const tabCount = await tabs.count();
  for (let i = 0; i < tabCount; i++) {
    await tabs.nth(i).click();
    await page.waitForTimeout(WAIT);
    const tabName = (await tabs.nth(i).textContent())
      ?.trim().replace(/\s+/g, '-').toLowerCase();
    await shot(page, `contact/07-branch-${i + 1}-${tabName}`);
  }

  const map = page.locator('.branch-map, iframe').first();
  if (await map.isVisible()) {
    await map.scrollIntoViewIfNeeded();
    await page.waitForTimeout(WAIT);
    await shot(page, 'contact/08-map-visible');
  }
});

// ═════════════════════════════════════════════════════════════════════════════
// 6. LOGIN
// ═════════════════════════════════════════════════════════════════════════════
test('06 – Login page', async ({ page }) => {
  await page.goto(`${BASE}/login`, { waitUntil: 'domcontentloaded' });
  await page.waitForTimeout(WAIT);
  await shot(page, 'auth/01-login-empty');

  await page.locator('[type="email"]').click().catch(() => {});
  await shot(page, 'auth/02-login-email-focus');

  await page.fill('[type="email"]', CREDS.email);
  await shot(page, 'auth/03-login-email-filled');

  await page.fill('[type="password"]', CREDS.password);
  await shot(page, 'auth/04-login-ready');

  // Credențiale greșite → eroare
  await page.fill('[type="email"]', 'gresit@test.ro');
  await page.fill('[type="password"]', 'wrong');
  await page.locator('[type="submit"]').click();
  await page.waitForTimeout(2000);
  await shot(page, 'auth/05-login-error');

  // Login corect
  await page.fill('[type="email"]', CREDS.email);
  await page.fill('[type="password"]', CREDS.password);
  await page.locator('[type="submit"]').click();
  await Promise.race([
    page.waitForURL(`${BASE}/dashboard`, { timeout: 8000 }),
    page.waitForURL(`${BASE}/`, { timeout: 8000 }),
  ]).catch(() => {});
  await shot(page, 'auth/06-login-result');
});

// ═════════════════════════════════════════════════════════════════════════════
// 7. SIGNUP
// ═════════════════════════════════════════════════════════════════════════════
test('07 – Signup page', async ({ page }) => {
  await page.goto(`${BASE}/signup`, { waitUntil: 'domcontentloaded' });
  await page.waitForTimeout(WAIT);
  await shot(page, 'auth/07-signup-empty');

  const firstField = page.locator('input').first();
  if (await firstField.isVisible()) {
    await firstField.click();
    await firstField.fill('Ion');
    await shot(page, 'auth/08-signup-name-filled');
  }

  const inputs = page.locator('input');
  const inputCount = await inputs.count();
  for (let i = 0; i < inputCount; i++) {
    const input = inputs.nth(i);
    const type = await input.getAttribute('type').catch(() => 'text');
    if (type === 'email') {
      await input.fill('ion.nou@test.ro').catch(() => {});
    } else if (type === 'password') {
      await input.fill('Parola123!').catch(() => {});
    } else if (type === 'text' || !type) {
      const placeholder = await input.getAttribute('placeholder').catch(() => '');
      if (placeholder?.toLowerCase().includes('last') || placeholder?.toLowerCase().includes('nume')) {
        await input.fill('Popescu').catch(() => {});
      } else {
        await input.fill('Ion').catch(() => {});
      }
    }
  }

  await shot(page, 'auth/09-signup-email-filled');
  await shot(page, 'auth/10-signup-complete');
});

// ═════════════════════════════════════════════════════════════════════════════
// 8a. DASHBOARD – Overview → Alerts
// ═════════════════════════════════════════════════════════════════════════════
test('08a – Dashboard Overview to Alerts', async ({ page }) => {
  await login(page);

  if (page.url().includes('/login')) {
    console.error('  ✗ Login eșuat – skip dashboard');
    return;
  }

  await page.goto(`${BASE}/dashboard`, { waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(1500);
  await shot(page, 'dashboard/01-overview');

  const statCards = page.locator('.dash-stat-card');
  for (let i = 0; i < await statCards.count(); i++) {
    await statCards.nth(i).hover();
    await page.waitForTimeout(200);
  }
  await shot(page, 'dashboard/02-stat-cards-hover');

  const actionBtns = page.locator('.dash-action-btn');
  for (let i = 0; i < await actionBtns.count(); i++) {
    await actionBtns.nth(i).hover();
    await page.waitForTimeout(150);
  }
  await shot(page, 'dashboard/03-quick-actions-hover');

  await page.locator('.dash-sidebar__toggle').click().catch(() => {});
  await page.waitForTimeout(400);
  await shot(page, 'dashboard/04-sidebar-collapsed');
  await page.locator('.dash-sidebar__toggle').click().catch(() => {});
  await page.waitForTimeout(400);
  await shot(page, 'dashboard/05-sidebar-expanded');

  const navClick = async (label: string) => {
    const items = page.locator('.dash-sidebar__nav-item');
    for (let i = 0; i < await items.count(); i++) {
      const text = await items.nth(i).textContent().catch(() => '');
      if (text?.includes(label)) {
        await items.nth(i).click();
        await page.waitForTimeout(1500);
        return;
      }
    }
  };

  await navClick('Vehicles');
  await shot(page, 'dashboard/06-vehicles');
  const vehicleSearch = page.locator('.dash-search-bar input').first();
  if (await vehicleSearch.isVisible().catch(() => false)) {
    await vehicleSearch.fill('BMW');
    await page.waitForTimeout(400);
    await shot(page, 'dashboard/07-vehicles-search');
    await vehicleSearch.fill('');
    await page.waitForTimeout(300);
  }
  const firstRow = page.locator('.dash-table tbody tr').first();
  if (await firstRow.isVisible().catch(() => false)) {
    await firstRow.hover();
    await page.waitForTimeout(200);
    await shot(page, 'dashboard/08-vehicles-row-hover');
  }

  await navClick('Rentals');
  await shot(page, 'dashboard/09-rentals');
  const rentalSelect = page.locator('.dash-select').first();
  if (await rentalSelect.isVisible().catch(() => false)) {
    for (const s of ['Active', 'Completed', 'Cancelled']) {
      await rentalSelect.selectOption(s);
      await page.waitForTimeout(300);
      await shot(page, `dashboard/10-rentals-${s.toLowerCase()}`);
    }
    await rentalSelect.selectOption('');
  }

  await navClick('Clients');
  await shot(page, 'dashboard/11-clients');
  const clientSearch = page.locator('.dash-search-bar input').first();
  if (await clientSearch.isVisible().catch(() => false)) {
    await clientSearch.fill('Ion');
    await page.waitForTimeout(600);
    await shot(page, 'dashboard/12-clients-search');
    await clientSearch.fill('');
  }

  await navClick('Staff');
  await shot(page, 'dashboard/13-staff');

});

// ═════════════════════════════════════════════════════════════════════════════
// 08e. DASHBOARD – Branches (context fresh)
// ═════════════════════════════════════════════════════════════════════════════
test('08e – Dashboard Branches', async ({ page }) => {
  await login(page);
  if (page.url().includes('/login')) { console.error('  ✗ Login eșuat'); return; }

  await page.goto(`${BASE}/dashboard`, { waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(1500);

  const items = page.locator('.dash-sidebar__nav-item');
  for (let i = 0; i < await items.count(); i++) {
    const text = await items.nth(i).textContent().catch(() => '');
    if (text?.includes('Branches')) {
      await items.nth(i).click().catch(() => {});
      break;
    }
  }
  await page.waitForTimeout(1500);
  await shot(page, 'dashboard/15-branches');

  const branchCard = page.locator('.dash-branch-card').first();
  if (await branchCard.isVisible().catch(() => false)) {
    await branchCard.hover();
    await page.waitForTimeout(200);
    await shot(page, 'dashboard/16-branch-card-hover');
  }
});

// ═════════════════════════════════════════════════════════════════════════════
// 08d. DASHBOARD – Security Alerts (context fresh)
// ═════════════════════════════════════════════════════════════════════════════
test('08d – Dashboard Security Alerts', async ({ page }) => {
  await login(page);

  if (page.url().includes('/login')) {
    console.error('  ✗ Login eșuat – skip alerts');
    return;
  }

  await page.goto(`${BASE}/dashboard`, { waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(1500);

  const items = page.locator('.dash-sidebar__nav-item');
  for (let i = 0; i < await items.count(); i++) {
    const text = await items.nth(i).textContent().catch(() => '');
    if (text?.includes('Security Alerts')) {
      await items.nth(i).click().catch(() => {});
      break;
    }
  }
  await page.waitForTimeout(1500);
  await shot(page, 'dashboard/17-security-alerts');
});

// ═════════════════════════════════════════════════════════════════════════════
// 8b. DASHBOARD – Revenue (context fresh)
// ═════════════════════════════════════════════════════════════════════════════
test('08b – Dashboard Revenue', async ({ page }) => {
  await login(page);

  if (page.url().includes('/login')) {
    console.error('  ✗ Login eșuat – skip revenue');
    return;
  }

  await page.goto(`${BASE}/dashboard`, { waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(1500);

  const navClick = async (label: string) => {
    const items = page.locator('.dash-sidebar__nav-item');
    for (let i = 0; i < await items.count(); i++) {
      const text = await items.nth(i).textContent().catch(() => '');
      if (text?.includes(label)) {
        await items.nth(i).click();
        await page.waitForTimeout(1500);
        return;
      }
    }
  };

  await navClick('Revenue');
  await shot(page, 'dashboard/19-revenue');
  await page.screenshot({ path: `${OUT}/dashboard/20-revenue-charts.png`, fullPage: true }).catch(() => {});
  console.log('  ✓ dashboard/20-revenue-charts');
});

// ═════════════════════════════════════════════════════════════════════════════
// 8c. DASHBOARD – Report Builder (context fresh)
// ═════════════════════════════════════════════════════════════════════════════
test('08c – Dashboard Report Builder', async ({ page }) => {
  await login(page);

  if (page.url().includes('/login')) {
    console.error('  ✗ Login eșuat – skip report builder');
    return;
  }

  await page.goto(`${BASE}/dashboard`, { waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(1500);

  const items = page.locator('.dash-sidebar__nav-item');
  for (let i = 0; i < await items.count(); i++) {
    const text = await items.nth(i).textContent().catch(() => '');
    if (text?.includes('Reports')) {
      await items.nth(i).click().catch(() => {});
      break;
    }
  }
  await page.waitForTimeout(500);
  await page.screenshot({ path: `${OUT}/dashboard/22-report-builder.png`, fullPage: true }).catch(() => {});
  console.log('  ✓ dashboard/22-report-builder');

  const reportSectionItems = page.locator('.dash-report-section');
  if (await reportSectionItems.count().catch(() => 0) > 1) {
    await reportSectionItems.nth(0).click().catch(() => {});
    await page.waitForTimeout(150);
    await reportSectionItems.nth(1).click().catch(() => {});
    await page.waitForTimeout(150);
    await page.screenshot({ path: `${OUT}/dashboard/23-report-sections-selected.png`, fullPage: true }).catch(() => {});
    console.log('  ✓ dashboard/23-report-sections-selected');
  }

  const generateBtn = page.locator('.dash-report-generate-btn');
  if (await generateBtn.isVisible().catch(() => false)) {
    await generateBtn.hover().catch(() => {});
    await page.waitForTimeout(200);
    await page.screenshot({ path: `${OUT}/dashboard/24-report-generate-hover.png`, fullPage: true }).catch(() => {});
    console.log('  ✓ dashboard/24-report-generate-hover');
  }

  await page.goto(`${BASE}/dashboard`, { waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(1500);
  await page.screenshot({ path: `${OUT}/dashboard/25-overview-final.png`, fullPage: true }).catch(() => {});
  console.log('  ✓ dashboard/25-overview-final');
});

// ═════════════════════════════════════════════════════════════════════════════
// 9. MY BOOKINGS
// ═════════════════════════════════════════════════════════════════════════════
test('09 – My Bookings', async ({ page }) => {
  await login(page);

  if (page.url().includes('/login')) {
    console.error('  ✗ Login eșuat – skip bookings');
    return;
  }

  await page.goto(`${BASE}/my-bookings`, { waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(1500);
  await shot(page, 'bookings/01-list');

  const filters = page.locator('[class*="filter"], [class*="tab-btn"]');
  const count = await filters.count();
  for (let i = 0; i < Math.min(count, 3); i++) {
    await filters.nth(i).click();
    await page.waitForTimeout(WAIT);
    await shot(page, `bookings/02-filter-${i + 1}`);
  }

  const firstBooking = page.locator('[class*="booking-card"], [class*="rental-card"]').first();
  if (await firstBooking.isVisible()) {
    await firstBooking.hover();
    await page.waitForTimeout(300);
    await shot(page, 'bookings/03-card-hover');
    await firstBooking.click();
    await page.waitForTimeout(WAIT);
    await shot(page, 'bookings/04-detail');
  }

  await scrollReveal(page);
  await shot(page, 'bookings/05-full-page');
});

// ═════════════════════════════════════════════════════════════════════════════
// 10. BOOKING FLOW END-TO-END
// ═════════════════════════════════════════════════════════════════════════════
test('10 – Booking flow end-to-end', async ({ page }) => {
  await login(page);

  if (page.url().includes('/login')) {
    console.error('  ✗ Login eșuat – skip booking flow');
    return;
  }

  await page.goto(`${BASE}/cars`, { waitUntil: 'domcontentloaded', timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(1500);
  await shot(page, 'bookings/flow-01-cars-list');

  const rentBtn = page.locator('[class*="rent"], [class*="book"], .btn--primary').first();
  if (await rentBtn.isVisible()) {
    await rentBtn.click();
    await page.waitForTimeout(WAIT);
    await shot(page, 'bookings/flow-02-car-selected');
  }

  const startDate = page.locator('[name*="start"], [placeholder*="start"], [type="date"]').first();
  if (await startDate.isVisible()) {
    await startDate.fill('2025-07-01');
    await page.waitForTimeout(300);
    await shot(page, 'bookings/flow-03-start-date');

    const endDate = page.locator('[name*="end"], [placeholder*="end"], [type="date"]').nth(1);
    if (await endDate.isVisible()) {
      await endDate.fill('2025-07-05');
      await page.waitForTimeout(300);
      await shot(page, 'bookings/flow-04-end-date');
    }
  }

  const confirmBtn = page.locator('[class*="confirm"], [class*="submit"]').last();
  if (await confirmBtn.isVisible().catch(() => false)) {
    await page.screenshot({ path: `${OUT}/bookings/flow-07-before-confirm.png`, fullPage: true }).catch(() => {});
    console.log('  ✓ bookings/flow-07-before-confirm');
    await confirmBtn.click().catch(() => {});
    await page.waitForTimeout(2000);
    await page.screenshot({ path: `${OUT}/bookings/flow-08-after-confirm.png`, fullPage: true }).catch(() => {});
    console.log('  ✓ bookings/flow-08-after-confirm');
  }
});

// ═════════════════════════════════════════════════════════════════════════════
// 11. NAVBAR STĂRI
// ═════════════════════════════════════════════════════════════════════════════
test('11 – Navbar states', async ({ page }) => {
  await page.goto(`${BASE}/`);
  await page.waitForLoadState('networkidle');
  await shot(page, 'home/navbar-01-logged-out');

  await page.locator('nav a, .navbar__link').first().hover();
  await page.waitForTimeout(300);
  await shot(page, 'home/navbar-02-link-hover');

  await login(page);
  await page.goto(`${BASE}/`, { waitUntil: 'domcontentloaded' });
  await page.waitForTimeout(1000);
  await shot(page, 'home/navbar-03-logged-in');
});

// ═════════════════════════════════════════════════════════════════════════════
// 12. MOBILE – iPhone 14
// ═════════════════════════════════════════════════════════════════════════════
test('12 – Mobile screenshots', async ({ browser }) => {
  const ctx = await browser.newContext({
    viewport: { width: 390, height: 844 },
    deviceScaleFactor: 2,
  });
  const page = await ctx.newPage();

  const mobilePages = [
    { url: '/',        name: 'home'    },
    { url: '/cars',    name: 'cars'    },
    { url: '/about',   name: 'about'   },
    { url: '/contact', name: 'contact' },
    { url: '/login',   name: 'login'   },
  ];

  for (const p of mobilePages) {
    try {
      await page.goto(`${BASE}${p.url}`, { waitUntil: 'domcontentloaded', timeout: 8000 });
    } catch { /* ignoră timeout */ }
    await page.waitForTimeout(1000);
    await page.screenshot({ path: `${OUT}/mobile/${p.name}.png`, fullPage: true }).catch(() => {});
    console.log(`  ✓ mobile/${p.name}`);
    await page.evaluate(() => window.stop()).catch(() => {});
    await page.waitForTimeout(200);
  }

  const hamburger = page.locator('[class*="hamburger"], [class*="menu-toggle"], .navbar__toggle');
  if (await hamburger.isVisible().catch(() => false)) {
    await hamburger.click().catch(() => {});
    await page.waitForTimeout(500);
    await page.screenshot({ path: `${OUT}/mobile/menu-open.png`, fullPage: true }).catch(() => {});
    console.log('  ✓ mobile/menu-open');
  }

  await ctx.close();
});