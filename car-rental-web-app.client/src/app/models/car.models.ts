// ============================================================
// VEHICLE / CAR MODELS — aliniate cu VehicleDto din backend
// ============================================================

export type VehicleCategory = 'Economy' | 'Compact' | 'SUV' | 'Premium' | 'Van';
export type VehicleStatus   = 'Available' | 'Rented' | 'Maintenance';

export interface CarSpec {
  icon: string;
  value: string;
}

/** Model folosit în frontend (primit de la API) */
export interface Car {
  id: number;
  name: string;
  brand: string;
  model: string;
  year: number;
  fuelType?: string;
  fuel?: string;
  category: VehicleCategory;
  branch: string;
  branchId: number;
  registrationNumber: string;
  dailyRate: number;
  transmission?: string;
  seats?: number;
  status: VehicleStatus;
  rating?: number;
  colorHex?: string;
  color?: string;
  imageUrl?: string;          // ← adăugat
  isOffer: boolean;
  discountPercent?: number;
  isActive: boolean;
  specs: CarSpec[];
  isFavorite: boolean;
}

export interface CarFilters {
  branch?: string;
  category?: string;
  pickupDate?: string;
  returnDate?: string;
  transmission?: string;
  isOffer?: boolean;
}

/** Răspuns paginat (opțional, pentru viitor) */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}