export interface CommissionPolicy {
  id: string;
  name: string;
  description: string;
  entityType: number;
  entityId: string;
  entityName: string;
  calculationType: number;
  periodType: number;
  productId: string;
  productName: string;
  brandId: string;
  brandName: string;
  cylinderSizeId: string;
  cylinderSizeName: string;
  targetQuantity: number;
  commissionValue: number;
  tierConfig: string;
  autoApply: boolean;
  isActive: boolean;
  startDate: string;
  endDate: string;
  createdAt: string;
}

export interface CommissionLedger {
  id: string;
  policyId: string;
  policyName: string;
  entityType: number;
  entityId: string;
  entityName: string;
  periodKey: string;
  actualQuantity: number;
  actualAmount: number;
  commissionEarned: number;
  status: number;
  periodStart: string;
  periodEnd: string;
  appliedDate: string;
  reference: string;
  notes: string;
  createdAt: string;
}
