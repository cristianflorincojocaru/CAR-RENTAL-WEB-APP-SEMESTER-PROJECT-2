import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { Car, CarsComponent } from '../cars/cars';

interface Extra {
  id: string;
  icon: string;
  name: string;
  description: string;
  price: number;
  selected: boolean;
}

interface Protection {
  id: string;
  name: string;
  subtitle: string;
  features: string[];
  price: number;
  recommended?: boolean;
}

interface BookingForm {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  pickupDate: string;
  returnDate: string;
  pickupLocation: string;
  returnLocation: string;
  sameReturn: boolean;
  flightNumber: string;
  notes: string;
  payNow: boolean;
}

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './booking.html',
  styleUrls: ['./booking.scss']
})
export class BookingComponent implements OnInit {

  currentStep = 1;
  totalSteps = 4;
  isLoading = false;
  bookingConfirmed = false;
  bookingRef = '';

  car: Car | null = null;
  today: string = new Date().toISOString().split('T')[0];
  minReturn: string = '';
  rentalDays = 1;

  form: BookingForm = {
    firstName: '', lastName: '', email: '', phone: '',
    pickupDate: '', returnDate: '',
    pickupLocation: '', returnLocation: '',
    sameReturn: true, flightNumber: '', notes: '',
    payNow: true
  };

  formErrors: Partial<BookingForm & { dates: string }> = {};

  selectedProtection = 'basic';

  protections: Protection[] = [
    {
      id: 'basic',
      name: 'Basic Cover',
      subtitle: 'Minimum required by law',
      features: ['Third party liability', 'Theft protection (excess applies)', '€1,500 excess on damage'],
      price: 0
    },
    {
      id: 'standard',
      name: 'Standard Protection',
      subtitle: 'Most popular choice',
      features: ['All Basic benefits', 'Collision Damage Waiver', '€500 excess on damage', 'Windscreen & tyre cover'],
      price: 8,
      recommended: true
    },
    {
      id: 'premium',
      name: 'Premium Zero Excess',
      subtitle: 'Full peace of mind',
      features: ['All Standard benefits', 'Zero excess on all damage', 'Personal accident cover', '24/7 priority roadside'],
      price: 16
    }
  ];

  extras: Extra[] = [
    {
      id: 'gps', icon: '🗺️', name: 'GPS Navigation',
      description: 'Never get lost — latest maps pre-loaded',
      price: 4, selected: false
    },
    {
      id: 'child_seat', icon: '👶', name: 'Child Seat',
      description: 'EU-approved, suitable for 9–36 kg',
      price: 5, selected: false
    },
    {
      id: 'additional_driver', icon: '👥', name: 'Additional Driver',
      description: 'Share the driving, same full protection',
      price: 7, selected: false
    },
    {
      id: 'full_insurance', icon: '🛡️', name: 'Full Insurance / Zero Excess',
      description: 'Upgrade your protection to zero excess',
      price: 0, selected: false
    },
    {
      id: 'roadside', icon: '🔧', name: 'Roadside Assistance Plus',
      description: '24/7 priority roadside, towing included',
      price: 3, selected: false
    }
  ];

  locations = [
    'Henri Coandă Airport (OTP), Bucharest',
    'Otopeni — City Centre',
    'Craiova — Central',
    'Craiova — Airport',
    'Cluj-Napoca',
    'Timișoara',
    'Iași',
    'Constanța'
  ];

  constructor(private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    const source = new CarsComponent();
    this.car = source.allCars.find(c => c.id === id) || null;

    if (!this.car) {
      this.router.navigate(['/cars']);
      return;
    }

    // Pre-fill location from car branch
    this.form.pickupLocation = this.mapBranchToLocation(this.car.branch);
    this.form.returnLocation = this.form.pickupLocation;
  }

  private mapBranchToLocation(branch: string): string {
    if (branch.includes('Otopeni') || branch.includes('Bucharest')) return 'Henri Coandă Airport (OTP), Bucharest';
    if (branch.includes('Craiova — Airport')) return 'Craiova — Airport';
    if (branch.includes('Craiova')) return 'Craiova — Central';
    if (branch.includes('Timișoara')) return 'Timișoara';
    return branch;
  }

