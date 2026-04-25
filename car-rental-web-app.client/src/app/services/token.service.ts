import { Injectable } from '@angular/core';
import { UserInfo } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class TokenService {

  private readonly ACCESS_KEY  = 'wd_access_token';
  private readonly REFRESH_KEY = 'wd_refresh_token';
  private readonly USER_KEY    = 'wd_user';

  // ── Stocare ─────────────────────────────────────────────────

  saveTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.ACCESS_KEY,  accessToken);
    localStorage.setItem(this.REFRESH_KEY, refreshToken);
  }

  saveUser(user: UserInfo): void {
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  // ── Citire ──────────────────────────────────────────────────

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_KEY);
  }

  getUser(): UserInfo | null {
    const raw = localStorage.getItem(this.USER_KEY);
    if (!raw) return null;
    try { return JSON.parse(raw) as UserInfo; }
    catch { return null; }
  }

  // ── Ștergere ─────────────────────────────────────────────────

  clearTokens(): void {
    localStorage.removeItem(this.ACCESS_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  // ── Utilitar ─────────────────────────────────────────────────

  /** Verifică dacă access token-ul există (nu verifică expirarea — asta o face interceptorul) */
  hasValidToken(): boolean {
    return !!this.getAccessToken();
  }

  /**
   * Decodează payload-ul JWT pentru a citi câmpuri (fără verificare semnătură).
   * Folosit doar pentru a citi exp/role din token-ul primit.
   */
  decodeToken(token: string): Record<string, unknown> | null {
    try {
      const payload = token.split('.')[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch {
      return null;
    }
  }

  /** Returnează true dacă access token-ul a expirat */
  isTokenExpired(): boolean {
    const token = this.getAccessToken();
    if (!token) return true;
    const payload = this.decodeToken(token);
    if (!payload || typeof payload['exp'] !== 'number') return true;
    // exp este în secunde Unix
    return Date.now() / 1000 > payload['exp'];
  }
}
