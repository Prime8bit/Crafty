export class Pagination {
    currentPage = 0;
    itemsPerPage = 0;
    totalItems = 0;
    totalPages = 0;
}

export class PaginatedResult<T> {
    items?: T;
    pagination = new Pagination();
}