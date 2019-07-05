using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SwingDataViewer.Models;
using SwingDataViewer.Services;

namespace SwingDataViewer.Controllers
{
	public class AuthController : Controller
	{
		TableService _tableService;
		ILogger _logger;

		public AuthController(TableService tableService, ILogger<AuthController> logger)
		{
			_tableService = tableService;
			_logger = logger;
		}

		[Route("login")]
		public IActionResult Login(string returnUrl)
		{
			return View(new LoginViewModel
			{
				ReturnUrl = returnUrl
			});
		}

		[Route("login")]
		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _tableService.Authenticate(model);
				if (user != null)
				{
					await LoginAsync(user);

					if (!string.IsNullOrEmpty(model.ReturnUrl))
					{
						return LocalRedirect(model.ReturnUrl);
					}

					return RedirectToAction("Index", "Home");
				}

				ModelState.AddModelError("InvalidCredentials", "Invalid credentials.");
			}

			return View(model);
		}

		private async Task LoginAsync(UserModel user)
		{
			var identity = new ClaimsIdentity(user.ToClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);
			await HttpContext.SignInAsync(principal, new AuthenticationProperties
			{
				IsPersistent = true,
				ExpiresUtc = DateTime.UtcNow.AddDays(7)
			});
		}

		[Route("logout")]
		public async Task<IActionResult> Logout(string returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;

			if (User.Identity.IsAuthenticated)
			{
				await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			}

			return RedirectToAction("Index", "Home");
		}

	}
}