﻿namespace JWTAuth.WebApi.Controllers;

[Route("api/token")]
[ApiController]
public class TokenController : ControllerBase
{
    public IConfiguration _configuration;
    private readonly DatabaseContext _context;

    public TokenController(IConfiguration config, DatabaseContext context)
    {
        _configuration = config;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Post(UserInfo _userData)
    {
        if (_userData != null && _userData.Email != null && _userData.Password != null)
        {
            var user = await GetUser(_userData.Email, _userData.Password);

            if (user != null)
            {
                //create claims details based on the user information
                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("DisplayName", user.DisplayName ?? string.Empty),
                    new Claim("UserName", user.UserName ?? string.Empty),
                    new Claim("Email", user.Email ?? string.Empty)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(10),
                    signingCredentials: signIn);

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
            else
            {
                return BadRequest("Invalid credentials");
            }
        }
        else
        {
            return BadRequest();
        }
    }

    private async Task<UserInfo?> GetUser(string email, string password) => await _context.UserInfos.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
}
