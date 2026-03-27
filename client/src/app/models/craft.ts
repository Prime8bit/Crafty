import { CraftMedia } from "./media"

export interface Craft {
    id: number;
    sellerId: number;
    sellerDisplayName: string;
    name: string;
    price: number;
    description: string;
    stock: number;
    createdAt: string;
    medias: CraftMedia[];
    searchImageId: number | null;
    searchImage: CraftMedia | null;
    isArchived: boolean;
  }
  