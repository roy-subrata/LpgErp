import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ApiResponse, PagedResult, PaginationMeta } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getAll<T>(endpoint: string, page: number = 1, pageSize: number = 100, extraParams?: Record<string, string>): Observable<PagedResult<T>> {
    let params = new HttpParams()
      .set('pageNumber', page.toString())
      .set('pageSize', pageSize.toString());

    if (extraParams) {
      Object.entries(extraParams).forEach(([key, value]) => {
        params = params.set(key, value);
      });
    }

    return this.http.get<ApiResponse<unknown>>(`${this.baseUrl}/${endpoint}`, { params })
      .pipe(
        map(response => {
          const raw = response.data as any;
          const items: T[] = Array.isArray(raw) ? raw : (raw?.items ?? []);
          const pagination = response.pagination
            ?? (Array.isArray(raw) ? undefined : raw?.pagination)
            ?? { pageNumber: 1, pageSize: 10, totalCount: 0, totalPages: 0, hasPreviousPage: false, hasNextPage: false };
          return { items, pagination };
        })
      );
  }

  getAllList<T>(endpoint: string): Observable<T[]> {
    return this.http.get<ApiResponse<T[]>>(`${this.baseUrl}/${endpoint}/list`)
      .pipe(map(response => response.data || []));
  }

  getById<T>(endpoint: string, id: string): Observable<T> {
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}/${endpoint}/${id}`)
      .pipe(map(response => response.data!));
  }

  create<T>(endpoint: string, body: unknown): Observable<T> {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, body)
      .pipe(map(response => response.data!));
  }

  update<T>(endpoint: string, id: string, body: unknown): Observable<T> {
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}/${endpoint}/${id}`, body)
      .pipe(map(response => response.data!));
  }

  delete(endpoint: string, id: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.baseUrl}/${endpoint}/${id}`)
      .pipe(map(() => void 0));
  }

  get<T>(endpoint: string, params?: Record<string, string>): Observable<T> {
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        httpParams = httpParams.set(key, value);
      });
    }
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, { params: httpParams })
      .pipe(map(response => response.data!));
  }

  post<T>(endpoint: string, body: unknown): Observable<T> {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, body)
      .pipe(map(response => response.data!));
  }
}
