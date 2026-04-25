// ============================================================
// BOOKING / RENTAL MODELS
// ============================================================

export type RentalStatus = 'Active' | 'Completed' | 'Cancelled';
export type ProtectionPlan = 'basic' | 'standard' | 'premium';

export interface CreateBookingRequest {
  vehicleId: number;
  pickupDate: string;          // ISO date "yyyy-MM-dd"
  returnDate: string;
  pickupLocation: string;
  returnLocation: string;
  protectionPlan: ProtectionPlan;
  extras: string[];            // ex: ['gps', 'child_seat']
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  flightNumber?: string;
  notes?: string;
  payNow: boolean;
}

export interface BookingResponse {
  id: number;
  bookingReference: string;    // ex: "DN-AB12CD"
  vehicleId: number;
  clientId: number;
  branchId: number;
  startDate: string;
  endDate: string;
  totalCost: number;
  status: RentalStatus;
  createdAt: string;
}

export interface Rental {
  id: number;
  bookingReference: string;
  vehicleName: string;
  vehicleCategory: string;
  startDate: string;
  endDate: string;
  totalCost: number;
  status: RentalStatus;
  pickupLocation: string;
  returnLocation: string;
  createdAt: string;
}
