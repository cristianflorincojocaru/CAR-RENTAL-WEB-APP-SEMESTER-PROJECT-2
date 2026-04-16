import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

export interface CarSpec {
  icon: string;
  value: string;
}

export interface Car {
  id: number;
  name: string;
  year: number;
  fuel: string;
  category: string;
  branch: string;
  price: number;
  rating: number;
  color: string;
  isFavorite: boolean;
  isOffer: boolean;
  discountPercent?: number;
  specs: CarSpec[];
}

@Component({
  selector: 'app-cars',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './cars.html',
  styleUrls: ['./cars.scss']
})
export class CarsComponent implements OnInit {

  branches = [
    { key: 'All',                   label: 'All Branches', icon: 'globe' },
    { key: 'Craiova — Central',     label: 'Craiova Central', icon: 'pin' },
    { key: 'Craiova — Airport',     label: 'Craiova Airport', icon: 'pin' },
    { key: 'Bucharest — Otopeni',   label: 'Bucharest', icon: 'pin' },
    { key: 'Timișoara',             label: 'Timișoara', icon: 'pin' },
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

  allCars: Car[] = [
    // ── Craiova Central ──────────────────────────────────────
    {
      id: 1, name: 'Dacia Logan', year: 2023, fuel: 'Petrol',
      category: 'Economy', branch: 'Craiova — Central',
      price: 19, rating: 4.8, color: '#60A5FA',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚙️', value: 'Manual' }, { icon: '👥', value: '5 seats' }, { icon: '❄️', value: 'A/C' }]
    },
    {
      id: 2, name: 'VW Golf 8', year: 2023, fuel: 'Diesel',
      category: 'Compact', branch: 'Craiova — Central',
      price: 32, rating: 4.9, color: '#1A56DB',
      isFavorite: false, isOffer: true, discountPercent: 20,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '5 seats' }, { icon: '❄️', value: 'A/C' }]
    },
    {
      id: 3, name: 'BMW X3', year: 2024, fuel: 'Hybrid',
      category: 'SUV', branch: 'Craiova — Central',
      price: 65, rating: 4.9, color: '#1340B0',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '5 seats' }, { icon: '🌱', value: 'Hybrid' }]
    },
    {
      id: 4, name: 'Toyota Corolla', year: 2023, fuel: 'Hybrid',
      category: 'Compact', branch: 'Craiova — Central',
      price: 28, rating: 4.7, color: '#3B82F6',
      isFavorite: false, isOffer: true, discountPercent: 15,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '5 seats' }, { icon: '🌱', value: 'Hybrid' }]
    },
    {
      id: 5, name: 'Ford Transit', year: 2022, fuel: 'Diesel',
      category: 'Van', branch: 'Craiova — Central',
      price: 55, rating: 4.6, color: '#2563EB',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚙️', value: 'Manual' }, { icon: '👥', value: '9 seats' }, { icon: '🧳', value: 'XL boot' }]
    },

    // ── Craiova Airport ───────────────────────────────────────
    {
      id: 6, name: 'Skoda Octavia', year: 2023, fuel: 'Diesel',
      category: 'Compact', branch: 'Craiova — Airport',
      price: 30, rating: 4.7, color: '#3B82F6',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚙️', value: 'Manual' }, { icon: '👥', value: '5 seats' }, { icon: '🧳', value: 'Large boot' }]
    },
    {
      id: 7, name: 'Dacia Duster 4x4', year: 2024, fuel: 'Petrol',
      category: 'SUV', branch: 'Craiova — Airport',
      price: 39, rating: 4.8, color: '#1D4ED8',
      isFavorite: false, isOffer: true, discountPercent: 25,
      specs: [{ icon: '⚙️', value: 'Manual' }, { icon: '👥', value: '5 seats' }, { icon: '🏔️', value: '4x4' }]
    },
    {
      id: 8, name: 'Renault Clio', year: 2023, fuel: 'Petrol',
      category: 'Economy', branch: 'Craiova — Airport',
      price: 22, rating: 4.6, color: '#60A5FA',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚙️', value: 'Manual' }, { icon: '👥', value: '5 seats' }, { icon: '❄️', value: 'A/C' }]
    },
    {
      id: 9, name: 'Mercedes C220', year: 2024, fuel: 'Diesel',
      category: 'Premium', branch: 'Craiova — Airport',
      price: 85, rating: 5.0, color: '#0F172A',
      isFavorite: false, isOffer: true, discountPercent: 10,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '5 seats' }, { icon: '⭐', value: 'Premium' }]
    },
    {
      id: 10, name: 'VW Passat', year: 2023, fuel: 'Diesel',
      category: 'Compact', branch: 'Craiova — Airport',
      price: 35, rating: 4.8, color: '#1A56DB',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '5 seats' }, { icon: '🧳', value: 'Large boot' }]
    },

    // ── Bucharest Otopeni ─────────────────────────────────────
    {
      id: 11, name: 'BMW 5 Series', year: 2024, fuel: 'Diesel',
      category: 'Premium', branch: 'Bucharest — Otopeni',
      price: 95, rating: 4.9, color: '#0F172A',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '5 seats' }, { icon: '⭐', value: 'Premium' }]
    },
    {
      id: 12, name: 'Audi Q5', year: 2024, fuel: 'Hybrid',
      category: 'SUV', branch: 'Bucharest — Otopeni',
      price: 75, rating: 4.9, color: '#1D4ED8',
      isFavorite: false, isOffer: true, discountPercent: 20,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '5 seats' }, { icon: '🌱', value: 'Hybrid' }]
    },
    {
      id: 13, name: 'Toyota RAV4', year: 2024, fuel: 'Hybrid',
      category: 'SUV', branch: 'Bucharest — Otopeni',
      price: 70, rating: 4.8, color: '#2563EB',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '5 seats' }, { icon: '🌱', value: 'Hybrid' }]
    },
    {
      id: 14, name: 'Ford Focus', year: 2023, fuel: 'Petrol',
      category: 'Compact', branch: 'Bucharest — Otopeni',
      price: 26, rating: 4.7, color: '#3B82F6',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚙️', value: 'Manual' }, { icon: '👥', value: '5 seats' }, { icon: '❄️', value: 'A/C' }]
    },
    {
      id: 15, name: 'Mercedes GLE', year: 2024, fuel: 'Diesel',
      category: 'SUV', branch: 'Bucharest — Otopeni',
      price: 110, rating: 5.0, color: '#0F172A',
      isFavorite: false, isOffer: true, discountPercent: 15,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '7 seats' }, { icon: '⭐', value: 'Premium' }]
    },
    {
      id: 16, name: 'Peugeot 308', year: 2023, fuel: 'Petrol',
      category: 'Compact', branch: 'Bucharest — Otopeni',
      price: 24, rating: 4.6, color: '#60A5FA',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚙️', value: 'Manual' }, { icon: '👥', value: '5 seats' }, { icon: '❄️', value: 'A/C' }]
    },
    {
      id: 17, name: 'Tesla Model 3', year: 2024, fuel: 'Electric',
      category: 'Premium', branch: 'Bucharest — Otopeni',
      price: 90, rating: 4.9, color: '#1A56DB',
      isFavorite: false, isOffer: true, discountPercent: 30,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '5 seats' }, { icon: '🔋', value: 'Electric' }]
    },

    // ── Timișoara ─────────────────────────────────────────────
    {
      id: 18, name: 'Hyundai Tucson', year: 2024, fuel: 'Petrol',
      category: 'SUV', branch: 'Timișoara',
      price: 45, rating: 4.7, color: '#2563EB',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '5 seats' }, { icon: '❄️', value: 'A/C' }]
    },
    {
      id: 19, name: 'Seat Leon', year: 2023, fuel: 'Petrol',
      category: 'Compact', branch: 'Timișoara',
      price: 27, rating: 4.6, color: '#3B82F6',
      isFavorite: false, isOffer: false,
      specs: [{ icon: '⚙️', value: 'Manual' }, { icon: '👥', value: '5 seats' }, { icon: '❄️', value: 'A/C' }]
    },
    {
      id: 20, name: 'Kia Sportage', year: 2024, fuel: 'Hybrid',
      category: 'SUV', branch: 'Timișoara',
      price: 48, rating: 4.8, color: '#1D4ED8',
      isFavorite: false, isOffer: true, discountPercent: 20,
      specs: [{ icon: '⚡', value: 'Automatic' }, { icon: '👥', value: '5 seats' }, { icon: '🌱', value: 'Hybrid' }]
    },
  ];

  get filteredCars(): Car[] {
    let cars = [...this.allCars];

    if (this.activeBranch !== 'All') {
      cars = cars.filter(c => c.branch === this.activeBranch);
    }
    if (this.activeCategory !== 'All') {
      cars = cars.filter(c => c.category === this.activeCategory);
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
      return Math.round(car.price * (1 - car.discountPercent / 100));
    }
    return car.price;
  }

  ngOnInit(): void {}

  setBranch(key: string): void {
    this.activeBranch = key;
  }

  setCategory(cat: string): void {
    this.activeCategory = cat;
  }

  toggleFavorite(car: Car): void {
    car.isFavorite = !car.isFavorite;
  }

  trackById = (_: number, car: Car): number => car.id;
}
