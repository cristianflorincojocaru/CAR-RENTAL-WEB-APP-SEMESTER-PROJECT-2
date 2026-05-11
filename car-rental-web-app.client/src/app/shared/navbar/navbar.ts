import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { AuthService } from '../../services/auth.service';
import { UserInfo } from '../../models/auth.models';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.scss']
})
export class NavbarComponent implements OnInit, OnDestroy {

  isScrolled       = false;
  menuOpen         = false;
  userDropdownOpen = false;

  get currentUser(): UserInfo | null {
    return this.authService.currentUser();
  }

  get isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  get userInitials(): string {
    const user = this.currentUser;
    if (!user?.fullName) return '?';
    return user.fullName
      .split(' ')
      .slice(0, 2)
      .map(n => n.charAt(0).toUpperCase())
      .join('');
  }

  /**
   * Returns true if the logged-in user has a staff role
   * (Administrator, Manager, or Operator).
   * Used to show/hide the Dashboard button in the navbar.
   */
  get isStaff(): boolean {
    const role = this.currentUser?.role;
    return role === 'Administrator' || role === 'Manager' || role === 'Operator';
  }

  constructor(public authService: AuthService) {}

  ngOnInit(): void {
    this.checkScroll();
  }

  ngOnDestroy(): void {}

  @HostListener('window:scroll')
  checkScroll(): void {
    this.isScrolled = window.scrollY > 20;
  }

  // Inchide dropdown-ul daca utilizatorul face click in afara lui
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.navbar__user-menu')) {
      this.userDropdownOpen = false;
    }
  }

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
    document.body.style.overflow = this.menuOpen ? 'hidden' : '';
  }

  closeMenu(): void {
    this.menuOpen = false;
    document.body.style.overflow = '';
  }

  toggleUserDropdown(): void {
    this.userDropdownOpen = !this.userDropdownOpen;
  }

  closeUserDropdown(): void {
    this.userDropdownOpen = false;
  }

  logout(): void {
    this.closeMenu();
    this.closeUserDropdown();
    this.authService.logout();
  }
}
