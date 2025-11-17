using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkingManagementSystem.Models
{
    internal class Tariff
    {
        public static readonly Tariff Default = new Tariff(Guid.Empty, site: null, firstHour: 0m, additionalHour: 0m, isDefault: true);

        private readonly bool _isDefault;

        public Tariff(Guid id, Site site, decimal firstHour, decimal additionalHour, bool isDefault)
        {
            Id = id;
            if (!isDefault && site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            Site = site;
            FirstHour = firstHour;
            AdditionalHour = additionalHour;
            _isDefault = isDefault;
        }

        public Guid Id { get; private set; }
        public Site Site { get; private set; }
        public decimal FirstHour { get; private set; }
        public decimal AdditionalHour { get; private set; }
        public bool IsDefault => _isDefault;

        public decimal Calculate(DateTime from, DateTime to)   // DateTime instead of timeonly should handle the case of going through two days.
        {
            if (to <= from)
            {
                throw new ArgumentException("End time must be greater than start time.");
            }

            // total duration in hours (rounded up to avoid the mistakes in calculations)
            var duration = to - from;
            var totalHours = (int)Math.Ceiling(duration.TotalHours);

            // calculate tariff
            var additionalHours = Math.Max(0, totalHours - 1);

            return FirstHour + (AdditionalHour * additionalHours);
        }

    }
}
