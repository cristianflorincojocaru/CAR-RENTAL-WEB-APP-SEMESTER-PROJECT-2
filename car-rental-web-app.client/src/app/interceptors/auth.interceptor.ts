import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const token = tokenService.getAccessToken();

  console.log('Interceptor — URL:', req.url, '| Token exists:', !!token, '| includes /api:', req.url.includes('/api'));

  if (!token || !req.url.includes(getApiBase())) {
    return next(req);
  }

  const clonedReq = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  });

  return next(clonedReq);
};

function getApiBase(): string {
  return '/api';
}