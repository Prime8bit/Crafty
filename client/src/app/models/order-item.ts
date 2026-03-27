export enum OrderItemStatus
{
    None = 0,
    Pending = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

export class OrderItem {
    id: number = 0;
    quantity: number = 0;
    pricePerCraft: number = 0;
    discount: number = 1;
    status: OrderItemStatus = OrderItemStatus.None;
    orderId: number = 0;
    craftId: number = 0;
    craftName: string = "";
    craftMediaUrl?: string;
    sellerId: number = 0;
    sellerDisplayName: string = "";
    sellerEmail: string = "";
    sellerProfileImageUrl?: string;
}