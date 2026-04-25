import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { CarService } from '../../services/car.service';
import { Car } from '../../models/car.models';

@Component({
  selector: 'app-offers',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './offers.html',
  styleUrls: ['./offers.scss']
})
export class OffersComponent implements OnInit {

  // ── State ─────────────────────────────────────────────────────

  offerCars: Car[] = [];
  isLoading = false;
  loadError = false;
  totalOffersCount = 0;

  categories = ['All', 'Economy', 'Compact', 'SUV', 'Premium'];
  activeCategory = 'All';

  sortOptions = [
    { value: 'discount-desc', label: 'Biggest Discount' },
    { value: 'price-asc',     label: 'Price: Low to High' },
    { value: 'price-desc',    label: 'Price: High to Low' },
    { value: 'rating',        label: 'Best Rated' },
  ];
  activeSort = 'discount-desc';

  promos = [
    { code: 'SUMMER30', desc: '30% off all weekend rentals', expires: 'Valid until 31 Aug 2026' },
    { code: 'FLEET10',  desc: '10% off all Premium models',  expires: 'Valid until 30 Jun 2026' },
    { code: 'DRIVE25',  desc: '25% off Dacia Duster 4x4',    expires: 'Valid until 15 Jul 2026' },
  ];
  copiedCode: string | null = null;

  constructor(private carService: CarService) {}

  ngOnInit(): void {
    this.loadOffers();
    this.loadTotalCount();
  }

  // ── Data loading ──────────────────────────────────────────────

  private loadOffers(): void {
    this.isLoading = true;
    this.loadError = false;

    this.carService.getOffers(this.activeCategory).subscribe({
      next: cars => {
        this.offerCars = this.sortCars(cars);
        this.isLoading = false;
      },
      error: () => {
        this.loadError = true;
        this.isLoading = false;
      }
    });
  }

  private loadTotalCount(): void {
    this.carService.getOffers().subscribe({
      next: cars => this.totalOffersCount = cars.length,
      error: () => { /* ignorăm */ }
    });
  }

  // ── Sortare locală ────────────────────────────────────────────

  private sortCars(cars: Car[]): Car[] {
    const sorted = [...cars];
    switch (this.activeSort) {
      case 'discount-desc':
        sorted.sort((a, b) => (b.discountPercent ?? 0) - (a.discountPercent ?? 0));
        break;
      case 'price-asc':
        sorted.sort((a, b) => this.discounted(a) - this.discounted(b));
        break;
      case 'price-desc':
        sorted.sort((a, b) => this.discounted(b) - this.discounted(a));
        break;
      case 'rating':
        sorted.sort((a, b) => b.rating - a.rating);
        break;
    }
    return sorted;
  }

  // ── Helpers preț ──────────────────────────────────────────────

  discounted(car: Car): number {
    return car.discountPercent
      ? Math.round(car.dailyRate * (1 - car.discountPercent / 100))
      : car.dailyRate;
  }

  savings(car: Car): number {
    return car.dailyRate - this.discounted(car);
  }

  // ── Acțiuni ───────────────────────────────────────────────────

  setCategory(cat: string): void {
    this.activeCategory = cat;
    this.loadOffers();
  }

  onSortChange(): void {
    this.offerCars = this.sortCars(this.offerCars);
  }

  copyCode(code: string): void {
    navigator.clipboard.writeText(code).then(() => {
      this.copiedCode = code;
      setTimeout(() => { this.copiedCode = null; }, 2000);
    });
  }

  trackById = (_: number, car: Car): number => car.id;
}
