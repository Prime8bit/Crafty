import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AccountService } from './services/account.service';

import { HomeComponent } from './home/home.component';
import { NavComponent } from './nav/nav.component';
import { NgxSpinnerComponent } from 'ngx-spinner';
import { OrderService } from './services/order.service';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [
        HomeComponent, 
        NavComponent, 
        RouterOutlet,
        NgxSpinnerComponent
    ],
    templateUrl: './app.component.html',
    styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
    private accountService = inject(AccountService);
    private orderService = inject(OrderService);
    title = 'Crafty';

    ngOnInit(): void { 
        this.accountService.loadUserFromStorage();
        this.orderService.loadCartFromStorage();
    }
}
