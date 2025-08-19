using Microsoft.EntityFrameworkCore;
using SawirahMunicipalityWeb.Data;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Enums;
using SawirahMunicipalityWeb.Helpers;
using SawirahMunicipalityWeb.Models;

namespace SawirahMunicipalityWeb.Services.MunicipalServices
{
    public class MunicipalService : IMunicipalService
    {
        private readonly DBContext _context;
        public MunicipalService(DBContext context)
        {
            _context = context;
        }


        public async Task<ServicesCategories> CreateServiceCategoryAsync(CreateServiceCategoryDto dto)
        {
            bool exists = await _context.ServicesCategories
             .AnyAsync(e => e.Name == dto.Name);
            if (exists)
            {
                return null;
            }

            var newServiceCategory = new ServicesCategories
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };
            _context.ServicesCategories.Add(newServiceCategory);
            await _context.SaveChangesAsync();
            return newServiceCategory;
        }

        public async Task<Service?> CreateService(CreateServiceDto dto)
        {
            var slug = await SlugHelper.GenerateUniqueSlug<Service>(
            dto.Title,
            _context,
            s => s.Slug
            );
            var service = new Service
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Slug = slug,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                Status = (Status)dto.Status,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return service;
        }

        public async Task<PaginatedResponse<MunicipalServiceResponceDto>> GetServicesAsync(PaginationParams paginationParams)
        {
            try
            {
                var query = _context.Services
                    .Select(s => new MunicipalServiceResponceDto
                    {
                        Id = s.Id,
                        ImageUrl = s.ImageUrl,
                        CategoryId = s.CategoryId,
                        CategoryName = s.Category != null ? s.Category.Name : string.Empty,
                        Title = s.Title,
                        Description = s.Description,
                        Status = s.Status,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    }).AsQueryable();
                if (paginationParams.CategoryId.HasValue)
                {
                    query = query.Where(n => n.CategoryId == paginationParams.CategoryId);
                }
                if (!string.IsNullOrEmpty(paginationParams.SearchTerm))
                {

                    var search = $"%{paginationParams.SearchTerm}%";
                    query = query.Where(n =>
                        EF.Functions.Like(n.Title, search) ||
                        EF.Functions.Like(n.Description, search));
                }
                var sortBy = paginationParams.SortBy?.ToLower();
                var sortDirection = paginationParams.SortDirection?.ToLower();

                if (sortBy == "updatedat")
                {
                    query = sortDirection == "asc"
                        ? query.OrderBy(n => n.UpdatedAt)
                        : query.OrderByDescending(n => n.UpdatedAt);
                }
                else if (sortBy == "title")
                {
                    query = sortDirection == "asc"
                        ? query.OrderBy(n => n.Title)
                        : query.OrderByDescending(n => n.Title);
                }


                var paginated = await query.ToPaginatedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
                return paginated;
            }
            catch (Exception ex)
            {
                // Log ex.Message and ex.StackTrace appropriately
                throw;
            }
        }

        public async Task<List<ServicesCategories>> GetServiceCategoriesAsync()
        {
            var categories = await _context.ServicesCategories.OrderByDescending(x => x.Id).ToListAsync();
            return categories;
        }

        public async Task<Service?> UpdateServiceAsync(Guid id, CreateServiceDto dto)
        {
            var updatedService = await _context.Services.FindAsync(id);
            if (updatedService is null) return null;
            updatedService.Title = dto.Title;
            updatedService.Description = dto.Description;
            updatedService.Status = (Status)dto.Status;
            updatedService.ImageUrl = dto.ImageUrl;
            updatedService.Slug = await SlugHelper.GenerateUniqueSlug<Service>(
            dto.Title,
            _context,
            s => s.Slug
            );

            await _context.SaveChangesAsync();
            return updatedService;
        }

        public async Task<bool> DeleteServiceAsync(Guid id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service is null) return false;
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ServicesCategories> UpdateServiceCategoryAsync(Guid id, CreateServiceCategoryDto dto)
        {
            var updatedCateg = await _context.ServicesCategories.FindAsync(id);
            if (updatedCateg is null)
            {
                return null;
            }
            updatedCateg.Name = dto.Name;
            await _context.SaveChangesAsync();
            return updatedCateg;
        }

        public async Task<bool> DeleteServiceCategoryAsync(Guid id)
        {
            var DeletedCateg = await _context.ServicesCategories.FindAsync(id);
            if (DeletedCateg is null)
            {
                return false;
            }
            _context.ServicesCategories.Remove(DeletedCateg);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MunicipalServiceResponceDto?> GetServiceByIdAsync(Guid id)
        {
            return await _context.Services
                .Include(s => s.Category)
                .Select(s => new MunicipalServiceResponceDto
                {
                    Id = s.Id,
                    ImageUrl = s.ImageUrl,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category != null ? s.Category.Name : string.Empty,
                    Title = s.Title,
                    Description = s.Description,
                    Status = s.Status
                })
                .FirstOrDefaultAsync(s => s.Id == id);
        }

    }

}



