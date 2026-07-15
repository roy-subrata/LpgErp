import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Customer {
  id: string;
  name: string;
  code: string;
  type: string;
  phone: string;
  creditLimit: number;
  isActive: boolean;
}

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-header">
      <h1>Customers</h1>
      <button class="btn-primary">+ New Customer</button>
    </div>
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Code</th>
            <th>Type</th>
            <th>Phone</th>
            <th>Credit Limit</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          @for (customer of customers(); track customer.id) {
            <tr>
              <td>{{ customer.name }}</td>
              <td>{{ customer.code }}</td>
              <td>{{ customer.type }}</td>
              <td>{{ customer.phone }}</td>
              <td>{{ customer.creditLimit | currency }}</td>
              <td>{{ customer.isActive ? 'Active' : 'Inactive' }}</td>
              <td>
                <button class="btn-sm">Edit</button>
                <button class="btn-sm btn-danger">Delete</button>
              </td>
            </tr>
          } @empty {
            <tr>
              <td colspan="7">No customers found.</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
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
export class CustomerListComponent implements OnInit {
  customers = signal<Customer[]>([]);

  ngOnInit() {
    // TODO: Load from API
  }
}
