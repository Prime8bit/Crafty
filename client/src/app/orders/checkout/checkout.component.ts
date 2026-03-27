import { Component, inject, OnInit } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { Order } from '../../models/order';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
    selector: 'app-checkout',
    standalone: true,
    imports: [
        RouterLink,
        ReactiveFormsModule,
        CurrencyPipe,
        DatePipe
    ],
    templateUrl: './checkout.component.html',
    styleUrl: './checkout.component.css'
})
export class CheckoutComponent implements OnInit {
    private orderService = inject(OrderService);
    private formBuilder = inject(FormBuilder);
    private router = inject(Router);

    order?: Order;
    checkoutForm: FormGroup = new FormGroup({});
    validationErrors?: string[];

    ngOnInit(): void {
        this.order = this.orderService.cart();
        this.initializeForm();
    }

    initializeForm(): void {
        this.checkoutForm = this.formBuilder.group({
            shippingName: [this.order?.shippingName ?? '', [Validators.required]],
            shippingAddress: [this.order?.shippingAddress ?? '', [Validators.required]], // I would build a custom address validator in 2.0
            billingName: [this.order?.billingName ?? '', [Validators.required]],
            billingAddress: [this.order?.billingAddress ?? '', [Validators.required]],
            cardNumber: ['', [Validators.required]], // I would build a custom credit card validator in 2.0
            ccv: ['', [Validators.required, Validators.pattern('^[0-9]{3}$')]],
        });
    }

    submitOrder(): void {
        if (!this.order) {
            return;
        }

        this.orderService.createOrder(this.order).subscribe({
            next: _ => this.router.navigate(['/user/update'], {queryParams: {'tab':'Orders'}}),
            error: error => this.validationErrors = error.error
        });
    }

    backToCart(): void {
        this.router.navigate(['/cart']);
    }
}
