using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using haobtPC11458_ASM01.Data;
using haobtPC11458_ASM01.DTOs;
using haobtPC11458_ASM01.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace haobtPC11458_ASM01.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<Account> _passwordHasher = new();

    public AuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    // null = username đã tồn tại (Controller sẽ trả 409)
    public async Task<AccountResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Accounts.AnyAsync(a => a.Username == request.Username))
            return null;

        var account = new Account
        {
            Username = request.Username,
            CreatedAt = DateTime.UtcNow
        };
        account.PasswordHash = _passwordHasher.HashPassword(account, request.Password);

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        return new AccountResponse
        {
            Id = account.Id,
            Username = account.Username
        };
    }

    // null = sai username hoặc sai password (Controller sẽ trả 401)
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var account = await _db.Accounts.SingleOrDefaultAsync(a => a.Username == request.Username);
        if (account is null)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        return new LoginResponse
        {
            Token = CreateToken(account),
            Username = account.Username
        };
    }

    private string CreateToken(Account account)
    {
        var secret = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Chưa cấu hình Jwt:Key.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, account.Username)
        ];

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
