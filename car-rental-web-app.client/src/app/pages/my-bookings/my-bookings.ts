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
}