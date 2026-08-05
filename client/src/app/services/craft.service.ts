import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Craft } from '../models/craft';
import { PagedList } from '../models/paged-list';
import { CraftListParams } from '../models/craft-list-params';
import { map, Observable, of, tap } from 'rxjs';
import { PaginationParams } from '../models/pagination-params';

@Injectable({
    providedIn: 'root'
})

export class CraftService {
    private http = inject(HttpClient);
    private craftListCache = new Map<string, HttpResponse<PagedList<Craft>>>();
    private craftCache = new Map<string, Craft>();

    baseUrl = environment.apiUrl;
    paginatedResult = signal<PagedList<Craft>>(new PagedList<Craft>());
    craftListParams = signal<CraftListParams>(new CraftListParams());

    resetCraftListParams(): void {
        this.craftListParams.set(new CraftListParams());
    }
    
    getCrafts(): void {
        const cacheKey = Object.values(this.craftListParams()).join('-');
        const response = this.craftListCache.get(cacheKey);
        if (response) {
            this.setPaginatedResponse(response);
        }
        
        let params = new HttpParams();
        if (this.craftListParams().pageNumber && this.craftListParams().pageSize) {
            params = params.append("pageNumber", this.craftListParams().pageNumber);
            params = params.append("pageSize", this.craftListParams().pageSize);
        }

        params = params.append("minPrice", this.craftListParams().minPrice);
        params = params.append("maxPrice", this.craftListParams().maxPrice);
        params = params.append("inStockOnly", this.craftListParams().inStockOnly);
        params = params.append("orderBy", this.craftListParams().orderBy);
        params = params.append("isOrderDescending", this.craftListParams().isOrderDescending);
        params = params.append("archiveFilter", this.craftListParams().archiveFilter);

        this.http.get<PagedList<Craft>>(`${this.baseUrl}crafts`, { observe: 'response', params}).subscribe({
            next: response => {
                this.setPaginatedResponse(response);
                this.craftListCache.set(cacheKey, response);
            }
        });
    }

    getCraft(idStr: string): Observable<Craft> {
        const craft = this.craftCache.get(idStr);

        if (craft) return of(craft);

        return this.http.get<Craft>(`${this.baseUrl}crafts/${idStr}`).pipe(
            tap((newCraft : Craft) => this.craftCache.set(idStr, newCraft))
        );
    }

    newCraft(model: Craft): Observable<Craft> {
        return this.http.post<Craft>(`${this.baseUrl}crafts`, model);
    }

    updateCraft(idStr: string, model: Craft): Observable<Craft> {
        return this.http.put<Craft>(`${this.baseUrl}crafts/${idStr}`, model);
    }

    archiveCraft(id: number): Observable<Craft> {
        return this.http.put<Craft>(`${this.baseUrl}crafts/${id}/archive`, {});
    }

    markCraftAsInappropriate(id: number): Observable<Craft> {
        return this.http.put<Craft>(`${this.baseUrl}crafts/${id}/inappropriate`, {});
    }

    markCraftAsAppropriate(id: number): Observable<Craft> {
        return this.http.put<Craft>(`${this.baseUrl}crafts/${id}/appropriate`, {});
    }

    getInappropriateCrafts(paginationParams: PaginationParams): Observable<PagedList<Craft>> {
        let params = new HttpParams();
        if (paginationParams.pageNumber && paginationParams.pageSize) {
            params = params.append("pageNumber", paginationParams.pageNumber);
            params = params.append("pageSize", paginationParams.pageSize);
        }

        return this.http.get<PagedList<Craft>>(`${this.baseUrl}crafts/inappropriate`, { observe: 'response', params}).pipe(
            map((response: HttpResponse<PagedList<Craft>>) => response.body as PagedList<Craft>)
        );
    }

    private setPaginatedResponse(response: HttpResponse<PagedList<Craft>> ): void {
        this.paginatedResult.set(response.body as PagedList<Craft>);
    }
}