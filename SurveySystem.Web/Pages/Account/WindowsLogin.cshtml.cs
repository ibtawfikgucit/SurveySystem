using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;

namespace SurveySystem.Web.Pages.Account
{
    [Authorize(AuthenticationSchemes = "Windows")]
    public class WindowsLoginModel : PageModel
    {
        private readonly ILogger<WindowsLoginModel> _logger;
        private readonly IConfiguration _configuration;

        public WindowsLoginModel(ILogger<WindowsLoginModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (User.Identity.IsAuthenticated)
            {
                // Get user information from Active Directory
                try
                {
                    var windowsIdentity = User.Identity;
                    var domainUser = windowsIdentity.Name;

                    // Create AD claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, domainUser),
                        new Claim(ClaimTypes.NameIdentifier, domainUser)
                    };

                    // Get user details from AD and group memberships
                    var userName = domainUser.Contains('\\')
                        ? domainUser.Split('\\')[1] // Remove domain prefix if present
                        : domainUser;

                    using (var context = new PrincipalContext(ContextType.Domain, _configuration["ActiveDirectory:Domain"]))
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(context, userName);

                        if (userPrincipal != null)
                        {
                            // Add user information
                            if (!string.IsNullOrEmpty(userPrincipal.EmailAddress))
                            {
                                claims.Add(new Claim(ClaimTypes.Email, userPrincipal.EmailAddress));
                            }

                            if (!string.IsNullOrEmpty(userPrincipal.DisplayName))
                            {
                                claims.Add(new Claim("DisplayName", userPrincipal.DisplayName));
                            }

                            // Get AD groups
                            var groups = GetUserGroups(userName);

                            // Add each group as a role claim
                            foreach (var group in groups)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, group));
                            }

                            // Add standard survey roles based on AD group membership
                            var adminGroup = _configuration["ActiveDirectory:AdminGroup"];
                            var creatorGroup = _configuration["ActiveDirectory:CreatorGroup"];

                            if (groups.Contains(adminGroup) || groups.Contains("Domain Admins"))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
                            }
                            else if (groups.Contains(creatorGroup))
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "SurveyCreator"));
                            }
                        }
                    }

                    // Create identity
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // Sign in
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddHours(12)
                        });

                    _logger.LogInformation("User {UserName} logged in with Windows Authentication", domainUser);

                    // Redirect to return URL or home page
                    return LocalRedirect(ReturnUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during Windows Authentication for user {UserName}", User.Identity.Name);
                    ErrorMessage = "Error during Windows Authentication: " + ex.Message;
                    return Page();
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (User.Identity.IsAuthenticated)
            {
                return LocalRedirect(ReturnUrl);
            }

            // Challenge Windows Authentication
            return Challenge(new AuthenticationProperties { RedirectUri = ReturnUrl }, "Windows");
        }

        // Helper method to get AD groups for a user
        private List<string> GetUserGroups(string username)
        {
            var groups = new List<string>();

            try
            {
                // Set up domain context
                var domainPath = $"LDAP://{_configuration["ActiveDirectory:SearchBase"]}";

                using (var searcher = new DirectorySearcher())
                {
                    searcher.Filter = $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={username}))";
                    searcher.PropertiesToLoad.Add("memberOf");

                    var result = searcher.FindOne();
                    if (result != null && result.Properties.Contains("memberOf"))
                    {
                        var memberOf = result.Properties["memberOf"];
                        foreach (var groupDn in memberOf)
                        {
                            // Extract the group name from DN (e.g., CN=GroupName,OU=Groups,DC=domain,DC=local)
                            var groupDnString = groupDn.ToString();
                            var cnPart = groupDnString.Split(',')[0];
                            if (cnPart.StartsWith("CN="))
                            {
                                var groupName = cnPart.Substring(3); // Remove "CN=" prefix
                                groups.Add(groupName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving AD groups for user {Username}", username);
            }

            return groups;
        }
    }
}