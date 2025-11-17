using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkingManagementSystem.Models
{
    internal class Site
    {
        private static HashSet<Guid> _existingIds = new HashSet<Guid>();

        public Site(Guid id, string name, string lat, string @long)
        {
            if (_existingIds.Contains(id))
            {
                throw new InvalidOperationException($"Site with ID {id} already exists. Each site must have a unique ID.");
            }

            Id = id;
            Name = name;
            Lat = lat;
            Long = @long;

            _existingIds.Add(id);
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Lat { get; private set; }
        public string Long { get; private set; }
    }
}
