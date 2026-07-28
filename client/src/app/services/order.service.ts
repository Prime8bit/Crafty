import { computed, inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { PaginatedList } from '../models/pagination';
import { Order, OrderStatus } from '../models/order';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { OrderListParams } from '../models/order-list-params';
import { OrderListItem } from '../models/order-list-item';
import { OrderItem, OrderItemStatus } from '../models/order-item';
import { Craft } from '../models/craft';
import { User } from '../models/user';
import { Observable, Subscription } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
    private http = inject(HttpClient);
    private cartKey = "cart";

    baseUrl = environment.apiUrl;

    paginatedResult = signal<PaginatedList<OrderListItem>>(new PaginatedList<OrderListItem>());
    orderListParams = signal<OrderListParams>(new OrderListParams());
    // Record should be used instead of Map because Map doesn't play well with Angular's signal change detection

    cart = signal<Order>({
        id: 0,
        orderDate: new Date(Date.now()),
        totalPrice: 0,
        shippingName: "",
        shippingAddress: "",
        billingName: "",
        billingAddress: "",
        // Obviously I wouldn't send credit card numbers unencyprted in a real application
        // but for this portfolio piece, I don't want to complicate things with encryption
        creditCardNumber: "",
        ccv: 0,
        status: OrderStatus.PaymentReceived,
        buyerId: 0,
        buyerDisplayName: "",
        buyerEmail: "",
        buyerProfileImageUrl: "",
        orderItems: []
    } as Order)
    readonly cartCount = computed(() => this.cart().orderItems.length);
    
    resetOrderListParams(): void {
        this.orderListParams.set(new OrderListParams());
    }

    getOrders(pageNumber: number, pageSize: number): Subscription {
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

        return this.http.get<OrderListItem[]>(`${this.baseUrl}orders`, {observe: 'response', params}).subscribe({
            next: response => this.setPaginatedResponse(response)
        });
    }

    getOrder(id: number): Observable<Order> {
        return this.http.get<Order>(`${this.baseUrl}orders/${id}`);
    }

    createOrder(order: Order): Observable<Order> {
        return this.http.post<Order>(`${this.baseUrl}orders/`, order);
    }

    setOrderStatus(orderId: number, newStatus: OrderStatus): Observable<Order> {
        return this.http.put<Order>(`${this.baseUrl}orders/${orderId}/setStatus/${newStatus}`, null);
    }

    setOrderItemStatus(orderItemId: number, newStatus: OrderItemStatus): Observable<OrderItem> {
        return this.http.put<OrderItem>(`${this.baseUrl}orders/withOrderItem/${orderItemId}/setStatus/${newStatus}`, null);
    }

    addToCart(craft: Craft): void {
        const newCart = this.cart();
        const existingItem = newCart.orderItems.find(orderItem => orderItem.craftId === craft.id);

        if (existingItem) {
            existingItem.quantity ++;
            newCart.totalPrice += existingItem.pricePerCraft;
        } else {
            const orderItem = new OrderItem();
            orderItem.pricePerCraft = craft.price;
            orderItem.status = OrderItemStatus.Pending;
            orderItem.craftId = craft.id;
            orderItem.craftName = craft.name;
            orderItem.craftMediaUrl = craft.searchImage?.url;
            orderItem.sellerId = craft.sellerId;
            orderItem.sellerDisplayName = craft.sellerDisplayName;
            orderItem.quantity = 1;
            newCart.orderItems.push(orderItem);
            newCart.totalPrice += orderItem.pricePerCraft;
        }

        this.cart.set({...newCart});
        localStorage.setItem(this.cartKey, JSON.stringify(this.cart()));
    }

    // This function assumes it already exists in the this.cart() dictionary
    // If not, this function does nothing.
    setCartItemQuantity(craftId: number, quantity: number): void {
        const newCart = this.cart();
        const existingItem = newCart.orderItems.find(orderItem => orderItem.craftId === craftId);
        if (!existingItem) {
            return;
        }

        if (quantity > 0) {
            newCart.totalPrice += (quantity - existingItem.quantity) * existingItem.pricePerCraft;
            existingItem.quantity = quantity;
        } else {
            newCart.totalPrice -= existingItem.quantity * existingItem.pricePerCraft;
            newCart.orderItems = newCart.orderItems.filter(orderItem => orderItem.craftId !== craftId);
        }
        
        this.cart.set(newCart);
        localStorage.setItem(this.cartKey, JSON.stringify(this.cart()));
    }

    loadCartFromStorage(): void {
        const cartString = localStorage.getItem(this.cartKey);
        if (!cartString) {
            return;
        }
        
        this.cart.set(JSON.parse(cartString) as Order);
    }

    deleteCart(): void {
        this.cart.set({
            id: 0,
            orderDate: new Date(Date.now()),
            totalPrice: 0,
            shippingName: "",
            shippingAddress: "",
            billingName: "",
            billingAddress: "",
            // Obviously I wouldn't send credit card numbers unencyprted in a real application
            // but for this portfolio piece, I don't want to complicate things with encryption
            creditCardNumber: "",
            ccv: 0,
            status: OrderStatus.PaymentReceived,
            buyerId: 0,
            buyerDisplayName: "",
            buyerEmail: "",
            buyerProfileImageUrl: "",
            orderItems: []
        } as Order);

        localStorage.removeItem(this.cartKey);
    }

    updateCartWithUser(user: User): void {
        const newCart = this.cart();
        newCart.buyerId = user.id;
        newCart.buyerDisplayName = user.displayName;
        newCart.buyerEmail = user.email;
        newCart.buyerProfileImageUrl = user.profileImage.url;
        if (!newCart.shippingAddress || newCart.shippingAddress == "") {
            newCart.shippingName = user.fullName;
            newCart.shippingAddress = user.address;
        }
        if (!newCart.billingAddress || newCart.billingAddress == "") {
            newCart.billingName = user.fullName;
            newCart.billingAddress = user.address;
        }

        this.cart.set(newCart);
        localStorage.setItem(this.cartKey, JSON.stringify(this.cart()));
    }

    private setPaginatedResponse(response: HttpResponse<OrderListItem[]>): void {
        this.paginatedResult.set({
            items: response.body as OrderListItem[], 
            pagination: JSON.parse(response.headers.get('Pagination')!)
        })
    }
}
