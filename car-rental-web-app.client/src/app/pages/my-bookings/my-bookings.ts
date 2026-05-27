import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { BookingService } from '../../services/booking.service';
import { Rental } from '../../models/booking.models';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './my-bookings.html',
  styleUrls: ['./my-bookings.scss'],
})
export class MyBookingsComponent implements OnInit {

  rentals: Rental[] = [];
  isLoading = false;
  loadError = false;

  filterOptions = ['All', 'Active', 'Completed', 'Cancelled'];
  activeFilter = 'All';

  sortOptions = [
    { value: 'date-desc',  label: 'Newest First'      },
    { value: 'date-asc',   label: 'Oldest First'       },
    { value: 'price-desc', label: 'Price: High to Low' },
    { value: 'price-asc',  label: 'Price: Low to High' },
  ];
  activeSort = 'date-desc';

  constructor(private bookingService: BookingService) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  private loadBookings(): void {
    this.isLoading = true;
    this.loadError = false;

    this.bookingService.getMyBookings().subscribe({
      next: (data) => {
        this.rentals = data;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = true;
        this.isLoading = false;
      },
    });
  }

  // ── Filtering & sorting ───────────────────────────────────────

  get filteredRentals(): Rental[] {
    let result = [...this.rentals];

    if (this.activeFilter !== 'All') {
      result = result.filter(r => r.status === this.activeFilter);
    }

    switch (this.activeSort) {
      case 'date-desc':
        result.sort((a, b) => +new Date(b.startDate) - +new Date(a.startDate));
        break;
      case 'date-asc':
        result.sort((a, b) => +new Date(a.startDate) - +new Date(b.startDate));
        break;
      case 'price-desc':
        result.sort((a, b) => b.totalCost - a.totalCost);
        break;
      case 'price-asc':
        result.sort((a, b) => a.totalCost - b.totalCost);
        break;
    }

    return result;
  }

  // ── Helpers ───────────────────────────────────────────────────

  getRentalDays(rental: Rental): number {
    const diff = +new Date(rental.endDate) - +new Date(rental.startDate);
    return Math.max(1, Math.round(diff / 86_400_000));
  }

  getDailyRate(rental: Rental): number {
    const days = this.getRentalDays(rental);
    return Math.round(rental.totalCost / days);
  }

  getCategoryColor(category: string): string {
    const map: Record<string, string> = {
      'Economy':  '#60A5FA',
      'Compact':  '#1A56DB',
      'SUV':      '#1340B0',
      'Premium':  '#0F172A',
      'Van':      '#334155',
    };
    return map[category] ?? '#1A56DB';
  }

  sameLocation(rental: Rental): boolean {
    return rental.pickupLocation === rental.returnLocation;
  }

  setFilter(f: string): void {
    this.activeFilter = f;
  }

  retry(): void {
    this.loadBookings();
  }

  trackById = (_: number, r: Rental): number => r.id;

  // ── Stats ─────────────────────────────────────────────────────

  get totalBookings(): number {
    return this.rentals.length;
  }

  get activeCount(): number {
    return this.rentals.filter(r => r.status === 'Active').length;
  }

  get completedCount(): number {
    return this.rentals.filter(r => r.status === 'Completed').length;
  }

  get totalSpent(): number {
    return this.rentals
      .filter(r => r.status === 'Completed')
      .reduce((sum, r) => sum + r.totalCost, 0);
  }

