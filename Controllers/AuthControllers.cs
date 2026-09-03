using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using auth12.Data;
using auth12.Models;
using auth12.Records;
using auth12.Services;
using auth12.DTO;
using Microsoft.EntityFrameworkCore;

namespace auth12.Controllers;


[ApiController]
[Route("auth")]

public class AuthControllers : ControllerBase
{
    private readonly IHttpClientFactory _client;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly JwtService _jwtService;
    public AuthControllers(IHttpClientFactory client,AppDbContext context,IConfiguration configuration,JwtService jwtService)
    {
        _context = context;
        _client = client;
        _configuration = configuration;
        _jwtService = jwtService;

    }
    [HttpPost("reg")]
    public async Task<IActionResult> RegUser(RegDto dto)
    {
        if(string.IsNullOrEmpty(dto.Name)|| string.IsNullOrEmpty(dto.Password))
        {
            return BadRequest("Please add username and password");
        }
        try
        {
            if(await _context.User.AnyAsync(u=>(u.Name ?? "").ToLower()== dto.Name.ToLower()))
            {
                return BadRequest("Username already in use");
            }
            var user = new User
            {
                Name= dto.Name,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            var response = new RegResp
            {
                Id=user.Id,
                Name= user.Name
            };
            return Ok(response);
            

        }catch(Exception ex)
        {
            return StatusCode(503,"Service unavailable try again later");
        }
    }
    [HttpPost("Log")]
    public async Task<IActionResult> LogUser(LogDto dto){
        if(string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Password))
        {
            return BadRequest("please add username snd password");
        }
        try
        {
            var user = await _context.User.FirstOrDefaultAsync(u=> (u.Name ?? "").ToLower() == dto.Name.ToLower());

            if(user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
            {
                return Unauthorized("Invlid username or password");
            }
            var tokenString = _jwtService.GenerateToken(user);
            return Ok(new
            {
                Token = tokenString,
                UserId = user.Id,
                UserName = user.Name
            });
            
        }catch(Exception ex)
        {
            return StatusCode(503, "Service not available try agin later");
        }
    
    }
    [HttpGet("puzzle")]
    public async Task<IActionResult> GetPuzzle()
    {
        try
        {
            var client = _client.CreateClient();
            var url = "https://lichess.org/api/puzzle/daily";
            var response =await client.GetFromJsonAsync<DailyPuzzle>(url);
            if(response?.Puzzle?.Fen == null || response.Puzzle.Solution == null)
            {
                return StatusCode(503,"Service not available try again later");
            }
            return Ok(response);

        }catch(Exception ex)
        {
            return StatusCode(503,"service not available please try agin later");
        }
    }
}