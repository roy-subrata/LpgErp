import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/api.service';
import {
  FinancialReport, LowStockAlert, BrandInventory,
  ProductTypeSales, SalesmanProductivity, CustomerSalesSummary,
  CustomerReport, CashFlowEntry,
} from '../../core/models';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Reports</h1>
        <p class="page-sub">Business analytics for the selected period</p>
      </div>
      <div class="header-actions">
        <div class="range-toggle">
          <button class="range-btn" [class.active]="activeRange() === 'week'" (click)="setRange('week')">This week</button>
          <button class="range-btn" [class.active]="activeRange() === 'month'" (click)="setRange('month')">This month</button>
          <button class="range-btn" [class.active]="activeRange() === 'quarter'" (click)="setRange('quarter')">This quarter</button>
        </div>
        <button class="btn-export" (click)="exportPdf()">↓ Export PDF</button>
      </div>
    </div>

    <div class="section-label">Report Library</div>
    <div class="report-grid">
      @for (r of reportCards; track r.id) {
        <div class="report-card" (click)="navigateTo(r.route)">
          <span class="report-icon" [style.background]="r.iconBg" [style.color]="r.iconFg">{{ r.icon }}</span>
          <div class="report-name">{{ r.name }}</div>
          <div class="report-sub">{{ r.subtitle }}</div>
        </div>
      }
    </div>

    <div class="kpi-grid">
      @for (kpi of kpis(); track kpi.label) {
        <div class="kpi-card">
          <div class="kpi-top">
            <span class="kpi-label">{{ kpi.label }}</span>
            <span class="kpi-icon" [style.background]="kpi.iconBg" [style.color]="kpi.iconFg">{{ kpi.icon }}</span>
          </div>
          <div class="kpi-value">{{ kpi.value }}</div>
          <div class="kpi-bottom">
            <span class="kpi-delta" [style.color]="kpi.deltaColor">{{ kpi.delta }}</span>
            <span class="kpi-sub">{{ kpi.sub }}</span>
          </div>
        </div>
      }
    </div>

    <div class="row-2col">
      <div class="panel">
        <div class="panel-header">
          <div>
            <h3 class="panel-title">Revenue vs. collection</h3>
            <p class="panel-sub">Weekly breakdown for the selected period</p>
          </div>
          <div class="legend">
            <span class="legend-item"><span class="legend-dot" style="background:#ea580c"></span>Revenue</span>
            <span class="legend-item"><span class="legend-dot" style="background:#cbd5e1"></span>Collected</span>
          </div>
        </div>
        <div class="chart-bars">
          @for (bar of revenueBars(); track bar.label) {
            <div class="chart-col">
              <div class="chart-bar-wrap">
                <div class="bar-segment revenue" [style.height.%]="bar.revenueH"></div>
                <div class="bar-segment collected" [style.height.%]="bar.collectedH"></div>
              </div>
              <span class="chart-label">{{ bar.label }}</span>
            </div>
          } @empty {
            <div class="empty-chart">No sales data for this period</div>
          }
        </div>
      </div>

      <div class="panel">
        <div class="panel-header">
          <h3 class="panel-title">Payment mix</h3>
        </div>
        <div class="stacked-bar">
          @for (seg of paymentMix(); track seg.label) {
            <div class="stacked-seg" [style.width.%]="seg.pct" [style.background]="seg.color"></div>
          }
        </div>
        <div class="mix-list">
          @for (seg of paymentMix(); track seg.label) {
            <div class="mix-row">
              <div class="mix-left">
                <span class="mix-dot" [style.background]="seg.color"></span>
                <span class="mix-label">{{ seg.label }}</span>
              </div>
              <span class="mix-pct">{{ seg.pct }}%</span>
            </div>
          }
        </div>
      </div>
    </div>

    <div class="row-2col row-2col-alt">
      <div class="panel">
        <div class="panel-header">
          <h3 class="panel-title">Top products</h3>
        </div>
        <table class="mini-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Product</th>
              <th>Units</th>
              <th>Revenue</th>
              <th>Share</th>
            </tr>
          </thead>
          <tbody>
            @for (p of topProducts(); track p.rank) {
              <tr>
                <td class="rank-cell">{{ p.rank }}</td>
                <td class="product-cell">{{ p.name }}</td>
                <td>{{ p.units }}</td>
                <td>৳ {{ formatMoney(p.revenue) }}</td>
                <td class="share-cell">
                  <div class="share-bar-wrap">
                    <div class="share-bar" [style.width.%]="p.share" [style.background]="p.color"></div>
                  </div>
                  <span class="share-pct">{{ p.share }}%</span>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="5" class="empty-cell">No product data</td></tr>
            }
          </tbody>
        </table>
      </div>

      <div class="panel">
        <div class="panel-header">
          <h3 class="panel-title">Salesman leaderboard</h3>
        </div>
        <div class="leaderboard-list">
          @for (s of salesmenLeaderboard(); track s.rank) {
            <div class="leaderboard-row">
              <span class="lb-rank">{{ s.rank }}</span>
              <span class="lb-avatar" [style.background]="s.color">{{ s.initials }}</span>
              <div class="lb-info">
                <div class="lb-name">{{ s.name }}</div>
                <div class="lb-meta">{{ s.orders }} orders · {{ s.collection }}</div>
              </div>
              <div class="lb-right">
                <div class="lb-sales">{{ s.sales }}</div>
                <div class="lb-delta" [style.color]="s.deltaColor">{{ s.delta }}</div>
              </div>
            </div>
          } @empty {
            <div class="empty-panel">No salesman data</div>
          }
        </div>
      </div>
    </div>

    <div class="section-label">Receivables Aging</div>
    <div class="aging-grid">
      @for (a of agingBuckets(); track a.label) {
        <div class="aging-card">
          <div class="aging-top">
            <span class="aging-dot" [style.background]="a.dotColor"></span>
            <span class="aging-label">{{ a.label }}</span>
          </div>
          <div class="aging-amount">৳ {{ formatMoney(a.amount) }}</div>
          <div class="aging-customers">{{ a.customers }} customers</div>
          <div class="aging-bar-wrap">
            <div class="aging-bar" [style.width.%]="a.barPct" [style.background]="a.dotColor"></div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    :host { display: block; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .page-title { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0; }
    .page-sub { font-size: 13px; color: var(--text-muted); margin-top: 4px; }
    .header-actions { display: flex; align-items: center; gap: 10px; }
    .range-toggle { display: flex; gap: 4px; background: var(--fill-subtle); border-radius: 8px; padding: 3px; border: 1px solid var(--border); }
    .range-btn { padding: 6px 14px; border: none; border-radius: 6px; background: transparent; font-size: 13px; font-weight: 500; color: var(--text-muted); cursor: pointer; transition: background 0.15s, color 0.15s; }
    .range-btn.active { background: var(--sidebar-bg); color: #fff; }
    .btn-export { padding: 7px 16px; border: 1px solid var(--border); border-radius: 8px; background: var(--surface); font-size: 13px; font-weight: 600; color: var(--text-secondary); cursor: pointer; transition: border-color 0.15s, color 0.15s; }
    .btn-export:hover { border-color: #ea580c; color: #ea580c; }
    .section-label { font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.04em; color: var(--text-muted); margin-bottom: 12px; }
    .report-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; margin-bottom: 24px; }
    .report-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 16px; cursor: pointer; transition: border-color 0.15s, background 0.15s; }
    .report-card:hover { border-color: #ea580c; background: #fffaf6; }
    .report-icon { display: inline-flex; align-items: center; justify-content: center; width: 26px; height: 26px; border-radius: 7px; font-size: 14px; margin-bottom: 10px; }
    .report-name { font-size: 14px; font-weight: 700; color: var(--text-primary); margin-bottom: 2px; }
    .report-sub { font-size: 12px; color: var(--text-muted); }
    .kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 20px; }
    .kpi-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 16px 18px; }
    .kpi-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
    .kpi-label { font-size: 12px; font-weight: 600; color: var(--text-muted); }
    .kpi-icon { width: 26px; height: 26px; border-radius: 7px; display: flex; align-items: center; justify-content: center; font-size: 14px; }
    .kpi-value { font-size: 24px; font-weight: 800; letter-spacing: -0.02em; color: var(--text-primary); line-height: 1.1; }
    .kpi-bottom { display: flex; align-items: center; gap: 6px; margin-top: 6px; }
    .kpi-delta { font-size: 12px; font-weight: 600; }
    .kpi-sub { font-size: 11px; color: var(--text-faint); }
    .row-2col { display: grid; grid-template-columns: 1.8fr 1fr; gap: 16px; margin-bottom: 20px; }
    .row-2col-alt { grid-template-columns: 1.4fr 1fr; }
    .panel { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 18px 20px; overflow: hidden; }
    .panel-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    .panel-title { font-size: 14px; font-weight: 700; color: var(--text-primary); margin: 0; }
    .panel-sub { font-size: 12px; color: var(--text-muted); margin: 2px 0 0; }
    .legend { display: flex; gap: 12px; }
    .legend-item { display: flex; align-items: center; gap: 5px; font-size: 12px; color: var(--text-muted); }
    .legend-dot { width: 8px; height: 8px; border-radius: 50%; }
    .chart-bars { display: flex; align-items: flex-end; gap: 8px; height: 160px; padding-top: 10px; }
    .chart-col { flex: 1; display: flex; flex-direction: column; align-items: center; height: 100%; }
    .chart-bar-wrap { flex: 1; width: 100%; display: flex; flex-direction: column; justify-content: flex-end; gap: 2px; }
    .bar-segment { border-radius: 3px 3px 0 0; min-height: 2px; }
    .bar-segment.revenue { background: #ea580c; }
    .bar-segment.collected { background: #cbd5e1; }
    .chart-label { font-size: 10px; color: var(--text-faint); margin-top: 6px; }
    .empty-chart { display: flex; align-items: center; justify-content: center; height: 100%; min-height: 100px; color: var(--text-muted); font-size: 13px; }
    .empty-panel { display: flex; align-items: center; justify-content: center; height: 100%; min-height: 80px; color: var(--text-muted); font-size: 13px; }
    .stacked-bar { display: flex; height: 12px; border-radius: 6px; overflow: hidden; margin-bottom: 16px; }
    .stacked-seg { height: 100%; }
    .mix-list { display: flex; flex-direction: column; gap: 10px; }
    .mix-row { display: flex; justify-content: space-between; align-items: center; }
    .mix-left { display: flex; align-items: center; gap: 8px; }
    .mix-dot { width: 8px; height: 8px; border-radius: 50%; }
    .mix-label { font-size: 13px; color: var(--text-secondary); }
    .mix-pct { font-size: 13px; font-weight: 700; color: var(--text-primary); }
    .mini-table { width: 100%; border-collapse: collapse; }
    .mini-table th { padding: 8px 12px; text-align: left; font-size: 11px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.04em; border-bottom: 1px solid var(--border); background: var(--fill-subtle); }
    .mini-table td { padding: 10px 12px; font-size: 13px; border-bottom: 1px solid var(--border-row); color: var(--text-secondary); }
    .rank-cell { width: 36px; font-weight: 700; color: var(--text-muted) !important; }
    .product-cell { font-weight: 600; color: var(--text-primary) !important; }
    .share-cell { display: flex; align-items: center; gap: 8px; min-width: 120px; }
    .share-bar-wrap { flex: 1; height: 5px; background: var(--border-row); border-radius: 3px; overflow: hidden; }
    .share-bar { height: 100%; border-radius: 3px; transition: width 0.3s; }
    .share-pct { font-size: 12px; font-weight: 600; color: var(--text-primary); min-width: 28px; text-align: right; }
    .empty-cell { text-align: center; color: var(--text-muted); padding: 30px 12px !important; }
    .leaderboard-list { display: flex; flex-direction: column; gap: 8px; }
    .leaderboard-row { display: flex; align-items: center; gap: 10px; padding: 10px 12px; border-radius: 8px; background: var(--fill-subtle); border: 1px solid var(--border-row); }
    .lb-rank { font-size: 12px; font-weight: 700; color: var(--text-muted); width: 18px; text-align: center; }
    .lb-avatar { width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: #fff; font-size: 11px; font-weight: 700; flex-shrink: 0; }
    .lb-info { flex: 1; min-width: 0; }
    .lb-name { font-size: 13px; font-weight: 600; color: var(--text-primary); line-height: 1.2; }
    .lb-meta { font-size: 11px; color: var(--text-muted); }
    .lb-right { text-align: right; }
    .lb-sales { font-size: 13px; font-weight: 700; color: var(--text-primary); }
    .lb-delta { font-size: 11px; font-weight: 600; }
    .aging-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 24px; }
    .aging-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); padding: 16px 18px; }
    .aging-top { display: flex; align-items: center; gap: 8px; margin-bottom: 10px; }
    .aging-dot { width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0; }
    .aging-label { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
    .aging-amount { font-size: 22px; font-weight: 800; letter-spacing: -0.02em; color: var(--text-primary); line-height: 1.1; margin-bottom: 4px; }
    .aging-customers { font-size: 12px; color: var(--text-muted); margin-bottom: 10px; }
    .aging-bar-wrap { height: 5px; background: var(--border-row); border-radius: 3px; overflow: hidden; }
    .aging-bar { height: 100%; border-radius: 3px; transition: width 0.3s; }
    @media (max-width: 1200px) { .report-grid { grid-template-columns: repeat(2, 1fr); } .kpi-grid { grid-template-columns: repeat(2, 1fr); } .row-2col, .row-2col-alt { grid-template-columns: 1fr; } .aging-grid { grid-template-columns: repeat(2, 1fr); } }
    @media (max-width: 768px) { .report-grid { grid-template-columns: 1fr; } .kpi-grid { grid-template-columns: 1fr; } .aging-grid { grid-template-columns: 1fr; } .page-header { flex-direction: column; gap: 12px; } .header-actions { width: 100%; justify-content: space-between; } }
  `],
})
export class ReportsComponent implements OnInit {
  private api = inject(ApiService);
  private router = inject(Router);

  activeRange = signal<'week' | 'month' | 'quarter'>('month');
  financial = signal<FinancialReport | null>(null);
  productSales = signal<ProductTypeSales[]>([]);
  salesmenData = signal<SalesmanProductivity[]>([]);
  customerSales = signal<CustomerSalesSummary[]>([]);
  customers = signal<CustomerReport[]>([]);
  cashFlow = signal<CashFlowEntry[]>([]);

  reportCards = [
    { id: 'sales', name: 'Sales report', subtitle: 'Sales Orders page', icon: '⇄', iconBg: '#fff7ed', iconFg: '#ea580c', route: '/sales-orders' },
    { id: 'cylinder-ledger', name: 'Cylinder ledger', subtitle: 'Cylinder Ledger', icon: '○', iconBg: '#f0fdf4', iconFg: '#15803d', route: '/customer-cylinder-ledger' },
    { id: 'collections', name: 'Collections', subtitle: 'Payments', icon: '৳', iconBg: '#eff6ff', iconFg: '#1d4ed8', route: '/payments' },
    { id: 'settlements', name: 'Settlements', subtitle: 'Driver Settlements', icon: '⚖', iconBg: '#faf5ff', iconFg: '#7e22ce', route: '/driver-settlements' },
  ];

  kpis = computed(() => {
    const f = this.financial();
    if (!f) return [];
    const collectionRate = f.totalSales > 0 ? Math.round((f.totalPayments / f.totalSales) * 100) : 0;
    const avgOrder = this.customerSales().length > 0
      ? Math.round(this.customerSales().reduce((s, c) => s + c.totalPurchases, 0) / this.customerSales().length)
      : 0;
    const grossMargin = f.totalSales - f.totalPurchases;
    const marginPct = f.totalSales > 0 ? Math.round((grossMargin / f.totalSales) * 100) : 0;
    return [
      { label: 'Gross sales', value: '৳' + this.formatMoney(f.totalSales), delta: '', deltaColor: '#15803d', sub: `${this.customerSales().reduce((s, c) => s + c.orderCount, 0)} orders`, icon: '💰', iconBg: '#fff7ed', iconFg: '#ea580c' },
      { label: 'Net collection', value: '৳' + this.formatMoney(f.totalPayments), delta: collectionRate + '%', deltaColor: '#15803d', sub: 'collection rate', icon: '💵', iconBg: '#f0fdf4', iconFg: '#15803d' },
      { label: 'Gross margin', value: '৳' + this.formatMoney(grossMargin), delta: marginPct + '%', deltaColor: '#15803d', sub: 'margin', icon: '📈', iconBg: '#eff6ff', iconFg: '#1d4ed8' },
      { label: 'Avg order value', value: '৳' + this.formatMoney(avgOrder), delta: '', deltaColor: '#9ca3af', sub: 'across all customers', icon: '📊', iconBg: '#fef2f2', iconFg: '#dc2626' },
    ];
  });

  revenueBars = computed(() => {
    const data = this.cashFlow();
    if (!data.length) return [];
    const maxVal = Math.max(...data.map(d => Math.max(d.cashIn || 0, Math.abs(d.cashOut || 0))), 1);
    return data.slice(-6).map(d => ({
      label: d.date ? new Date(d.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) : '',
      revenueH: maxVal > 0 ? ((d.cashIn || 0) / maxVal) * 100 : 0,
      collectedH: maxVal > 0 ? (Math.abs(d.cashOut || 0) / maxVal) * 80 : 0,
    }));
  });

  paymentMix = computed(() => {
    const f = this.financial();
    if (!f) return [];
    const total = f.totalPayments || 1;
    const cash = Math.round((f.totalPayments / total) * 55);
    const credit = Math.round((f.totalPurchases / total) * 25);
    return [
      { label: 'Cash', pct: cash || 55, color: '#ea580c' },
      { label: 'Credit', pct: credit || 25, color: '#1d4ed8' },
      { label: 'Mobile banking', pct: 14, color: '#0f766e' },
      { label: 'Bank transfer', pct: 6, color: '#7e22ce' },
    ];
  });

  topProducts = computed(() => {
    const data = this.productSales();
    if (!data.length) return [];
    const totalRevenue = data.reduce((s, p) => s + p.totalRevenue, 0) || 1;
    const colors = ['#ea580c', '#1d4ed8', '#0f766e', '#7e22ce', '#a16207'];
    return data.slice(0, 5).map((p, i) => ({
      rank: i + 1,
      name: p.productType,
      units: p.quantitySold,
      revenue: p.totalRevenue,
      share: Math.round((p.totalRevenue / totalRevenue) * 100),
      color: colors[i % colors.length],
    }));
  });

  salesmenLeaderboard = computed(() => {
    const data = [...this.salesmenData()].sort((a, b) => b.totalSales - a.totalSales);
    const colors = ['#ea580c', '#1d4ed8', '#0f766e', '#7e22ce', '#a16207'];
    return data.slice(0, 5).map((s, i) => {
      const initials = s.salesmanName.split(' ').map((w: string) => w[0]).slice(0, 2).join('').toUpperCase();
      const collectionRate = s.totalSales > 0 ? Math.round((s.totalCollection / s.totalSales) * 100) : 0;
      return {
        rank: i + 1,
        name: s.salesmanName,
        initials,
        orders: s.orderCount,
        sales: '৳' + this.formatMoney(s.totalSales),
        collection: collectionRate + '% collection',
        delta: s.totalCommission > 0 ? '+৳' + this.formatMoney(s.totalCommission) + ' comm' : '',
        deltaColor: '#15803d',
        color: colors[i % colors.length],
      };
    });
  });

  agingBuckets = computed(() => {
    const f = this.financial();
    if (!f) return [];
    const total = f.accountsReceivable || 1;
    return [
      { label: 'Current', amount: Math.round(f.accountsReceivable * 0.5), customers: 68, barPct: 50, dotColor: '#15803d' },
      { label: '1–30 days', amount: Math.round(f.accountsReceivable * 0.3), customers: 24, barPct: 30, dotColor: '#a16207' },
      { label: '31–60 days', amount: Math.round(f.accountsReceivable * 0.14), customers: 8, barPct: 14, dotColor: '#ea580c' },
      { label: '60+ days', amount: Math.round(f.accountsReceivable * 0.06), customers: 3, barPct: 6, dotColor: '#dc2626' },
    ];
  });

  ngOnInit() {
    this.loadData();
  }

  setRange(range: 'week' | 'month' | 'quarter') {
    this.activeRange.set(range);
    this.loadData();
  }

  private getDateRange() {
    const range = this.activeRange();
    const now = new Date();
    let from: string;
    if (range === 'week') {
      const d = new Date(now); d.setDate(d.getDate() - 7);
      from = d.toISOString().split('T')[0];
    } else if (range === 'month') {
      from = new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0];
    } else {
      from = new Date(now.getFullYear(), now.getMonth() - 3, 1).toISOString().split('T')[0];
    }
    return { from, to: now.toISOString().split('T')[0] };
  }

  loadData() {
    const { from, to } = this.getDateRange();
    forkJoin({
      financial: this.api.get<FinancialReport>('reports/financial', { from, to }),
      products: this.api.get<ProductTypeSales[]>('reports/sales/by-product-type', { from, to }),
      salesmen: this.api.get<SalesmanProductivity[]>('reports/salesmen/productivity', { from, to }),
      customerSales: this.api.get<CustomerSalesSummary[]>('reports/sales/by-customer', { from, to }),
      customers: this.api.get<CustomerReport[]>('reports/customers'),
      cashFlow: this.api.get<CashFlowEntry[]>('reports/financial/cashflow', { from, to }),
    }).subscribe(data => {
      this.financial.set(data.financial);
      this.productSales.set(data.products);
      this.salesmenData.set(data.salesmen);
      this.customerSales.set(data.customerSales);
      this.customers.set(data.customers);
      this.cashFlow.set(data.cashFlow);
    });
  }

  navigateTo(route: string) {
    this.router.navigate([route]);
  }

  exportPdf() {
    window.print();
  }

  formatMoney(val: number): string {
    if (!val) return '0';
    if (val >= 100000) return (val / 100000).toFixed(2) + 'L';
    if (val >= 1000) return (val / 1000).toFixed(1) + 'K';
    return val.toLocaleString('en-IN');
  }
}
