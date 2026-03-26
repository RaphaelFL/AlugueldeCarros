import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { SessionUser } from '@/types/domain';

interface AuthState {
  token: string | null;
  user: SessionUser | null;
  paymentRegistry: Record<number, number>;
  hydrated: boolean;
  setSession: (token: string, user: SessionUser) => void;
  updateUser: (user: SessionUser) => void;
  signOut: () => void;
  markHydrated: () => void;
  rememberPayment: (reservationId: number, paymentId: number) => void;
  getPaymentId: (reservationId: number) => number | null;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      user: null,
      paymentRegistry: {},
      hydrated: false,
      setSession: (token, user) => set({ token, user }),
      updateUser: (user) => set({ user }),
      signOut: () => set({ token: null, user: null, paymentRegistry: {} }),
      markHydrated: () => set({ hydrated: true }),
      rememberPayment: (reservationId, paymentId) =>
        set((state) => ({
          paymentRegistry: {
            ...state.paymentRegistry,
            [reservationId]: paymentId,
          },
        })),
      getPaymentId: (reservationId) => get().paymentRegistry[reservationId] ?? null,
    }),
    {
      name: 'alugueldecarros-auth',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        token: state.token,
        user: state.user,
        paymentRegistry: state.paymentRegistry,
      }),
      onRehydrateStorage: () => (state) => {
        state?.markHydrated();
      },
    },
  ),
);
