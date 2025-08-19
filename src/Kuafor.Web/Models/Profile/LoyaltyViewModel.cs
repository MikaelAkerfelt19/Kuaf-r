using System.Collections.Generic;

namespace Kuafor.Web.Models.Profile
{
    public class LoyaltyViewModel
    {
        public string Tier { get; set; } = "Gümüş"; // Bronz/Gümüş/Altın/Platin
        public int Points { get; set; } = 0;
        public int NextTierPoints { get; set; } = 200; // Bir üst seviye için gereken puan
        public int PointsToNext => NextTierPoints > Points ? NextTierPoints - Points : 0;
        public List<string> Benefits { get; set; } = new();
    }
}