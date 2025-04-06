using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EasyContinuity_API.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ECDbContext _ecDbContext;
        private readonly JwtSettings _jwtSettings;

        public AuthenticationService(ECDbContext ecDbContext, JwtSettings jwtSettings)
        {
            _ecDbContext = ecDbContext;
            _jwtSettings = jwtSettings;
        }

        public async Task<Response<UserDto>> Register(RegisterDto registerDto)
        {
            try
            {
                if (await _ecDbContext.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return Response<UserDto>.BadRequest("Email is already registered.");
                }

                CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = new User
                {
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedOn = DateTime.UtcNow
                };

                await _ecDbContext.Users.AddAsync(user);
                await _ecDbContext.SaveChangesAsync();

                string token = GenerateJwtToken(user);

                // Return response with user data and token
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token
                };

                return Response<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                return Response<UserDto>.InternalError(ex);
            }
        }

        public async Task<Response<UserDto>> Login(LoginDto loginDto)
        {
            try
            {
                var user = await _ecDbContext.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email && !u.IsDeleted);

                if (user == null)
                {
                    return Response<UserDto>.BadRequest("Invalid email or password.");
                }

                if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return Response<UserDto>.BadRequest("Invalid email or password.");
                }

                user.LastLoginOn = DateTime.UtcNow;
                await _ecDbContext.SaveChangesAsync();

                string token = GenerateJwtToken(user);

                // Return response with user data and token
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token
                };

                return Response<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                return Response<UserDto>.InternalError(ex);
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.GivenName, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

}