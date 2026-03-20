using API.DTOs;
using API.Entities;
using API.Misc;
using API.Pagination;

namespace API.Data;

public interface ICraftManager
{
    Task<PagedList<CraftDto>> GetCraftsAsync(CraftListParams craftListParams);
    Task<CraftDto?> GetCraftAsync(long id);
    Task<ManagerResponse<CraftDto>> CreateCraftAsync(CraftDto craftDto);
    Task<ManagerResponse<CraftDto>> UpdateCraftAsync(long userId, CraftDto craftDto);
    Task<ManagerResponse<CraftDto>> ArchiveCraftAsync(long userId, long craftId);
    Task<ManagerResponse> MarkCraftAsInappropriateAsync(long userId, long craftId);
    Task<ManagerResponse> MarkCraftAsAppropriateAsync(long craftId);
    Task<PagedList<CraftDto>> GetInappropriateCraftsAsync(PaginationParams paginationParams);       
}
