import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { PaginatedResult } from '../models/pagination';
import { Order, OrderStatus } from '../models/order';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { OrderListParams } from '../models/order-list-params';
import { of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
    baseUrl = environment.apiUrl;
    private http = inject(HttpClient);
    paginatedResult = signal<PaginatedResult<Order[]>>(new PaginatedResult<Order[]>());
    orderListParams = signal<OrderListParams>(new OrderListParams());
    
    resetOrderListParams()
    {
        this.orderListParams.set(new OrderListParams());
    }

    getOrders(pageNumber: number, pageSize: number)
    {
        this.orderListParams().pageNumber = pageNumber;
        this.orderListParams().pageSize = pageSize;

        let params = new HttpParams();
        if (this.orderListParams().pageNumber && this.orderListParams().pageSize) {
            params = params.append("pageNumber", this.orderListParams().pageNumber);
            params = params.append("pageSize", this.orderListParams().pageSize);
        }

        params = params.append("typeFilter", this.orderListParams().typeFilter);
        params = params.append("showIncompleteOnly", this.orderListParams().showIncompleteOnly);

        params = params.append("orderBy", this.orderListParams().orderBy);
        params = params.append("isOrderDescending", this.orderListParams().isOrderDescending)

        return this.http.get<Order[]>(`${this.baseUrl}orders`, {observe: 'response', params}).subscribe({
            next: response => this.setPaginatedResponse(response)
        });
    }

    getOrder(idStr: string) {
        return this.http.get<Order>(`${this.baseUrl}orders/${idStr}`);
    }

    setOrderStatus (order: Order, status: OrderStatus)
    {
        if (order.status !== status)
        {
            order.status = status;
            return this.http.put<Order>(`${this.baseUrl}orders/${order.id}`, order);
        }

        // This shouldn't happen since the user should check the status before attempting to set it,
        // but better safe than sorry.
        return of(order);
    }

    private setPaginatedResponse(response: HttpResponse<Order[]> ) {
            this.paginatedResult.set({
                items: response.body as Order[], 
                pagination: JSON.parse(response.headers.get('Pagination')!)
            })
        }
}
