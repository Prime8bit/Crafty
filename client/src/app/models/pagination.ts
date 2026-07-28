export class PaginationHeader {
    currentPage = 0;
    itemsPerPage = 0;
    totalItems = 0;
    totalPages = 0;
}

export class PaginatedList<T> {
    items?: T[];
    pagination = new PaginationHeader();
}