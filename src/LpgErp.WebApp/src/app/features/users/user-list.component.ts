import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, of, Observable } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { UserDto, RoleDto } from '../../core/auth.models';
import { DialogComponent } from '../../shared/dialog.component';
import { DropdownMenuComponent, DropdownMenuItem } from '../../shared/dropdown-menu.component';

/**
 * User accounts and role assignment. The backend (AuthController) and the data layer
 * (AuthService.getUsers/createUser/updateUser/deleteUser/assignRole/removeRole) already existed —
 * this was the one screen in the app with no UI built for it at all, so an admin had no way to
 * create a login, change a role, or deactivate an account except by hand in the database.
 */
@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DialogComponent, DropdownMenuComponent],
  template: `
    <div class="page-header">
      <h1 class="page-title">Users</h1>
      <div class="header-actions">
        <button class="btn-primary-sm" (click)="openNew()">+ New User</button>
      </div>
    </div>

    <div class="table-card">
      <div class="table-toolbar">
        <div class="search-box">
          <input type="text" class="search-input" placeholder="Search users..."
                 [ngModel]="query()" (ngModelChange)="query.set($event)" />
        </div>
        <span class="result-count">{{ filteredUsers().length }} users</span>
      </div>

      <div class="table-scroll">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width:20%">User</th>
              <th style="width:18%">Contact</th>
              <th style="width:22%">Roles</th>
              <th style="width:10%">Status</th>
              <th style="width:14%">Last Login</th>
              <th class="actions-col"></th>
            </tr>
          </thead>
          <tbody>
            @for (u of filteredUsers(); track u.id) {
              <tr class="data-row">
                <td>
                  <div class="main-cell">
                    <span class="main-text">{{ u.fullName || u.username }}</span>
                    <span class="sub-text">&#64;{{ u.username }}{{ isSelf(u) ? ' (you)' : '' }}</span>
                  </div>
                </td>
                <td>
                  <div class="main-cell">
                    <span class="muted-text">{{ u.email }}</span>
                    @if (u.phone) { <span class="sub-text">{{ u.phone }}</span> }
                  </div>
                </td>
                <td>
                  @for (r of u.roles; track r) {
                    <span class="badge role-badge">{{ r }}</span>
                  }
                  @if (u.roles.length === 0) { <span class="muted-text">No roles</span> }
                </td>
                <td>
                  <span class="badge" [style.background]="u.isActive ? '#f0fdf4' : '#fef2f2'" [style.color]="u.isActive ? '#15803d' : '#b91c1c'">
                    {{ u.isActive ? 'Active' : 'Inactive' }}
                  </span>
                </td>
                <td><span class="muted-text">{{ u.lastLoginAt ? (u.lastLoginAt | date:'dd MMM, h:mm a') : 'Never' }}</span></td>
                <td class="actions-col">
                  <app-dropdown-menu [items]="menuFor(u)" (selected)="onMenuAction($event, u)" />
                </td>
              </tr>
            } @empty {
              <tr><td colspan="6" class="empty-row">No users found.</td></tr>
            }
          </tbody>
        </table>
      </div>
    </div>

    <app-dialog [open]="formOpen()" [title]="editingId() ? 'Edit User' : 'New User'" (close)="closeForm()">
      <form (ngSubmit)="submit()">
        @if (!editingId()) {
          <div class="form-group">
            <label>Username</label>
            <input type="text" [(ngModel)]="username" name="username" required autocomplete="off" />
          </div>
          <div class="form-group">
            <label>Password</label>
            <input type="password" [(ngModel)]="password" name="password" required minlength="6" autocomplete="new-password" />
          </div>
        } @else {
          <div class="form-group">
            <label>New Password <span class="optional">(leave blank to keep the current one)</span></label>
            <input type="password" [(ngModel)]="password" name="password" minlength="6" autocomplete="new-password" />
          </div>
        }
        <div class="form-group">
          <label>Email</label>
          <input type="email" [(ngModel)]="email" name="email" required />
        </div>
        <div class="form-group">
          <label>Full Name</label>
          <input type="text" [(ngModel)]="fullName" name="fullName" />
        </div>
        <div class="form-group">
          <label>Phone</label>
          <input type="text" [(ngModel)]="phone" name="phone" />
        </div>
        @if (editingId()) {
          <div class="form-group checkbox-group">
            <label>
              <input type="checkbox" [(ngModel)]="isActive" name="isActive" [disabled]="isSelfEditing()" />
              Active
            </label>
            @if (isSelfEditing()) {
              <small class="field-hint">You cannot deactivate your own account.</small>
            }
          </div>
        }
        <div class="form-group">
          <label>Roles</label>
          <div class="role-grid">
            @for (r of roles(); track r.id) {
              <label class="role-check">
                <input type="checkbox" [checked]="selectedRoleIds.has(r.id)" (change)="toggleRole(r.id)" />
                {{ r.name }}
              </label>
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
    .table-toolbar { display: flex; justify-content: space-between; align-items: center; padding: 14px 18px; border-bottom: 1px solid var(--border); }
    .search-input { padding: 8px 12px; border: 1px solid var(--border-input); border-radius: 7px; font-size: 13px; width: 280px; }
    .result-count { font-size: 12px; color: var(--text-muted); }
    .table-scroll { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 13px; }
    .data-table th { text-align: left; padding: 10px 16px; font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.04em; color: var(--text-muted); border-bottom: 1px solid var(--border); }
    .data-row td { padding: 10px 16px; border-bottom: 1px solid var(--border-row); vertical-align: top; }
    .main-cell { display: flex; flex-direction: column; gap: 2px; }
    .main-text { font-weight: 600; color: var(--text-primary); }
    .sub-text { font-size: 11px; color: var(--text-muted); }
    .muted-text { color: var(--text-secondary); }
    .badge { display: inline-block; padding: 3px 9px; border-radius: 20px; font-size: 11px; font-weight: 600; }
    .role-badge { background: #eff6ff; color: #1d4ed8; margin: 0 4px 4px 0; }
    .actions-col { width: 50px; text-align: center; }
    .empty-row { text-align: center; padding: 40px 14px !important; color: var(--text-muted); }

    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; margin-bottom: 0.25rem; font-weight: 600; font-size: 0.9rem; }
    .form-group label .optional { font-weight: 400; color: #6b7280; font-size: 0.8rem; }
    .form-group input[type="text"], .form-group input[type="email"], .form-group input[type="password"] {
      width: 100%; padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box;
    }
    .checkbox-group label { display: flex; align-items: center; gap: 0.5rem; font-weight: 400; }
    .field-hint { display: block; margin-top: 0.25rem; font-size: 0.75rem; color: #6b7280; }
    .role-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.4rem 1rem; }
    .role-check { display: flex; align-items: center; gap: 0.4rem; font-weight: 400; font-size: 0.9rem; }
    .error { color: #dc3545; font-size: 0.85rem; margin-top: 0.75rem; }
    .form-actions { display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer; border: 1px solid #ddd; }
    .btn-primary { background: #1a1a2e; color: white; border-color: #1a1a2e; }
    .btn-secondary { background: white; color: #333; }
  `],
})
export class UserListComponent implements OnInit {
  private auth = inject(AuthService);

