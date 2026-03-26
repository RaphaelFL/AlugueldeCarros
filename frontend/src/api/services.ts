import { http } from '@/api/http';
import type {
  AuthResponse,
  Branch,
  CreateReservationPayload,
  LoginPayload,
  Payment,
  PaymentLookupResult,
  PreauthPayload,
  PricingRule,
  PricingRulePayload,
  RegisterPayload,
  Reservation,
  SearchFilters,
  SessionUser,
  UpdateReservationPayload,
  Vehicle,
  VehicleCategory,
  VehicleUpsertPayload,
} from '@/types/domain';
import { useAuthStore } from '@/store/auth-store';

export const authService = {
  login: async (payload: LoginPayload) => {
    const { data } = await http.post<AuthResponse>('/api/v1/auth/login', payload);
    return data;
  },
  register: async (payload: RegisterPayload) => {
    const { data } = await http.post<AuthResponse>('/api/v1/auth/register', payload);
    return data;
  },
  refresh: async (token: string) => {
    const { data } = await http.post<AuthResponse>('/api/v1/auth/refresh', token);
    return data;
  },
  me: async (token?: string) => {
    const { data } = await http.get<SessionUser>('/api/v1/users/me', {
      headers: token
        ? {
            Authorization: `Bearer ${token}`,
          }
        : undefined,
    });
    return data;
  },
};

export const catalogService = {
  getBranches: async () => {
    const { data } = await http.get<Branch[]>('/api/v1/branches');
    return data;
  },
  getCategories: async () => {
    const { data } = await http.get<VehicleCategory[]>('/api/v1/vehicles/categories');
    return data;
  },
  searchVehicles: async (filters: SearchFilters) => {
    const { data } = await http.get<Vehicle[]>('/api/v1/vehicles/search', { params: filters });
    return data;
  },
  getVehicle: async (id: number) => {
    const { data } = await http.get<Vehicle>(`/api/v1/vehicles/${id}`);
    return data;
  },
};

export const reservationService = {
  create: async (payload: CreateReservationPayload) => {
    const { data } = await http.post<Reservation>('/api/v1/reservations', payload);
    return data;
  },
  getMine: async () => {
    const { data } = await http.get<Reservation[]>('/api/v1/users/me/reservations');
    return data;
  },
  getById: async (id: number) => {
    const { data } = await http.get<Reservation>(`/api/v1/reservations/${id}`);
    return data;
  },
  update: async (id: number, payload: UpdateReservationPayload) => {
    await http.patch(`/api/v1/reservations/${id}`, payload);
  },
  cancel: async (id: number) => {
    await http.post(`/api/v1/reservations/${id}/cancel`);
  },
};

export const paymentService = {
  preauthorize: async (payload: PreauthPayload) => {
    const { data } = await http.post<Payment>('/api/v1/payments/preauth', payload);
    useAuthStore.getState().rememberPayment(payload.reservationId, data.id);
    return data;
  },
  capture: async (paymentId: number) => {
    const { data } = await http.post<Payment>('/api/v1/payments/capture', { paymentId });
    return data;
  },
  refund: async (paymentId: number) => {
    const { data } = await http.post<Payment>('/api/v1/payments/refund', { paymentId });
    return data;
  },
  getById: async (id: number) => {
    const { data } = await http.get<Payment>(`/api/v1/payments/${id}`);
    return data;
  },
  resolveForReservation: async (reservationId: number): Promise<PaymentLookupResult> => {
    const registryId = useAuthStore.getState().getPaymentId(reservationId);
    if (registryId) {
      try {
        const payment = await paymentService.getById(registryId);
        if (payment.reservationId === reservationId) {
          return { payment, source: 'registry' };
        }
      } catch {
      }
    }

    try {
      const seedMatch = await paymentService.getById(reservationId);
      if (seedMatch.reservationId === reservationId) {
        useAuthStore.getState().rememberPayment(reservationId, seedMatch.id);
        return { payment: seedMatch, source: 'seed-match' };
      }
    } catch {
    }

    return { payment: null, source: 'none' };
  },
};

export const adminService = {
  getUsers: async () => {
    const { data } = await http.get<SessionUser[]>('/api/v1/admin/users');
    return data;
  },
  assignRoles: async (userId: number, roles: string[]) => {
    await http.post(`/api/v1/admin/users/${userId}/roles`, { roles });
  },
  createVehicle: async (payload: VehicleUpsertPayload) => {
    const { data } = await http.post<Vehicle>('/api/v1/admin/vehicles', payload);
    return data;
  },
  updateVehicle: async (id: number, payload: VehicleUpsertPayload) => {
    const { data } = await http.patch<Vehicle>(`/api/v1/admin/vehicles/${id}`, payload);
    return data;
  },
  getPricingRules: async () => {
    const { data } = await http.get<PricingRule[]>('/api/v1/pricing/rules');
    return data;
  },
  getPricingRule: async (id: number) => {
    const { data } = await http.get<PricingRule>(`/api/v1/pricing/rules/${id}`);
    return data;
  },
  createPricingRule: async (payload: PricingRulePayload) => {
    const { data } = await http.post<PricingRule>('/api/v1/pricing/rules', payload);
    return data;
  },
  updatePricingRule: async (id: number, payload: PricingRulePayload) => {
    const { data } = await http.patch<PricingRule>(`/api/v1/pricing/rules/${id}`, payload);
    return data;
  },
};
