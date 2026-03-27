import { OrderItem } from "./order-item";

export enum OrderStatus {
    None = 0,
    PaymentReceived = 1,
    Cancelled = 2,
    Complete = 3
}

export interface Order {
    id: number;
    orderDate: Date;
    totalPrice: number;
    shippingName: string;
    shippingAddress: string;
    billingName: string;
    billingAddress: string;
    // Obviously I wouldn't send credit card numbers unencyprted in a real application
    // but for this portfolio piece, I don't want to complicate things with encryption
    creditCardNumber: string;
    ccv: number;
    status: OrderStatus;
    buyerId: number;
    buyerDisplayName: string;
    buyerEmail: string;
    buyerProfileImageUrl: string;
    orderItems: OrderItem[]
}