  users = signal<UserDto[]>([]);
  roles = signal<RoleDto[]>([]);
  query = signal('');

  formOpen = signal(false);
  editingId = signal<string | null>(null);
  saving = signal(false);
  error = signal('');

  username = '';
  password = '';
  email = '';
  fullName = '';
  phone = '';
  isActive = true;
  selectedRoleIds = new Set<string>();

  filteredUsers = computed(() => {
    const q = this.query().toLowerCase();
    if (!q) return this.users();
    return this.users().filter(u =>
      u.username.toLowerCase().includes(q) ||
      (u.fullName ?? '').toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q) ||
      u.roles.some(r => r.toLowerCase().includes(q)));
  });

  ngOnInit() {
    this.loadAll();
  }

  loadAll() {
    this.auth.getUsers().subscribe(u => this.users.set(u));
    this.auth.getRoles().subscribe(r => this.roles.set(r));
  }

  isSelf(u: UserDto): boolean {
    return u.username === this.auth.currentUser()?.username;
  }

  isSelfEditing(): boolean {
    const id = this.editingId();
    if (!id) return false;
    return this.users().find(u => u.id === id)?.username === this.auth.currentUser()?.username;
  }

  // A user cannot deactivate or delete their own account — the only path back in is another
  // admin's account, and nothing in this dataset assumes there's a second one.
  menuFor(u: UserDto): DropdownMenuItem[] {
    const self = this.isSelf(u);
    return [
      { label: 'Edit', icon: '✎' },
      { label: u.isActive ? 'Deactivate' : 'Activate', icon: u.isActive ? '⏻' : '✓', disabled: self },
      { label: 'Delete', icon: '🗑', danger: true, disabled: self },
    ];
  }

  onMenuAction(index: number, u: UserDto) {
    if (index === 0) this.openEdit(u);
    else if (index === 1) this.toggleActive(u);
    else this.onDelete(u);
  }

  openNew() {
    this.resetForm();
    this.editingId.set(null);
    this.formOpen.set(true);
  }

  openEdit(u: UserDto) {
    this.editingId.set(u.id);
    this.username = u.username;
    this.password = '';
    this.email = u.email;
    this.fullName = u.fullName ?? '';
    this.phone = u.phone ?? '';
    this.isActive = u.isActive;
    this.selectedRoleIds = new Set(
      this.roles().filter(r => u.roles.includes(r.name)).map(r => r.id));
    this.error.set('');
    this.formOpen.set(true);
  }

  closeForm() {
    this.formOpen.set(false);
    this.resetForm();
  }

  toggleRole(roleId: string) {
    if (this.selectedRoleIds.has(roleId)) this.selectedRoleIds.delete(roleId);
    else this.selectedRoleIds.add(roleId);
  }

  toggleActive(u: UserDto) {
    this.auth.updateUser(u.id, { isActive: !u.isActive }).subscribe({
      next: () => this.loadAll(),
      error: () => this.error.set('Could not update this user.'),
    });
  }

  onDelete(u: UserDto) {
    if (!confirm(`Delete ${u.username}? This cannot be undone.`)) return;
    this.auth.deleteUser(u.id).subscribe({
      next: () => this.loadAll(),
      error: () => alert('Could not delete this user.'),
    });
  }

  submit() {
    this.error.set('');
    this.saving.set(true);

    if (this.editingId()) {
      const id = this.editingId()!;
      const before = this.users().find(u => u.id === id);
      const beforeRoleIds = new Set(
        this.roles().filter(r => before?.roles.includes(r.name)).map(r => r.id));

      const toAdd = [...this.selectedRoleIds].filter(id => !beforeRoleIds.has(id));
      const toRemove = [...beforeRoleIds].filter(id => !this.selectedRoleIds.has(id));

      this.auth.updateUser(id, {
        email: this.email,
        fullName: this.fullName || undefined,
        phone: this.phone || undefined,
        isActive: this.isSelfEditing() ? true : this.isActive,
        newPassword: this.password || undefined,
      }).subscribe({
        next: () => {
          const roleCalls: Observable<void>[] = [
            ...toAdd.map(rid => this.auth.assignRole(id, rid)),
            ...toRemove.map(rid => this.auth.removeRole(id, rid)),
          ];
          const roleUpdate$: Observable<unknown> = roleCalls.length ? forkJoin(roleCalls) : of(null);
          roleUpdate$.subscribe({
            next: () => { this.saving.set(false); this.closeForm(); this.loadAll(); },
            error: () => { this.saving.set(false); this.error.set('User was updated, but a role change failed.'); },
          });
        },
        error: (e) => { this.saving.set(false); this.error.set(e?.error?.errors?.[0] ?? 'Could not update this user.'); },
      });
    } else {
      this.auth.createUser({
        username: this.username,
        email: this.email,
        password: this.password,
        fullName: this.fullName || undefined,
        phone: this.phone || undefined,
        roleIds: [...this.selectedRoleIds],
      }).subscribe({
        next: () => { this.saving.set(false); this.closeForm(); this.loadAll(); },
        error: (e) => { this.saving.set(false); this.error.set(e?.error?.errors?.[0] ?? 'Username may already exist.'); },
      });
    }
  }

  private resetForm() {
    this.username = '';
    this.password = '';
    this.email = '';
    this.fullName = '';
    this.phone = '';
    this.isActive = true;
    this.selectedRoleIds = new Set();
    this.error.set('');
  }
}
