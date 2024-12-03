using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/lesson/categories")]
[ApiController]
public class LessonCategoriesAPIController : BaseController
{
    private readonly ILessonCategoryService _categoryService;

    public LessonCategoriesAPIController(ILessonCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        var categories = _categoryService.GetDashboardCategories();
        return Ok(categories);
    }

    [HttpGet]
    public IActionResult GetCategories()
    {
        var categories = _categoryService.GetCategories();
        return Ok(categories);
    }

    [HttpPost]
    public IActionResult CreateCategory([FromBody] LessonCategory category)
    {
        if (category == null)
        {
            return BadRequest();
        }

        var createdCategory = _categoryService.CreateCategory(category);
        return CreatedAtAction(nameof(GetCategories), new { id = createdCategory.Id }, createdCategory);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateCategory(int id, [FromBody] LessonCategory updatedCategory)
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

