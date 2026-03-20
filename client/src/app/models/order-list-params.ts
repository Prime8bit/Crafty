import { PaginationParams } from "./pagination-params";

export enum OrderListType
{
    All = 0,
    SellOnly = 1,
    BuyOnly = 2
}

export class OrderListParams extends PaginationParams{
    typeFilter = OrderListType.All;
    showIncompleteOnly = false;
}