import { test, expect, Page } from '@playwright/test';

// ─── Config ──────────────────────────────────────────────────────────────────
const BASE_URL = 'http://localhost:4200';

const ADMIN = { email: 'admin@wheeldeal.ro',       password: 'Admin@123!' };
const MGR   = { email: 'manager.buc@wheeldeal.ro', password: 'Manager@123!' };
const OP    = { email: 'op1.buc@wheeldeal.ro',     password: 'Operator@123!' };

// ─── Helper: login ────────────────────────────────────────────────────────────
async function login(page: Page, email: string, password: string) {
  await page.goto(`${BASE_URL}/login`);
  await page.fill('#email', email);
  await page.fill('#password', password);
  await page.click('button[type="submit"]');
  await page.waitForURL(/localhost:4200\/(?!login)/, { timeout: 10_000 });
}

// ─── Helper: logout (clear storage) ──────────────────────────────────────────
async function logout(page: Page) {
  await page.evaluate(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
  await page.goto(`${BASE_URL}/`);
}

// =============================================================================
// TEST 1 — Pagina de home se încarcă corect
// =============================================================================
test('T01 - Home page loads and shows hero title', async ({ page }) => {
  await page.goto(BASE_URL);
  await expect(page).toHaveTitle(/WheelDeal/i);
  await expect(page.locator('.hero__title')).toBeVisible();
  await expect(page.locator('.hero__search-card')).toBeVisible();
});

// =============================================================================
// TEST 2 — Navigarea prin navbar funcționează
// =============================================================================
test('T02 - Navbar navigation works for public routes', async ({ page }) => {
  await page.goto(BASE_URL);

  // Cars
  await page.click('a[href="/cars"]');
  await expect(page).toHaveURL(/\/cars/);
  await expect(page.locator('.cars-hero__title')).toBeVisible();

  // About
  await page.click('a[href="/about"]');
  await expect(page).toHaveURL(/\/about/);
  await expect(page.locator('.about-hero__title')).toBeVisible();

  // Contact
  await page.click('a[href="/contact"]');
  await expect(page).toHaveURL(/\/contact/);
  await expect(page.locator('.contact-hero__title')).toBeVisible();
});

// =============================================================================
// TEST 3 — Pagina /login se afișează corect
// =============================================================================
test('T03 - Login page renders form elements', async ({ page }) => {
  await page.goto(`${BASE_URL}/login`);

  await expect(page.locator('#email')).toBeVisible();
  await expect(page.locator('#password')).toBeVisible();
  await expect(page.locator('button[type="submit"]')).toBeVisible();
  await expect(page.locator('a[href="/signup"]').first()).toBeVisible();
});

// =============================================================================
// TEST 4 — Login cu credențiale greșite afișează eroare
// =============================================================================
test('T04 - Login with wrong credentials shows error message', async ({ page }) => {
  await page.goto(`${BASE_URL}/login`);
  await page.fill('#email', 'wrong@email.com');
  await page.fill('#password', 'wrongpassword');
  await page.click('button[type="submit"]');

  await expect(page.locator('.auth-alert')).toBeVisible({ timeout: 8_000 });
  await expect(page.locator('.auth-alert')).toContainText(/invalid|password|email/i);
});

// =============================================================================
// TEST 5 — Login cu credențiale valide (Admin)
// =============================================================================
test('T05 - Admin login redirects to home', async ({ page }) => {
  await login(page, ADMIN.email, ADMIN.password);
  // After login, should not be on /login anymore
  await expect(page).not.toHaveURL(/\/login/);
  // Hero section should be visible on home page
  await expect(page.locator('.hero__title')).toBeVisible({ timeout: 8_000 });
});

// =============================================================================
// TEST 6 — Pagina /signup se afișează și validează câmpurile
// =============================================================================
test('T06 - Signup form validation works', async ({ page }) => {
  await page.goto(`${BASE_URL}/signup`);

  await expect(page.locator('#fullName')).toBeVisible();
  await expect(page.locator('#username')).toBeVisible();
  await expect(page.locator('#email')).toBeVisible();
  await expect(page.locator('#phone')).toBeVisible();
  await expect(page.locator('#password')).toBeVisible();

  // Submit fără date — trebuie să apară erori de validare
  await page.click('button[type="submit"]');
  await expect(page.locator('.form-error').first()).toBeVisible({ timeout: 5_000 });
});

// =============================================================================
// TEST 7 — Pagina /cars afișează mașini și filtrele funcționează
// =============================================================================
test('T07 - Cars page loads vehicles and category filter works', async ({ page }) => {
  await page.goto(`${BASE_URL}/cars`);

  // Asteaptă să apară cardurile de mașini
  await expect(page.locator('.car-card').first()).toBeVisible({ timeout: 12_000 });

  const initialCount = await page.locator('.car-card').count();
  expect(initialCount).toBeGreaterThan(0);

  // Click pe filtrul Economy
  await page.click('.fleet__category-btn:has-text("Economy")');
  await page.waitForTimeout(2000);

  // Toate cardurile trebuie să fie Economy
  const badges = await page.locator('.car-card__badge').allTextContents();
  for (const badge of badges) {
    expect(badge.trim()).toBe('Economy');
  }
});

// =============================================================================
// TEST 8 — Filtrarea pe branch (filiale) funcționează pe /cars
// =============================================================================
test('T08 - Cars page branch filter works', async ({ page }) => {
  await page.goto(`${BASE_URL}/cars`);
  await expect(page.locator('.car-card').first()).toBeVisible({ timeout: 12_000 });

  // Click pe filiala Bucharest Central
  await page.click('.fleet__branch-tab:has-text("Bucharest Central")');
  await page.waitForTimeout(2000);

  const branchLabels = await page.locator('.car-card__branch').allTextContents();
  for (const label of branchLabels) {
    expect(label).toContain('Bucharest');
  }
});

// =============================================================================
// TEST 9 — Pagina /offers afișează mașini cu reduceri
// =============================================================================
test('T09 - Offers page shows discounted cars', async ({ page }) => {
  await page.goto(`${BASE_URL}/offers`);

  await expect(page.locator('.offers-hero__title')).toBeVisible();
  await expect(page.locator('.offer-card').first()).toBeVisible({ timeout: 12_000 });

  // Fiecare card trebuie să aibă un ribbon cu procentaj
  const ribbons = await page.locator('.offer-card__ribbon').allTextContents();
  for (const ribbon of ribbons) {
    expect(ribbon).toMatch(/-\d+%/);
  }

  // Codul promoțional SUMMER30 trebuie să fie vizibil
  await expect(page.locator('.promo-code__code:has-text("SUMMER30")')).toBeVisible();
});

// =============================================================================
// TEST 10 — Pagina /about se încarcă cu date complete
// =============================================================================
test('T10 - About page displays team and milestones', async ({ page }) => {
  await page.goto(`${BASE_URL}/about`);

  await expect(page.locator('.about-hero__title')).toContainText('Driving freedom');
  await expect(page.locator('.team-card').first()).toBeVisible();
  await expect(page.locator('.timeline__item').first()).toBeVisible();

  const teamCards = await page.locator('.team-card').count();
  expect(teamCards).toBeGreaterThanOrEqual(3);
});

// =============================================================================
// TEST 11 — Formularul de contact trimite mesaj cu succes
// =============================================================================
test('T11 - Contact form submits successfully', async ({ page }) => {
  await page.goto(`${BASE_URL}/contact`);

  await page.fill('input[placeholder="John"]', 'Test');
  await page.fill('input[placeholder="Doe"]', 'User');
  await page.fill('input[placeholder="john@email.com"]', 'test@example.com');
  await page.selectOption('select', { label: 'Booking issue' });
  await page.fill('textarea', 'Acesta este un mesaj de test automat cu mai mult de 10 caractere.');

  await page.click('.c-form__submit');

  // Asteaptă starea de succes
  await expect(page.locator('.c-form-success--visible')).toBeVisible({ timeout: 10_000 });
  await expect(page.locator('.c-form-success__title')).toContainText('Message sent');
});

// =============================================================================
// TEST 12 — /dashboard redirecționează neautentificații la /login
// =============================================================================
test('T12 - Dashboard redirects unauthenticated users to login', async ({ page }) => {
  // Nu suntem logați — navigăm direct la dashboard
  await page.goto(`${BASE_URL}/dashboard`);
  await expect(page).toHaveURL(/\/login/, { timeout: 8_000 });
});

// =============================================================================
// TEST 13 — Dashboard-ul Adminului se încarcă cu statistici
// =============================================================================
test('T13 - Admin dashboard loads with stats cards', async ({ page }) => {
  await login(page, ADMIN.email, ADMIN.password);
  await page.goto(`${BASE_URL}/dashboard`);

  // Sidebar-ul și stat cards-urile trebuie să fie vizibile
  await expect(page.locator('.dash-sidebar')).toBeVisible({ timeout: 10_000 });
  await expect(page.locator('.dash-stat-card').first()).toBeVisible({ timeout: 10_000 });

  const statCards = await page.locator('.dash-stat-card').count();
  expect(statCards).toBeGreaterThanOrEqual(4);

  await logout(page);
});

// =============================================================================
// TEST 14 — Dashboard: navigare la secțiunea Vehicles
// =============================================================================
test('T14 - Admin can navigate to Vehicles section in dashboard', async ({ page }) => {
  await login(page, ADMIN.email, ADMIN.password);
  await page.goto(`${BASE_URL}/dashboard`);

  // Click pe butonul "Manage Fleet" din Quick Actions
  await page.click('.dash-action-btn:has-text("Manage Fleet")');
  await page.waitForTimeout(2000);

  // Tabelul de vehicule trebuie să apară
  await expect(page.locator('.dash-table')).toBeVisible({ timeout: 10_000 });
  await expect(page.locator('.dash-topbar__title')).toContainText('Fleet Management');

  await logout(page);
});

// =============================================================================
// TEST 15 — /my-bookings redirecționează neautentificații
// =============================================================================
test('T15 - My Bookings redirects unauthenticated users', async ({ page }) => {
  await page.goto(`${BASE_URL}/my-bookings`);
  await expect(page).toHaveURL(/\/login/, { timeout: 8_000 });
});

// =============================================================================
// TEST 16 — /my-bookings se încarcă pentru utilizator autentificat
// =============================================================================
test('T16 - My Bookings page loads for authenticated user', async ({ page }) => {
  await login(page, ADMIN.email, ADMIN.password);
  await page.goto(`${BASE_URL}/my-bookings`);

  await expect(page.locator('.bookings-hero__title')).toBeVisible({ timeout: 10_000 });
  await expect(page.locator('.bookings-filter-btn').first()).toBeVisible();

  // Filtrele de status trebuie să fie prezente
  const filters = (await page.locator('.bookings-filter-btn').allTextContents()).map(f => f.trim());
  expect(filters).toContain('All');
  expect(filters).toContain('Active');

  await logout(page);
});

// =============================================================================
// TEST 17 — Căutarea de mașini de pe Home redirecționează la /cars cu parametri
// =============================================================================
test('T17 - Home search form navigates to /cars with query params', async ({ page }) => {
  await page.goto(BASE_URL);

  // Completăm formularul de căutare
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const afterTomorrow = new Date();
  afterTomorrow.setDate(afterTomorrow.getDate() + 3);

  const fmt = (d: Date) => d.toISOString().split('T')[0];

  await page.fill('input[name="pickupDate"]', fmt(tomorrow));
  await page.fill('input[name="returnDate"]', fmt(afterTomorrow));
  await page.selectOption('select[name="location"]', { label: 'Bucharest — Central' });

  await page.click('.hero__search-btn');

  await expect(page).toHaveURL(/\/cars/, { timeout: 8_000 });
  await expect(page).toHaveURL(/fromSearch=1/);
  await expect(page.locator('.search-context-banner')).toBeVisible({ timeout: 8_000 });
});

// =============================================================================
// TEST 18 — Toggle-ul de parolare din login funcționează
// =============================================================================
test('T18 - Password visibility toggle works on login page', async ({ page }) => {
  await page.goto(`${BASE_URL}/login`);

  const passwordInput = page.locator('#password');
  await passwordInput.fill('TestParola123!');

  // Inițial tipul e "password"
  await expect(passwordInput).toHaveAttribute('type', 'password');

  // Click pe butonul de toggle
  await page.click('.input-wrap__toggle');
  await expect(passwordInput).toHaveAttribute('type', 'text');

  // Click din nou — revine la "password"
  await page.click('.input-wrap__toggle');
  await expect(passwordInput).toHaveAttribute('type', 'password');
});

// =============================================================================
// TEST 19 — Manager vede dashboard-ul cu secțiunile corecte
// =============================================================================
test('T19 - Manager sees correct dashboard sections', async ({ page }) => {
  await login(page, MGR.email, MGR.password);
  await page.goto(`${BASE_URL}/dashboard`);

  await expect(page.locator('.dash-sidebar')).toBeVisible({ timeout: 10_000 });

  // Managerul NU trebuie să vadă Staff sau Branches
  await expect(page.locator('.dash-sidebar__nav-item:has-text("Staff")')).not.toBeVisible();
  await expect(page.locator('.dash-sidebar__nav-item:has-text("Branches")')).not.toBeVisible();

  // Managerul TREBUIE să vadă Revenue și Fleet
  await expect(page.locator('.dash-sidebar__nav-item:has-text("Revenue")')).toBeVisible();

  await logout(page);
});

// =============================================================================
// TEST 20 — Sortarea mașinilor pe /cars funcționează
// =============================================================================
test('T20 - Cars page sort by price works', async ({ page }) => {
  await page.goto(`${BASE_URL}/cars`);
  await expect(page.locator('.car-card').first()).toBeVisible({ timeout: 12_000 });

  // Selectăm sortarea "Price: High to Low"
  await page.selectOption('.fleet__sort-select', { label: 'Price: High to Low' });
  await page.waitForTimeout(2000);

  // Extragem prețurile afișate
  const priceTexts = await page.locator('.car-card__price-amount').allTextContents();
  const prices = priceTexts.map(p => parseFloat(p.replace('€', '').trim())).filter(n => !isNaN(n));

  // Verificăm că sunt sortate descrescător
  for (let i = 0; i < prices.length - 1; i++) {
    expect(prices[i]).toBeGreaterThanOrEqual(prices[i + 1]);
  }
});

// =============================================================================
// TESTE API BACKEND — Playwright request context (fără browser)
// Base URL: https://localhost:7273/api
// =============================================================================

const API = 'https://localhost:7273/api';

// Helper: obține token JWT pentru admin
async function getAdminToken(request: any): Promise<string> {
  const res = await request.post(`${API}/auth/login`, {
    data: { email: ADMIN.email, password: ADMIN.password },
    ignoreHTTPSErrors: true,
  });
  const body = await res.json();
  return body.accessToken;
}

// =============================================================================
// TEST A01 — Health check endpoint răspunde cu status 200
// =============================================================================
test('A01 - API health check returns 200 and healthy status', async ({ request }) => {
  const res = await request.get(`${API}/health`, {
    ignoreHTTPSErrors: true,
  });

  expect(res.status()).toBe(200);
  const body = await res.json();
  expect(body.status).toBe('healthy');
});

// =============================================================================
// TEST A02 — Login cu credențiale valide returnează JWT token
// =============================================================================
test('A02 - POST /auth/login with valid credentials returns JWT', async ({ request }) => {
  const res = await request.post(`${API}/auth/login`, {
    data: { email: ADMIN.email, password: ADMIN.password },
    ignoreHTTPSErrors: true,
  });

  expect(res.status()).toBe(200);

  const body = await res.json();
  expect(body.accessToken).toBeTruthy();
  expect(body.refreshToken).toBeTruthy();
  expect(body.user).toBeDefined();
  expect(body.user.role).toBe('Administrator');
});

// =============================================================================
// TEST A03 — Login cu credențiale greșite returnează 401
// =============================================================================
test('A03 - POST /auth/login with wrong password returns 401', async ({ request }) => {
  const res = await request.post(`${API}/auth/login`, {
    data: { email: ADMIN.email, password: 'WrongPassword999!' },
    ignoreHTTPSErrors: true,
  });

  expect(res.status()).toBe(401);

  const body = await res.json();
  expect(body.title).toBeTruthy();
});

// =============================================================================
// TEST A04 — GET /vehicles returnează lista de vehicule (public endpoint)
// =============================================================================
test('A04 - GET /vehicles returns list of vehicles without auth', async ({ request }) => {
  const res = await request.get(`${API}/vehicles`, {
    ignoreHTTPSErrors: true,
  });

  expect(res.status()).toBe(200);

  const body = await res.json();
  expect(Array.isArray(body)).toBeTruthy();
  expect(body.length).toBeGreaterThan(0);

  // Verificăm structura primului vehicul
  const vehicle = body[0];
  expect(vehicle).toHaveProperty('id');
  expect(vehicle).toHaveProperty('name');
  expect(vehicle).toHaveProperty('dailyRate');
  expect(vehicle).toHaveProperty('category');
  expect(vehicle).toHaveProperty('branch');
});

// =============================================================================
// TEST A05 — GET /reports/dashboard necesită autentificare (401 fără token)
// =============================================================================
test('A05 - GET /reports/dashboard returns 401 without token', async ({ request }) => {
  const res = await request.get(`${API}/reports/dashboard`, {
    ignoreHTTPSErrors: true,
  });

  expect(res.status()).toBe(401);
});

// =============================================================================
// TEST A06 — GET /reports/dashboard returnează statistici cu token valid
// =============================================================================
test('A06 - GET /reports/dashboard returns stats with valid admin token', async ({ request }) => {
  const token = await getAdminToken(request);

  const res = await request.get(`${API}/reports/dashboard`, {
    headers: { Authorization: `Bearer ${token}` },
    ignoreHTTPSErrors: true,
  });

  expect(res.status()).toBe(200);

  const body = await res.json();
  expect(body).toHaveProperty('totalVehicles');
  expect(body).toHaveProperty('availableVehicles');
  expect(body).toHaveProperty('activeRentals');
  expect(body).toHaveProperty('todayRevenue');
  expect(body.totalVehicles).toBeGreaterThan(0);
});

test.afterAll(async () => {
  // mic buffer pentru cleanup
  await new Promise(res => setTimeout(res, 100));
});
