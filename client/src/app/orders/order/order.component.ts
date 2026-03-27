import { Component, inject, input, OnInit } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe, DatePipe, KeyValuePipe } from '@angular/common';
import { Order, OrderStatus } from '../../models/order';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ToastrService } from 'ngx-toastr';
import { User } from '../../models/user';
import { AccountService } from '../../services/account.service';
import { UserService } from '../../services/user.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { OrderItem, OrderItemStatus } from '../../models/order-item';

@Component({
  selector: 'app-order',
  standalone: true,
  imports: [
    ButtonsModule, 
    FormsModule,
    PaginationModule,
    BsDropdownModule,
    KeyValuePipe,
    CurrencyPipe,
    RouterLink,
    DatePipe
  ],
  templateUrl: './order.component.html',
  styleUrl: './order.component.css'
})
export class OrderComponent implements OnInit {
    orderService = inject(OrderService);
    private accountService = inject(AccountService);
    private userService = inject(UserService);
    private toastr = inject(ToastrService);
    private route: ActivatedRoute = inject(ActivatedRoute);

    isCart = false;

    // Expose the enum to the template
    orderStatus = OrderStatus;
    orderStatusOptions = new Map<OrderStatus, string>([
        [OrderStatus.PaymentReceived, "Payment Received"],
        [OrderStatus.Cancelled, "Cancelled"]
    ]);
    orderStatusStrings = new Map<OrderStatus, string>([
        [OrderStatus.PaymentReceived, "Payment Received"],
        [OrderStatus.Cancelled, "Cancelled"],
        [OrderStatus.Complete, "Complete"]
    ]);
    selectedOrderStatus = OrderStatus.Cancelled;

    
    orderItemStatus = OrderItemStatus;
    orderItemStatusOptions = new Map<OrderItemStatus, string>([
        [OrderItemStatus.Pending, "Pending"],
        [OrderItemStatus.Shipped, "Shipped"],
        [OrderItemStatus.Delivered, "Delivered"],
        [OrderItemStatus.Cancelled, "Cancelled"]
    ]);
    selectedOrderItemStatusDict = new Map<number, OrderItemStatus>();

    user?: User;
    order?: Order;
    isOrderDirty = false;
    isUserBuyer = true;

    ngOnInit(): void {
        this.loadUser();
    }

    loadUser(): void {
        const user = this.accountService.currentUser();
        if (!user) {
            return;
        }

        this.userService.getUser(user.userId).subscribe({
            next: (user) => {
                this.user = user;
                this.loadOrder();
            } 
        });
    }

    loadOrder(): void {
        const orderId = Number(this.route.snapshot.paramMap.get('orderId'));

        if (!orderId) {
            if (this.user) {
                this.orderService.updateCartWithUser(this.user);
            }
            this.order = this.orderService.cart();
            this.selectedOrderStatus = this.order.status;
            this.isCart = true;
            this.isUserBuyer = true;
        } else {
            this.orderService.getOrder(orderId).subscribe({
                next: order => {
                    this.order = order;
                    this.selectedOrderStatus = this.order.status;
                    this.order.orderItems.map(orderItem => this.selectedOrderItemStatusDict.set(orderItem.id, orderItem.status));
                    // The order should be loaded first so the check if the current user is the order's buyer can be run.                
                    this.isUserBuyer = this.user?.id === order.buyerId;
                },
                error: error => this.toastr.error(error)
            });
        }
    }

    updateOrderStatus(newStatus: OrderStatus): void {
        if (newStatus == OrderStatus.None || !this.order)
        {
            return;
        }

        this.orderService.setOrderStatus(this.order.id, newStatus);
    }

    setSelectedOrderStatus(newStatus: OrderStatus): void {
        this.selectedOrderStatus = newStatus;
    }

    setOrderStatus(): void {
        if (!this.order)
        {
            return;
        }

        this.orderService.setOrderStatus(this.order.id, this.selectedOrderStatus).subscribe({
            next: _ => this.toastr.info("Order Status changed."),
            error: error => {
                this.selectedOrderStatus = this.order!.status,
                this.toastr.error(error.error)
            }
        })
    }

    setSelectedOrderItemStatus(orderItemId: number, newStatus: OrderItemStatus): void {
        this.selectedOrderItemStatusDict.set(orderItemId, newStatus);
    }

    setOrderItemStatus(orderItemId: number): void {
        const selectedStatus = this.selectedOrderItemStatusDict.get(orderItemId);
        if (!this.order || !selectedStatus)
        {
            return;
        }

        this.orderService.setOrderItemStatus(orderItemId, selectedStatus).subscribe({
            next: _ => this.toastr.info("Order Item Status changed."),
            error: error => {
                const orderItem = this.order!.orderItems.find(orderItem => orderItem.id === orderItemId);
                if (orderItem) {
                    this.selectedOrderItemStatusDict.set(orderItemId, orderItem.status);
                }
                this.toastr.error(error.error)
            }
        })
    }

    onOrderItemQuantityChanged(event: any, orderItem: OrderItem): void {
        const value = event.target.value as number;
        if (!value || !this.order)
        {
            return;
        }

        // The service still needs to be updated in case the user disconnects and the
        // value must be loaded from local storage
        this.setCartItemQuantity(orderItem.craftId, value);
    }
    
    setCartItemQuantity(craftId: number, value: number): void {
        if (!this.order) {
            return;
        }

        // The service still needs to be updated in case the user disconnects and the
        // value must be loaded from local storage
        this.orderService.setCartItemQuantity(craftId, value);
        this.order = this.orderService.cart();
    }
}
