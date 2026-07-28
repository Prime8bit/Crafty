using API.Data.Configuration;
using API.Entities;
using API.Misc;
using CraftyCommon.Pagination;
using CraftyCommon.DTOs;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class CraftWishlistManager (
    DataContext context,
    UserManager<User> userManager): ICraftWishlistManager
{
    public async Task<ManagerResponse> ToggleWishlistItemAsync(WishlistItemDto craftWishlistDto)
    {

        var craft = await context.Crafts.FindAsync(craftWishlistDto.WishlistedCraftId);

        if (craft == null)
        {
            return new ManagerResponse()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A craft with id {craftWishlistDto.WishlistedCraftId} does not exist."]
            };
        }

        if (craft.SellerId == craftWishlistDto.WishlistingUserId)
        {            
            return new ManagerResponse()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"You cannot wishlist your own crafts."]
            };
        }

        var craftWishlistItem = await context.CraftWishlistItems
                .Where(craftWishlistItem => craftWishlistItem.WishlistedCraftId == craftWishlistDto.WishlistedCraftId 
                && craftWishlistItem.WishlistingUserId == craftWishlistDto.WishlistingUserId)
                .FirstOrDefaultAsync();

        if (craftWishlistItem == null)
        {
            var wishlistItem = new WishlistItem()
            {
                WishlistingUserId = craftWishlistDto.WishlistingUserId,
                WishlistedCraftId = craftWishlistDto.WishlistedCraftId
            };

            context.CraftWishlistItems.Add(wishlistItem);
            if (await context.SaveChangesAsync() == 0)
            {  
                return new ManagerResponse()
                {
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = [$"User with id {craftWishlistDto.WishlistingUserId} failed to wishlist craft with id {craftWishlistDto.WishlistedCraftId}."]
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
                    ErrorMessages = [$"User with id {craftWishlistDto.WishlistingUserId} failed to delete wishlist for craft with id {craftWishlistDto.WishlistedCraftId}."]
                };
            }
        }

        return new ManagerResponse() { ResponseType = ManagerResponseType.Ok };
    }

    public async Task<ManagerResponse<WishlistItemDto>> GetCraftWishlistItemAsync(long userId, long craftId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            return new ManagerResponse<WishlistItemDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A user with id {userId} does not exist."]
            };
        }

        var craft = await context.Crafts.FindAsync(craftId);

        if (craft == null)
        {
            return new ManagerResponse<WishlistItemDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A craft with id {craftId} does not exist."]
            };
        }

        var craftWishlistItem = await context.CraftWishlistItems
                .Where(craftWishlistItem => craftWishlistItem.WishlistedCraftId == craftId && craftWishlistItem.WishlistingUserId == userId)
                .ProjectToType<WishlistItemDto>()
                .FirstOrDefaultAsync();

        if (craftWishlistItem == null)
        {
            // It is not an error for an item to not be wishlisted.
            return new ManagerResponse<WishlistItemDto>() { ResponseType = ManagerResponseType.Ok };
        }

        return new ManagerResponse<WishlistItemDto>(craftWishlistItem);
    }

    public async Task<ManagerResponse<List<long>>> GetWishlistedCraftIdsForUserAsync(long userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        
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
            .Select(wishlist => wishlist.WishlistedCraftId)
            .ToListAsync());
    }

    public async Task<ManagerResponse<PagedList<CraftDto>>> GetWishlistedCraftsForUserAsync(long userId, CraftListParams craftListParams)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        
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
                && wishlist.WishlistedCraft!.Price >= craftListParams.MinPrice 
                && wishlist.WishlistedCraft!.Price <= craftListParams.MaxPrice);

        if (craftListParams.InStockOnly)
        {
            query = query.Where(wishlist => wishlist.WishlistedCraft!.Stock > 0);
        }

        query = craftListParams.OrderBy.ToLower() switch
        {
            "price" => craftListParams.IsOrderDescending ? 
                query.OrderByDescending(wishlist => wishlist.WishlistedCraft!.Price) 
                : query.OrderBy(wishlist => wishlist.WishlistedCraft!.Price),
            "name" => craftListParams.IsOrderDescending ? 
                query.OrderByDescending(wishlist => wishlist.WishlistedCraft!.Name) 
                : query.OrderBy(wishlist => wishlist.WishlistedCraft!.Name),
            _ => craftListParams.IsOrderDescending ? 
                query.OrderByDescending(wishlist => wishlist.WishlistedCraft!.CreatedAt) 
                : query.OrderBy(wishlist => wishlist.WishlistedCraft!.CreatedAt),
        };

        var resultQuery = query.Include(wishlist => wishlist.WishlistedCraft!.Seller)
            .Select(wishlist => new CraftDto()
            {
                Id = wishlist.WishlistedCraft!.Id,
                Name = wishlist.WishlistedCraft!.Name,
                Price = wishlist.WishlistedCraft!.Price,
                Stock = wishlist.WishlistedCraft!.Stock,
                CreatedAt = wishlist.WishlistedCraft!.CreatedAt.ToString("o"), // ISO 8601 format,
                SellerId = wishlist.WishlistedCraft!.SellerId,
                SellerDisplayName = wishlist.WishlistedCraft!.Seller == null ? 
                    null : wishlist.WishlistedCraft.Seller!.DisplayName,
                SearchImageId = wishlist.WishlistedCraft!.SearchImageId,
                SearchImage = wishlist.WishlistedCraft!.SearchImage == null ? 
                    null : wishlist.WishlistedCraft.SearchImage.Adapt<CraftMediaDto>(),
                // I intentionally leave out the Media collection here since the list endpoint doesn't need it, and it would be a waste of resources to include it.            
                IsArchived = wishlist.WishlistedCraft!.IsArchived
            });
        return new ManagerResponse<PagedList<CraftDto>>(
            await PagedList<CraftDto>.CreateAsync(resultQuery, craftListParams.PageNumber, craftListParams.PageSize));
    }

    public async Task<ManagerResponse<int>> GetNumLikesForCraftAsync(long craftId)
    {
        var craft = await context.Crafts.FindAsync(craftId);

        if (craft == null)
        {
            return new ManagerResponse<int>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"A craft with id {craftId} does not exist."]
            };
        }

        return new ManagerResponse<int>(await context.CraftWishlistItems
            .Where(wishlist => wishlist.WishlistedCraftId == craftId)
            .CountAsync());
    }
}