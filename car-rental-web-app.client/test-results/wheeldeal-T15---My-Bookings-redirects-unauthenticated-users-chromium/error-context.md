# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: wheeldeal.spec.ts >> T15 - My Bookings redirects unauthenticated users
- Location: wheeldeal.spec.ts:252:5

# Error details

```
Error: expect(page).toHaveURL(expected) failed

Expected pattern: /\/login1/
Received string:  "http://localhost:4200/login?returnUrl=%2Fmy-bookings"
Timeout: 8000ms

Call log:
  - Expect "toHaveURL" with timeout 8000ms
    19 × unexpected value "http://localhost:4200/login?returnUrl=%2Fmy-bookings"

```

```yaml
- navigation:
  - link "WheelDeal":
    - /url: /
    - img
    - text: Wheel
    - strong: Deal
  - list:
    - listitem:
      - link "HOME":
        - /url: /
    - listitem:
      - link "CARS":
        - /url: /cars
    - listitem:
      - link "OFFERS":
        - /url: /offers
    - listitem:
      - link "ABOUT US":
        - /url: /about
    - listitem:
      - link "CONTACT":
        - /url: /contact
  - link "Sign In":
    - /url: /login
  - link "Register":
    - /url: /signup
- link "DriveNow":
  - /url: /
  - img
  - text: DriveNow
- heading "Your journey starts here" [level=2]
- paragraph: Access your bookings, manage rentals and discover exclusive member deals.
- text: ⭐ 4.9/5 member rating 🚗 15,000+ happy customers 🏷️ Members save 10% on every rental
- paragraph:
  - text: Don't have an account?
  - link "Sign up free →":
    - /url: /signup
- img
- text: Secure login
- heading "Welcome back" [level=1]
- paragraph: Sign in to your WheelDeal account.
- button "Continue with Google":
  - img
  - text: Continue with Google
- text: or sign in with email Email address
- img
- textbox "Email address":
  - /placeholder: you@email.com
- text: Password
- button "Forgot password?"
- img
- textbox "Password":
  - /placeholder: Your password
- button "Show password":
  - img
- checkbox "Keep me signed in"
- text: Keep me signed in
- button "Sign In"
- img
- heading "Reset password" [level=2]
- button "Close":
  - img
- paragraph: Enter the email address linked to your account and we'll send you a reset link.
- text: Email address
- img
- textbox "Email address":
  - /placeholder: you@email.com
- img
- paragraph: Check your inbox
- paragraph:
  - text: We've sent a reset link to
  - strong
  - text: . It expires in 30 minutes.
- paragraph:
  - text: Didn't receive it? Check your spam folder or
  - button "try again"
  - text: .
- button "Cancel"
- button "Send reset link"
```

# Test source

