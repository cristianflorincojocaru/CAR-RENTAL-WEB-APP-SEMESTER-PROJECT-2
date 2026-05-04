import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../../services/auth.service';
import { SignupRequest } from '../../../models/auth.models';

interface SignupForm {
  fullName: string;
  username: string;
  email: string;
  phone: string;
  password: string;
  confirmPassword: string;
  terms: boolean;
}

interface SignupErrors {
  fullName?: string;
  username?: string;
  email?: string;
  phone?: string;
  password?: string;
  confirmPassword?: string;
  terms?: string;
}

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './signup.html',
  styleUrls: ['./signup.scss'],
})
export class SignupComponent {

  form: SignupForm = {
    fullName: '',
    username: '',
    email: '',
    phone: '',
    password: '',
    confirmPassword: '',
    terms: false,
  };

  errors: SignupErrors = {};
  signupError: string = '';
  isLoading = false;
  showPassword = false;
  showConfirmPassword = false;

  legalModal: 'terms' | 'privacy' | null = null;

  perks = [
    'Free account — no credit card needed',
    '10% discount on your first rental',
    'Exclusive member-only deals & early access',
    'Full rental history & easy re-booking',
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  openLegalModal(type: 'terms' | 'privacy'): void {
    this.legalModal = type;
    document.body.style.overflow = 'hidden';
  }

  closeLegalModal(): void {
    this.legalModal = null;
    document.body.style.overflow = '';
  }

  get passwordStrength(): number {
    const p = this.form.password;
    if (!p) return 0;
    let score = 0;
    if (p.length >= 8)  score++;
    if (p.length >= 12) score++;
    if (/[A-Z]/.test(p) && /[a-z]/.test(p)) score++;
    if (/\d/.test(p) && /[^A-Za-z0-9]/.test(p)) score++;
    return score;
  }

  get strengthLabel(): string {
    const labels = ['', 'Weak', 'Fair', 'Good', 'Strong'];
    return labels[this.passwordStrength] ?? '';
  }

  strengthColor(_bar: number): string {
    const s = this.passwordStrength;
    if (s === 0) return 'empty';
    if (s === 1) return 'weak';
    if (s === 2) return 'fair';
    if (s === 3) return 'good';
    return 'strong';
  }

  validateField(field: keyof SignupForm): void {
    switch (field) {
      case 'fullName':
        if (!this.form.fullName.trim()) {
          this.errors.fullName = 'Full name is required.';
        } else if (this.form.fullName.trim().length < 2) {
          this.errors.fullName = 'Name must be at least 2 characters.';
        } else {
          delete this.errors.fullName;
        }
        break;

      case 'username':
        if (!this.form.username.trim()) {
          this.errors.username = 'Username is required.';
        } else if (!/^[a-z0-9_]{3,20}$/.test(this.form.username)) {
          this.errors.username = '3–20 lowercase letters, numbers or underscores.';
        } else {
          delete this.errors.username;
        }
        break;

      case 'email':
        const emailRx = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!this.form.email) {
          this.errors.email = 'Email address is required.';
        } else if (!emailRx.test(this.form.email)) {
          this.errors.email = 'Please enter a valid email address.';
        } else {
          delete this.errors.email;
        }
        break;

      case 'phone':
        const phoneRx = /^[+]?[\d\s\-().]{7,20}$/;
        if (!this.form.phone) {
          this.errors.phone = 'Phone number is required.';
        } else if (!phoneRx.test(this.form.phone)) {
          this.errors.phone = 'Please enter a valid phone number.';
        } else {
          delete this.errors.phone;
        }
        break;

      case 'password':
        if (!this.form.password) {
          this.errors.password = 'Password is required.';
        } else if (this.form.password.length < 8) {
          this.errors.password = 'Password must be at least 8 characters.';
        } else {
          delete this.errors.password;
        }
        if (this.form.confirmPassword) {
          this.validateField('confirmPassword');
        }
        break;

      case 'confirmPassword':
        if (!this.form.confirmPassword) {
          this.errors.confirmPassword = 'Please confirm your password.';
        } else if (this.form.confirmPassword !== this.form.password) {
          this.errors.confirmPassword = 'Passwords do not match.';
        } else {
          delete this.errors.confirmPassword;
        }
        break;
    }
  }

  private validateAll(): boolean {
    (['fullName', 'username', 'email', 'phone', 'password', 'confirmPassword'] as const)
      .forEach(f => this.validateField(f));

    if (!this.form.terms) {
      this.errors.terms = 'You must accept the Terms of Service.';
    } else {
      delete this.errors.terms;
    }

    return Object.keys(this.errors).length === 0;
  }

  onSignup(): void {
    this.signupError = '';
    if (!this.validateAll()) return;

    this.isLoading = true;

    const request: SignupRequest = {
      fullName: this.form.fullName.trim(),
      username: this.form.username.trim(),
      email: this.form.email.trim(),
      phone: this.form.phone.trim(),
      password: this.form.password
    };

    this.authService.signup(request).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/']);
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        if (err.status === 409) {
          const detail: string = err.error?.detail ?? '';
          if (detail.toLowerCase().includes('email')) {
            this.errors.email = 'This email is already registered.';
          } else if (detail.toLowerCase().includes('username')) {
            this.errors.username = 'This username is already taken.';
          } else {
            this.signupError = 'An account with these details already exists.';
          }
        } else if (err.status === 400 && err.error?.errors) {
          const serverErrors = err.error.errors as Record<string, string[]>;
          Object.entries(serverErrors).forEach(([key, messages]) => {
            const fieldKey = key.charAt(0).toLowerCase() + key.slice(1) as keyof SignupErrors;
            (this.errors as Record<string, string>)[fieldKey] = messages[0];
          });
        } else if (err.status === 0) {
          this.signupError = 'Cannot connect to server. Please try again later.';
        } else {
          this.signupError = err.error?.detail || 'Registration failed. Please try again.';
        }
      }
    });
  }

  signupWithGoogle(): void {
    console.log('Google OAuth — not yet implemented');
  }
}