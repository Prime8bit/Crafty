import { Component, inject, OnInit } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { FormsModule } from '@angular/forms';
import { OrderListType } from '../../models/order-list-params';
import { CurrencyPipe, KeyValuePipe } from '@angular/common';
import { OrderStatus } from '../../models/order';
import { RouterLink } from '@angular/router';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';


@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [
    ButtonsModule, 
    FormsModule,
    PaginationModule,
    BsDropdownModule,
    KeyValuePipe,
    CurrencyPipe,
    RouterLink
  ],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.css'
})
export class OrderListComponent implements OnInit {
    orderService = inject(OrderService);
    // Expose the enum to the template
    orderStatus = OrderStatus;

    pageNumber = 1;
    pageSize = 20;

    orderFilterDict: Record<string, OrderListType> = {"All Orders": OrderListType.All, "Sell Orders": OrderListType.SellOnly, "Buy Orders": OrderListType.BuyOnly}
    selectedFilterOption = Object.keys(this.orderFilterDict)[0];
    orderSortDict: Record<string, string> = {"Date": "orderDate", "Buyer Name":"buyerName", "Seller Username": "sellerUserName"};
    selectedSortOption = Object.keys(this.orderSortDict)[0];
    orderStatusOptions = new Map<OrderStatus, string>([
        [OrderStatus.PaymentReceived, "Payment Received"],
        [OrderStatus.Cancelled, "Cancelled"],
        [OrderStatus.Complete, "Complete"]
    ]);


    ngOnInit(): void {
        this.loadOrders();
    }

    loadOrders(): void {
        this.orderService.orderListParams().typeFilter = this.orderFilterDict[this.selectedFilterOption];
        this.orderService.getOrders(this.pageNumber, this.pageSize);
    }

    resetFilters(): void {
        this.orderService.resetOrderListParams();
        this.sort(this.selectedSortOption);
    }

    setSortOrder(newOrder: boolean): void {
        this.orderService.orderListParams().isOrderDescending = newOrder;
        this.loadOrders();
    }

    sort(sortOption: string): void {
        if (sortOption in this.orderSortDict) {
            this.selectedSortOption = sortOption;
            this.orderService.orderListParams().orderBy = this.orderSortDict[sortOption];
            this.loadOrders();
        }
    }

    pageChanged(event: any): void {
        if (this.pageNumber !== event.page) {
            this.pageNumber = event.page;
            this.loadOrders();
        }
    }
}
