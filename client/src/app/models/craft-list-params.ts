import { PaginationParams } from "./pagination-params";

export enum ArchiveFilterType
{
    All = 1,
    NotArchivedOnly = 2,
    ArchivedOnly = 3
}

export class CraftListParams extends PaginationParams{
    minPrice = 0.0;
    maxPrice = 9999.0;
    inStockOnly = false;
    archiveFilter = ArchiveFilterType.NotArchivedOnly;
}