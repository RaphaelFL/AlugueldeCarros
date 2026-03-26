export type Role = 'Customer' | 'Admin';

export type ReservationStatus = 'PENDING_PAYMENT' | 'CONFIRMED' | 'CANCELLED' | 'EXPIRED';
export type PaymentStatus = 'PENDING' | 'APPROVED' | 'DECLINED' | 'REFUNDED';
export type VehicleStatus = 'AVAILABLE' | 'RESERVED' | 'RENTED' | 'MAINTENANCE' | 'BLOCKED';

export interface AuthResponse {
  token: string;
  email?: string;
}

export interface SessionUser {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  createdAt: string;
  roles: Role[];
}

export interface Branch {
  id: number;
  name: string;
  address: string;
  phone: string;
}

export interface VehicleCategory {
  id: number;
  name: string;
  description: string;
}

export interface Vehicle {
  id: number;
  licensePlate: string;
  model: string;
  year: number;
  categoryId: number;
  branchId: number;
  status: VehicleStatus;
  dailyRate: number;
  category?: VehicleCategory;
  branch?: Branch;
}

export interface Reservation {
  id: number;
  userId: number;
  categoryId: number;
  vehicleId?: number | null;
  startDate: string;
  endDate: string;
  status: ReservationStatus;
  totalAmount: number;
  user?: SessionUser;
  category?: VehicleCategory;
  vehicle?: Vehicle | null;
}

export interface Payment {
  id: number;
  reservationId: number;
  amount: number;
  status: PaymentStatus;
  createdAt: string;
  reservation?: Reservation;
}

export interface PricingRule {
  id: number;
  categoryId: number;
  baseDailyRate: number;
  weekendMultiplier: number;
  peakSeasonMultiplier: number;
  category?: VehicleCategory;
}

export interface SearchFilters {
  branchId?: number;
  categoryId?: number;
  from?: string;
  to?: string;
  priceMin?: number;
  priceMax?: number;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload extends LoginPayload {
  firstName: string;
  lastName: string;
}

export interface CreateReservationPayload {
  categoryId: number;
  startDate: string;
  endDate: string;
}

export interface UpdateReservationPayload {
  startDate?: string;
  endDate?: string;
  status?: ReservationStatus;
}

export interface PreauthPayload {
  reservationId: number;
  amount: number;
}

export interface PaymentLookupResult {
  payment: Payment | null;
  source: 'registry' | 'seed-match' | 'none';
}

export interface VehicleUpsertPayload {
  licensePlate: string;
  model: string;
  year: number;
  categoryId: number;
  branchId: number;
  dailyRate: number;
  status: VehicleStatus;
}

export interface PricingRulePayload {
  categoryId: number;
  baseDailyRate: number;
  weekendMultiplier: number;
  peakSeasonMultiplier: number;
}
