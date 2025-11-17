using System;
using System.Linq;
using System.Threading.Tasks;
using ParkingManagementSystem.Implementations;

namespace ParkingManagementSystem
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var db = new DataBase();
            var paymentService = new PaymentService();
            var bookingService = new BookingService(paymentService, db);

            Console.WriteLine("Available sites:");
            foreach (var s in db.GetSites(null))
            {
                Console.WriteLine($"{s.Id} - {s.Name} ({s.Lat}, {s.Long})");
            }

            Console.WriteLine();
            Console.WriteLine("Available tariffs:");
            foreach (var tr in db.GetTariffs(null))
            {
                Console.WriteLine($"{tr.Id} - Site: {tr.Site?.Name ?? "N/A"} - FirstHour: {tr.FirstHour:C}, AdditionalHour: {tr.AdditionalHour:C}, IsDefault: {tr.IsDefault}");
            }

            // choose first site from database for demo
            var site = db.GetSites(null).FirstOrDefault();
            if (site == null)
            {
                Console.WriteLine("No site configured. Exiting.");
                return;
            }

            // Booking 
            var from = DateTime.UtcNow.AddMinutes(15);
            var to = from.AddHours(2);
            string cardNumber = "4111111111111111"; 

            try
            {
                var ticketIdStr = await bookingService.BookParkingAsync("ABC-123", site.Id, from, to, cardNumber);
                Console.WriteLine($"Booking succeeded. Ticket Id: {ticketIdStr}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Booking failed: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Saved tickets:");
            foreach (var t in db.GetTickets(null))
            {
                Console.WriteLine($"{t.Id} | {t.PlateNumber} | {t.From:u} -> {t.To:u} | {t.Price:C} | Site: {t.SiteId} | TariffId: {t.TariffId?.ToString() ?? "Default"} | PaymentRef: {t.PaymentReference}");
            }
            Console.WriteLine();
            // Try extend the first saved ticket
            var firstTicket = db.GetTickets(null).FirstOrDefault();
            if (firstTicket != null)
            {
                try
                {
                    var newTo = firstTicket.To.AddHours(1);
                    var extendedId = await bookingService.ExtendParkingAsync(firstTicket.Id, newTo, cardNumber);
                    Console.WriteLine($"Extension succeeded. Ticket Id: {extendedId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Extension failed: {ex.Message}");
                }
            }
        }
    }
}
