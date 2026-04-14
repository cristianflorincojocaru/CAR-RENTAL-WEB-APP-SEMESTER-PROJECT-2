import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

interface Car {
  id: number;
  name: string;
  year: number;
  fuel: string;
  category: string;
  price: number;
  rating: number;
  color: string;
  isFavorite: boolean;
  specs: { icon: string; value: string }[];
}

interface Step {
  icon: string;
  title: string;
  description: string;
}

interface Testimonial {
  name: string;
  date: string;
  text: string;
  car: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './home.html',
  styleUrls: ['./home.scss']
})
export class HomeComponent implements OnInit {

  // --- Banner ---
  bannerVisible = true;

  // --- Hero Search ---
  activeTab: 'short' | 'long' = 'short';
today: string = new Date().toISOString().split('T')[0];
maxReturnDate: string = '';

  searchForm = {
    location:     '',
    pickupDate:   '',
    returnDate:   '',
    category:     '',
    transmission: ''
  };

  trustItems = [
    { icon: '✅', text: 'no hidden fees' },
    { icon: '🚚', text: 'hotel delivery' },
    { icon: '📞', text: '24/7 support' },
    { icon: '🔒', text: 'secure payment' }
  ];

  // --- Cars ---
  carCategories = ['All', 'Economy', 'Compact', 'SUV', 'Premium'];
  activeCategory = 'All';

  allCars: Car[] = [
    {
      id: 1, name: 'Dacia Logan',   year: 2023, fuel: 'Petrol',
      category: 'Economy', price: 19, rating: 4.8, color: '#60A5FA', isFavorite: false,
      specs: [
        { icon: '⚡', value: 'Manual' },
        { icon: '👥', value: '5 seats' },
        { icon: '❄️',  value: 'A/C' }
      ]
    },
    {
      id: 2, name: 'VW Golf 8',     year: 2023, fuel: 'Diesel',
      category: 'Compact', price: 32, rating: 4.9, color: '#1A56DB', isFavorite: false,
      specs: [
        { icon: '⚡', value: 'Automatic' },
        { icon: '👥', value: '5 seats' },
        { icon: '❄️',  value: 'A/C' }
      ]
    },
    {
      id: 3, name: 'BMW X3',        year: 2024, fuel: 'Hybrid',
      category: 'SUV', price: 65, rating: 4.9, color: '#1340B0', isFavorite: false,
      specs: [
        { icon: '⚡', value: 'Automatic' },
        { icon: '👥', value: '5 seats' },
        { icon: '🌱', value: 'Hybrid' }
      ]
    },
    {
      id: 4, name: 'Mercedes C220', year: 2024, fuel: 'Diesel',
      category: 'Premium', price: 85, rating: 5.0, color: '#0F172A', isFavorite: false,
      specs: [
        { icon: '⚡', value: 'Automatic' },
        { icon: '👥', value: '5 seats' },
        { icon: '⭐', value: 'Premium' }
      ]
    },
    {
      id: 5, name: 'Skoda Octavia', year: 2023, fuel: 'Diesel',
      category: 'Compact', price: 28, rating: 4.7, color: '#3B82F6', isFavorite: false,
      specs: [
        { icon: '⚡', value: 'Manual' },
        { icon: '👥', value: '5 seats' },
        { icon: '🧳', value: 'Large boot' }
      ]
    },
    {
      id: 6, name: 'Duster 4x4',   year: 2024, fuel: 'Petrol',
      category: 'SUV', price: 39, rating: 4.8, color: '#1D4ED8', isFavorite: false,
      specs: [
        { icon: '⚡', value: 'Manual' },
        { icon: '👥', value: '5 seats' },
        { icon: '🏔️', value: '4x4' }
      ]
    }
  ];

  get filteredCars(): Car[] {
    if (this.activeCategory === 'All') return this.allCars;
    return this.allCars.filter(c => c.category === this.activeCategory);
  }

