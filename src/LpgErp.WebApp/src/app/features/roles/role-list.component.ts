import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RoleAdminService } from '../../core/role-admin.service';
import { RoleSummary } from '../../core/auth.models';
import { PermissionDto } from '../../core/auth.models';
import { DialogComponent } from '../../shared/dialog.component';
import { DropdownMenuComponent, DropdownMenuItem } from '../../shared/dropdown-menu.component';

interface PermissionGroup {
  group: string;
  permissions: PermissionDto[];
}

/**
 * Role definitions: what a role is called and which permissions it grants. Until this screen,
 * roles were fixed at seed time (Admin, Manager, Salesman, Driver, Warehouse, Accountant, Viewer)
 * with no way to add a new one or change what an existing one can do.
 */
@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent, DropdownMenuComponent],
  template: `
    <div class="page-header">
      <h1 class="page-title">Roles &amp; Permissions</h1>
      <div class="header-actions">
        <button class="btn-primary-sm" (click)="openNew()">+ New Role</button>
      </div>
    </div>

    <div class="table-card">
      <div class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width:18%">Role</th>
              <th style="width:32%">Description</th>
              <th style="width:24%">Permissions</th>
              <th style="width:10%">Users</th>
              <th style="width:10%">Status</th>
              <th class="actions-col"></th>
            </tr>
          </thead>
          <tbody>
            @for (r of roles(); track r.id) {
              <tr class="data-row">
                <td><span class="main-text">{{ r.name }}</span></td>
                <td><span class="muted-text">{{ r.description || '—' }}</span></td>
                <td><span class="muted-text">{{ r.permissions.length }} of {{ totalPermissions() }}</span></td>
                <td><span class="num-text">{{ r.userCount }}</span></td>
                <td>
                  <span class="badge" [style.background]="r.isActive ? '#f0fdf4' : '#fef2f2'" [style.color]="r.isActive ? '#15803d' : '#b91c1c'">
                    {{ r.isActive ? 'Active' : 'Inactive' }}
                  </span>
                </td>
                <td class="actions-col">
                  <app-dropdown-menu [items]="menuFor(r)" (selected)="onMenuAction($event, r)" />
                </td>
              </tr>
            } @empty {
              <tr><td colspan="6" class="empty-row">No roles found.</td></tr>
            }
          </tbody>
        </table>
      </div>
    </div>

    <app-dialog [open]="formOpen()" [title]="editingId() ? 'Edit Role' : 'New Role'" (close)="closeForm()">
      <form (ngSubmit)="submit()">
        <div class="form-group">
          <label>Name</label>
          <input type="text" [(ngModel)]="name" name="name" required [disabled]="isBuiltIn()" />
          @if (isBuiltIn()) { <small class="field-hint">The Admin role's name is fixed.</small> }
        </div>
        <div class="form-group">
          <label>Description</label>
          <input type="text" [(ngModel)]="description" name="description" />
        </div>
        <div class="form-group checkbox-group">
          <label><input type="checkbox" [(ngModel)]="isActive" name="isActive" /> Active</label>
        </div>

        <div class="form-group">
          <label>Permissions <span class="optional">({{ selectedPermissionIds.size }} of {{ totalPermissions() }} selected)</span></label>
          <div class="permission-groups">
            @for (g of groupedPermissions(); track g.group) {
              <div class="permission-group">
                <div class="group-header">
                  <span class="group-name">{{ g.group }}</span>
                  <button type="button" class="group-toggle" (click)="toggleGroup(g)">
                    {{ allSelected(g) ? 'Clear' : 'Select all' }}
                  </button>
                </div>
                <div class="group-perms">
                  @for (p of g.permissions; track p.id) {
                    <label class="perm-check">
                      <input type="checkbox" [checked]="selectedPermissionIds.has(p.id)" (change)="togglePermission(p.id)" />
                      {{ p.name }}
                    </label>
                  }
                </div>
              </div>
            }
          </div>
        </div>

        @if (error()) { <p class="error">{{ error() }}</p> }
        <div class="form-actions">
          <button type="button" class="btn btn-secondary" (click)="closeForm()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">
            {{ saving() ? 'Saving...' : (editingId() ? 'Update' : 'Create') }}
          </button>
        </div>
      </form>
    </app-dialog>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .page-title { font-size: 22px; font-weight: 800; letter-spacing: -0.01em; color: var(--text-primary); margin: 0; }
    .header-actions { display: flex; gap: 8px; }
    .btn-primary-sm { padding: 9px 18px; border-radius: 7px; border: none; background: var(--primary); color: #fff; font-size: 13px; font-weight: 600; cursor: pointer; box-shadow: var(--shadow-btn); }
    .btn-primary-sm:hover { background: var(--primary-hover); }
    .table-card { background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius-card); overflow: hidden; }
    .table-scroll { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 13px; }
    .data-table th { text-align: left; padding: 10px 16px; font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.04em; color: var(--text-muted); border-bottom: 1px solid var(--border); }
    .data-row td { padding: 10px 16px; border-bottom: 1px solid var(--border-row); }
    .main-text { font-weight: 600; color: var(--text-primary); }
    .muted-text { color: var(--text-secondary); }
    .num-text { font-variant-numeric: tabular-nums; }
    .badge { display: inline-block; padding: 3px 9px; border-radius: 20px; font-size: 11px; font-weight: 600; }
    .actions-col { width: 50px; text-align: center; }
    .empty-row { text-align: center; padding: 40px 14px !important; color: var(--text-muted); }

    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; }
    .form-group label .optional { font-weight: 400; color: #6b7280; font-size: 0.8rem; }
    .form-group input[type="text"] { width: 100%; padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .checkbox-group label { display: flex; align-items: center; gap: 0.5rem; font-weight: 400; }
    .field-hint { display: block; margin-top: 0.25rem; font-size: 0.75rem; color: #6b7280; }

    .permission-groups { max-height: 320px; overflow-y: auto; border: 1px solid #eee; border-radius: 6px; padding: 0.5rem 0.75rem; }
    .permission-group { padding: 0.5rem 0; border-bottom: 1px solid #f1f3f6; }
    .permission-group:last-child { border-bottom: none; }
    .group-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.35rem; }
    .group-name { font-weight: 700; font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.04em; color: #6b7280; }
    .group-toggle { border: none; background: none; color: #ea580c; font-size: 0.75rem; font-weight: 600; cursor: pointer; padding: 0; }
    .group-toggle:hover { text-decoration: underline; }
    .group-perms { display: grid; grid-template-columns: 1fr 1fr; gap: 0.3rem 1rem; }
    .perm-check { display: flex; align-items: center; gap: 0.4rem; font-weight: 400; font-size: 0.85rem; }

    .error { color: #dc3545; font-size: 0.85rem; margin-top: 0.75rem; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
  `],
})
export class RoleListComponent implements OnInit {
  private roleAdmin = inject(RoleAdminService);

