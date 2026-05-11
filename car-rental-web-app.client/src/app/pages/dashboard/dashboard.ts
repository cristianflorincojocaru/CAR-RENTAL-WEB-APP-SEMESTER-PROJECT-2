import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AuthService } from '../../services/auth.service';
import { TokenService } from '../../services/token.service';
import { environment } from '../../../environments/environment';

// ── DTOs ──────────────────────────────────────────────────────

interface DashboardStats {
  totalVehicles: number;
  availableVehicles: number;
  rentedVehicles: number;
  totalClients: number;
  activeRentals: number;
  completedToday: number;
  todayRevenue: number;
  unreadAlerts: number;
}

interface RentalListItem {
  id: number;
  bookingReference: string;
  vehicleName: string;
  vehicleCategory: string;
  clientName: string;
  clientEmail: string;
  startDate: string;
  endDate: string;
  totalCost: number;
  status: string;
  pickupLocation: string;
  returnLocation: string;
  createdAt: string;
}

interface VehicleItem {
  id: number;
  name: string;
  brand: string;
  model: string;
  year: number;
  fuelType: string;
  category: string;
  branch: string;
  registrationNumber: string;
  dailyRate: number;
  status: string;
  rating: number;
  colorHex: string;
  isOffer: boolean;
  discountPercent: number;
  isActive: boolean;
  specs: { icon: string; value: string }[];
}

interface ClientItem {
  id: number;
  fullName: string;
  email: string;
  phone: string;
  address: string;
  isFlagged: boolean;
  isActive: boolean;
  createdAt: string;
}

interface UserItem {
  id: number;
  fullName: string;
  username: string;
  email: string;
  phone: string;
  role: string;
  branchId: number;
  branchName: string;
  isActive: boolean;
  isLocked: boolean;
  lastLoginAt: string;
  createdAt: string;
}

interface AlertItem {
  id: number;
  userId: number;
  userName: string;
  alertType: string;
  description: string;
  isRead: boolean;
  createdAt: string;
}

interface BranchItem {
  id: number;
  name: string;
  city: string;
  address: string;
  phone: string;
  managerName: string;
  isActive: boolean;
  vehicleCount: number;
  activeRentalCount: number;
}

interface RevenueSummary {
  totalRevenue: number;
  totalRentals: number;
  activeRentals: number;
  completedRentals: number;
  cancelledRentals: number;
  averageDailyRate: number;
  revenueByBranch: Record<string, number>;
  rentalsByCategory: Record<string, number>;
}

type AdminSection =
  | 'overview'
  | 'users'
  | 'branches'
  | 'rentals'
  | 'vehicles'
  | 'alerts'
  | 'revenue';

type ManagerSection =
  | 'overview'
  | 'vehicles'
  | 'clients'
  | 'rentals'
  | 'revenue';

type OperatorSection =
  | 'overview'
  | 'create-rental'
  | 'active-rentals'
  | 'search-vehicle'
  | 'search-client';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss'],
})
export class DashboardComponent implements OnInit {
  // ── User info ──────────────────────────────────────────────
  get role(): string {
    return this.authService.currentUser()?.role ?? '';
  }
  get userName(): string {
    return this.authService.currentUser()?.fullName ?? '';
  }
  get branchId(): number | undefined {
    return this.authService.currentUser()?.branchId;
  }

  // ── Active section ─────────────────────────────────────────
  activeSection: AdminSection | ManagerSection | OperatorSection = 'overview';

  // ── Loading / error states ─────────────────────────────────
  isLoading = false;
  loadError = '';

  // ── Data ──────────────────────────────────────────────────
  stats: DashboardStats | null = null;
  rentals: RentalListItem[] = [];
  vehicles: VehicleItem[] = [];
  clients: ClientItem[] = [];
  users: UserItem[] = [];
  alerts: AlertItem[] = [];
  branches: BranchItem[] = [];
  revenue: RevenueSummary | null = null;

