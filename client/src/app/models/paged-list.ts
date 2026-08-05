export class PagedList<T> {
    items?: T[];
    currentPage = 0;
    totalPages = 0;
    pageSize = 0;
    totalCount = 0;
}