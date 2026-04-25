import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { environment } from '../../environments/environment';
import { TokenService } from './token.service';
import {
  AuthResponse,
  LoginRequest,
  SignupRequest,
  RefreshTokenRequest,
  ChangePasswordRequest,
  UserInfo
} from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private readonly apiUrl = `${environment.apiUrl}/auth`;

  // Signal reactiv pentru utilizatorul curent — folosit în navbar și alte componente
  readonly currentUser = signal<UserInfo | null>(null);

  constructor(
    private http: HttpClient,
    private tokenService: TokenService,
    private router: Router
  ) {
    this.currentUser.set(this.tokenService.getUser());
  }

  // ── Login ────────────────────────────────────────────────────

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(res => this.handleAuthResponse(res))
    );
  }

  // ── Signup ───────────────────────────────────────────────────

  signup(request: SignupRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(res => this.handleAuthResponse(res))
    );
  }

  // ── Logout ───────────────────────────────────────────────────

  logout(): void {
    const refreshToken = this.tokenService.getRefreshToken();
    // Trimitem revocarea token-ului către server (fire and forget)
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/logout`, { refreshToken }).subscribe({
        error: () => { /* ignorăm eroarea — curățăm local oricum */ }
      });
    }
    this.tokenService.clearTokens();
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  // ── Refresh Token ────────────────────────────────────────────

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.tokenService.getRefreshToken();
    const request: RefreshTokenRequest = { refreshToken: refreshToken ?? '' };
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, request).pipe(
      tap(res => this.handleAuthResponse(res))
    );
  }

  // ── Change Password ──────────────────────────────────────────

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/change-password`, request);
  }

  // ── Stare autentificare ──────────────────────────────────────

  isLoggedIn(): boolean {
    return this.tokenService.hasValidToken();
  }

  getUser(): UserInfo | null {
    return this.currentUser();
  }

  hasRole(role: string): boolean {
    return this.currentUser()?.role === role;
  }

  // ── Helper privat ─────────────────────────────────────────────

  private handleAuthResponse(res: AuthResponse): void {
    this.tokenService.saveTokens(res.accessToken, res.refreshToken);
    this.tokenService.saveUser(res.user);
    this.currentUser.set(res.user);
  }
}