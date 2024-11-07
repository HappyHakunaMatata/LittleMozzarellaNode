using System.Collections;
using System.Numerics;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TokenCreation.Model;

namespace TokenCreation.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly UserManager<IdentityUser> _UserManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public IndexModel(ILogger<IndexModel> logger, UserManager<IdentityUser> UserManager,
        SignInManager<IdentityUser> signInManager)
    {
        _logger = logger;
        _UserManager = UserManager;
        _signInManager = signInManager;
    }

    [BindProperty(SupportsGet = true)]
    public RegistrationToken? registrationToken { get; set; }

    public IActionResult OnGet()
    {
        var ctx = HttpContext.User.Identity;
        if (ctx != null)
        {
            if (ctx.IsAuthenticated)
            {
                return RedirectToPage("./Registered");
            }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        
        if (registrationToken != null && !string.IsNullOrEmpty(registrationToken.Email))
        {
            IdentityUser? user = await _UserManager.FindByEmailAsync(registrationToken.Email);

            if (!(user != null))
            {
                user = new IdentityUser()
                {
                    UserName = registrationToken.Email,
                    Email = registrationToken.Email,
                };
                var createdUser = await _UserManager.CreateAsync(user);
                if (!createdUser.Succeeded)
                {
                    user = null;
                }
            }
            if (user != null)
            {
                var claims = NewAuthorizationClaims();
                if (claims == null)
                {
                    return Page();
                }
                await _UserManager.AddClaimsAsync(user, claims);
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = false,
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(2)

                };
                await _signInManager.SignInWithClaimsAsync(user, authProperties, claims);
                return RedirectToPage("./Registered");
            }
        }

        return RedirectToPage("./Index");
    }

    private List<Claim>? NewAuthorizationClaims()
    {
        Random rnd = new Random();
        var tokenByte = new byte[64];
        rnd.NextBytes(tokenByte);
        var encoder = new Base58();
        var token = encoder.CheckEncode(tokenByte, 0);
        if (token == null)
        {
            return null;
        }

        List<Claim> claims = new List<Claim>() {
            new Claim("Token", token),
            new Claim("Timestamp", DateTime.Now.ToString()),
        };


        var httpcontext = HttpContext;
        if (httpcontext == null) {
            return claims;
        }
        var remoteIP = httpcontext.Connection.RemoteIpAddress;
        if (remoteIP == null)
        {
            return claims;
        }
        claims.Add(new Claim("IPV4Address", remoteIP.MapToIPv4().ToString()));
        claims.Add(new Claim("IPv6", remoteIP.MapToIPv6().ToString()));
        var ipv6address = remoteIP.MapToIPv6().GetAddressBytes().FirstOrDefault();
        if (ipv6address == 2)
        {
            claims.Add(new Claim("YggdrasilAddress", remoteIP.MapToIPv6().ToString()));
        }
        return claims;
    }

    
        /*List<Claim> claim = new()
            {
                new Claim("IPv4", ""),
                new Claim("Timestamp", ""),
                new Claim("IPv6", ""),
                new Claim("YggdrasilAddress", ""),
                new Claim("Identity", ""),
                new Claim("SignedChainBytes", ""),
            };*/
}