  roles = signal<RoleSummary[]>([]);
  permissions = signal<PermissionDto[]>([]);

  formOpen = signal(false);
  editingId = signal<string | null>(null);
  saving = signal(false);
  error = signal('');

  name = '';
  description = '';
  isActive = true;
  selectedPermissionIds = new Set<string>();

  totalPermissions = computed(() => this.permissions().length);

  groupedPermissions = computed<PermissionGroup[]>(() => {
    const byGroup = new Map<string, PermissionDto[]>();
    for (const p of this.permissions()) {
      if (!byGroup.has(p.group)) byGroup.set(p.group, []);
      byGroup.get(p.group)!.push(p);
    }
    return [...byGroup.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([group, permissions]) => ({ group, permissions }));
  });

  isBuiltIn(): boolean {
    return this.roles().find(r => r.id === this.editingId())?.name === 'Admin';
  }

  ngOnInit() {
    this.loadAll();
  }

  loadAll() {
    this.roleAdmin.getRoles().subscribe(r => this.roles.set(r));
    this.roleAdmin.getPermissions().subscribe(p => this.permissions.set(p));
  }

  // Deleting the Admin role or a role still assigned to users is blocked server-side too — this
  // just tells the admin why before they try, instead of letting the call fail.
  menuFor(r: RoleSummary): DropdownMenuItem[] {
    const locked = r.name === 'Admin' || r.userCount > 0;
    return [
      { label: 'Edit', icon: '✎' },
      { label: 'Delete', icon: '🗑', danger: true, disabled: locked },
    ];
  }

  onMenuAction(index: number, r: RoleSummary) {
    if (index === 0) this.openEdit(r);
    else this.onDelete(r);
  }

  openNew() {
    this.resetForm();
    this.editingId.set(null);
    this.formOpen.set(true);
  }

  openEdit(r: RoleSummary) {
    this.editingId.set(r.id);
    this.name = r.name;
    this.description = r.description ?? '';
    this.isActive = r.isActive;
    this.selectedPermissionIds = new Set(r.permissionIds);
    this.error.set('');
    this.formOpen.set(true);
  }

  closeForm() {
    this.formOpen.set(false);
    this.resetForm();
  }

  togglePermission(id: string) {
    if (this.selectedPermissionIds.has(id)) this.selectedPermissionIds.delete(id);
    else this.selectedPermissionIds.add(id);
  }

  allSelected(g: PermissionGroup): boolean {
    return g.permissions.every(p => this.selectedPermissionIds.has(p.id));
  }

  toggleGroup(g: PermissionGroup) {
    const allOn = this.allSelected(g);
    for (const p of g.permissions) {
      if (allOn) this.selectedPermissionIds.delete(p.id);
      else this.selectedPermissionIds.add(p.id);
    }
  }

  onDelete(r: RoleSummary) {
    if (!confirm(`Delete the ${r.name} role?`)) return;
    this.roleAdmin.deleteRole(r.id).subscribe({
      next: () => this.loadAll(),
      error: (e) => alert(e?.error?.errors?.[0] ?? 'Could not delete this role.'),
    });
  }

  submit() {
    this.error.set('');
    this.saving.set(true);
    const body = {
      name: this.name,
      description: this.description || undefined,
      isActive: this.isActive,
      permissionIds: [...this.selectedPermissionIds],
    };

    const req$ = this.editingId()
      ? this.roleAdmin.updateRole(this.editingId()!, body)
      : this.roleAdmin.createRole(body);

    req$.subscribe({
      next: () => { this.saving.set(false); this.closeForm(); this.loadAll(); },
      error: (e) => { this.saving.set(false); this.error.set(e?.error?.errors?.[0] ?? 'Could not save this role.'); },
    });
  }

  private resetForm() {
    this.name = '';
    this.description = '';
    this.isActive = true;
    this.selectedPermissionIds = new Set();
    this.error.set('');
  }
}
