using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/categories")]
[ApiController]
public class CategoriesAPIController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesAPIController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public IActionResult GetCategories()
    {
        var categories = _categoryService.GetCategories();
        return Ok(categories);
    }

    [HttpPost]
    public IActionResult CreateCategory([FromBody] Category category)
    {
        if (category == null)
        {
            return BadRequest();
        }

        var createdCategory = _categoryService.CreateCategory(category);
        return CreatedAtAction(nameof(GetCategories), new { id = createdCategory.ID }, createdCategory);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateCategory(int id, [FromBody] Category updatedCategory)
    {
        var category = _categoryService.UpdateCategory(id, updatedCategory);
        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteCategory(int id)
    {
        var result = _categoryService.DeleteCategory(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}

