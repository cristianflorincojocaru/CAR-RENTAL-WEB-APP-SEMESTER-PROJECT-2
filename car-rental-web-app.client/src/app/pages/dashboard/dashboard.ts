import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

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

interface ReportSection {
  id: string;
  label: string;
  selected: boolean;
  adminOnly: boolean;
  operatorAllowed: boolean;
}

type AdminSection =
  | 'overview' | 'users' | 'branches' | 'rentals'
  | 'vehicles' | 'alerts' | 'revenue' | 'report-builder';

type ManagerSection =
  | 'overview' | 'vehicles' | 'clients'
  | 'rentals' | 'revenue' | 'report-builder';

type OperatorSection =
  | 'overview' | 'create-rental' | 'active-rentals'
  | 'search-vehicle' | 'search-client' | 'report-builder';

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

  // ── Sidebar ────────────────────────────────────────────────
  sidebarCollapsed = false;

  // ── Report Builder ─────────────────────────────────────────
  reportSections: ReportSection[] = [
    { id: 'summary',  label: '📊 Company Overview', selected: true,  adminOnly: false, operatorAllowed: true  },
    { id: 'branches', label: '🏢 Branches',          selected: true,  adminOnly: true,  operatorAllowed: false },
    { id: 'fleet',    label: '🚗 Fleet / Vehicles',  selected: true,  adminOnly: false, operatorAllowed: true  },
    { id: 'rentals',  label: '📋 Rentals',           selected: true,  adminOnly: false, operatorAllowed: true  },
    { id: 'revenue',  label: '💰 Revenue',           selected: false, adminOnly: false, operatorAllowed: false },
    { id: 'clients',  label: '👥 Clients',           selected: false, adminOnly: false, operatorAllowed: false },
    { id: 'staff',    label: '👤 Staff Members',     selected: false, adminOnly: true,  operatorAllowed: false },
  ];
  reportDateFrom = '';
  reportDateTo = '';
  isGeneratingReport = false;

  get availableReportSections(): ReportSection[] {
    return this.reportSections.filter(s => {
      if (s.adminOnly) return this.isAdmin;
      if (!s.operatorAllowed && this.isOperator) return false;
      return true;
    });
  }

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
    if (this.role !== 'Administrator' && this.role !== 'Manager' && this.role !== 'Operator') {
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
      case 'overview':        this.loadOverview(); break;
      case 'rentals':
      case 'active-rentals':  this.loadRentals(); break;
      case 'vehicles':
      case 'search-vehicle':  this.loadVehicles(); break;
      case 'clients':
      case 'search-client':   this.loadClients(); break;
      case 'users':           this.loadUsers(); break;
      case 'alerts':          this.loadAlerts(); break;
      case 'branches':        this.loadBranches(); break;
      case 'revenue':         this.loadRevenue(); break;
      case 'report-builder':  break; // no preload needed
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
    return this.http.get<T>(`${environment.apiUrl}${path}`, { headers: this.headers() });
  }

  // ── Data loaders ───────────────────────────────────────────
  loadOverview(): void {
    this.isLoading = true;
    this.get<DashboardStats>('/reports/dashboard').subscribe({
      next: (data) => { this.stats = data; this.isLoading = false; },
      error: () => {
        this.loadError = 'Could not load dashboard statistics.';
        this.isLoading = false;
        this.stats = { totalVehicles: 0, availableVehicles: 0, rentedVehicles: 0, totalClients: 0, activeRentals: 0, completedToday: 0, todayRevenue: 0, unreadAlerts: 0 };
      },
    });
  }

  loadRentals(): void {
    this.isLoading = true;
    const path = this.role === 'Manager' && this.branchId ? `/rentals?branchId=${this.branchId}` : '/rentals';
    this.get<RentalListItem[]>(path).subscribe({
      next: (data) => { this.rentals = data; this.isLoading = false; },
      error: () => { this.loadError = 'Could not load rentals.'; this.isLoading = false; },
    });
  }

  loadVehicles(): void {
    this.isLoading = true;
    const path = this.role === 'Manager' && this.branchId ? `/vehicles?branch=${encodeURIComponent(this.branchId.toString())}` : '/vehicles';
    this.get<VehicleItem[]>(path).subscribe({
      next: (data) => { this.vehicles = data; this.filteredVehicles = data; this.isLoading = false; },
      error: () => { this.loadError = 'Could not load vehicles.'; this.isLoading = false; },
    });
  }

  loadClients(): void {
    this.isLoading = true;
    this.get<ClientItem[]>('/clients').subscribe({
      next: (data) => { this.clients = data; this.filteredClients = data; this.isLoading = false; },
      error: () => { this.loadError = 'Could not load clients.'; this.isLoading = false; },
    });
  }

  loadUsers(): void {
    this.isLoading = true;
    this.get<UserItem[]>('/users').subscribe({
      next: (data) => { this.users = data; this.isLoading = false; },
      error: () => { this.loadError = 'Could not load staff members.'; this.isLoading = false; },
    });
  }

  loadAlerts(): void {
    this.isLoading = true;
    this.get<AlertItem[]>('/reports/security-alerts').subscribe({
      next: (data) => { this.alerts = data; this.isLoading = false; },
      error: () => { this.loadError = 'Could not load security alerts.'; this.isLoading = false; },
    });
  }

  loadBranches(): void {
    this.isLoading = true;
    this.get<BranchItem[]>('/branches').subscribe({
      next: (data) => { this.branches = data; this.isLoading = false; },
      error: () => { this.loadError = 'Could not load branches.'; this.isLoading = false; },
    });
  }

  loadRevenue(): void {
    this.isLoading = true;
    const path = this.role === 'Manager' && this.branchId ? `/reports/revenue?branchId=${this.branchId}` : '/reports/revenue';
    this.get<RevenueSummary>(path).subscribe({
      next: (data) => { this.revenue = data; this.isLoading = false; },
      error: () => { this.loadError = 'Could not load revenue data.'; this.isLoading = false; },
    });
  }

  // ── Search handlers ────────────────────────────────────────
  onSearchVehicle(): void {
    const q = this.searchVehicleQuery.toLowerCase().trim();
    this.filteredVehicles = q
      ? this.vehicles.filter(v => v.name.toLowerCase().includes(q) || v.registrationNumber.toLowerCase().includes(q) || v.category.toLowerCase().includes(q))
      : [...this.vehicles];
  }

  onSearchClient(): void {
    const q = this.searchClientQuery.toLowerCase().trim();
    if (!q) { this.filteredClients = [...this.clients]; return; }
    this.isLoading = true;
    this.get<ClientItem[]>(`/clients?search=${encodeURIComponent(q)}`).subscribe({
      next: (data) => { this.filteredClients = data; this.isLoading = false; },
      error: () => {
        this.filteredClients = this.clients.filter(c => c.fullName.toLowerCase().includes(q) || c.email.toLowerCase().includes(q) || c.phone.includes(q));
        this.isLoading = false;
      },
    });
  }

  // ── Rental actions ─────────────────────────────────────────
  completeRental(id: number): void {
    if (!confirm('Mark this rental as completed?')) return;
    this.http.patch(`${environment.apiUrl}/rentals/${id}/complete`, {}, { headers: this.headers() })
      .subscribe({ next: () => this.loadRentals(), error: () => alert('Could not complete rental.') });
  }

  cancelRental(id: number): void {
    const reason = prompt('Enter cancellation reason:');
    if (!reason) return;
    this.http.patch(`${environment.apiUrl}/rentals/${id}/cancel`, { reason }, { headers: this.headers() })
      .subscribe({ next: () => this.loadRentals(), error: () => alert('Could not cancel rental.') });
  }

  // ── Vehicle actions ────────────────────────────────────────
  deactivateVehicle(id: number): void {
    if (!confirm('Deactivate this vehicle?')) return;
    this.http.delete(`${environment.apiUrl}/vehicles/${id}`, { headers: this.headers() })
      .subscribe({ next: () => this.loadVehicles(), error: () => alert('Could not deactivate vehicle.') });
  }

  // ── Client actions ─────────────────────────────────────────
  flagClient(id: number): void {
    const reason = prompt('Enter flag reason:');
    if (!reason) return;
    this.http.post(`${environment.apiUrl}/clients/${id}/flag`, { reason }, { headers: this.headers() })
      .subscribe({ next: () => this.loadClients(), error: () => alert('Could not flag client.') });
  }

  unflagClient(id: number): void {
    if (!confirm('Remove flag from this client?')) return;
    this.http.delete(`${environment.apiUrl}/clients/${id}/flag`, { headers: this.headers() })
      .subscribe({ next: () => this.loadClients(), error: () => alert('Could not unflag client.') });
  }

  // ── User actions ───────────────────────────────────────────
  unlockUser(id: number): void {
    if (!confirm('Unlock this user account?')) return;
    this.http.post(`${environment.apiUrl}/users/${id}/unlock`, {}, { headers: this.headers() })
      .subscribe({ next: () => this.loadUsers(), error: () => alert('Could not unlock user.') });
  }

  lockUser(id: number): void {
    if (!confirm('Lock this user account?')) return;
    this.http.post(`${environment.apiUrl}/users/${id}/lock`, {}, { headers: this.headers() })
      .subscribe({ next: () => this.loadUsers(), error: () => alert('Could not lock user.') });
  }

  deactivateUser(id: number): void {
    if (!confirm('Deactivate this staff member?')) return;
    this.http.delete(`${environment.apiUrl}/users/${id}`, { headers: this.headers() })
      .subscribe({ next: () => this.loadUsers(), error: () => alert('Could not deactivate user.') });
  }

  // ── Alert actions ──────────────────────────────────────────
  markAlertRead(id: number): void {
    this.http.patch(`${environment.apiUrl}/reports/security-alerts/${id}/read`, {}, { headers: this.headers() })
      .subscribe({ next: () => this.loadAlerts(), error: () => {} });
  }

  // ── Report Builder ─────────────────────────────────────────
  generateReport(): void {
    const selected = this.availableReportSections.filter(s => s.selected);
    if (!selected.length) { alert('Please select at least one section.'); return; }

    this.isGeneratingReport = true;
    const obs: Record<string, any> = {};

    if (selected.find(s => s.id === 'summary'))
      obs['stats'] = this.get<DashboardStats>('/reports/dashboard').pipe(catchError(() => of(null)));

    if (selected.find(s => s.id === 'branches') && this.isAdmin)
      obs['branches'] = this.get<BranchItem[]>('/branches').pipe(catchError(() => of([])));

    if (selected.find(s => s.id === 'fleet')) {
      const path = this.isManager && this.branchId ? `/vehicles?branchId=${this.branchId}` : '/vehicles';
      obs['vehicles'] = this.get<VehicleItem[]>(path).pipe(catchError(() => of([])));
    }

    if (selected.find(s => s.id === 'rentals')) {
      const params: string[] = [];
      if (this.isManager && this.branchId) params.push(`branchId=${this.branchId}`);
      if (this.reportDateFrom) params.push(`from=${this.reportDateFrom}`);
      if (this.reportDateTo) params.push(`to=${this.reportDateTo}`);
      obs['rentals'] = this.get<RentalListItem[]>('/rentals' + (params.length ? '?' + params.join('&') : '')).pipe(catchError(() => of([])));
    }

    if (selected.find(s => s.id === 'revenue')) {
      const path = this.isManager && this.branchId ? `/reports/revenue?branchId=${this.branchId}` : '/reports/revenue';
      obs['revenue'] = this.get<RevenueSummary>(path).pipe(catchError(() => of(null)));
    }

    if (selected.find(s => s.id === 'clients'))
      obs['clients'] = this.get<ClientItem[]>('/clients').pipe(catchError(() => of([])));

    if (selected.find(s => s.id === 'staff') && this.isAdmin)
      obs['users'] = this.get<UserItem[]>('/users').pipe(catchError(() => of([])));

    forkJoin(obs).subscribe({
      next: (data: any) => {
        this.isGeneratingReport = false;
        this.buildAndPrintReport(data);
      },
      error: () => { this.isGeneratingReport = false; },
    });
  }

  private buildAndPrintReport(data: any): void {
    const date = new Date().toLocaleDateString('en-GB', { day: '2-digit', month: 'long', year: 'numeric' });
    let sections = '';

    // ── Summary ──────────────────────────────────────────────
    if (data.stats) {
      const s = data.stats;
      sections += `
        <div class="section">
          <div class="section-title">📊 Company Overview</div>
          <div class="stats-grid">
            <div class="stat-box"><div class="stat-val">${s.totalVehicles}</div><div class="stat-lbl">Total Vehicles</div></div>
            <div class="stat-box"><div class="stat-val">${s.availableVehicles}</div><div class="stat-lbl">Available</div></div>
            <div class="stat-box"><div class="stat-val">${s.activeRentals}</div><div class="stat-lbl">Active Rentals</div></div>
            <div class="stat-box"><div class="stat-val">${s.totalClients}</div><div class="stat-lbl">Total Clients</div></div>
            <div class="stat-box"><div class="stat-val">€${s.todayRevenue.toFixed(0)}</div><div class="stat-lbl">Today's Revenue</div></div>
            <div class="stat-box"><div class="stat-val">${s.completedToday}</div><div class="stat-lbl">Completed Today</div></div>
          </div>
        </div>`;
    }

    // ── Branches ─────────────────────────────────────────────
    if (data.branches?.length) {
      const rows = data.branches.map((b: BranchItem) => `
        <tr>
          <td><strong>${b.name}</strong></td>
          <td>${b.city}</td>
          <td>${b.address}</td>
          <td>${b.phone}</td>
          <td>${b.managerName || '—'}</td>
          <td style="text-align:center">${b.vehicleCount}</td>
          <td style="text-align:center">${b.activeRentalCount}</td>
          <td><span class="${b.isActive ? 'status-active' : 'status-cancelled'}">${b.isActive ? 'Active' : 'Inactive'}</span></td>
        </tr>`).join('');
      sections += `
        <div class="section">
          <div class="section-title">🏢 Branches (${data.branches.length})</div>
          <table>
            <thead><tr><th>Name</th><th>City</th><th>Address</th><th>Phone</th><th>Manager</th><th>Vehicles</th><th>Active Rentals</th><th>Status</th></tr></thead>
            <tbody>${rows}</tbody>
          </table>
        </div>`;
    }

    // ── Fleet ────────────────────────────────────────────────
    if (data.vehicles?.length) {
      const byStatus = (s: string) => data.vehicles.filter((v: VehicleItem) => v.status === s).length;
      const rows = data.vehicles.map((v: VehicleItem) => `
        <tr>
          <td><strong>${v.name}</strong><br><small style="color:#64748B">${v.year} · ${v.fuelType || '—'}</small></td>
          <td><code style="background:#F1F5F9;padding:2px 6px;border-radius:4px;font-size:10px">${v.registrationNumber}</code></td>
          <td>${v.category}</td>
          <td>${v.branch}</td>
          <td>€${v.dailyRate}/day</td>
          <td><span class="${v.status === 'Available' ? 'status-active' : v.status === 'Rented' ? 'status-completed' : 'status-cancelled'}">${v.status}</span></td>
        </tr>`).join('');
      sections += `
        <div class="section">
          <div class="section-title">🚗 Fleet (${data.vehicles.length} vehicles — ${byStatus('Available')} available · ${byStatus('Rented')} rented · ${byStatus('Maintenance')} maintenance)</div>
          <table>
            <thead><tr><th>Vehicle</th><th>Plate</th><th>Category</th><th>Branch</th><th>Daily Rate</th><th>Status</th></tr></thead>
            <tbody>${rows}</tbody>
          </table>
        </div>`;
    }

    // ── Rentals ──────────────────────────────────────────────
    if (data.rentals?.length) {
      const totalCost = data.rentals.reduce((s: number, r: RentalListItem) => s + r.totalCost, 0);
      const rows = data.rentals.map((r: RentalListItem) => `
        <tr>
          <td><code style="background:#F1F5F9;padding:2px 6px;border-radius:4px;font-size:10px">${r.bookingReference}</code></td>
          <td><strong>${r.clientName}</strong><br><small style="color:#64748B">${r.clientEmail}</small></td>
          <td>${r.vehicleName}<br><small style="color:#64748B">${r.vehicleCategory}</small></td>
          <td>${this.formatDate(r.startDate)}</td>
          <td>${this.formatDate(r.endDate)}</td>
          <td style="font-size:10px;color:#64748B">${r.pickupLocation}</td>
          <td><strong>€${r.totalCost.toFixed(2)}</strong></td>
          <td><span class="${r.status === 'Active' ? 'status-active' : r.status === 'Completed' ? 'status-completed' : 'status-cancelled'}">${r.status}</span></td>
        </tr>`).join('');
      sections += `
        <div class="section">
          <div class="section-title">📋 Rentals (${data.rentals.length})</div>
          <table>
            <thead><tr><th>Reference</th><th>Client</th><th>Vehicle</th><th>Start</th><th>End</th><th>Pick-up</th><th>Total</th><th>Status</th></tr></thead>
            <tbody>${rows}</tbody>
            <tfoot><tr><td colspan="6"><strong>Total</strong></td><td><strong>€${totalCost.toFixed(2)}</strong></td><td></td></tr></tfoot>
          </table>
        </div>`;
    }

    // ── Revenue ──────────────────────────────────────────────
    if (data.revenue) {
      const rv = data.revenue;
      const branchRows = Object.entries(rv.revenueByBranch || {}).map(([b, a]: [string, any]) =>
        `<tr><td>${b}</td><td><strong>€${a.toFixed(2)}</strong></td><td style="width:160px"><div style="background:#DBEAFE;height:8px;border-radius:4px"><div style="background:#1A56DB;height:8px;border-radius:4px;width:${rv.totalRevenue > 0 ? Math.round((a / rv.totalRevenue) * 100) : 0}%"></div></div></td></tr>`).join('');
      const catRows = Object.entries(rv.rentalsByCategory || {}).map(([c, n]: [string, any]) =>
        `<tr><td>${c}</td><td style="text-align:center"><strong>${n}</strong></td></tr>`).join('');
      sections += `
        <div class="section">
          <div class="section-title">💰 Revenue Summary</div>
          <div class="stats-grid" style="grid-template-columns:repeat(4,1fr)">
            <div class="stat-box"><div class="stat-val">€${rv.totalRevenue.toFixed(2)}</div><div class="stat-lbl">Total Revenue</div></div>
            <div class="stat-box"><div class="stat-val">${rv.totalRentals}</div><div class="stat-lbl">Total Rentals</div></div>
            <div class="stat-box"><div class="stat-val">€${rv.averageDailyRate.toFixed(2)}</div><div class="stat-lbl">Avg Daily Rate</div></div>
            <div class="stat-box"><div class="stat-val">${rv.cancelledRentals}</div><div class="stat-lbl">Cancelled</div></div>
          </div>
          <div style="display:grid;grid-template-columns:1fr 1fr;gap:20px;margin-top:16px">
            <div>
              <div class="subsection-title">Revenue by Branch</div>
              <table><thead><tr><th>Branch</th><th>Revenue</th><th>Share</th></tr></thead><tbody>${branchRows}</tbody></table>
            </div>
            <div>
              <div class="subsection-title">Rentals by Category</div>
              <table><thead><tr><th>Category</th><th>Count</th></tr></thead><tbody>${catRows}</tbody></table>
            </div>
          </div>
        </div>`;
    }

    // ── Clients ──────────────────────────────────────────────
    if (data.clients?.length) {
      const rows = data.clients.map((c: ClientItem) => `
        <tr>
          <td><strong>${c.fullName}</strong></td>
          <td>${c.email}</td>
          <td>${c.phone || '—'}</td>
          <td><span class="${c.isActive ? 'status-active' : 'status-cancelled'}">${c.isActive ? 'Active' : 'Inactive'}</span></td>
          <td>${c.isFlagged ? '<span style="color:#B45309;font-weight:600">⚠ Flagged</span>' : '—'}</td>
          <td>${this.formatDate(c.createdAt)}</td>
        </tr>`).join('');
      sections += `
        <div class="section">
          <div class="section-title">👥 Clients (${data.clients.length})</div>
          <table>
            <thead><tr><th>Name</th><th>Email</th><th>Phone</th><th>Status</th><th>Flag</th><th>Member Since</th></tr></thead>
            <tbody>${rows}</tbody>
          </table>
        </div>`;
    }

    // ── Staff ────────────────────────────────────────────────
    if (data.users?.length) {
      const rows = data.users.map((u: UserItem) => `
        <tr>
          <td><strong>${u.fullName}</strong><br><small style="color:#64748B">@${u.username}</small></td>
          <td>${u.email}</td>
          <td><span style="padding:2px 8px;border-radius:12px;font-size:10px;font-weight:600;background:${u.role === 'Administrator' ? '#FEE2E2' : u.role === 'Manager' ? '#D1FAE5' : '#FEF3C7'};color:${u.role === 'Administrator' ? '#B91C1C' : u.role === 'Manager' ? '#065F46' : '#92400E'}">${u.role}</span></td>
          <td>${u.branchName || '—'}</td>
          <td><span class="${u.isLocked ? 'status-cancelled' : u.isActive ? 'status-active' : 'status-cancelled'}">${u.isLocked ? '🔒 Locked' : u.isActive ? 'Active' : 'Inactive'}</span></td>
          <td>${u.lastLoginAt ? this.formatDate(u.lastLoginAt) : 'Never'}</td>
        </tr>`).join('');
      sections += `
        <div class="section">
          <div class="section-title">👤 Staff Members (${data.users.length})</div>
          <table>
            <thead><tr><th>Name</th><th>Email</th><th>Role</th><th>Branch</th><th>Status</th><th>Last Login</th></tr></thead>
            <tbody>${rows}</tbody>
          </table>
        </div>`;
    }

    // ── Build HTML ───────────────────────────────────────────
    const periodNote = this.reportDateFrom || this.reportDateTo
      ? `<br>Period: ${this.reportDateFrom || '—'} → ${this.reportDateTo || '—'}`
      : '';

    const html = `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>WheelDeal — Business Report</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: 'Segoe UI', Arial, sans-serif; font-size: 12px; color: #0F172A; padding: 36px 44px; }
    .report-header { display: flex; justify-content: space-between; align-items: flex-start; padding-bottom: 20px; border-bottom: 3px solid #1A56DB; margin-bottom: 32px; }
    .report-header__logo { font-size: 28px; font-weight: 800; color: #1A56DB; letter-spacing: -0.5px; }
    .report-header__tagline { font-size: 11px; color: #64748B; margin-top: 3px; }
    .report-header__meta { text-align: right; font-size: 11px; color: #64748B; line-height: 1.7; }
    .report-header__meta strong { display: block; font-size: 16px; color: #0F172A; font-weight: 700; margin-bottom: 4px; }
    .section { margin-bottom: 36px; }
    .section-title { font-size: 14px; font-weight: 700; color: #1A56DB; padding: 9px 16px; background: #EFF6FF; border-left: 4px solid #1A56DB; border-radius: 0 6px 6px 0; margin-bottom: 16px; }
    .subsection-title { font-size: 11px; font-weight: 600; color: #475569; margin-bottom: 8px; text-transform: uppercase; letter-spacing: 0.05em; }
    .stats-grid { display: grid; grid-template-columns: repeat(6, 1fr); gap: 10px; }
    .stat-box { background: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 8px; padding: 12px; text-align: center; }
    .stat-val { font-size: 20px; font-weight: 800; color: #1A56DB; line-height: 1.1; }
    .stat-lbl { font-size: 9px; color: #64748B; margin-top: 3px; text-transform: uppercase; letter-spacing: 0.04em; }
    table { width: 100%; border-collapse: collapse; font-size: 11px; }
    thead tr { background: #1A56DB; color: white; }
    thead th { padding: 8px 10px; text-align: left; font-size: 10px; font-weight: 600; letter-spacing: 0.03em; text-transform: uppercase; white-space: nowrap; }
    tbody tr { border-bottom: 1px solid #F1F5F9; }
    tbody tr:nth-child(even) { background: #F8FAFC; }
    tbody td { padding: 8px 10px; vertical-align: middle; }
    tfoot td { padding: 8px 10px; background: #EFF6FF; }
    .status-active { color: #15803D; font-weight: 600; }
    .status-completed { color: #1E40AF; font-weight: 600; }
    .status-cancelled { color: #B91C1C; font-weight: 600; }
    .report-footer { margin-top: 44px; padding-top: 14px; border-top: 1px solid #E2E8F0; text-align: center; font-size: 10px; color: #94A3B8; }
    @media print { body { padding: 20px 24px; } .section { page-break-inside: avoid; } }
  </style>
</head>
<body>
  <div class="report-header">
    <div>
      <div class="report-header__logo">WheelDeal</div>
      <div class="report-header__tagline">Car Rental Management System</div>
    </div>
    <div class="report-header__meta">
      <strong>Business Report</strong>
      Generated: ${date}<br>
      By: ${this.userName} (${this.role})${periodNote}
    </div>
  </div>
  ${sections}
  <div class="report-footer">WheelDeal Car Rental · Confidential Business Report · Generated ${date} by ${this.userName}</div>
</body>
</html>`;

    const win = window.open('', '_blank');
    if (!win) return;
    win.document.write(html);
    win.document.close();
    win.focus();
    setTimeout(() => { win.print(); }, 600);
  }

  // ── Helpers ────────────────────────────────────────────────
  get isAdmin(): boolean { return this.role === 'Administrator'; }
  get isManager(): boolean { return this.role === 'Manager'; }
  get isOperator(): boolean { return this.role === 'Operator'; }
  get roleLabel(): string { return this.role; }

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
    return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  formatCurrency(n: number): string {
    return `€${(n ?? 0).toFixed(2)}`;
  }

  getNavItems(): { icon: string; label: string; section: string; divider?: boolean }[] {
    let items: { icon: string; label: string; section: string; divider?: boolean }[] = [];

    if (this.isAdmin) {
      items = [
        { icon: '📊', label: 'Overview',        section: 'overview'  },
        { icon: '🚗', label: 'Vehicles',         section: 'vehicles'  },
        { icon: '📋', label: 'Rentals',          section: 'rentals'   },
        { icon: '👥', label: 'Clients',          section: 'clients'   },
        { icon: '👤', label: 'Staff',            section: 'users'     },
        { icon: '🏢', label: 'Branches',         section: 'branches'  },
        { icon: '💰', label: 'Revenue',          section: 'revenue'   },
        { icon: '🔔', label: 'Security Alerts',  section: 'alerts'    },
      ];
    } else if (this.isManager) {
      items = [
        { icon: '📊', label: 'Overview', section: 'overview'  },
        { icon: '🚗', label: 'Fleet',    section: 'vehicles'  },
        { icon: '📋', label: 'Rentals',  section: 'rentals'   },
        { icon: '👥', label: 'Clients',  section: 'clients'   },
        { icon: '💰', label: 'Revenue',  section: 'revenue'   },
      ];
    } else {
      items = [
        { icon: '📊', label: 'Overview',        section: 'overview'        },
        { icon: '📋', label: 'Active Rentals',  section: 'active-rentals'  },
        { icon: '🔍', label: 'Find Vehicle',    section: 'search-vehicle'  },
        { icon: '🔍', label: 'Find Client',     section: 'search-client'   },
      ];
    }

    items.push({ icon: '📄', label: 'Reports', section: 'report-builder', divider: true });
    return items;
  }

  get filteredRentals(): RentalListItem[] {
    if (!this.searchRentalStatus) return this.rentals;
    return this.rentals.filter(r => r.status.toLowerCase() === this.searchRentalStatus.toLowerCase());
  }
}