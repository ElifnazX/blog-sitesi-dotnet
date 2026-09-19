using System.Security.Claims;
using BlogSite.Data;
using BlogSite.Models;
using BlogSite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogSite.Controllers;

[Route("admin")]
[Authorize(Roles = Roller.Sahip)]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // ============================================
    // PANEL (yazı listesi)
    // ============================================
    [HttpGet("")]
    public async Task<IActionResult> Dashboard()
    {
        var yazilar = await _db.Posts.OrderByDescending(p => p.OlusturulmaTarihi).ToListAsync();
        ViewBag.BekleyenSoruSayisi = await _db.Sorular.CountAsync(s => s.Durum == "bekliyor");
        return View(yazilar);
    }

    // ============================================
    // YAZI EKLE
    // ============================================
    [HttpGet("yazi-ekle")]
    public IActionResult YaziEkle() => View(new YaziFormViewModel());

    [HttpPost("yazi-ekle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YaziEkle(YaziFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (basarili, dosyaAdi) = await YardimciServisler.KapakResmiKaydet(model.KapakResimDosya, _env.WebRootPath);
        if (!basarili)
        {
            ModelState.AddModelError("", "Kapak resmi yüklenemedi. JPG, PNG veya WEBP, 5MB altında olmalı.");
            return View(model);
        }

        var yazarIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var yazi = new Post
        {
            Baslik = model.Baslik,
            Slug = await YardimciServisler.BenzersizSlugAl(_db, model.Baslik),
            Ozet = model.Ozet,
            Icerik = model.Icerik,
            KapakResim = dosyaAdi,
            Durum = model.Durum == "yayinda" ? "yayinda" : "taslak",
            YazarId = int.Parse(yazarIdStr!),
        };

        _db.Posts.Add(yazi);
        await _db.SaveChangesAsync();

        return RedirectToAction("Dashboard");
    }

    // ============================================
    // YAZI DÜZENLE
    // ============================================
    [HttpGet("yazi-duzenle/{id:int}")]
    public async Task<IActionResult> YaziDuzenle(int id)
    {
        var yazi = await _db.Posts.FindAsync(id);
        if (yazi == null) return RedirectToAction("Dashboard");

        var model = new YaziFormViewModel
        {
            Id = yazi.Id,
            Baslik = yazi.Baslik,
            Ozet = yazi.Ozet,
            Icerik = yazi.Icerik,
            Durum = yazi.Durum,
            MevcutKapakResim = yazi.KapakResim,
        };

        return View(model);
    }

    [HttpPost("yazi-duzenle/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YaziDuzenle(int id, YaziFormViewModel model)
    {
        var yazi = await _db.Posts.FindAsync(id);
        if (yazi == null) return RedirectToAction("Dashboard");

        if (!ModelState.IsValid)
        {
            model.MevcutKapakResim = yazi.KapakResim;
            return View(model);
        }

        var (basarili, dosyaAdi) = await YardimciServisler.KapakResmiKaydet(model.KapakResimDosya, _env.WebRootPath);
        if (!basarili)
        {
            ModelState.AddModelError("", "Kapak resmi yüklenemedi. JPG, PNG veya WEBP, 5MB altında olmalı.");
            model.MevcutKapakResim = yazi.KapakResim;
            return View(model);
        }

        yazi.Baslik = model.Baslik;
        yazi.Slug = await YardimciServisler.BenzersizSlugAl(_db, model.Baslik, id);
        yazi.Ozet = model.Ozet;
        yazi.Icerik = model.Icerik;
        yazi.Durum = model.Durum == "yayinda" ? "yayinda" : "taslak";
        yazi.GuncellenmeTarihi = DateTime.Now;
        if (dosyaAdi != null) yazi.KapakResim = dosyaAdi;

        await _db.SaveChangesAsync();
        return RedirectToAction("Dashboard");
    }

    // ============================================
    // YAZI SİL
    // ============================================
    [HttpPost("yazi-sil/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> YaziSil(int id)
    {
        var yazi = await _db.Posts.FindAsync(id);
        if (yazi != null)
        {
            _db.Posts.Remove(yazi);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Dashboard");
    }

    // ============================================
    // SORULAR (herkese açık S.S.S. için cevap yazma)
    // ============================================
    [HttpGet("sorular")]
    public async Task<IActionResult> Sorular()
    {
        var sorular = await _db.Sorular
            .Include(s => s.SoranKullanici)
            .OrderBy(s => s.Durum == "bekliyor" ? 0 : 1)
            .ThenByDescending(s => s.OlusturulmaTarihi)
            .ToListAsync();

        return View(sorular);
    }

    [HttpPost("sorular/{id:int}/cevapla")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoruCevapla(int id, string cevap)
    {
        var soru = await _db.Sorular.FindAsync(id);
        if (soru != null && !string.IsNullOrWhiteSpace(cevap))
        {
            soru.Cevap = cevap.Trim();
            soru.Durum = "cevaplandi";
            soru.CevaplanmaTarihi = DateTime.Now;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Sorular");
    }
}
