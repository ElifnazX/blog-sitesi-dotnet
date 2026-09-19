using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BlogSite.Data;
using Microsoft.EntityFrameworkCore;

namespace BlogSite.Services;

public static class YardimciServisler
{
    /// <summary>
    /// Türkçe karakterleri de destekleyerek başlıktan URL-dostu slug üretir.
    /// </summary>
    public static string SlugOlustur(string metin)
    {
        var harfEslesmeleri = new Dictionary<char, char>
        {
            {'ı','i'}, {'İ','i'}, {'ş','s'}, {'Ş','s'}, {'ğ','g'}, {'Ğ','g'},
            {'ü','u'}, {'Ü','u'}, {'ö','o'}, {'Ö','o'}, {'ç','c'}, {'Ç','c'}
        };

        var sb = new StringBuilder();
        foreach (var karakter in metin.ToLower(new CultureInfo("tr-TR")))
        {
            sb.Append(harfEslesmeleri.TryGetValue(karakter, out var yeni) ? yeni : karakter);
        }

        var sonuc = sb.ToString();
        sonuc = Regex.Replace(sonuc, @"[^a-z0-9]+", "-");
        return sonuc.Trim('-');
    }

    /// <summary>
    /// Aynı slug varsa sonuna sayı ekleyerek benzersiz hale getirir.
    /// </summary>
    public static async Task<string> BenzersizSlugAl(AppDbContext db, string baslik, int? haricId = null)
    {
        var temel = SlugOlustur(baslik);
        var slug = temel;
        var i = 2;

        while (await db.Posts.AnyAsync(p => p.Slug == slug && (haricId == null || p.Id != haricId)))
        {
            slug = $"{temel}-{i}";
            i++;
        }

        return slug;
    }

    /// <summary>
    /// Kapak resmini wwwroot/uploads klasörüne güvenli şekilde kaydeder.
    /// Başarılıysa dosya adını, izinsiz tipte ise null, hiç dosya yoksa boş string döner.
    /// </summary>
    public static async Task<(bool Basarili, string? DosyaAdi)> KapakResmiKaydet(IFormFile? dosya, string webRootPath)
    {
        if (dosya == null || dosya.Length == 0)
        {
            return (true, null); // Dosya seçilmedi, hata değil
        }

        var izinliTipler = new Dictionary<string, string>
        {
            { "image/jpeg", "jpg" },
            { "image/png", "png" },
            { "image/webp", "webp" }
        };

        if (!izinliTipler.TryGetValue(dosya.ContentType, out var uzanti))
        {
            return (false, null); // izinli olmayan tip
        }

        if (dosya.Length > 5 * 1024 * 1024) // 5MB sınır
        {
            return (false, null);
        }

        var yeniAd = $"yazi_{Guid.NewGuid():N}.{uzanti}";
        var hedefKlasor = Path.Combine(webRootPath, "uploads");
        Directory.CreateDirectory(hedefKlasor);
        var hedefYol = Path.Combine(hedefKlasor, yeniAd);

        using (var stream = new FileStream(hedefYol, FileMode.Create))
        {
            await dosya.CopyToAsync(stream);
        }

        return (true, yeniAd);
    }

    /// <summary>
    /// Tarihi Türkçe okunaklı formatta gösterir (örn: 10 Eylül 2026).
    /// </summary>
    public static string TarihFormatla(DateTime tarih)
    {
        var ci = new CultureInfo("tr-TR");
        return tarih.ToString("d MMMM yyyy", ci);
    }
}
