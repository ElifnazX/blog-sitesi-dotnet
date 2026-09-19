using System.ComponentModel.DataAnnotations;

namespace BlogSite.Models;

public class GirisViewModel
{
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    public string Parola { get; set; } = string.Empty;
}

public class KayitViewModel
{
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(150)]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalı.")]
    [DataType(DataType.Password)]
    public string Parola { get; set; } = string.Empty;

    // Boş bırakılırsa "kullanici" rolüyle kaydolur.
    // Doğru kod girilirse "sahip" rolüyle kaydolur.
    public string? SahipKodu { get; set; }
}

public class SoruSorViewModel
{
    [Required(ErrorMessage = "Soru metni zorunludur.")]
    [StringLength(1000)]
    public string SoruMetni { get; set; } = string.Empty;
}

public class SoruSorSayfaViewModel
{
    public SoruSorViewModel YeniSoru { get; set; } = new();
    public List<Soru> Sorularim { get; set; } = new();
}

public class YaziFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(255)]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Ozet { get; set; }

    [Required(ErrorMessage = "İçerik zorunludur.")]
    public string Icerik { get; set; } = string.Empty;

    public string Durum { get; set; } = "taslak";

    public IFormFile? KapakResimDosya { get; set; }

    // Düzenleme ekranında mevcut resmi göstermek için
    public string? MevcutKapakResim { get; set; }
}
