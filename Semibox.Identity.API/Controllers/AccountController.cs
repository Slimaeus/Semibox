using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semibox.Identity.API.Services;
using Semibox.Identity.Application.Account;
using Semibox.Identity.Domain.Constants;
using Semibox.Identity.Domain.Entities;

namespace Semibox.Identity.API.Controllers
{
    /// <summary>
    /// The <see cref="AppUser"/>'s controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;

        public AccountController(UserManager<AppUser> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Get the current user details
        /// </summary>
        /// <returns>
        ///     The <see cref="Task"/> that represents the asynchronous operation, containing the user DTO of current user.
        /// </returns>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserDTO>> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var token = await _userManager
                .GetAuthenticationTokenAsync(user, AuthenticateConstants.SEMIBOX_PROVIDER, AuthenticateConstants.ACCESS_TOKEN_NAME);

            if (token == null)
            {
                token = _tokenService.CreateToken(user);
                await _userManager.SetAuthenticationTokenAsync(user, AuthenticateConstants.SEMIBOX_PROVIDER, AuthenticateConstants.ACCESS_TOKEN_NAME, token);
                return Ok(UserDTO.Create(user, token));
            }

            var expireDate = _tokenService.GetExpireDate(token);

            if (expireDate <= DateTime.UtcNow.AddMinutes(2))
            {
                token = _tokenService.CreateToken(user);
                await _userManager.SetAuthenticationTokenAsync(user, AuthenticateConstants.SEMIBOX_PROVIDER, AuthenticateConstants.ACCESS_TOKEN_NAME, token);
            }
            return Ok(UserDTO.Create(user, token));
        }

        /// <summary>
        /// Login and return an user DTO
        /// </summary>
        /// <param name="input">The user information</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user DTO of logged in user.</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDTO>> LoginAsync(LoginInput input)
        {

            var user = await _userManager.FindByNameAsync(input.UserName);

            if (user == null) return Unauthorized();

            var success = await _userManager.CheckPasswordAsync(user, input.Password);

            if (!success) return Unauthorized();

            var token = await _userManager
                .GetAuthenticationTokenAsync(user, AuthenticateConstants.SEMIBOX_PROVIDER, AuthenticateConstants.ACCESS_TOKEN_NAME);

            if (token == null)
            {
                token = _tokenService.CreateToken(user);
                await _userManager.SetAuthenticationTokenAsync(user, AuthenticateConstants.SEMIBOX_PROVIDER, AuthenticateConstants.ACCESS_TOKEN_NAME, token);
                return Ok(UserDTO.Create(user, token));
            }

            var expireDate = _tokenService.GetExpireDate(token);

            if (expireDate <= DateTime.UtcNow.AddMinutes(2))
            {
                token = _tokenService.CreateToken(user);
                await _userManager.SetAuthenticationTokenAsync(user, AuthenticateConstants.SEMIBOX_PROVIDER, AuthenticateConstants.ACCESS_TOKEN_NAME, token);
            }

            return Ok(UserDTO.Create(user, token));
        }

        /// <summary>
        /// Register and return an user DTO
        /// </summary>
        /// <param name="input">The user infromation</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user DTO of registered user.</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDTO>> RegisterAsync(RegisterInput input)
        {
            var hasUserName = await _userManager.Users
                .AnyAsync(u => u.UserName.Equals(input.UserName));

            if (hasUserName)
            {
                ModelState.AddModelError(nameof(input.UserName), $"The {nameof(input.UserName)} is already taken.");
                return ValidationProblem();
            }

            var hasEmail = await _userManager.Users
                .AnyAsync(u => u.Email.Equals(input.Email));

            if (hasEmail)
            {
                ModelState.AddModelError(nameof(input.Email), $"The {nameof(input.Email)} is already taken.");
                return ValidationProblem();
            }

            var user = new AppUser
            {
                UserName = input.UserName,
                Email = input.Email,
                DisplayName = input.DisplayName,
            };

            var result = await _userManager.CreateAsync(user, input.Password);

            if (!result.Succeeded) return BadRequest();

            var token = _tokenService.CreateToken(user);

            await _userManager.SetAuthenticationTokenAsync(user, AuthenticateConstants.SEMIBOX_PROVIDER, AuthenticateConstants.ACCESS_TOKEN_NAME, token);

            return Ok(UserDTO.Create(user, token));
        }
    }
}
