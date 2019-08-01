using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwingDataViewer.Models;
using SwingDataViewer.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SwingDataViewer.Controllers
{
	[Authorize]
	public class UserController : Controller
	{
		readonly TableService _tableService;

		public UserController(TableService tableService)
		{
			_tableService = tableService;
		}

		public async Task<IActionResult> Index()
		{
			var user = UserModel.FromUserClaims(HttpContext.User);
			if (user == null) return RedirectToAction("Login", "Auth", new { returnUrl = "~/edit/" });

			var logger = await _tableService.GetLogger(user.DeviceId);
			if (logger == null) return RedirectToAction("Login", "Auth", new { returnUrl = "~/edit/" });

			var viewModel = new UserViewModel
			{
				Name = logger.Name,
				Public = logger.Public
			};
			if (TempData.TryGetValue("UserEdit", out var temp) && temp is string status)
			{
				viewModel.HaveResult = true;
				viewModel.Error = false;
				viewModel.Message = status;
				TempData.Remove("UserEdit");
			}
			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Index(UserViewModel viewModel)
		{
			var user = UserModel.FromUserClaims(HttpContext.User);
			if (user == null) return RedirectToAction("Login", "Auth", new { returnUrl = "~/edit/" });

			if (await _tableService.UpdateLogger(user.DeviceId, viewModel.Name, viewModel.Public))
			{
				#region Cookieの中身更新
				// ログインしなおさなきゃだめ？
				user.Name = viewModel.Name;
				if (User.Identity.IsAuthenticated)
				{
					await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
				}
				await AuthController.LoginAsync(HttpContext, user);
				#endregion

				TempData["UserEdit"] = "保存しました";
				return RedirectToAction();
			}

			viewModel.HaveResult = true;
			viewModel.Error = true;
			viewModel.Message = "保存に失敗しました";
			return View(viewModel);
		}
	}
}
