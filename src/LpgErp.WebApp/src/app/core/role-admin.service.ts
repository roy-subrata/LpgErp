import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from 'src/environments/environment';
import { RoleSummary, CreateRoleRequest, UpdateRoleRequest, PermissionDto } from './auth.models';

/**
 * Role *definitions* — separate from AuthService, which only ever reads roles to populate a
 * picker. This talks to RolesController, the piece that lets an admin create a role or change
 * what permissions it grants, rather than that being fixed forever at seed time.
 */
@Injectable({ providedIn: 'root' })
export class RoleAdminService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getRoles(): Observable<RoleSummary[]> {
    return this.http.get<{ success: boolean; data: RoleSummary[] }>(`${this.baseUrl}/Roles`)
      .pipe(map(res => res.data));
  }

  getPermissions(): Observable<PermissionDto[]> {
    return this.http.get<{ success: boolean; data: PermissionDto[] }>(`${this.baseUrl}/Auth/permissions`)
      .pipe(map(res => res.data));
  }

  createRole(request: CreateRoleRequest): Observable<RoleSummary> {
    return this.http.post<{ success: boolean; data: RoleSummary }>(`${this.baseUrl}/Roles`, request)
      .pipe(map(res => res.data));
  }

  updateRole(id: string, request: UpdateRoleRequest): Observable<RoleSummary> {
    return this.http.put<{ success: boolean; data: RoleSummary }>(`${this.baseUrl}/Roles/${id}`, request)
      .pipe(map(res => res.data));
  }

  deleteRole(id: string): Observable<void> {
    return this.http.delete<{ success: boolean }>(`${this.baseUrl}/Roles/${id}`)
      .pipe(map(() => void 0));
  }
}
