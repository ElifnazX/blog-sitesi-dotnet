using System.ComponentModel.DataAnnotations;

namespace BlogSite.Models;

public static class Roller
{
    public const string Sahip = "sahip";
    public const string Kullanici = "kullanici";
}

public class Kullanici
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string AdSoyad { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string ParolaHash { get; set; } = string.Empty;

    // "sahip" (Ayşe Nur - blog yazabilir) veya "kullanici" (soru sorabilir)
    [Required, StringLength(20)]
    public string Rol { get; set; } = Roller.Kullanici;

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;
}
