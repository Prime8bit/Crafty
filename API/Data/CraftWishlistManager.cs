using API.Data.Configuration;
using API.DTOs;
using API.Entities;
using API.Misc;
using API.Pagination;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class CraftWishlistManager (
    DataContext context, 
    ICraftyUserManager userManager, 
    ICraftManager craftManager): ICraftWishlistManager
{
    public async Task<ManagerResponse> ToggleWishlistItemAsync(CraftWishlistItemDto craftWishlistDto)
    {
        var user = await userManager.GetUserAsync(craftWishlistDto.WishlistingUserId);
        
        if (user == null)
        {
            return new ManagerResponse()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A user with id {craftWishlistDto.WishlistingUserId} does not exist."]
            };
        }

        var craft = await craftManager.GetCraftAsync(craftWishlistDto.WishListedCraftId);

        if (craft == null)
        {
            return new ManagerResponse()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A craft with id {craftWishlistDto.WishListedCraftId} does not exist."]
            };
        }

        if (craft.SellerUserName?.ToUpper() == user.UserName?.ToUpper())
        {            
            return new ManagerResponse()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"You cannot wishlist your own crafts."]
            };
        }

        var craftWishlistItem = await context.CraftWishlistItems
                .Where(craftWishlistItem => craftWishlistItem.WishListedCraftId == craftWishlistDto.WishListedCraftId 
                && craftWishlistItem.WishlistingUserId == craftWishlistDto.WishlistingUserId)
                .FirstOrDefaultAsync();

        if (craftWishlistItem == null)
        {
            var wishlistItem = new WishlistItem()
            {
                WishlistingUserId = craftWishlistDto.WishlistingUserId,
                WishListedCraftId = craftWishlistDto.WishListedCraftId
            };

            context.CraftWishlistItems.Add(wishlistItem);
            if (await context.SaveChangesAsync() == 0)
            {  
                return new ManagerResponse()
                {
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = [$"User with id {craftWishlistDto.WishlistingUserId} failed to wishlist craft with id {craftWishlistDto.WishListedCraftId}."]
                };
            }
            
        }
        else
        {
            context.CraftWishlistItems.Remove(craftWishlistItem);

            if (await context.SaveChangesAsync() == 0)
            {  
                return new ManagerResponse()
                {
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = [$"User with id {craftWishlistDto.WishlistingUserId} failed to delete wishlist for craft with id {craftWishlistDto.WishListedCraftId}."]
                };
            }
        }

        return new ManagerResponse() { ResponseType = ManagerResponseType.Ok };
    }

    public async Task<ManagerResponse<CraftWishlistItemDto>> GetCraftWishlistItemAsync(long userId, long craftId)
    {
        var user = await userManager.GetUserAsync(userId);
        
        if (user == null)
        {
            return new ManagerResponse<CraftWishlistItemDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A user with id {userId} does not exist."]
            };
        }

        var craft = await craftManager.GetCraftAsync(craftId);

        if (craft == null)
        {
            return new ManagerResponse<CraftWishlistItemDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A craft with id {craftId} does not exist."]
            };
        }

        var craftWishlistItem = await context.CraftWishlistItems
                .Select(craftWishlistItem => new CraftWishlistItemDto(craftWishlistItem))
                .Where(craftWishlistItem => craftWishlistItem.WishListedCraftId == craftId && craftWishlistItem.WishlistingUserId == userId)
                .FirstOrDefaultAsync();

        if (craftWishlistItem == null)
        {
            // It is not an error for an item to not be wishlisted.
            return new ManagerResponse<CraftWishlistItemDto>() { ResponseType = ManagerResponseType.Ok };
        }

        return new ManagerResponse<CraftWishlistItemDto>(craftWishlistItem);
    }

    public async Task<ManagerResponse<List<long>>> GetWishlistedCraftIdsForUserAsync(long userId)
    {
        var user = await userManager.GetUserAsync(userId);
        
        if (user == null)
        {
            return new ManagerResponse<List<long>>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A user with id {userId} does not exist."]
            };
        }

        return new ManagerResponse<List<long>>(await context.CraftWishlistItems
            .Where(wishlist => wishlist.WishlistingUserId == userId)
            .Select(wishlist => wishlist.WishListedCraftId)
            .ToListAsync());
    }

    public async Task<ManagerResponse<PagedList<CraftDto>>> GetWishlistedCraftsForUserAsync(long userId, CraftListParams craftListParams)
    {
        var user = await userManager.GetUserAsync(userId);
        
        if (user == null)
        {
            return new ManagerResponse<PagedList<CraftDto>>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A user with id {userId} does not exist."]
            };
        }

        var query = context.CraftWishlistItems
            .Where( wishlist => wishlist.WishlistingUserId == userId
                && wishlist.WishListedCraft.Price >= craftListParams.MinPrice 
                && wishlist.WishListedCraft.Price <= craftListParams.MaxPrice);

        if (craftListParams.InStockOnly)
        {
            query = query.Where(wishlist => wishlist.WishListedCraft.Stock > 0);
        }

        query = craftListParams.OrderBy.ToLower() switch
        {
            "price" => craftListParams.IsOrderDescending ? 
                query.OrderByDescending(wishlist => wishlist.WishListedCraft.Price) 
                : query.OrderBy(wishlist => wishlist.WishListedCraft.Price),
            "name" => craftListParams.IsOrderDescending ? 
                query.OrderByDescending(wishlist => wishlist.WishListedCraft.Name) 
                : query.OrderBy(wishlist => wishlist.WishListedCraft.Name),
            _ => craftListParams.IsOrderDescending ? 
                query.OrderByDescending(wishlist => wishlist.WishListedCraft.CreatedAt) 
                : query.OrderBy(wishlist => wishlist.WishListedCraft.CreatedAt),
        };

        var resultQuery = query.Include(wishlist => wishlist.WishListedCraft.Seller)
            .Select(wishlist => new CraftDto()
            {
                Id = wishlist.WishListedCraft.Id,
                Name = wishlist.WishListedCraft.Name,
                Price = wishlist.WishListedCraft.Price,
                Stock = wishlist.WishListedCraft.Stock,
                CreatedAt = wishlist.WishListedCraft.CreatedAt.ToString("o"), // ISO 8601 format,
                SellerUserName = wishlist.WishListedCraft.Seller.UserName,
                SellerDisplayName = wishlist.WishListedCraft.Seller.DisplayName,
                SearchImageId = wishlist.WishListedCraft.SearchImageId,
                SearchImage = wishlist.WishListedCraft.SearchImage == null ? 
                    null :
                    new CraftMediaDto(wishlist.WishListedCraft.SearchImage){ CraftName = wishlist.WishListedCraft.Name },
                // I intentionally leave out the Media collection here since the list endpoint doesn't need it, and it would be a waste of resources to include it.            
                IsArchived = wishlist.WishListedCraft.IsArchived
            });
        return new ManagerResponse<PagedList<CraftDto>>(
            await PagedList<CraftDto>.CreateAsync(resultQuery, craftListParams.PageNumber, craftListParams.PageSize));
    }

    public async Task<ManagerResponse<int>> GetNumLikesForCraftAsync(long craftId)
    {
        var craft = await craftManager.GetCraftAsync(craftId);

        if (craft == null)
        {
            return new ManagerResponse<int>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A craft with id {craftId} does not exist."]
            };
        }

        return new ManagerResponse<int>(await context.CraftWishlistItems
            .Where(wishlist => wishlist.WishListedCraftId == craftId)
            .CountAsync());
    }
}