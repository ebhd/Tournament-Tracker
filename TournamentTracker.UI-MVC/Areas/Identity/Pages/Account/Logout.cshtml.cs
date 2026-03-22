using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TournamentTracker.UI_MVC.Areas.Identity.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public string ReturnUrl { get; private set; } = string.Empty;

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = GetSafeReturnUrl(returnUrl) ?? Url.Action("Index", "Team", new { area = "" }) ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");

        var safeReturnUrl = GetSafeReturnUrl(returnUrl);
        if (safeReturnUrl is not null)
        {
            return LocalRedirect(safeReturnUrl);
        }

        return RedirectToPage();
    }

    private string? GetSafeReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : null;
    }
}