  get selectedProtectionData(): Protection {
    return this.protections.find(p => p.id === this.selectedProtection)!;
  }

  get discountedBasePrice(): number {
    if (!this.car) return 0;
    if (this.car.isOffer && this.car.discountPercent) {
      return Math.round(this.car.price * (1 - this.car.discountPercent / 100));
    }
    return this.car.price;
  }

  get extrasTotal(): number {
    return this.extras.filter(e => e.selected).reduce((sum, e) => sum + e.price, 0);
  }

  get protectionPrice(): number {
    return this.selectedProtectionData.price;
  }

  get dailyTotal(): number {
    return this.discountedBasePrice + this.extrasTotal + this.protectionPrice;
  }

  get grandTotal(): number {
    return this.dailyTotal * Math.max(this.rentalDays, 1);
  }

  onPickupDateChange(): void {
    if (!this.form.pickupDate) { this.minReturn = ''; return; }
    const d = new Date(this.form.pickupDate);
    d.setDate(d.getDate() + 1);
    this.minReturn = d.toISOString().split('T')[0];
    if (this.form.returnDate && this.form.returnDate <= this.form.pickupDate) {
      this.form.returnDate = '';
    }
    this.calcDays();
  }

  onReturnDateChange(): void { this.calcDays(); }

  private calcDays(): void {
    if (!this.form.pickupDate || !this.form.returnDate) { this.rentalDays = 1; return; }
    const diff = new Date(this.form.returnDate).getTime() - new Date(this.form.pickupDate).getTime();
    this.rentalDays = Math.max(1, Math.round(diff / 86400000));
  }

  onSameReturnChange(): void {
    if (this.form.sameReturn) {
      this.form.returnLocation = this.form.pickupLocation;
    }
  }

  toggleExtra(extra: Extra): void {
    // If full insurance selected, deselect roadside (already included)
    if (extra.id === 'full_insurance' && !extra.selected) {
      const roadside = this.extras.find(e => e.id === 'roadside');
      if (roadside) roadside.selected = false;
    }
    extra.selected = !extra.selected;
  }

  // ── Navigation ──────────────────────────────────────────────
  nextStep(): void {
    if (!this.validateStep()) return;
    if (this.currentStep < this.totalSteps) this.currentStep++;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  prevStep(): void {
    if (this.currentStep > 1) { this.currentStep--; window.scrollTo({ top: 0, behavior: 'smooth' }); }
  }

  goToStep(n: number): void {
    if (n < this.currentStep) { this.currentStep = n; window.scrollTo({ top: 0, behavior: 'smooth' }); }
  }

  private validateStep(): boolean {
    this.formErrors = {};
    if (this.currentStep === 1) {
      if (!this.form.pickupDate) { this.formErrors.pickupDate = 'Please select a pick-up date.'; }
      if (!this.form.returnDate) { this.formErrors.returnDate = 'Please select a return date.'; }
      if (!this.form.pickupLocation) { this.formErrors.pickupLocation = 'Please select a pick-up location.'; }
      if (!this.form.sameReturn && !this.form.returnLocation) { this.formErrors.returnLocation = 'Please select a return location.'; }
    }
    if (this.currentStep === 3) {
      const emailRx = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!this.form.firstName.trim()) this.formErrors.firstName = 'Required';
      if (!this.form.lastName.trim()) this.formErrors.lastName = 'Required';
      if (!this.form.email || !emailRx.test(this.form.email)) this.formErrors.email = 'Valid email required';
      if (!this.form.phone.trim()) this.formErrors.phone = 'Required';
    }
    return Object.keys(this.formErrors).length === 0;
  }

  confirmBooking(): void {
    if (!this.validateStep()) return;
    this.isLoading = true;
    setTimeout(() => {
      this.isLoading = false;
      this.bookingRef = 'DN-' + Math.random().toString(36).substring(2, 8).toUpperCase();
      this.currentStep = 4;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }, 1800);
  }

  get stepLabel(): string {
    const labels = ['Vehicle & Dates', 'Protection', 'Your Details', 'Confirmation'];
    return labels[this.currentStep - 1];
  }
}