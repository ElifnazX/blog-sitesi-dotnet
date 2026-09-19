using BlogSite.Data;
using BlogSite.Models;
using BlogSite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogSite.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    [Route("/")]
    [Route("/anasayfa")]
    public async Task<IActionResult> Index()
    {
        var sonYazilar = await _db.Posts
            .Where(p => p.Durum == "yayinda")
            .OrderByDescending(p => p.OlusturulmaTarihi)
            .Take(3)
            .ToListAsync();

        return View(sonYazilar);
    }

    [Route("/hakkinda")]
    public IActionResult Hakkinda()
    {
        return View();
    }

    [Route("/hizmetler")]
    public IActionResult Hizmetler()
    {
        return View();
    }

    [Route("/blog")]
    public async Task<IActionResult> Blog()
    {
        var yazilar = await _db.Posts
            .Where(p => p.Durum == "yayinda")
            .OrderByDescending(p => p.OlusturulmaTarihi)
            .ToListAsync();

        return View(yazilar);
    }

    [Route("/blog/{slug}")]
    public async Task<IActionResult> BlogDetay(string slug)
    {
        var yazi = await _db.Posts.FirstOrDefaultAsync(p => p.Slug == slug && p.Durum == "yayinda");

        if (yazi == null)
        {
            Response.StatusCode = 404;
            return View("YaziBulunamadi");
        }

        return View(yazi);
    }

    [Route("/iletisim")]
    public IActionResult Iletisim()
    {
        return View();
    }

    [Route("/sorular")]
    public async Task<IActionResult> SSS()
    {
        var cevaplanmisSorular = await _db.Sorular
            .Where(s => s.Durum == "cevaplandi")
            .OrderByDescending(s => s.CevaplanmaTarihi)
            .ToListAsync();

        return View(cevaplanmisSorular);
    }

    [Route("/hata")]
    public IActionResult Hata()
    {
        return View();
    }
}
