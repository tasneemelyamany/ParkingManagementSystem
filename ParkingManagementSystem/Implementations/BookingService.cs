using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParkingManagementSystem.Abstractions;
using ParkingManagementSystem.Models;

namespace ParkingManagementSystem.Implementations
{
    internal class BookingService : IBookingService
    {
        private const int MAX_TICKET_DURATION_HOURS = 24;

        private readonly IPaymentService _paymentService;
        private readonly IDataBase _database;
        public BookingService(IPaymentService paymentService, IDataBase database)
        {
            _paymentService = paymentService;
            _database = database;
        }

        public async Task<string> BookParkingAsync(string plateNumber, Guid siteId, DateTime from, DateTime to, string cardNumber)
        {
            ValidateBooking(plateNumber, siteId, from, to);

            //getting site, tariff
            var site = GetSite(siteId);
            var tariff = GetTariff(siteId);
            var amount = tariff.Calculate(from, to);

            string paymentReference = null;

            if (amount > 0)
            {
                var paymentResult = await _paymentService.ProcessPaymentAsync(cardNumber, amount);
                if (!paymentResult.IsSuccessful)
                {
                    throw new Exception("Payment failed. Booking cannot be completed.");
                }
                paymentReference = paymentResult.RefNumber;
            }
            
            var ticket = Ticket.Create(plateNumber, from, to, site, tariff, paymentReference);

            await _database.SaveTicket(ticket);

            return ticket.Id.ToString();
        }

        public async Task<string> ExtendParkingAsync(Guid ticketId, DateTime newTo, string cardNumber)
        {
            var existingTicket = _database.GetTickets(t => t.Id == ticketId).FirstOrDefault();
            if (existingTicket == null)
            {
                throw new Exception("Ticket not found.");
            }

            if (!existingTicket.IsExtendable())
            {
                throw new InvalidOperationException("Ticket cannot be extended. It has expired beyond the grace period.");
            }

            // get the site and tariff
            var site = GetSite(existingTicket.SiteId);

            var tariff = existingTicket.TariffId.HasValue
                ? _database.GetTariffs(t => t.Id == existingTicket.TariffId.Value).FirstOrDefault() ?? Tariff.Default
                : Tariff.Default;


            var additionalAmount = tariff.Calculate(existingTicket.To, newTo);

            // Payment for extending
            string paymentReference = null;

            if (additionalAmount > 0)
            {
                if (string.IsNullOrWhiteSpace(cardNumber))
                {
                    throw new ArgumentException("Card number is required for paid extensions.", nameof(cardNumber));
                }

                var paymentResult = await _paymentService.ProcessPaymentAsync(cardNumber, additionalAmount);

                if (!paymentResult.IsSuccessful)
                {
                    throw new Exception("Payment failed. Extension cannot be completed.");
                }

                paymentReference = paymentResult.RefNumber;
            }

            // the new extended ticket
            var extendedTicket = existingTicket.Extend(newTo, site, tariff, paymentReference);

            // Save extended ticket
            await _database.SaveTicket(extendedTicket);

            return extendedTicket.Id.ToString();
        }

        /// <summary>
        /// Validates the booking details
        /// </summary>
        /// <param name="plateNumber"></param>
        /// <param name="siteId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void ValidateBooking(string plateNumber, Guid siteId, DateTime from, DateTime to)
        {
            // validate plate number
            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                throw new ArgumentException("Plate number is Invalid");
            }

            // validate site
            if (siteId == Guid.Empty)
            {
                throw new ArgumentException("site id is missing");
            }

            // validate time
            if (from >= to)
            {
                throw new ArgumentException("enter a valid period");
            }

            // Validate not in the past
            var utcFrom = from.Kind == DateTimeKind.Utc ? from : from.ToUniversalTime();
            if (utcFrom < DateTime.UtcNow)
            {
                throw new ArgumentException("Cannot book parking for past time.");
            }

            // Validate maximum duration
            if ((to - from).TotalHours > MAX_TICKET_DURATION_HOURS)
            {
                throw new ArgumentException("Cannot book parking for more than 24 hours.");
            }

        }
        

        private Site GetSite(Guid siteId)
        {
            var site = _database.GetSites(s => s.Id == siteId).FirstOrDefault();

            if (site == null)
            {
                throw new Exception("Site not found for Id");
            }

            return site;
        }

        private Tariff GetTariff(Guid siteId)
        {
            var tariff = _database.GetTariffs(t => t.Site != null && t.Site.Id == siteId).FirstOrDefault();

            return tariff ?? Tariff.Default;

            /* return _database.GetTariffs(null).FirstOrDefault(t => t.Site.Id == siteId)
                ?? throw new Exception("Tariff not found for the specified site."); */
        }

    }
}
