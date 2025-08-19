using System.Collections.Generic;

namespace Kuafor.Web.Models.Profile
{
    public record AddressItemVm (int Id, string Title, string Line1, string City, string District, string Zip, bool IsDefault);

    public class AddressesViewModel
    {
        public List<AddressItemVm> Items { get; set; } = new();
    }
}