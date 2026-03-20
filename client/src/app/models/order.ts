import { OrderItem } from "./order-item";

export enum OrderStatus {
    None = 0,
    Pending = 1,
    PaymentReceived = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

export interface Order {
    id: number,
    orderDate: Date,
    totalPrice: number,
    status: OrderStatus,
    sellerId: number,
    sellerUserName: string,
    buyerId: number,
    buyerName: string,
    buyerAddress: string,
    orderItems: OrderItem[]
}