```ts
  154 | // TEST 9 — Pagina /offers afișează mașini cu reduceri
  155 | // =============================================================================
  156 | test('T09 - Offers page shows discounted cars', async ({ page }) => {
  157 |   await page.goto(`${BASE_URL}/offers`);
  158 | 
  159 |   await expect(page.locator('.offers-hero__title')).toBeVisible();
  160 |   await expect(page.locator('.offer-card').first()).toBeVisible({ timeout: 12_000 });
  161 | 
  162 |   // Fiecare card trebuie să aibă un ribbon cu procentaj
  163 |   const ribbons = await page.locator('.offer-card__ribbon').allTextContents();
  164 |   for (const ribbon of ribbons) {
  165 |     expect(ribbon).toMatch(/-\d+%/);
  166 |   }
  167 | 
  168 |   // Codul promoțional SUMMER30 trebuie să fie vizibil
  169 |   await expect(page.locator('.promo-code__code:has-text("SUMMER30")')).toBeVisible();
  170 | });
  171 | 
  172 | // =============================================================================
  173 | // TEST 10 — Pagina /about se încarcă cu date complete
  174 | // =============================================================================
  175 | test('T10 - About page displays team and milestones', async ({ page }) => {
  176 |   await page.goto(`${BASE_URL}/about`);
  177 | 
  178 |   await expect(page.locator('.about-hero__title')).toContainText('Driving freedom');
  179 |   await expect(page.locator('.team-card').first()).toBeVisible();
  180 |   await expect(page.locator('.timeline__item').first()).toBeVisible();
  181 | 
  182 |   const teamCards = await page.locator('.team-card').count();
  183 |   expect(teamCards).toBeGreaterThanOrEqual(3);
  184 | });
  185 | 
  186 | // =============================================================================
  187 | // TEST 11 — Formularul de contact trimite mesaj cu succes
  188 | // =============================================================================
  189 | test('T11 - Contact form submits successfully', async ({ page }) => {
  190 |   await page.goto(`${BASE_URL}/contact`);
  191 | 
  192 |   await page.fill('input[placeholder="John"]', 'Test');
  193 |   await page.fill('input[placeholder="Doe"]', 'User');
  194 |   await page.fill('input[placeholder="john@email.com"]', 'test@example.com');
  195 |   await page.selectOption('select', { label: 'Booking issue' });
  196 |   await page.fill('textarea', 'Acesta este un mesaj de test automat cu mai mult de 10 caractere.');
  197 | 
  198 |   await page.click('.c-form__submit');
  199 | 
  200 |   // Asteaptă starea de succes
  201 |   await expect(page.locator('.c-form-success--visible')).toBeVisible({ timeout: 10_000 });
  202 |   await expect(page.locator('.c-form-success__title')).toContainText('Message sent');
  203 | });
  204 | 
  205 | // =============================================================================
  206 | // TEST 12 — /dashboard redirecționează neautentificații la /login
  207 | // =============================================================================
  208 | test('T12 - Dashboard redirects unauthenticated users to login', async ({ page }) => {
  209 |   // Nu suntem logați — navigăm direct la dashboard
  210 |   await page.goto(`${BASE_URL}/dashboard`);
  211 |   await expect(page).toHaveURL(/\/login/, { timeout: 8_000 });
  212 | });
  213 | 
  214 | // =============================================================================
  215 | // TEST 13 — Dashboard-ul Adminului se încarcă cu statistici
  216 | // =============================================================================
  217 | test('T13 - Admin dashboard loads with stats cards', async ({ page }) => {
  218 |   await login(page, ADMIN.email, ADMIN.password);
  219 |   await page.goto(`${BASE_URL}/dashboard`);
  220 | 
  221 |   // Sidebar-ul și stat cards-urile trebuie să fie vizibile
  222 |   await expect(page.locator('.dash-sidebar')).toBeVisible({ timeout: 10_000 });
  223 |   await expect(page.locator('.dash-stat-card').first()).toBeVisible({ timeout: 10_000 });
  224 | 
  225 |   const statCards = await page.locator('.dash-stat-card').count();
  226 |   expect(statCards).toBeGreaterThanOrEqual(4);
  227 | 
  228 |   await logout(page);
  229 | });
  230 | 
  231 | // =============================================================================
  232 | // TEST 14 — Dashboard: navigare la secțiunea Vehicles
  233 | // =============================================================================
  234 | test('T14 - Admin can navigate to Vehicles section in dashboard', async ({ page }) => {
  235 |   await login(page, ADMIN.email, ADMIN.password);
  236 |   await page.goto(`${BASE_URL}/dashboard`);
  237 | 
  238 |   // Click pe butonul "Manage Fleet" din Quick Actions
  239 |   await page.click('.dash-action-btn:has-text("Manage Fleet")');
  240 |   await page.waitForTimeout(2000);
  241 | 
  242 |   // Tabelul de vehicule trebuie să apară
  243 |   await expect(page.locator('.dash-table')).toBeVisible({ timeout: 10_000 });
  244 |   await expect(page.locator('.dash-topbar__title')).toContainText('Fleet Management');
  245 | 
  246 |   await logout(page);
  247 | });
  248 | 
  249 | // =============================================================================
  250 | // TEST 15 — /my-bookings redirecționează neautentificații
  251 | // =============================================================================
  252 | test('T15 - My Bookings redirects unauthenticated users', async ({ page }) => {
  253 |   await page.goto(`${BASE_URL}/my-bookings`);
> 254 |   await expect(page).toHaveURL(/\/login1/, { timeout: 8_000 });
      |                      ^ Error: expect(page).toHaveURL(expected) failed
  255 | });
  256 | 
  257 | // =============================================================================
  258 | // TEST 16 — /my-bookings se încarcă pentru utilizator autentificat
  259 | // =============================================================================
  260 | test('T16 - My Bookings page loads for authenticated user', async ({ page }) => {
  261 |   await login(page, ADMIN.email, ADMIN.password);
  262 |   await page.goto(`${BASE_URL}/my-bookings`);
  263 | 
  264 |   await expect(page.locator('.bookings-hero__title')).toBeVisible({ timeout: 10_000 });
  265 |   await expect(page.locator('.bookings-filter-btn').first()).toBeVisible();
  266 | 
  267 |   // Filtrele de status trebuie să fie prezente
  268 |   const filters = (await page.locator('.bookings-filter-btn').allTextContents()).map(f => f.trim());
  269 |   expect(filters).toContain('All');
  270 |   expect(filters).toContain('Active');
  271 | 
  272 |   await logout(page);
  273 | });
  274 | 
  275 | // =============================================================================
  276 | // TEST 17 — Căutarea de mașini de pe Home redirecționează la /cars cu parametri
  277 | // =============================================================================
  278 | test('T17 - Home search form navigates to /cars with query params', async ({ page }) => {
  279 |   await page.goto(BASE_URL);
  280 | 
  281 |   // Completăm formularul de căutare
  282 |   const tomorrow = new Date();
  283 |   tomorrow.setDate(tomorrow.getDate() + 1);
  284 |   const afterTomorrow = new Date();
  285 |   afterTomorrow.setDate(afterTomorrow.getDate() + 3);
  286 | 
  287 |   const fmt = (d: Date) => d.toISOString().split('T')[0];
  288 | 
  289 |   await page.fill('input[name="pickupDate"]', fmt(tomorrow));
  290 |   await page.fill('input[name="returnDate"]', fmt(afterTomorrow));
  291 |   await page.selectOption('select[name="location"]', { label: 'Bucharest — Central' });
  292 | 
  293 |   await page.click('.hero__search-btn');
  294 | 
  295 |   await expect(page).toHaveURL(/\/cars/, { timeout: 8_000 });
  296 |   await expect(page).toHaveURL(/fromSearch=1/);
  297 |   await expect(page.locator('.search-context-banner')).toBeVisible({ timeout: 8_000 });
  298 | });
  299 | 
  300 | // =============================================================================
  301 | // TEST 18 — Toggle-ul de parolare din login funcționează
  302 | // =============================================================================
  303 | test('T18 - Password visibility toggle works on login page', async ({ page }) => {
  304 |   await page.goto(`${BASE_URL}/login`);
  305 | 
  306 |   const passwordInput = page.locator('#password');
  307 |   await passwordInput.fill('TestParola123!');
  308 | 
  309 |   // Inițial tipul e "password"
  310 |   await expect(passwordInput).toHaveAttribute('type', 'password');
  311 | 
  312 |   // Click pe butonul de toggle
  313 |   await page.click('.input-wrap__toggle');
  314 |   await expect(passwordInput).toHaveAttribute('type', 'text');
  315 | 
  316 |   // Click din nou — revine la "password"
  317 |   await page.click('.input-wrap__toggle');
  318 |   await expect(passwordInput).toHaveAttribute('type', 'password');
  319 | });
  320 | 
  321 | // =============================================================================
  322 | // TEST 19 — Manager vede dashboard-ul cu secțiunile corecte
  323 | // =============================================================================
  324 | test('T19 - Manager sees correct dashboard sections', async ({ page }) => {
  325 |   await login(page, MGR.email, MGR.password);
  326 |   await page.goto(`${BASE_URL}/dashboard`);
  327 | 
  328 |   await expect(page.locator('.dash-sidebar')).toBeVisible({ timeout: 10_000 });
  329 | 
  330 |   // Managerul NU trebuie să vadă Staff sau Branches
  331 |   await expect(page.locator('.dash-sidebar__nav-item:has-text("Staff")')).not.toBeVisible();
  332 |   await expect(page.locator('.dash-sidebar__nav-item:has-text("Branches")')).not.toBeVisible();
  333 | 
  334 |   // Managerul TREBUIE să vadă Revenue și Fleet
  335 |   await expect(page.locator('.dash-sidebar__nav-item:has-text("Revenue")')).toBeVisible();
  336 | 
  337 |   await logout(page);
  338 | });
  339 | 
  340 | // =============================================================================
  341 | // TEST 20 — Sortarea mașinilor pe /cars funcționează
  342 | // =============================================================================
  343 | test('T20 - Cars page sort by price works', async ({ page }) => {
  344 |   await page.goto(`${BASE_URL}/cars`);
  345 |   await expect(page.locator('.car-card').first()).toBeVisible({ timeout: 12_000 });
  346 | 
  347 |   // Selectăm sortarea "Price: High to Low"
  348 |   await page.selectOption('.fleet__sort-select', { label: 'Price: High to Low' });
  349 |   await page.waitForTimeout(2000);
  350 | 
  351 |   // Extragem prețurile afișate
  352 |   const priceTexts = await page.locator('.car-card__price-amount').allTextContents();
  353 |   const prices = priceTexts.map(p => parseFloat(p.replace('€', '').trim())).filter(n => !isNaN(n));
  354 | 
```