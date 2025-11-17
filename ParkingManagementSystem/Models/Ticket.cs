using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkingManagementSystem.Models
{
    internal class Ticket
    {
        private const int GRACE_PERIOD_MINUTES = 10;
        private const int MAX_TICKET_DURATION_HOURS = 24;
        public Ticket(Guid id, string plateNumber, DateTime from, DateTime to, decimal price,
            bool isExtend, Guid siteId, string siteLat, string siteLong, Guid? tariffId, string? paymentRefrence = null)
        {
            Id = id;
            PlateNumber = plateNumber;
            From = DateTime.SpecifyKind(from,DateTimeKind.Utc);
            To = DateTime.SpecifyKind(to, DateTimeKind.Utc);
            Price = price;
            IsExtend = isExtend;
            SiteId = siteId;
            SiteLat = siteLat;
            SiteLong = siteLong;
            TariffId = tariffId;
                    PaymentReference = paymentRefrence;
        }

        public Guid Id { get; private set; }
        public string PlateNumber { get; private set; }
        public DateTime From { get; private set; }
        public DateTime To { get; private set; }
        public decimal Price { get; private set; }
        public bool IsExtend { get; private set; }
        public Guid SiteId { get; private set; }
        public string SiteLat { get; private set; }
        public string SiteLong { get; private set; }
        public Guid? TariffId { get; private set; }
        public string? PaymentReference { get; private set; }


        public static Ticket Create(string plateNumber, DateTime from, DateTime to, Site site,
                                    Tariff tariff = null, string? paymentReference = null)
        {
            //maintaining using the UTC 
            var utcFrom = from.Kind == DateTimeKind.Utc ? from : from.ToUniversalTime();
            var utcTo = to.Kind == DateTimeKind.Utc ? to : to.ToUniversalTime();
            var utcNow = DateTime.UtcNow;

            //Validation
            TicketCreationValidation(utcFrom, utcTo, utcNow, site);

            //handle  null tariff
            tariff ??= Tariff.Default;

            // price based on tariff
            decimal price = tariff.Calculate(utcFrom, utcTo);
            Guid? tariffId = tariff.IsDefault ? null : tariff.Id;

            return new Ticket(Guid.NewGuid(), plateNumber, utcFrom, utcTo, price, false, site.Id, site.Lat, site.Long, tariffId, paymentReference);
            
        }

        public Ticket Extend(DateTime newTo, Site site, Tariff tariff, string? paymentReference = null)
        {
            //maintaining using the UTC
            var utcNewTo = newTo.Kind == DateTimeKind.Utc ? newTo : newTo.ToUniversalTime();
            var utcNow = DateTime.UtcNow;

            //is extandable?
            if (!IsExtendable())
            {
                throw new InvalidOperationException("Ticket cannot be extended. It has expired beyond the grace period.");
            }

            // the ticket duration validation
            if (utcNewTo <= this.To)
            {
                throw new ArgumentException("New end time must be after current end time.");
            }

            if ((utcNewTo - this.To).TotalHours > MAX_TICKET_DURATION_HOURS)
            {
                throw new ArgumentException($"Extension cannot exceed {MAX_TICKET_DURATION_HOURS} hours.");
            }

            tariff ??= Tariff.Default;

            decimal additionalPrice = tariff.Calculate(this.To, utcNewTo);
            Guid? tariffId = tariff.IsDefault ? null : tariff.Id;

            return new Ticket(this.Id, this.PlateNumber, this.From, utcNewTo,
                this.Price + additionalPrice, true, this.SiteId, this.SiteLat, this.SiteLong, tariffId, paymentReference);
        }

        /// <summary>
        /// TODO: Implement method to check if the ticket is extendable
        /// </summary>
        /// <returns></returns>
        /// 
        private static void TicketCreationValidation (DateTime utcfrom, DateTime utcto, DateTime utcnow, Site site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site),"This site is not available now");
            }

            // the start and end part --> note ::: make sure that is working properly with the cieling you did
            if (utcfrom >= utcto)
            {
                throw new Exception("Start time must be before the end time");
            }

            // this is and old spots
            if (utcfrom < utcnow)
            {
                throw new ArgumentException("Cannot issue a ticket for past time.");
            }

            //check the max duration 
            var duration = utcto - utcfrom;
            if (duration.TotalHours > MAX_TICKET_DURATION_HOURS)
            {
                throw new ArgumentException($"Cannot issue a ticket for more than {MAX_TICKET_DURATION_HOURS} hours.");
            }

        }
        public bool IsExtendable()
        {
            var utcNow = DateTime.UtcNow;
            var gracePeriod = this.To.AddMinutes(GRACE_PERIOD_MINUTES);

            return utcNow <= gracePeriod;

        }  // we can add the statuses of the ticket like active, expired, etc.
    }
}
