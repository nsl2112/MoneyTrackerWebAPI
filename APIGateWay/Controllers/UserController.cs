using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MoneyTracker
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(
        CatalogDbContext context,
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        ITenantProvisioningService tenantProvisioningService,
        ILogger<UserController> logger)
        : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserCreateDTO userDTO)
        {
            var existUser = await context.Users.FirstOrDefaultAsync(u => u.Email == userDTO.Email);
            if (existUser != null)
            {
                return Conflict(new { message = "User with this email already exists." });
            }

            var user = new AppUser
            {
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                Email = userDTO.Email,
                UserName = userDTO.Email
            };

            var userTenant = new AppTenant();
            userTenant.SchemaName = $"tenant_{userTenant.Id.Replace("-", "")}";

            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var result = await userManager.CreateAsync(user, userDTO.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Error occurred while creating the user.",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }
                
                await userManager.AddToRoleAsync(user, Roles.User);
            
                userTenant.Users.Add(user);
                context.Tenants.Add(userTenant);
                await context.SaveChangesAsync();
                
                await transaction.CommitAsync();                     
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "An error occurred while creating user {Email} and tenant.", userDTO.Email);

                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while creating the user and tenant." });
            }
          
            await tenantProvisioningService.ProvisionAsync(userTenant);

            return Ok(new { message = "User created successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDTO loginDTO)
        {
            var user = await userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, loginDTO.Password))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var roles = await userManager.GetRolesAsync(user);
            await context.Entry(user).Collection(u => u.Tenants).LoadAsync();
            var (token, expiration) = tokenService.GenerateToken(user, roles);

            return Ok(new
            {
                token,
                expiration
            });
        }
    }
}
