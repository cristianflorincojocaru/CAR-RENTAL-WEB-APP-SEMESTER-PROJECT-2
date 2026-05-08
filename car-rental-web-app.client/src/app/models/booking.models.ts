// ============================================================
// BOOKING / RENTAL MODELS — aliniate cu RentalDtos din backend
// ============================================================

export type RentalStatus  = 'Active' | 'Completed' | 'Cancelled';
export type ProtectionPlan = 'basic' | 'standard' | 'premium';

export interface CreateBookingRequest {
  vehicleId: number;
  pickupDate: string;          // ISO date "yyyy-MM-dd"
  returnDate: string;
  pickupLocation: string;
  returnLocation: string;
  protectionPlan: string;      // 'basic' | 'standard' | 'premium'
  extras?: string[];           // ex: ['gps', 'child_seat']
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
  bookingReference: string;
  vehicleId: number;
  vehicleName: string;
  vehicleCategory: string;
  clientId: number;
  clientName: string;
  clientEmail: string;
  branchId: number;
  branchName: string;
  startDate: string;
  endDate: string;
  pickupLocation: string;
  returnLocation: string;
  totalCost: number;
  status: RentalStatus;
  cancellationReason?: string;
  protectionPlan?: string;
  extras?: string;
  notes?: string;
  payNow: boolean;
  createdAt: string;
  createdByUserName: string;
  completedByUserName?: string;
}

export interface Rental {
  id: number;
  bookingReference: string;
  vehicleName: string;
  vehicleCategory: string;
  clientName: string;
  clientEmail: string;
  startDate: string;
  endDate: string;
  totalCost: number;
  status: RentalStatus;
  pickupLocation: string;
  returnLocation: string;
  createdAt: string;
}