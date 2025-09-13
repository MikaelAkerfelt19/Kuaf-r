using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Kuafor.Web.ViewModels;
using Kuafor.Web.Services.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Kuafor.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _userSignInManager;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> userSignInManager,
            IJwtService jwtService,
            ILogger<AuthApiController> logger)
        {
            _userManager = userManager;
            _userSignInManager = userSignInManager;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Geçersiz model", 
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) 
                    });
                }

                var result = await _userSignInManager.PasswordSignInAsync(
                    model.UserNameOrEmail, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
                    if (user == null)
                        user = await _userManager.FindByNameAsync(model.UserNameOrEmail);

                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        var token = _jwtService.GenerateToken(user.Id, user.Email ?? "", roles);
                        
                        // Refresh Token oluştur
                        var refreshToken = await _jwtService.CreateRefreshTokenAsync(user.Id);

                        return Ok(new
                        {
                            success = true,
                            message = "Giriş başarılı",
                            data = new
                            {
                                token = token,
                                refreshToken = refreshToken,
                                userId = user.Id,
                                email = user.Email,
                                roles = roles,
                                expiresAt = DateTime.UtcNow.AddMinutes(120) // JWT ayarlarından alınabilir
                            }
                        });
                    }
                }

                return Unauthorized(new { 
                    success = false, 
                    message = "Geçersiz kullanıcı adı veya şifre" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login API error");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Sunucu hatası" 
                });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Geçersiz model", 
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) 
                    });
                }

                // Kullanıcı adı ve email benzersizlik kontrolü
                if (await _userManager.FindByNameAsync(model.UserName) != null)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Bu kullanıcı adı zaten kullanılıyor" 
                    });
                }

                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Bu e-posta adresi zaten kullanılıyor" 
                    });
                }

                // Identity User oluştur
                var user = new IdentityUser 
                { 
                    UserName = model.UserName, 
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Customer rolü ekle
                    await _userManager.AddToRoleAsync(user, "Customer");

                    // Otomatik giriş
                    await _userSignInManager.SignInAsync(user, isPersistent: false);

                    // JWT Token oluştur
                    var roles = await _userManager.GetRolesAsync(user);
                    var token = _jwtService.GenerateToken(user.Id, user.Email ?? "", roles);

                    return Ok(new
                    {
                        success = true,
                        message = "Kayıt başarılı",
                        data = new
                        {
                            token = token,
                            userId = user.Id,
                            email = user.Email,
                            roles = roles,
                            expiresAt = DateTime.UtcNow.AddMinutes(120)
                        }
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Kayıt başarısız", 
                        errors = result.Errors.Select(e => e.Description) 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register API error");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Sunucu hatası" 
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _userSignInManager.SignOutAsync();
                return Ok(new { 
                    success = true, 
                    message = "Çıkış başarılı" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout API error");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Sunucu hatası" 
                });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { 
                        success = false, 
                        message = "Kullanıcı bulunamadı" 
                    });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { 
                        success = false, 
                        message = "Kullanıcı bulunamadı" 
                    });
                }

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        userId = user.Id,
                        email = user.Email,
                        userName = user.UserName,
                        roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProfile API error");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Sunucu hatası" 
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Refresh token gerekli" 
                    });
                }

                var newToken = await _jwtService.RefreshTokenAsync(request.RefreshToken);
                
                return Ok(new
                {
                    success = true,
                    message = "Token yenilendi",
                    data = new
                    {
                        token = newToken,
                        expiresAt = DateTime.UtcNow.AddMinutes(120)
                    }
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { 
                    success = false, 
                    message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshToken API error");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Sunucu hatası" 
                });
            }
        }

        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Token gerekli" 
                    });
                }

                var principal = _jwtService.ValidateToken(request.Token);
                if (principal == null)
                {
                    return Unauthorized(new { 
                        success = false, 
                        message = "Geçersiz token" 
                    });
                }

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                return Ok(new
                {
                    success = true,
                    message = "Token geçerli",
                    data = new
                    {
                        userId = userId,
                        email = email,
                        roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateToken API error");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Sunucu hatası" 
                });
            }
        }
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
