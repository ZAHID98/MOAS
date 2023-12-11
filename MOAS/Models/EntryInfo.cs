using System.ComponentModel.DataAnnotations;

namespace MOAS.Models
{
    public abstract class EntryInfo
    {

        public int UserID { get; set; }

        public DateTime EntryDateTime { get; set; }
        [StringLength(100)]
        public string HostName { get; set; }
        public bool Cancel { get; set; }

    }
}
