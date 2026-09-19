using System.ComponentModel.DataAnnotations;

namespace BlogSite.Models;

public class Soru
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Soru metni zorunludur.")]
    [StringLength(1000)]
    public string SoruMetni { get; set; } = string.Empty;

    public string? Cevap { get; set; }

    // "bekliyor" veya "cevaplandi"
    public string Durum { get; set; } = "bekliyor";

    public int SoranKullaniciId { get; set; }
    public Kullanici? SoranKullanici { get; set; }

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
    public DateTime? CevaplanmaTarihi { get; set; }
}