  // ── Search ─────────────────────────────────────────────────
  searchVehicleQuery = '';
  searchClientQuery = '';
  searchRentalStatus = '';
  filteredVehicles: VehicleItem[] = [];
  filteredClients: ClientItem[] = [];

  // ── Sidebar collapsed ─────────────────────────────────────
  sidebarCollapsed = false;

  constructor(
    private authService: AuthService,
    private tokenService: TokenService,
    private http: HttpClient,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    // Only staff roles can access dashboard
    if (
      this.role !== 'Administrator' &&
      this.role !== 'Manager' &&
      this.role !== 'Operator'
    ) {
      this.router.navigate(['/']);
      return;
    }
    this.loadOverview();
  }

  // ── Navigation ─────────────────────────────────────────────
  setSection(section: string): void {
    this.activeSection = section as any;
    this.loadError = '';
    switch (section) {
      case 'overview':    this.loadOverview(); break;
      case 'rentals':
      case 'active-rentals': this.loadRentals(); break;
      case 'vehicles':
      case 'search-vehicle': this.loadVehicles(); break;
      case 'clients':
      case 'search-client':  this.loadClients(); break;
      case 'users':       this.loadUsers(); break;
      case 'alerts':      this.loadAlerts(); break;
      case 'branches':    this.loadBranches(); break;
      case 'revenue':     this.loadRevenue(); break;
    }
  }

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  // ── HTTP helpers ───────────────────────────────────────────
  private headers(): HttpHeaders {
    const token = this.tokenService.getAccessToken();
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  private get<T>(path: string) {
    return this.http.get<T>(`${environment.apiUrl}${path}`, {
      headers: this.headers(),
    });
  }

  // ── Data loaders ───────────────────────────────────────────
  loadOverview(): void {
    this.isLoading = true;
    this.get<DashboardStats>('/reports/dashboard').subscribe({
      next: (data) => {
        this.stats = data;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Could not load dashboard statistics.';
        this.isLoading = false;
        // Fallback mock so UI doesn't break
        this.stats = {
          totalVehicles: 0,
          availableVehicles: 0,
          rentedVehicles: 0,
          totalClients: 0,
          activeRentals: 0,
          completedToday: 0,
          todayRevenue: 0,
          unreadAlerts: 0,
        };
      },
    });
  }

  loadRentals(): void {
    this.isLoading = true;
    const path =
      this.role === 'Manager' && this.branchId
        ? `/rentals?branchId=${this.branchId}`
        : '/rentals';
    this.get<RentalListItem[]>(path).subscribe({
      next: (data) => {
        this.rentals = data;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Could not load rentals.';
        this.isLoading = false;
      },
    });
  }

  loadVehicles(): void {
    this.isLoading = true;
    const path =
      this.role === 'Manager' && this.branchId
        ? `/vehicles?branch=${encodeURIComponent(this.branchId.toString())}`
        : '/vehicles';
    this.get<VehicleItem[]>(path).subscribe({
      next: (data) => {
        this.vehicles = data;
        this.filteredVehicles = data;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Could not load vehicles.';
        this.isLoading = false;
      },
    });
  }

  loadClients(): void {
    this.isLoading = true;
    this.get<ClientItem[]>('/clients').subscribe({
      next: (data) => {
        this.clients = data;
        this.filteredClients = data;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Could not load clients.';
        this.isLoading = false;
      },
    });
  }

  loadUsers(): void {
    this.isLoading = true;
    this.get<UserItem[]>('/users').subscribe({
      next: (data) => {
        this.users = data;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Could not load staff members.';
        this.isLoading = false;
      },
    });
  }

  loadAlerts(): void {
    this.isLoading = true;
    this.get<AlertItem[]>('/reports/security-alerts').subscribe({
      next: (data) => {
        this.alerts = data;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Could not load security alerts.';
        this.isLoading = false;
      },
    });
  }

  loadBranches(): void {
    this.isLoading = true;
    this.get<BranchItem[]>('/branches').subscribe({
      next: (data) => {
        this.branches = data;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Could not load branches.';
        this.isLoading = false;
      },
    });
  }

  loadRevenue(): void {
    this.isLoading = true;
    const path =
      this.role === 'Manager' && this.branchId
        ? `/reports/revenue?branchId=${this.branchId}`
        : '/reports/revenue';
    this.get<RevenueSummary>(path).subscribe({
      next: (data) => {
        this.revenue = data;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Could not load revenue data.';
        this.isLoading = false;
      },
    });
  }

  // ── Search handlers ────────────────────────────────────────
  onSearchVehicle(): void {
    const q = this.searchVehicleQuery.toLowerCase().trim();
    this.filteredVehicles = q
      ? this.vehicles.filter(
          (v) =>
            v.name.toLowerCase().includes(q) ||
            v.registrationNumber.toLowerCase().includes(q) ||
            v.category.toLowerCase().includes(q)
        )
      : [...this.vehicles];
  }

  onSearchClient(): void {
    const q = this.searchClientQuery.toLowerCase().trim();
    if (!q) {
      this.filteredClients = [...this.clients];
      return;
    }
    this.isLoading = true;
    this.get<ClientItem[]>(`/clients?search=${encodeURIComponent(q)}`).subscribe({
      next: (data) => {
        this.filteredClients = data;
        this.isLoading = false;
      },
      error: () => {
        this.filteredClients = this.clients.filter(
          (c) =>
            c.fullName.toLowerCase().includes(q) ||
            c.email.toLowerCase().includes(q) ||
            c.phone.includes(q)
        );
        this.isLoading = false;
      },
    });
  }

  // ── Rental actions ─────────────────────────────────────────
  completeRental(id: number): void {
    if (!confirm('Mark this rental as completed?')) return;
    this.http
      .patch(
        `${environment.apiUrl}/rentals/${id}/complete`,
        {},
        { headers: this.headers() }
      )
      .subscribe({
        next: () => this.loadRentals(),
        error: () => alert('Could not complete rental.'),
      });
  }

  cancelRental(id: number): void {
    const reason = prompt('Enter cancellation reason:');
    if (!reason) return;
    this.http
      .patch(
        `${environment.apiUrl}/rentals/${id}/cancel`,
        { reason },
        { headers: this.headers() }
      )
      .subscribe({
        next: () => this.loadRentals(),
        error: () => alert('Could not cancel rental.'),
      });
  }

  // ── Vehicle actions ────────────────────────────────────────
  deactivateVehicle(id: number): void {
    if (!confirm('Deactivate this vehicle?')) return;
    this.http
      .delete(`${environment.apiUrl}/vehicles/${id}`, {
        headers: this.headers(),
      })
      .subscribe({
        next: () => this.loadVehicles(),
        error: () => alert('Could not deactivate vehicle.'),
      });
  }

  // ── Client actions ─────────────────────────────────────────
  flagClient(id: number): void {
    const reason = prompt('Enter flag reason:');
    if (!reason) return;
    this.http
      .post(
        `${environment.apiUrl}/clients/${id}/flag`,
        { reason },
        { headers: this.headers() }
      )
      .subscribe({
        next: () => this.loadClients(),
        error: () => alert('Could not flag client.'),
      });
  }

  unflagClient(id: number): void {
    if (!confirm('Remove flag from this client?')) return;
    this.http
      .delete(`${environment.apiUrl}/clients/${id}/flag`, {
        headers: this.headers(),
      })
      .subscribe({
        next: () => this.loadClients(),
        error: () => alert('Could not unflag client.'),
      });
  }

  // ── User actions ───────────────────────────────────────────
  unlockUser(id: number): void {
    if (!confirm('Unlock this user account?')) return;
    this.http
      .post(
        `${environment.apiUrl}/users/${id}/unlock`,
        {},
        { headers: this.headers() }
      )
      .subscribe({
        next: () => this.loadUsers(),
        error: () => alert('Could not unlock user.'),
      });
  }

  lockUser(id: number): void {
    if (!confirm('Lock this user account?')) return;
    this.http
      .post(
        `${environment.apiUrl}/users/${id}/lock`,
        {},
        { headers: this.headers() }
      )
      .subscribe({
        next: () => this.loadUsers(),
        error: () => alert('Could not lock user.'),
      });
  }

  deactivateUser(id: number): void {
    if (!confirm('Deactivate this staff member?')) return;
    this.http
      .delete(`${environment.apiUrl}/users/${id}`, {
        headers: this.headers(),
      })
      .subscribe({
        next: () => this.loadUsers(),
        error: () => alert('Could not deactivate user.'),
      });
  }

  // ── Alert actions ──────────────────────────────────────────
  markAlertRead(id: number): void {
    this.http
      .patch(
        `${environment.apiUrl}/reports/security-alerts/${id}/read`,
        {},
        { headers: this.headers() }
      )
      .subscribe({
        next: () => this.loadAlerts(),
        error: () => {},
      });
  }

  // ── Helpers ────────────────────────────────────────────────
  get isAdmin(): boolean {
    return this.role === 'Administrator';
  }
  get isManager(): boolean {
    return this.role === 'Manager';
  }
  get isOperator(): boolean {
    return this.role === 'Operator';
  }

  get roleLabel(): string {
    return this.role;
  }

  get roleColor(): string {
    if (this.isAdmin) return '#b85450';
    if (this.isManager) return '#82b366';
    return '#d6b656';
  }

  revenueEntries(): [string, number][] {
    return Object.entries(this.revenue?.revenueByBranch ?? {});
  }

  categoryEntries(): [string, number][] {
    return Object.entries(this.revenue?.rentalsByCategory ?? {});
  }

  statusBadge(status: string): string {
    switch (status?.toLowerCase()) {
      case 'active':    return 'badge-active';
      case 'completed': return 'badge-completed';
      case 'cancelled': return 'badge-cancelled';
      default:          return 'badge-default';
    }
  }

  formatDate(d: string): string {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  }

  formatCurrency(n: number): string {
    return `€${(n ?? 0).toFixed(2)}`;
  }

  getNavItems(): { icon: string; label: string; section: string }[] {
    if (this.isAdmin) {
      return [
        { icon: '📊', label: 'Overview',       section: 'overview' },
        { icon: '🚗', label: 'Vehicles',        section: 'vehicles' },
        { icon: '📋', label: 'Rentals',         section: 'rentals' },
        { icon: '👥', label: 'Clients',         section: 'clients' },
        { icon: '👤', label: 'Staff',           section: 'users' },
        { icon: '🏢', label: 'Branches',        section: 'branches' },
        { icon: '💰', label: 'Revenue',         section: 'revenue' },
        { icon: '🔔', label: 'Security Alerts', section: 'alerts' },
      ];
    }
    if (this.isManager) {
      return [
        { icon: '📊', label: 'Overview',   section: 'overview' },
        { icon: '🚗', label: 'Fleet',      section: 'vehicles' },
        { icon: '📋', label: 'Rentals',    section: 'rentals' },
        { icon: '👥', label: 'Clients',    section: 'clients' },
        { icon: '💰', label: 'Revenue',    section: 'revenue' },
      ];
    }
    // Operator
    return [
      { icon: '📊', label: 'Overview',       section: 'overview' },
      { icon: '📋', label: 'Active Rentals', section: 'active-rentals' },
      { icon: '🔍', label: 'Find Vehicle',   section: 'search-vehicle' },
      { icon: '🔍', label: 'Find Client',    section: 'search-client' },
    ];
  }

  get filteredRentals(): RentalListItem[] {
    if (!this.searchRentalStatus) return this.rentals;
    return this.rentals.filter(
      (r) => r.status.toLowerCase() === this.searchRentalStatus.toLowerCase()
    );
  }
}
