using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.RfidTable.TakeAwayPrice
{
    public class ListDataItem    :IEquatable<ListDataItem>
    {
        public string UUID { get; set; }
        public string Model { get; set; }
        public string Price { get; set; }
        public DateTime UpdatedDate { get; set; }

        public bool Equals(ListDataItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(UUID, other.UUID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ListDataItem) obj);
        }

        public override int GetHashCode()
        {
            return (UUID != null ? UUID.GetHashCode() : 0);
        }
    }
}
