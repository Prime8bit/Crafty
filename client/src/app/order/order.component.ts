import { Component, inject, OnInit } from '@angular/core';
import { OrderService } from '../services/order.service';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { FormsModule } from '@angular/forms';
import { OrderListType } from '../models/order-list-params';
import { CurrencyPipe, KeyValuePipe } from '@angular/common';
import { Order, OrderStatus } from '../models/order';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ToastrService } from 'ngx-toastr';
import { User } from '../models/user';
import { AccountService } from '../services/account.service';
import { UserService } from '../services/user.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

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
    RouterLink
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

    // Expose the enum to the template
    orderStatus = OrderStatus;

    orderFilterDict: Record<string, OrderListType> = {"All Orders": OrderListType.All, "Sell Orders": OrderListType.SellOnly, "Buy Orders": OrderListType.BuyOnly}
    selectedFilterOption = Object.keys(this.orderFilterDict)[0];
    orderSortDict: Record<string, string> = {"Date": "orderDate", "Buyer Name":"buyerName", "Seller Username": "sellerUserName"};
    selectedSortOption = Object.keys(this.orderSortDict)[0];

    user?: User;
    order?: Order;

    ngOnInit(): void {
        this.loadUser();
        this.loadOrder();
    }

    loadUser() {
        const user = this.accountService.currentUser();
        if (!user) {
            return;
        }

        this.userService.getUser(user.userName).subscribe({
            next: (user) => {
                this.user = user;
            } 
        });
    }

    loadOrder(): void {
        const orderIdStr = this.route.snapshot.paramMap.get('orderId');

        if (!orderIdStr) {
            return;
        }

        this.orderService.getOrder(orderIdStr).subscribe({
            next: order => this.order = order,
            error: error => this.toastr.error(error)
        });
    }
}
