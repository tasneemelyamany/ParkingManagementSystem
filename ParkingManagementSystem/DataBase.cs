using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParkingManagementSystem.Models;

namespace ParkingManagementSystem
{
    internal class DataBase : IDataBase
    {
        private static List<Site> sites = new List<Site>
        {
            new Site(Guid.Parse("7c8d24cf-0f3c-4d3f-8014-0b44b0cbbc43"), "Site A", "40.7128 N", "74.0060 W"),
            new Site(Guid.Parse("4f0c3b03-2b0b-4a13-b4f2-3aa5e66d0c42"), "Site B", "51.5074 N", "0.1278 W"),
            new Site(Guid.Parse("4f0c3b03-2b0b-4a13-b4f2-3aa5e66d0c44"), "Site C", "51.5074 N", "0.1278 W")
        };

        private static List<Tariff> tariffs = new List<Tariff>
        {
            new Tariff(Guid.Parse("4f0c3b03-9b0b-4a13-b4f2-3aa5e66d0c42"), sites[0], 5.00m, 3.00m, false),
            new Tariff(Guid.Parse("8f0c3b03-9b0b-4a13-b4f2-3aa5e66d0c42"), sites[1], 6.00m, 4.00m, false)
        };

        private static List<Ticket> tickets = new List<Ticket>();

        public IEnumerable<Site> GetSites(Func<Site, bool>? filter)
        {
            return filter == null ? sites : sites.Where(filter);
        }


        public IEnumerable<Tariff> GetTariffs(Func<Tariff, bool>? filter)
        {
            return filter == null ? tariffs : tariffs.Where(filter);
        }

        public IEnumerable<Ticket> GetTickets(Func<Ticket, bool>? filter)
        {
            return filter == null ? tickets : tickets.Where(filter);
        }

        public Task SaveTicket(Ticket ticket)
        {
            return Task.Run(()=>tickets.Add(ticket));
        }
    }
}
