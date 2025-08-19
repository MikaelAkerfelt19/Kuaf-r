using System.Collections.Generic;

namespace Kuafor.Web.Models.Profile
{
    public record QuickRebookItem(int ServiceId, int StylistId, string Label);

    public class PreferencesViewModel
    {
        // Seçimler
        public string? PreferredBranch   { get; set; }
        public string? PreferredStylist  { get; set; }
        public string  PreferredTimeBand { get; set; } = "Hafta içi 18:00-21:00";
        public int     FlexMinutes       { get; set; } = 15; // ± esneklik (dk)

        // UI seçenekleri (mock)
        public List<string> BranchOptions   { get; set; } = new();
        public List<string> StylistOptions  { get; set; } = new();
        public List<string> TimeBandOptions { get; set; } = new();

        // Hızlı "yenile" kısayolları (geçmişten)
        public List<QuickRebookItem> QuickRebooks { get; set; } = new();
    }
}