import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: '.',
  testMatch: '**/wheeldeal.spec.ts',
  testIgnore: ['**/src/**'],
  timeout: 30_000,
  retries: 0,
  workers: 1,

  use: {
    baseURL:          'http://localhost:4200',
    viewport:         { width: 1440, height: 900 },
    locale:           'ro-RO',
    headless:         false,
    screenshot:       'off',
    video:            'off',
    actionTimeout:    8_000,
    navigationTimeout: 15_000,
  },

  reporter: [
    ['list'],
    ['html', { outputFolder: 'playwright-report', open: 'never' }],
  ],
});