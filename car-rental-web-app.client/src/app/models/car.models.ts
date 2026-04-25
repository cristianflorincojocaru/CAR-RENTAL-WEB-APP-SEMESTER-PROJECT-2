// ============================================================
// VEHICLE / CAR MODELS
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
  name: string;             // brand + model concatenat (ex: "VW Golf 8")
  brand: string;
  model: string;
  year: number;
  fuel: string;
  category: VehicleCategory;
  branch: string;           // numele branch-ului (ex: "Craiova — Central")
  branchId: number;
  registrationNumber: string;
  dailyRate: number;        // prețul original
  status: VehicleStatus;
  rating: number;
  color: string;            // hex, folosit în SVG
  isOffer: boolean;
  discountPercent?: number;
  isFavorite: boolean;      // stare locală, nu vine de la API
  specs: CarSpec[];
}

export interface CarFilters {
  branch?: string;
  category?: string;
  pickupDate?: string;      // ISO date string
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
