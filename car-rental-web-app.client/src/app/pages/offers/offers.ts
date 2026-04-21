import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Car, CarsComponent } from '../cars/cars';   // re-use the interface & data


@Component({
  selector: 'app-offers',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './offers.html',
  styleUrls: ['./offers.scss']
})
export class OffersComponent implements OnInit {

  // ── All cars (sourced from CarsComponent) ───────────────────
  // !!! DANGEROUS !!! 
  // Temporary instance to access the cars data 
  // But avoids circular dependency for now; ideally we'd refactor to a shared service for the car data
  private readonly source = new CarsComponent({} as ActivatedRoute, {} as Router);

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
    { code: 'FLEET10',  desc: '10% off all Premium models',   expires: 'Valid until 30 Jun 2026' },
    { code: 'DRIVE25',  desc: '25% off Dacia Duster 4x4',     expires: 'Valid until 15 Jul 2026' },
  ];
  copiedCode: string | null = null;

  get offerCars(): Car[] {
    let cars = this.source.allCars.filter(c => c.isOffer);

    if (this.activeCategory !== 'All') {
      cars = cars.filter(c => c.category === this.activeCategory);
    }

    switch (this.activeSort) {
      case 'discount-desc':
        cars.sort((a, b) => (b.discountPercent ?? 0) - (a.discountPercent ?? 0));
        break;
      case 'price-asc':
        cars.sort((a, b) => this.discounted(a) - this.discounted(b));
        break;
      case 'price-desc':
        cars.sort((a, b) => this.discounted(b) - this.discounted(a));
        break;
      case 'rating':
        cars.sort((a, b) => b.rating - a.rating);
        break;
    }
    return cars;
  }

  get totalOffersCount(): number {
    return this.source.allCars.filter(c => c.isOffer).length;
  }

  discounted(car: Car): number {
    return car.discountPercent
      ? Math.round(car.price * (1 - car.discountPercent / 100))
      : car.price;
  }

  savings(car: Car): number {
    return car.price - this.discounted(car);
  }

  setCategory(cat: string): void { this.activeCategory = cat; }

  copyCode(code: string): void {
    navigator.clipboard.writeText(code).then(() => {
      this.copiedCode = code;
      setTimeout(() => { this.copiedCode = null; }, 2000);
    });
  }

  trackById = (_: number, car: Car): number => car.id;

  ngOnInit(): void {}
}