  // --- Steps ---
  steps: Step[] = [
    {
      icon: `<svg width="32" height="32" viewBox="0 0 32 32" fill="none">
               <circle cx="16" cy="16" r="14" fill="#EFF6FF"/>
               <circle cx="12" cy="13" r="5" stroke="#1A56DB" stroke-width="1.8"/>
               <path d="M8 27a8 8 0 0116 0" stroke="#1A56DB" stroke-width="1.8"
                     stroke-linecap="round"/>
             </svg>`,
      title: 'Create an account',
      description: 'Quick sign-up with email or Google. Takes less than a minute.'
    },
    {
      icon: `<svg width="32" height="32" viewBox="0 0 32 32" fill="none">
               <circle cx="16" cy="16" r="14" fill="#EFF6FF"/>
               <rect x="8" y="9" width="16" height="14" rx="3" stroke="#1A56DB" stroke-width="1.8"/>
               <path d="M11 7v4M21 7v4M8 15h16" stroke="#1A56DB" stroke-width="1.8"
                     stroke-linecap="round"/>
             </svg>`,
      title: 'Choose your dates',
      description: 'Select pick-up and return dates, location and the car category you need.'
    },
    {
      icon: `<svg width="32" height="32" viewBox="0 0 32 32" fill="none">
               <circle cx="16" cy="16" r="14" fill="#EFF6FF"/>
               <path d="M8 17L8 20Q8 24 12 24L20 24Q24 24 24 20L24 17L22 13Q21 10 18 10L14 10Q11 10 10 13Z"
                     stroke="#1A56DB" stroke-width="1.8" stroke-linejoin="round"/>
               <circle cx="12" cy="20" r="2" fill="#1A56DB"/>
               <circle cx="20" cy="20" r="2" fill="#1A56DB"/>
             </svg>`,
      title: 'Book your car',
      description: 'Secure online payment by card or pay on pick-up. Instant confirmation by email.'
    },
    {
      icon: `<svg width="32" height="32" viewBox="0 0 32 32" fill="none">
               <circle cx="16" cy="16" r="14" fill="#EFF6FF"/>
               <path d="M10 22 Q16 18 22 22" stroke="#1A56DB" stroke-width="1.8"
                     stroke-linecap="round"/>
               <circle cx="16" cy="13" r="4" stroke="#1A56DB" stroke-width="1.8"/>
             </svg>`,
      title: 'Pick up & drive!',
      description: 'We meet you at the chosen location, hand over the keys and you are on your way in 5 minutes.'
    }
  ];

  // --- Testimonials ---
  ratingBars = [
    { stars: 5, percent: 82 },
    { stars: 4, percent: 12 },
    { stars: 3, percent: 4 },
    { stars: 2, percent: 1 },
    { stars: 1, percent: 1 }
  ];

  testimonials: Testimonial[] = [
    {
      name: 'Andrew P.',
      date: 'March 2025',
      text: 'Flawless service! The car was clean, full of fuel and all documents in order. Will definitely come back.',
      car: 'VW Golf 8'
    },
    {
      name: 'Maria I.',
      date: 'February 2025',
      text: 'Booked for the weekend and everything went perfectly. The price was exactly as shown — no surprises at checkout.',
      car: 'Dacia Duster 4x4'
    },
    {
      name: 'Chris D.',
      date: 'January 2025',
      text: 'BMW X3 in perfect condition. Hotel delivery was a huge bonus. Highly recommended!',
      car: 'BMW X3 Hybrid'
    }
  ];

  ngOnInit(): void {}

  closeBanner(): void {
    this.bannerVisible = false;
  }

  setTab(tab: 'short' | 'long'): void {
    this.activeTab = tab;
  }

  onSearch(): void {
    console.log('Search:', this.searchForm);
  }

  setCategory(cat: string): void {
    this.activeCategory = cat;
  }

 trackByCat = (index: number, car: any): string => {
  return this.activeCategory + '_' + (car.id ?? car.name);
};

  toggleFavorite(car: Car): void {
    car.isFavorite = !car.isFavorite;
  }

  onPickupDateChange() {
  if (!this.searchForm.pickupDate) {
    this.maxReturnDate = '';
    return;
  }

  const pickup = new Date(this.searchForm.pickupDate);

  if (this.activeTab === 'short') {
    // max 7 zile
    pickup.setDate(pickup.getDate() + 7);
  } else {
    // max 30 zile
    pickup.setDate(pickup.getDate() + 30);
  }

  this.maxReturnDate = pickup.toISOString().split('T')[0];

  // dacă return date depășește noul max, resetează
  if (this.searchForm.returnDate > this.maxReturnDate) {
    this.searchForm.returnDate = '';
  }

  
}
}