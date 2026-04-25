import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';

/**
 * Adaugă header-ul Authorization: Bearer <token> la toate request-urile
 * către propriul API (exclude request-urile externe, ex: Google Maps).
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const token = tokenService.getAccessToken();

  // Nu adăugăm header-ul dacă nu avem token sau dacă e request extern
  if (!token || !req.url.includes(getApiBase())) {
    return next(req);
  }

  const clonedReq = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  });

  return next(clonedReq);
};

/** Extrage baza URL-ului API din environment pentru filtrare */
function getApiBase(): string {
  // Funcționează atât cu URL relativ (/api) cât și absolut (https://...)
  return '/api';
}
