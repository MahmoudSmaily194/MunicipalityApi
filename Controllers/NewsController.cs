﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SawirahMunicipalityWeb.Models;
using SawirahMunicipalityWeb.Services.NewsServices;

namespace SawirahMunicipalityWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController(INewsService newsService) : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpPost("create_news")]
        public async Task<IActionResult> CreateNews([FromForm] CreateNewsItemFormData request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // 1. حفظ الصورة في السيرفر
                string? imageUrl = null;

                if (request.Image is { Length: > 0 })
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "news");
                    Directory.CreateDirectory(folderPath); // تأكد من وجود المجلد

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await request.Image.CopyToAsync(stream);

                    // إنشاء رابط للوصول إلى الصورة
                    imageUrl = $"/images/news/{fileName}";
                }

                // 2. إنشاء DTO لتمريره إلى الـ service
                var createDto = new CreateNewsItemDto
                {
                    Title = request.Title,
                    Description = request.Description,
                    Visibility = request.Visibility,
                    ImageUrl = imageUrl,

                };

                // 3. إنشاء الخبر
                var news = await newsService.CreateNewsItemAsync(createDto);

                return CreatedAtAction(nameof(GetNewsBySlug), new { slug = news.Slug }, news);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetNewsBySlug([FromQuery] string slug)
        {
            var news = await newsService.GetBySlugAsync(slug);
            if (news == null)
                return NotFound();

            return Ok(news);
        }


        [HttpGet("get_visible_news")]
        public async Task<IActionResult> GetVisibleNews([FromQuery] PaginationParams paginationParams)
        {
            var result = await newsService.GetVisibleAsync(paginationParams);

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("get_all_news")]
        public async Task<IActionResult> GetAllNews([FromQuery] PaginationParams paginationParams)
        {
            var result = await newsService.GetAllAsync(paginationParams);

            if (result.Items == null || result.Items.Count == 0)
            {
                return NotFound(new { message = "No news found for the given filters." });
            }

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("update_newsItem")]
        public async Task<IActionResult> UpdateNews([FromQuery] Guid id, [FromBody] UpdateNewsItemDto dto)
        {
            var updated = await newsService.UpdateNewsItemAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = "News item not found." });

            return Ok(updated);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_newsItem")]

        public async Task<IActionResult> DeleteNewsItem([FromQuery] Guid id)
        {
            var deletedItem = await newsService.DeleteNewsItemAsync(id);

            if (deletedItem == false)
            {
                return NotFound(new { message = "News item not found." });
            }
            return NoContent();
        }

        [HttpGet("get-newsItem-by-admin")]

        public async Task<IActionResult> GetNewsItemByAdmin(string slug)
        {
            var res = await newsService.getAdminBySlugAsync(slug);
            if (res == null) return NotFound("NewsItem not found ");
            return Ok(res);
        }
    }
}
