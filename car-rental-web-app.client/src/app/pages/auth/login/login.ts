import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../../services/auth.service';
import { LoginRequest } from '../../../models/auth.models';

interface LoginForm {
  email: string;
  password: string;
  remember: boolean;
}

interface LoginErrors {
  email?: string;
  password?: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss'],
})
export class LoginComponent {

  form: LoginForm = {
    email: '',
    password: '',
    remember: false,
  };

  errors: LoginErrors = {};
  loginError: string = '';
  isLoading = false;
  showPassword = false;

  // ── Forgot password modal ─────────────────────────────────────
  forgotModal = false;
  forgotEmail = '';
  forgotSent = false;
  forgotLoading = false;
  forgotError = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  // ── Forgot password ───────────────────────────────────────────

  openForgotModal(): void {
    this.forgotModal = true;
    this.forgotEmail = this.form.email;
    this.forgotSent = false;
    this.forgotError = '';
    document.body.style.overflow = 'hidden';
  }

  closeForgotModal(): void {
    this.forgotModal = false;
    this.forgotEmail = '';
    this.forgotSent = false;
    this.forgotError = '';
    document.body.style.overflow = '';
  }

  submitForgot(): void {
    const emailRx = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!this.forgotEmail || !emailRx.test(this.forgotEmail)) {
      this.forgotError = 'Please enter a valid email address.';
      return;
    }

    this.forgotError = '';
    this.forgotLoading = true;

    this.authService.forgotPassword({ email: this.forgotEmail }).subscribe({
      next: () => {
        this.forgotLoading = false;
        this.forgotSent = true;
      },
      error: (err: HttpErrorResponse) => {
        this.forgotLoading = false;
        if (err.status === 0) {
          this.forgotError = 'Cannot connect to server. Please try again later.';
        } else {
          this.forgotError = 'Something went wrong. Please try again.';
        }
      }
    });
  }

  // ── Validare câmpuri ──────────────────────────────────────────

  validateEmail(): void {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!this.form.email) {
      this.errors.email = 'Email address is required.';
    } else if (!emailRegex.test(this.form.email)) {
      this.errors.email = 'Please enter a valid email address.';
    } else {
      delete this.errors.email;
    }
  }

  private validateForm(): boolean {
    this.errors = {};
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!this.form.email) {
      this.errors.email = 'Email address is required.';
    } else if (!emailRegex.test(this.form.email)) {
      this.errors.email = 'Please enter a valid email address.';
    }

    if (!this.form.password) {
      this.errors.password = 'Password is required.';
    }

    return Object.keys(this.errors).length === 0;
  }

  // ── Submit ────────────────────────────────────────────────────

  onLogin(): void {
    this.loginError = '';
    if (!this.validateForm()) return;

    this.isLoading = true;

    const request: LoginRequest = {
      email: this.form.email,
      password: this.form.password
    };

    this.authService.login(request).subscribe({
      next: () => {
        this.isLoading = false;
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        if (err.status === 401) {
          this.loginError = 'Invalid email or password. Please try again.';
        } else if (err.status === 423) {
          this.loginError = 'Your account has been locked. Please contact support.';
        } else if (err.status === 0) {
          this.loginError = 'Cannot connect to server. Please try again later.';
        } else {
          this.loginError = err.error?.detail || 'An unexpected error occurred. Please try again.';
        }
      }
    });
  }

  loginWithGoogle(): void {
    console.log('Google OAuth — not yet implemented');
  }
}