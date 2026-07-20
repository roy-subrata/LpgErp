import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { Product } from '../../core/models';
import { ProductFormComponent } from './product-form.component';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, ProductFormComponent],
  template: `
    <div class="page-header">
      <h1>Products</h1>
      <button class="btn-primary" (click)="openCreate()">+ New Product</button>
    </div>
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Code</th>
            <th>Type</th>
            <th>Purchase Price</th>
            <th>Sale Price</th>
            <th>Stock</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (product of products(); track product.id) {
            <tr>
              <td>{{ product.name }}</td>
              <td>{{ product.code }}</td>
              <td>{{ product.type }}</td>
              <td>{{ product.purchasePrice | currency }}</td>
              <td>{{ product.salePrice | currency }}</td>
              <td>{{ product.currentStock }}</td>
              <td>{{ product.isActive ? 'Active' : 'Inactive' }}</td>
              <td>
                <button class="btn-sm" (click)="openEdit(product.id)">Edit</button>
                <button class="btn-sm btn-danger" (click)="onDelete(product.id)">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr>
              <td colspan="8">No products found.</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
    <app-product-form [open]="showForm()" [entityId]="editingId()" (close)="showForm.set(false)" (saved)="showForm.set(false); loadData()" />
  `,
  styles: [`
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }
    .btn-primary {
      background: #1a1a2e;
      color: white;
      border: none;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      cursor: pointer;
    }
    .table-container {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      overflow: hidden;
    }
    table {
      width: 100%;
      border-collapse: collapse;
    }
    th, td {
      padding: 0.75rem 1rem;
      text-align: left;
      border-bottom: 1px solid #eee;
    }
    th {
      background: #f8f9fa;
      font-weight: 600;
    }
    .btn-sm {
      padding: 0.25rem 0.5rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      cursor: pointer;
      margin-right: 0.25rem;
    }
    .btn-danger {
      color: #dc3545;
      border-color: #dc3545;
    }
  `],
})
export class ProductListComponent implements OnInit {
  private api = inject(ApiService);
  products = signal<Product[]>([]);
  showForm = signal(false);
  editingId = signal<string | null>(null);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getAllList<Product>('products').subscribe(data => this.products.set(data));
  }

  openCreate() {
    this.editingId.set(null);
    this.showForm.set(true);
  }

  openEdit(id: string) {
    this.editingId.set(id);
    this.showForm.set(true);
  }

  onDelete(id: string) {
    if (confirm('Are you sure?')) {
      this.api.delete('products', id).subscribe(() => this.loadData());
    }
  }
}