  onImgError(event: Event): void {
  (event.target as HTMLImageElement).style.display = 'none';
}

// ── Export ────────────────────────────────────────────────────

exportCSV(): void {
  const headers = ['Reference', 'Vehicle', 'Category', 'Status', 'Pick-up Date', 'Return Date', 'Days', 'Pick-up Location', 'Return Location', 'Total (€)'];
  const rows = this.filteredRentals.map(r => [
    r.bookingReference,
    r.vehicleName,
    r.vehicleCategory,
    r.status,
    new Date(r.startDate).toLocaleDateString('en-GB'),
    new Date(r.endDate).toLocaleDateString('en-GB'),
    this.getRentalDays(r),
    r.pickupLocation,
    r.returnLocation,
    r.totalCost,
  ]);

  const csv = [headers, ...rows]
    .map(row => row.map(v => `"${v}"`).join(','))
    .join('\n');

  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `wheeldeal-bookings-${new Date().toISOString().split('T')[0]}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}

exportPDF(): void {
  const date = new Date().toLocaleDateString('en-GB');
  const rows = this.filteredRentals.map(r => `
    <tr>
      <td>${r.bookingReference}</td>
      <td>${r.vehicleName}</td>
      <td>${r.vehicleCategory}</td>
      <td><span class="status status--${r.status.toLowerCase()}">${r.status}</span></td>
      <td>${new Date(r.startDate).toLocaleDateString('en-GB')}</td>
      <td>${new Date(r.endDate).toLocaleDateString('en-GB')}</td>
      <td>${this.getRentalDays(r)}d</td>
      <td>${r.pickupLocation}</td>
      <td>€${r.totalCost}</td>
    </tr>
  `).join('');

  const html = `
    <!DOCTYPE html>
    <html>
    <head>
      <meta charset="utf-8">
      <title>WheelDeal — My Bookings</title>
      <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Segoe UI', sans-serif; padding: 40px; color: #0F172A; font-size: 13px; }
        .header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 32px; border-bottom: 2px solid #1A56DB; padding-bottom: 16px; }
        .header__logo { font-size: 22px; font-weight: 700; color: #1A56DB; letter-spacing: -0.5px; }
        .header__meta { text-align: right; color: #64748B; font-size: 12px; }
        .header__meta strong { display: block; color: #0F172A; font-size: 16px; margin-bottom: 4px; }
        .stats { display: flex; gap: 16px; margin-bottom: 28px; }
        .stat { flex: 1; background: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 8px; padding: 12px 16px; }
        .stat__val { font-size: 20px; font-weight: 700; color: #1A56DB; }
        .stat__label { font-size: 11px; color: #64748B; margin-top: 2px; }
        table { width: 100%; border-collapse: collapse; }
        thead tr { background: #1A56DB; color: white; }
        thead th { padding: 10px 12px; text-align: left; font-size: 11px; font-weight: 600; letter-spacing: 0.4px; text-transform: uppercase; }
        tbody tr { border-bottom: 1px solid #F1F5F9; }
        tbody tr:nth-child(even) { background: #F8FAFC; }
        tbody td { padding: 10px 12px; }
        .status { padding: 2px 8px; border-radius: 20px; font-size: 11px; font-weight: 600; }
        .status--active { background: #DCFCE7; color: #166534; }
        .status--completed { background: #DBEAFE; color: #1e40af; }
        .status--cancelled { background: #FEE2E2; color: #991B1B; }
        .footer { margin-top: 32px; text-align: center; font-size: 11px; color: #94A3B8; border-top: 1px solid #E2E8F0; padding-top: 16px; }
      </style>
    </head>
    <body>
      <div class="header">
        <div class="header__logo">WheelDeal</div>
        <div class="header__meta">
          <strong>Booking History</strong>
          Generated on ${date}
        </div>
      </div>
      <div class="stats">
        <div class="stat"><div class="stat__val">${this.totalBookings}</div><div class="stat__label">Total Rentals</div></div>
        <div class="stat"><div class="stat__val">${this.activeCount}</div><div class="stat__label">Active</div></div>
        <div class="stat"><div class="stat__val">${this.completedCount}</div><div class="stat__label">Completed</div></div>
        <div class="stat"><div class="stat__val">€${this.totalSpent}</div><div class="stat__label">Total Spent</div></div>
      </div>
      <table>
        <thead>
          <tr>
            <th>Reference</th><th>Vehicle</th><th>Category</th><th>Status</th>
            <th>Pick-up</th><th>Return</th><th>Days</th><th>Location</th><th>Total</th>
          </tr>
        </thead>
        <tbody>${rows}</tbody>
      </table>
      <div class="footer">WheelDeal Car Rental · wheeldeal.ro · Generated ${date}</div>
    </body>
    </html>
  `;

  const win = window.open('', '_blank');
  if (!win) return;
  win.document.write(html);
  win.document.close();
  win.focus();
  setTimeout(() => { win.print(); }, 500);
}

}