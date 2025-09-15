using System.Security.Cryptography;
using DinoServer.Interfaces;
using DinoServer.Services;
using DinoServer.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DinoServer.Controllers;
/*
 *   Контроллер. Предназначен преимущественно для обработки запросов протокола HTTP:
 *  Get, Post, Put, Delete, Patch, Head, Options
 */
[ApiController]
[Route("/api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IGetUsersService _getUsersService;
    private readonly IAddUserService _addUserService;

    public UserController(
        IGetUsersService getUsersService,
        IAddUserService addUserService)
    {
        _getUsersService = getUsersService;
        _addUserService = addUserService;
    }

    [HttpGet("getusers")]
    public async Task<ActionResult<IEnumerable<User>>> Get()
    {
        var books = await _getUsersService.GetUsersAsync();
        return Ok(books);
    }

    [HttpPost("addscore")]
    public async Task<IActionResult> AddBook([FromBody] User user, [FromQuery] int userId)
    {
        try
        {
            var addedBook = await _addUserService.AddUserAsync(user, userId);
            return Ok(addedBook);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
