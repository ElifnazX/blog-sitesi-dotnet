using System.ComponentModel.DataAnnotations;

namespace BlogSite.Models;

public class Post
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(255)]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(255)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Ozet { get; set; }

    [Required(ErrorMessage = "İçerik zorunludur.")]
    public string Icerik { get; set; } = string.Empty;

    [StringLength(255)]
    public string? KapakResim { get; set; }

    // "taslak" veya "yayinda"
    public string Durum { get; set; } = "taslak";

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    public DateTime GuncellenmeTarihi { get; set; } = DateTime.Now;

    public int YazarId { get; set; }
    public Kullanici? Yazar { get; set; }
}
