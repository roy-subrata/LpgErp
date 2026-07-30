/**
 * Single source of truth for the PaymentMethod enum on the client.
 *
 * These numbers are persisted by the API (LpgErp.Domain.Entities.PaymentMethod) — every screen
 * must read them from here. Three screens previously hard-coded their own dropdowns and disagreed,
 * which silently stored the wrong method.
 */
export enum PaymentMethod {
  Cash = 0,
  /** Legacy. Credit is a sale term, not a way of paying — kept only to label existing rows. */
  Credit = 1,
  Bank = 2,
  MobileBanking = 3,
  Cheque = 4,
}

/** Labels for every stored value, including the legacy one. */
export const PAYMENT_METHOD_LABELS: Record<number, string> = {
  [PaymentMethod.Cash]: 'Cash',
  [PaymentMethod.Credit]: 'Credit (legacy)',
  [PaymentMethod.Bank]: 'Bank Transfer',
  [PaymentMethod.MobileBanking]: 'Mobile Banking',
  [PaymentMethod.Cheque]: 'Cheque',
};

/** Methods offered on new records — Credit is deliberately absent. */
export const PAYMENT_METHOD_OPTIONS = [
  { label: PAYMENT_METHOD_LABELS[PaymentMethod.Cash], value: PaymentMethod.Cash },
  { label: PAYMENT_METHOD_LABELS[PaymentMethod.MobileBanking], value: PaymentMethod.MobileBanking },
  { label: PAYMENT_METHOD_LABELS[PaymentMethod.Bank], value: PaymentMethod.Bank },
  { label: PAYMENT_METHOD_LABELS[PaymentMethod.Cheque], value: PaymentMethod.Cheque },
];

/** [label, background, text] triples for the entity-list badge column. */
export const PAYMENT_METHOD_BADGES: Record<string, string[]> = {
  '0': [PAYMENT_METHOD_LABELS[PaymentMethod.Cash], '#f0fdf4', '#15803d'],
  '1': [PAYMENT_METHOD_LABELS[PaymentMethod.Credit], '#f4f5f7', '#6b7280'],
  '2': [PAYMENT_METHOD_LABELS[PaymentMethod.Bank], '#eff6ff', '#1d4ed8'],
  '3': [PAYMENT_METHOD_LABELS[PaymentMethod.MobileBanking], '#faf5ff', '#7e22ce'],
  '4': [PAYMENT_METHOD_LABELS[PaymentMethod.Cheque], '#fefce8', '#a16207'],
};

/** Methods that must name the wallet or bank account the money moved through. */
export function requiresAccount(method: number): boolean {
  return method === PaymentMethod.MobileBanking || method === PaymentMethod.Bank;
}

export const PAYMENT_DIRECTION_BADGES: Record<string, string[]> = {
  '0': ['Received', '#f0fdf4', '#15803d'],
  '1': ['Paid', '#fefce8', '#a16207'],
};
