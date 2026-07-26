import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, map } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AuthResult, UserDto, LoginRequest, RegisterRequest, RoleDto, PermissionDto } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = environment.apiUrl;
  private readonly TOKEN_KEY = 'lpg_access_token';
  private readonly REFRESH_KEY = 'lpg_refresh_token';
  private readonly USER_KEY = 'lpg_user';

  currentUser = signal<UserDto | null>(null);
  isAuthenticated = computed(() => !!this.currentUser());

  constructor(private http: HttpClient, private router: Router) {
    this.loadFromStorage();
  }

  login(request: LoginRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/Auth/login`, request).pipe(
      tap(result => {
        if (result.isSuccess && result.token) {
          this.setSession(result);
        }
      })
    );
  }

  register(request: RegisterRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/Auth/register`, request).pipe(
      tap(result => {
        if (result.isSuccess && result.token) {
          this.setSession(result);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  hasPermission(permission: string): boolean {
    const user = this.currentUser();
    return user?.permissions?.includes(permission) ?? false;
  }

  hasRole(role: string): boolean {
    const user = this.currentUser();
    return user?.roles?.includes(role) ?? false;
  }

  getRoles(): Observable<RoleDto[]> {
    return this.http.get<{ success: boolean; data: RoleDto[] }>(`${this.baseUrl}/Auth/roles`)
      .pipe(map(res => res.data));
  }

  getPermissions(): Observable<PermissionDto[]> {
    return this.http.get<{ success: boolean; data: PermissionDto[] }>(`${this.baseUrl}/Auth/permissions`)
      .pipe(map(res => res.data));
  }

  getUsers(): Observable<UserDto[]> {
    return this.http.get<{ success: boolean; data: UserDto[] }>(`${this.baseUrl}/Auth/users`)
      .pipe(map(res => res.data));
  }

  createUser(request: any): Observable<UserDto> {
    return this.http.post<{ success: boolean; data: UserDto }>(`${this.baseUrl}/Auth/users`, request)
      .pipe(map(res => res.data));
  }

  updateUser(id: string, request: any): Observable<UserDto> {
    return this.http.put<{ success: boolean; data: UserDto }>(`${this.baseUrl}/Auth/users/${id}`, request)
      .pipe(map(res => res.data));
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<{ success: boolean }>(`${this.baseUrl}/Auth/users/${id}`)
      .pipe(map(() => void 0));
  }

  assignRole(userId: string, roleId: string): Observable<void> {
    return this.http.post<{ success: boolean }>(`${this.baseUrl}/Auth/users/${userId}/roles/${roleId}`, {})
      .pipe(map(() => void 0));
  }

  removeRole(userId: string, roleId: string): Observable<void> {
    return this.http.delete<{ success: boolean }>(`${this.baseUrl}/Auth/users/${userId}/roles/${roleId}`)
      .pipe(map(() => void 0));
  }

  private setSession(result: AuthResult): void {
    if (result.token) localStorage.setItem(this.TOKEN_KEY, result.token);
    if (result.refreshToken) localStorage.setItem(this.REFRESH_KEY, result.refreshToken);
    if (result.user) {
      localStorage.setItem(this.USER_KEY, JSON.stringify(result.user));
      this.currentUser.set(result.user);
    }
  }

  private loadFromStorage(): void {
    try {
      const userJson = localStorage.getItem(this.USER_KEY);
      if (userJson) {
        this.currentUser.set(JSON.parse(userJson));
      }
    } catch {
      this.logout();
    }
  }
}
