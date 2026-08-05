import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Craft } from '../models/craft';
import { PagedList } from '../models/paged-list';
import { CraftListParams } from '../models/craft-list-params';
import { Observable, Subscription } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class WishlistService {
    private http = inject(HttpClient)

    baseUrl = environment.apiUrl;
    paginatedResult = signal<PagedList<Craft>>(new PagedList<Craft>());
    wishlistIds = signal<number[]>([]);
    craftListParams = signal<CraftListParams>(new CraftListParams());
    
    resetCraftListParams(): void {
        this.craftListParams.set(new CraftListParams());
    }

    toggleWishlist(craftId: number): Observable<Object> {
        return this.http.post(`${this.baseUrl}craftwishlists/${craftId}`, {})
    }

    getWishlist(pageNumber: number, pageSize: number): Subscription {
        this.craftListParams().pageNumber = pageNumber;
        this.craftListParams().pageSize = pageSize;

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

        return this.http.get<PagedList<Craft>>(`${this.baseUrl}craftwishlists`, {observe: 'response', params}).subscribe({
            next: response => this.setPaginatedResponse(response)
        });
    }

    getWishlistIds(): Subscription {
        return this.http.get<number[]>(`${this.baseUrl}craftwishlists/ids`).subscribe({
            next: ids => this.wishlistIds.set(ids)
        })
    }

    private setPaginatedResponse(response: HttpResponse<PagedList<Craft>>): void {
        this.paginatedResult.set(response.body as PagedList<Craft>)
    }
} 
