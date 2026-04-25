import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { authInterceptor } from './interceptors/auth.interceptor';
import { refreshInterceptor } from './interceptors/refresh.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),

    // Router cu suport pentru input binding (queryParams → @Input)
    provideRouter(routes, withComponentInputBinding()),

    // HttpClient cu interceptorii în ordinea corectă:
    // 1. authInterceptor  — adaugă Bearer token la request
    // 2. refreshInterceptor — interceptează 401 și face refresh
    provideHttpClient(
      withInterceptors([authInterceptor, refreshInterceptor])
    ),
  ]
};
