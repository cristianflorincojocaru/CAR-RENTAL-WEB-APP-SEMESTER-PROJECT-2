import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard pentru rute care necesită autentificare.
 * Dacă utilizatorul nu este autentificat, este redirecționat la /login
 * cu returnUrl setat pentru a reveni după login.
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router      = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  // Salvăm URL-ul curent pentru a reveni după login
  router.navigate(['/login'], {
    queryParams: { returnUrl: state.url }
  });
  return false;
};

/**
 * Guard pentru rute care necesită un anumit rol.
 * Folosire în routes: canActivate: [roleGuard('Administrator')]
 */
export const roleGuard = (requiredRole: string): CanActivateFn => {
  return () => {
    const authService = inject(AuthService);
    const router      = inject(Router);

    if (!authService.isLoggedIn()) {
      router.navigate(['/login']);
      return false;
    }

    if (authService.hasRole(requiredRole)) {
      return true;
    }

    // Utilizatorul e autentificat dar nu are rolul necesar
    router.navigate(['/']);
    return false;
  };
};
