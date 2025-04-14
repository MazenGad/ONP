using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ONP.API.DTO;
using ONP.API.Entity;
using ONP.API.Helpers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly JwtHelper _jwtHelper;

	public AuthController(UserManager<ApplicationUser> userManager,
						  RoleManager<IdentityRole> roleManager,
						  JwtHelper jwtHelper)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_jwtHelper = jwtHelper;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register(RegisterDto dto)
	{
		var user = new ApplicationUser
		{
			UserName = dto.Email,
			Email = dto.Email,
			FullName = dto.FullName
		};

		var result = await _userManager.CreateAsync(user, dto.Password);
		if (!result.Succeeded) return BadRequest(result.Errors);

		// Create role if not exists
		if (!await _roleManager.RoleExistsAsync(dto.Role))
		{
			await _roleManager.CreateAsync(new IdentityRole(dto.Role));
		}

		await _userManager.AddToRoleAsync(user, dto.Role);

		return Ok("User registered successfully");
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login(LoginDto dto)
	{
		var user = await _userManager.FindByEmailAsync(dto.Email);
		if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
			return Unauthorized("Invalid credentials");

		var roles = await _userManager.GetRolesAsync(user);
		var token = _jwtHelper.GenerateToken(user, roles);

		return Ok(new
		{
			token,
			user = new { user.Id, user.FullName, user.Email, roles }
		});
	}
}
