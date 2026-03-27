using API.Data.Configuration;
using API.Entities;
using API.Misc;
using API.Pagination;
using CraftyCommon.DTOs;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class CraftManager(
    DataContext context, 
    ICraftyUserManager craftyUserManager,
    UserManager<User> userManager
    ) : ICraftManager
{
    public async Task<CraftDto?> GetCraftAsync(long id)
    {
        // I optimized the query for only the information needed in each query
        return await context.Crafts
            .Include(craft => craft.Seller)
            .Include(craft => craft.SearchImage)
            .Include(craft => craft.Medias)
            .Where(craft => craft.Id == id)
            .ProjectToType<CraftDto>()
            .SingleOrDefaultAsync();
    }

    public async Task<PagedList<CraftDto>> GetCraftsAsync(CraftListParams craftListParams)
    {
        // Remember that filtering with .Where must be done before joining with .Include.Select. ASP.NET is picky like that.
        var query = context.Crafts
            .Include( craft => craft.Seller)
            .Where( craft => craft.Price >= craftListParams.MinPrice 
            && craft.Price <= craftListParams.MaxPrice);

        query = craftListParams.ArchiveFilter switch
        {
            ArchiveFilterType.ArchivedOnly => query.Where(craft => craft.IsArchived),
            ArchiveFilterType.NotArchivedOnly => query.Where (craft => !craft.IsArchived),
            _ => query
        };

        if (craftListParams.InStockOnly)
        {
            query = query.Where(craft => craft.Stock > 0);
        }

        query = craftListParams.OrderBy.ToLower() switch
        {
            "price" => craftListParams.IsOrderDescending ? query.OrderByDescending(craft => craft.Price) : query.OrderBy(craft => craft.Price),
            "name" => craftListParams.IsOrderDescending ? query.OrderByDescending(craft => craft.Name) : query.OrderBy(craft => craft.Name),
            _ => craftListParams.IsOrderDescending ? query.OrderByDescending(craft => craft.CreatedAt) : query.OrderBy(craft => craft.CreatedAt),
        };

        var resultQuery = query.Include(craft => craft.Seller)
            .Select(craft => new CraftDto()
            {
                Id = craft.Id,
                SellerId = craft.SellerId,
                SellerDisplayName = craft.Seller == null ? null : craft.Seller.DisplayName,
                Name = craft.Name,
                Price = craft.Price,
                Stock = craft.Stock,
                CreatedAt = craft.CreatedAt.ToString("o"),
                SearchImageId = craft.SearchImageId,
                SearchImage = craft.SearchImage == null ? null : craft.SearchImage.Adapt<CraftMediaDto>(),
                IsArchived = craft.IsArchived
                // I intentionally leave out the Media collection here since the list endpoint doesn't need it, and it would be a waste of resources to include it.            
            });
        return await PagedList<CraftDto>.CreateAsync(resultQuery, craftListParams.PageNumber, craftListParams.PageSize);
    }

    public async Task<ManagerResponse<CraftDto>> CreateCraftAsync(CraftDto craftDto)
    {
        var newCraft = new Craft()
        {
            Name = craftDto.Name ?? "",
            Price = craftDto.Price,
            Description = craftDto.Description ?? "",
            Stock = craftDto.Stock
        };
        
        if (DateOnly.TryParse(craftDto.CreatedAt, out var createdAt))
        {
            // This could be off by up to one day because of differences in time zones. The only way around this is to convert to local time on the client side.
            newCraft.CreatedAt = createdAt;
        }

        CraftMedia? searchImage = null;

        foreach (var mediaDto in craftDto.Medias)
        {
            // I don't need to set craft or craftID because EF will do that for me.
            var newMedia = new CraftMedia()
            {
                Url = mediaDto.Url ?? "",
                CloudId = mediaDto.CloudId ?? "",
                Type = mediaDto.Type
            };
            newCraft.Medias.Add(newMedia);
            // For new craftMedias, the Id will be negative. This ensures that they are unique from existing craftMedias and from all other new craftMedias.
            // These ID's should be ignored by the backend code so they are assigned database ID's appropriately.
            // Alternatively, I could have just used the cloudID, but then I would need to parse it as a string to a long when assigning existing media items to the search media
            // This seemed a bit cleaner.
            if (craftDto.SearchImageId == mediaDto.Id)
            {
                if (newMedia.Type != MediaType.Image)
                {                    
                    return new ManagerResponse<CraftDto>()
                    {
                        ResponseType = ManagerResponseType.BadRequest,
                        ErrorMessages = [$"The main image of a craft must be an image. You chose a {newMedia.Type}"]
                    };
                }
                searchImage = newMedia;
            }
        }

        var response = await craftyUserManager.AddCraftToSellerAsync(craftDto.SellerId, newCraft);
        if (response.ResponseType != ManagerResponseType.Ok)
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = response.ResponseType,
                ErrorMessages = response.ErrorMessages
            };
        }

        // I need to set the SearchImageId after saving to prevent a circular depency issue in EF. 
        // This is because the SearchImageId foreign key references the CraftMedia, which is in the same collection as the SearchImage navigation property.
        if (await context.SaveChangesAsync() == 0)
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["Failed to create new craft."]
            };
        }

        if (searchImage != null)
        {
            newCraft.SearchImageId = searchImage.Id;
            newCraft.SearchImage = searchImage;
            if (await context.SaveChangesAsync() == 0)
            {
                return new ManagerResponse<CraftDto>()
                {
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = ["Created new craft, but was unable to save search media."]
                };
            }
        }

        return new ManagerResponse<CraftDto>(newCraft.Adapt<CraftDto>());
    }

    public async Task<ManagerResponse<CraftDto>> UpdateCraftAsync(long userId, CraftDto craftDto)
    {        
        var user = await userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"User with id {userId} does not exist."]
            };
        }

        var craft = await context.Crafts
            .Include(craft => craft.Seller)
            .SingleOrDefaultAsync(craft => craft.Id == craftDto.Id);

        if (craft == null)
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"Craft with id \"{craftDto.Id}\" not found."]
            };
        }

        if (craft.SellerId != userId)
        {
            return new ManagerResponse<CraftDto>() { ResponseType = ManagerResponseType.Forbidden };
        }

        if ( craftDto.SellerId != craft.SellerId
            || craftDto.SellerId != userId)
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["You cannot edit the seller of a craft."]
            };
        }        

        CraftMedia? newSearchImage = null;

        craft.Name = craftDto.Name ?? "";
        craft.Price = craftDto.Price;
        craft.Description = craftDto.Description ?? "";
        craft.Stock = craftDto.Stock;

        // First remove any deleted media items
        var mediaDtoDict = craftDto.Medias.Where(item => item.CloudId != null).ToDictionary(item => item.CloudId!, item => item);
        // I need to remember to call ToList() to keep LINQ's deffered execution from causing issues when I remove items from the craft.Media collection.
        var removeItems = craft.Medias.Where(item => item.CloudId != null && !mediaDtoDict.ContainsKey(item.CloudId)).ToList();
        foreach (var removeItem in removeItems)
        {
            if (craft.SearchImageId == removeItem.Id)
            {
                craft.SearchImageId = 0;
                craft.SearchImage = null;
            }
            craft.Medias.Remove(removeItem);
        }

        // Update search media if the new search media already exists.
        var searchImage = craft.Medias.FirstOrDefault(media => media.Id == craftDto.SearchImageId);
        if (searchImage != null)
        {            
            if (searchImage.Type != MediaType.Image)
            {                    
                return new ManagerResponse<CraftDto>()
                {
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = [$"The main image of a craft must be an image. You chose a {searchImage.Type}"]
                };
            }
            craft.SearchImageId = searchImage.Id;
            // Entity framework should handle setting craft.SearchImage
        }
        
        // Now add any new items.
        var mediaDict = craft.Medias.Where(item => item.CloudId != null).ToDictionary(item => item.CloudId!, item => item);
        var newMediaDtos = craftDto.Medias.Where(media => media.CloudId != null && !mediaDict.ContainsKey(media.CloudId));
        foreach (var mediaDto in newMediaDtos)
        {
            var newCraftMedia = new CraftMedia()
            {
                Url = mediaDto.Url ?? "",
                CloudId = mediaDto.CloudId ?? "",
                Type = mediaDto.Type
            };
            // I don't need to set craft or craftID because EF will do that for me.
            craft.Medias.Add(newCraftMedia);
            
            if (craftDto.SearchImageId == mediaDto.Id)
            {
                newSearchImage = newCraftMedia;
            }
        }

        if (newSearchImage != null)
        {
            craft.SearchImageId = newSearchImage.Id;
            craft.SearchImage = newSearchImage;
        }
        // This shouldn't happen, but just in case.
        else if (craft.SearchImage == null && craft.SearchImageId == 0 && craft.Medias.Count > 0)
        {
            craft.SearchImageId = craft.Medias.First().Id;
            craft.SearchImage = craft.Medias.First();
        }
                
        if (await context.SaveChangesAsync() == 0)
        {            
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["Craft "]
            };
        }

        return new ManagerResponse<CraftDto>(craftDto);
    }
    
    public async Task<ManagerResponse<CraftDto>> ArchiveCraftAsync(long userId, long craftId)
    {
        var user = userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"User with id {userId} does not exist."]
            };
        }

        var craft = context.Crafts
            .Include(craft => craft.Seller)
            .FirstOrDefault(craft => craft.Id == craftId);

        if (craft == null)
        {            
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"Craft with id {craftId} does not exist."]
            };
        }

        if (craft.SellerId != userId && !await craftyUserManager.IsUserInRoleAsync(userId, Role.Admin))
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["You may only archive crafts that you produce."]
            };
        }

        craft.IsArchived = true;

        if (await context.SaveChangesAsync() == 0)
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"Unable to archive craft with id {craft.Id}"]
            };
        }

        return new ManagerResponse<CraftDto>(craft.Adapt<CraftDto>());
    }

    public async Task<ManagerResponse> MarkCraftAsInappropriateAsync(long userId, long craftId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"User with id {userId} does not exist."]
            };
        }

        var craft = context.Crafts
            .Include(craft => craft.Seller)
            .FirstOrDefault(craft => craft.Id == craftId);

        if (craft == null)
        {            
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"Craft with id {craftId} does not exist."]
            };
        }

        if (craft.SellerId == userId)
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = ["You may not mark crafts that you produce as inappropriate."]
            };
        }

        craft.IsInappropriate = true;

        if (await context.SaveChangesAsync() == 0)
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"Unable to mark craft with id {craft.Id} as inappropriate."]
            };
        }

        return new ManagerResponse<CraftDto>(craft.Adapt<CraftDto>());
    }

    public async Task<ManagerResponse> MarkCraftAsAppropriateAsync(long craftId)
    {
        var craft = context.Crafts
            .Include(craft => craft.Seller)
            .FirstOrDefault(craft => craft.Id == craftId);

        if (craft == null)
        {            
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"Craft with id {craftId} does not exist."]
            };
        }

        craft.IsInappropriate = false;

        if (await context.SaveChangesAsync() == 0)
        {
            return new ManagerResponse<CraftDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"Unable to mark craft with id {craft.Id} as inappropriate."]
            };
        }

        return new ManagerResponse<CraftDto>(craft.Adapt<CraftDto>());
    }

    public async Task<PagedList<CraftDto>> GetInappropriateCraftsAsync(PaginationParams paginationParams)
    {
        var resultQuery = context.Crafts.Where( craft => craft.IsInappropriate && !craft.IsArchived)
            .Select(craft => new CraftDto()
            {
                Id = craft.Id,
                Name = craft.Name,
                Price = craft.Price,
                Stock = craft.Stock,
                CreatedAt = craft.CreatedAt.ToString("o"),
                SellerDisplayName = craft.Seller == null ? null : craft.Seller.DisplayName,
                SellerId = craft.SellerId,
                SearchImageId = craft.SearchImageId,
                SearchImage = craft.SearchImage == null ? null : craft.SearchImage.Adapt<CraftMediaDto>(),
                IsArchived = craft.IsArchived
                // I intentionally leave out the Media collection here since the list endpoint doesn't need it, and it would be a waste of resources to include it.            
            });

        return await PagedList<CraftDto>.CreateAsync(resultQuery, paginationParams.PageNumber, paginationParams.PageSize);
    }
}
