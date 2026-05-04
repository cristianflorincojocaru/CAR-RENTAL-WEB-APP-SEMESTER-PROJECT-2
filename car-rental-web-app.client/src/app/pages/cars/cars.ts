import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';

import { CarService } from '../../services/car.service';
import { Car, CarFilters } from '../../models/car.models';

// Re-exportăm interfețele pentru compatibilitate cu BookingComponent
export type { Car } from '../../models/car.models';

@Component({
  selector: 'app-cars',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './cars.html',
  styleUrls: ['./cars.scss']
})
export class CarsComponent implements OnInit {

  // ── State ────────────────────────────────────────────────────

  branches = [
    { key: 'All',                   label: 'All Branches' },
    { key: 'Bucharest — Central',   label: 'Bucharest — Central' },
    { key: 'Bucharest — Otopeni',   label: 'Bucharest — Otopeni' },
    { key: 'Cluj — Airport',        label: 'Cluj — Airport' },
    { key: 'Timișoara — Airport',   label: 'Timișoara — Airport' },
  ];
  activeBranch = 'All';

  categories = ['All', 'Economy', 'Compact', 'SUV', 'Premium', 'Van'];
  activeCategory = 'All';

  sortOptions = [
    { value: 'price-asc',  label: 'Price: Low to High' },
    { value: 'price-desc', label: 'Price: High to Low' },
    { value: 'rating',     label: 'Best Rated' },
    { value: 'name',       label: 'Name A–Z' },
  ];
  activeSort = 'price-asc';

  allCars: Car[] = [];
  isLoading = false;
  loadError = false;

  // ── Search params venite de la Home ──────────────────────────
  fromSearch = false;
  searchPickupDate   = '';
  searchReturnDate   = '';
  searchLocation     = '';
  searchTransmission = '';

  private locationToBranch: Record<string, string> = {
    'Bucharest — Central':           'Bucharest — Central',
    'Bucharest — Otopeni (Henri Coandă Airport)': 'Bucharest — Otopeni',
    'Henri Coandă Airport':          'Bucharest — Otopeni',
    'Otopeni Airport':               'Bucharest — Otopeni',
    'Cluj — Airport':                'Cluj — Airport',
    'Timișoara — Airport':           'Timișoara — Airport',
  };

  constructor(
    private carService: CarService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.fromSearch         = params['fromSearch'] === '1';
      this.searchPickupDate   = params['pickupDate']    || '';
      this.searchReturnDate   = params['returnDate']    || '';
      this.searchLocation     = params['location']      || '';
      this.searchTransmission = params['transmission']  || '';

      if (params['category'] && params['category'] !== '') {
        this.activeCategory = params['category'];
      }

      if (params['location']) {
        const mapped = this.locationToBranch[params['location']];
        if (mapped) this.activeBranch = mapped;
      }

      this.loadCars();
    });
  }

  // ── Data loading ──────────────────────────────────────────────

  loadCars(): void {
    this.isLoading = true;
    this.loadError = false;

    const filters: CarFilters = {
      branch:       this.activeBranch !== 'All' ? this.activeBranch : undefined,
      category:     this.activeCategory !== 'All' ? this.activeCategory : undefined,
      pickupDate:   this.searchPickupDate  || undefined,
      returnDate:   this.searchReturnDate  || undefined,
      transmission: this.searchTransmission || undefined,
    };

    this.carService.getAll(filters).subscribe({
      next: cars => {
        // isFavorite este stare locală — o păstrăm între refresh-uri
        this.allCars = cars.map(car => ({
          ...car,
          isFavorite: this.getFavoriteState(car.id)
        }));
        this.isLoading = false;
      },
      error: () => {
        this.loadError = true;
        this.isLoading = false;
      }
    });
  }

  // ── Filtrare & sortare locală ─────────────────────────────────

  get filteredCars(): Car[] {
    let cars = [...this.allCars];

    // Filtru transmisie (silent, din search)
    if (this.fromSearch && this.searchTransmission) {
      const tx = this.searchTransmission.toLowerCase();
      cars = cars.filter(c => c.specs.some(s => s.value.toLowerCase() === tx));
    }

    switch (this.activeSort) {
      case 'price-asc':
        cars.sort((a, b) => this.getDiscountedPrice(a) - this.getDiscountedPrice(b));
        break;
      case 'price-desc':
        cars.sort((a, b) => this.getDiscountedPrice(b) - this.getDiscountedPrice(a));
        break;
      case 'rating':
        cars.sort((a, b) => b.rating - a.rating);
        break;
      case 'name':
        cars.sort((a, b) => a.name.localeCompare(b.name));
        break;
    }

    return cars;
  }

  get offerCount(): number {
    return this.filteredCars.filter(c => c.isOffer).length;
  }

  getDiscountedPrice(car: Car): number {
    if (car.isOffer && car.discountPercent) {
      return Math.round(car.dailyRate * (1 - car.discountPercent / 100));
    }
    return car.dailyRate;
  }

  // ── Acțiuni utilizator ────────────────────────────────────────

  setBranch(key: string): void {
    this.activeBranch = key;
    this.loadCars();
  }

  setCategory(cat: string): void {
    this.activeCategory = cat;
    this.loadCars();
  }

  toggleFavorite(car: Car): void {
    car.isFavorite = !car.isFavorite;
    this.saveFavoriteState(car.id, car.isFavorite);
  }

  clearSearch(): void {
    this.fromSearch         = false;
    this.searchPickupDate   = '';
    this.searchReturnDate   = '';
    this.searchLocation     = '';
    this.searchTransmission = '';
    this.activeCategory     = 'All';
    this.activeBranch       = 'All';
    this.router.navigate([], { queryParams: {} });
    this.loadCars();
  }

  /** Construiește query params pentru link-ul Book Now */
  bookingParams(car: Car): Record<string, string> {
    const p: Record<string, string> = {};
    if (this.fromSearch) {
      if (this.searchPickupDate)  p['pickupDate']  = this.searchPickupDate;
      if (this.searchReturnDate)  p['returnDate']  = this.searchReturnDate;
      if (this.searchLocation)    p['location']    = this.searchLocation;
      p['locationLocked'] = '1';
    }
    return p;
  }

  trackById = (_: number, car: Car): number => car.id;

  // ── Persistare favorite în localStorage ──────────────────────

  private getFavoriteState(carId: number): boolean {
    try {
      const raw = localStorage.getItem('wd_favorites');
      const ids: number[] = raw ? JSON.parse(raw) : [];
      return ids.includes(carId);
    } catch { return false; }
  }

  private saveFavoriteState(carId: number, isFavorite: boolean): void {
    try {
      const raw = localStorage.getItem('wd_favorites');
      let ids: number[] = raw ? JSON.parse(raw) : [];
      if (isFavorite) {
        if (!ids.includes(carId)) ids.push(carId);
      } else {
        ids = ids.filter(id => id !== carId);
      }
      localStorage.setItem('wd_favorites', JSON.stringify(ids));
    } catch { /* ignorăm */ }
  }
}
