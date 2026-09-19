using System.Security.Claims;
using BlogSite.Data;
using BlogSite.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlogSite.Controllers;

[Route("hesap")]
public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IOptions<SiteAyarlariOptions> _siteAyarlari;
    private readonly PasswordHasher<Kullanici> _hasher = new();

    public AccountController(AppDbContext db, IOptions<SiteAyarlariOptions> siteAyarlari)
    {
        _db = db;
        _siteAyarlari = siteAyarlari;
    }

    // ============================================
    // KAYIT OL (herkese açık)
    // ============================================
    [HttpGet("kayit")]
    public IActionResult Kayit() => View(new KayitViewModel());

    [HttpPost("kayit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kayit(KayitViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLower();

        if (await _db.Kullanicilar.AnyAsync(k => k.Email == email))
        {
            ModelState.AddModelError("", "Bu e-posta ile zaten bir hesap var. Giriş yapmayı deneyin.");
            return View(model);
        }

        // Doğru sahip kodu girildiyse "sahip" rolü, aksi halde "kullanici" rolü
        var dogruSahipKodu = _siteAyarlari.Value.SahipKodu;
        var rol = (!string.IsNullOrWhiteSpace(model.SahipKodu) && !string.IsNullOrWhiteSpace(dogruSahipKodu)
                    && model.SahipKodu.Trim() == dogruSahipKodu)
                   ? Roller.Sahip
                   : Roller.Kullanici;

        var kullanici = new Kullanici
        {
            AdSoyad = model.AdSoyad.Trim(),
            Email = email,
            Rol = rol,
        };
        kullanici.ParolaHash = _hasher.HashPassword(kullanici, model.Parola);

        _db.Kullanicilar.Add(kullanici);
        await _db.SaveChangesAsync();

        await GirisYap(kullanici);

        return rol == Roller.Sahip
            ? RedirectToAction("Dashboard", "Admin")
            : RedirectToAction("SoruSor", "Hesap");
    }

    // ============================================
    // GİRİŞ / ÇIKIŞ
    // ============================================
    [HttpGet("giris")]
    public IActionResult Giris()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("SonrakiSayfa");
        }
        return View(new GirisViewModel());
    }

    [HttpPost("giris")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(GirisViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLower();
        var kullanici = await _db.Kullanicilar.FirstOrDefaultAsync(k => k.Email == email);

        if (kullanici == null)
        {
            await Task.Delay(400);
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(model);
        }

        var sonuc = _hasher.VerifyHashedPassword(kullanici, kullanici.ParolaHash, model.Parola);
        if (sonuc == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(model);
        }

        await GirisYap(kullanici);
        return RedirectToAction("SonrakiSayfa");
    }

    // Rolüne göre kullanıcıyı doğru sayfaya yönlendirir
    [HttpGet("yonlendir")]
    [Authorize]
    public IActionResult SonrakiSayfa()
    {
        return User.IsInRole(Roller.Sahip)
            ? RedirectToAction("Dashboard", "Admin")
            : RedirectToAction("SoruSor", "Hesap");
    }

    [HttpGet("yetkisiz")]
    public IActionResult Yetkisiz() => View();

    [HttpPost("cikis")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Cikis()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task GirisYap(Kullanici kullanici)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new(ClaimTypes.Name, kullanici.AdSoyad),
            new(ClaimTypes.Role, kullanici.Rol),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }

    // ============================================
    // SORU SOR (sadece "kullanici" rolündekiler)
    // ============================================
    [HttpGet("soru-sor")]
    [Authorize(Roles = Roller.Kullanici)]
    public async Task<IActionResult> SoruSor()
    {
        var kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sorularim = await _db.Sorular
            .Where(s => s.SoranKullaniciId == kullaniciId)
            .OrderByDescending(s => s.OlusturulmaTarihi)
            .ToListAsync();

        return View(new SoruSorSayfaViewModel { Sorularim = sorularim });
    }

    [HttpPost("soru-sor")]
    [Authorize(Roles = Roller.Kullanici)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoruSor(SoruSorViewModel yeniSoru)
    {
        var kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (!ModelState.IsValid)
        {
            var sorularim = await _db.Sorular
                .Where(s => s.SoranKullaniciId == kullaniciId)
                .OrderByDescending(s => s.OlusturulmaTarihi)
                .ToListAsync();
            return View(new SoruSorSayfaViewModel { YeniSoru = yeniSoru, Sorularim = sorularim });
        }

        _db.Sorular.Add(new Soru
        {
            SoruMetni = yeniSoru.SoruMetni,
            SoranKullaniciId = kullaniciId,
        });
        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Sorunuz iletildi! Ayşe Nur cevapladığında burada görünecek.";
        return RedirectToAction("SoruSor");
    }
}
