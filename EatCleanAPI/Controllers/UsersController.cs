﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EatCleanAPI.Models;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using EatCleanAPI.Catalog.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using EatCleanAPI.Catalog.Common;
using EatCleanAPI.ViewModels;

namespace EatCleanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly VegafoodBotContext _context;
        private readonly JWTSettings _jwtsettings;
        private readonly IUserService _userService;

        public UsersController(VegafoodBotContext context, IOptions<JWTSettings> jwtsettings, IUserService userService)
        {
            _context = context;

            // ta lấy secretkey từ file appsettings gán vào field Secretkey của tham số jwtsettings. 
            _jwtsettings = jwtsettings.Value;

            _userService = userService;


            // string t= _jwtsettings.SecretKey;
        }
        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<User>> RefreshToken([FromBody] RefreshRequest refreshRequest)
        {
            // tạo 1 customer new từ accesstoken, customer có thể là null( tạo mới), hoặc là tìm ra đc customer đang giữ accestoken đó
            User customer = await GetUserFromAccessToken(refreshRequest.AccessToken);

            // trong customer hiện tại sẽ có refresh token, lấy refresh token đó đi kiểm tra
            // thực hiện Check customer và validate refreshtoken
            if (customer != null && ValidateRefreshToken(customer, refreshRequest.RefreshToken))
            {
                // create 1 customer có refreshtoken
                UsersWithToken customerWithToken = new UsersWithToken(customer);
                // cấp lại access token cho customer
                customerWithToken.AccessToken = GenerateAccessToken(customer.UserId);
                customerWithToken.RefreshToken = refreshRequest.RefreshToken;
                return customerWithToken;
            }

            return null;
        }

        private bool ValidateRefreshToken(User customer, string refreshToken)
        {
            // lấy ra đối tượng refreshtoken trong DB từ chuỗi refreshtoken từ parameter truyền vào , lấy ra token có expiry lớn nhất
            RefreshToken refreshTokenUser = _context.RefreshTokens.Where(rt => rt.Token == refreshToken)
                                                .OrderByDescending(rt => rt.ExpiryDate)
                                                .FirstOrDefault();
            // kiểm tra refreshtoken có null hay không, có customerid có bằng với token của user trong parameter hay không, có còn hạn sử dụng hay không
            if (refreshTokenUser != null && refreshTokenUser.CustomerId == customer.UserId
                && refreshTokenUser.ExpiryDate > DateTime.UtcNow)
            {
                return true;
            }

            return false;
        }


        private async Task<User> GetUserFromAccessToken(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                SecurityToken securityToken;
                var principle = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out securityToken);

                JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    var customerId = principle.FindFirst(ClaimTypes.Name)?.Value;

                    return await _context.Users
                                        .Where(u => u.UserId == Convert.ToInt32(customerId)).FirstOrDefaultAsync();
                }
            }
            catch (Exception)
            {
                return new User();
            }

            return new User();
        }

        // Create token for SignIn
        private RefreshToken GenerateRefreshToken()
        {
            RefreshToken refreshToken = new RefreshToken();

            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                refreshToken.Token = Convert.ToBase64String(randomNumber);
            }
            refreshToken.ExpiryDate = DateTime.UtcNow.AddMonths(6);

            return refreshToken;
        }
        //Create token for SignIn
        private string GenerateAccessToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // lấy secretkey để tạo ra phần signature cho accesstoken
            var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, Convert.ToString(userId))
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ApiResult<string>>> Authenticate([FromForm] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = _context.Users.Where(cus => cus.Email == request.Email).FirstOrDefault();
            if (customer == null) return new ApiErrorResult<string>("Tài khoản không tồn tại");

            if (customer.Password != request.Password)
                return new ApiErrorResult<string>("Mật khẩu không chính xác");

            customer = await _context.Users.
                Where(cus => cus.Email == customer.Email && cus.Password == customer.Password)
                .FirstOrDefaultAsync();

            // tạo đối tượng CustomerWithToken
            UsersWithToken customerWithToken = new UsersWithToken(customer);

            // nếu có customer trong db
            if (customer != null)
            {
                // tạo ra refreshToken 
                RefreshToken refreshToken = GenerateRefreshToken();
                // add refreshtoken cho customer 
                customer.RefreshTokens.Add(refreshToken);
                // save việc insert đối tượng vào trong csdl 
                await _context.SaveChangesAsync();

                // tạo 1 đối tượng customerwithToken truyền vào là customer đã có refreshtoken
                customerWithToken = new UsersWithToken(customer);
                // đối tượng customerWithToken đc đc refreshtoken
                customerWithToken.RefreshToken = refreshToken.Token;
            }

            if (customerWithToken == null)
            {
                return new ApiErrorResult<string>("Không có Token");
            }


            //    //sign your token here here..
            // tạo access token cho customer
            customerWithToken.AccessToken = GenerateAccessToken(customer.UserId);

            string access = customerWithToken.AccessToken;

            string refresh = customerWithToken.RefreshToken;

            return new ApiErrorResult<string>("Đăng nhập thành công")
            {
                IsSuccessed = true,
                usersWithToken = customerWithToken,

            };



        }

        [HttpPost("Register")]
        public async Task<ActionResult<ApiResult<bool>>> Register([FromForm] RegisterRequest request)
        {
            // thực hiện validate trên viewmodel trước, nếu validate ko thành công
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_context.Users.Where(cus => cus.Name == request.CustomerName).FirstOrDefault() != null)
            {
                return new ApiErrorResult<bool>("Tài khoản đã tồn tại");
            }
            // email đã đc 1 customer đăng kí rồi
            if (_context.Users.Where(cus => cus.Email == request.Email).FirstOrDefault() != null)
                return new ApiErrorResult<bool>("Emai đã tồn tại");


            var customer = new User()
            {
                Name = request.CustomerName,
                PhoneNumber = request.Phone,
                IsAdmin= request.IsAdmin,
                Password = request.Password,
                Email = request.Email
            };
            _context.Users.Add(customer);
            await _context.SaveChangesAsync();
            return new ApiSuccessResult<bool>() { Message = "Đăng kí thành công" };

            // khi validate đã success thì tiến hành đăng kí với đối tượng truyền vào là ViewModel RegisterRequest
            // Lấy thông tin của ViewModel truyền xuống cho Model rồi lưu vào DB
            //var result = await _userService.Register(request);
            //if (!result)
            //{
            //    return BadRequest("Register is unsuccessful.");
            //}
            //return Ok();
        }
    }
}
