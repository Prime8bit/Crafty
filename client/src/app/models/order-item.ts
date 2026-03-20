import { CraftMedia } from "./media";

export interface OrderItem {
    id: number;
    quantity: number;
    pricePerCraft: number;
    discount: number;
    craftId: number;
    craftName: string;
    craftImage: CraftMedia;    
}