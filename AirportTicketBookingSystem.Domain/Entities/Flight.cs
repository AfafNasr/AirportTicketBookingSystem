using AirportTicketBookingSystem.Domain.Common;
using AirportTicketBookingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AirportTicketBookingSystem.Domain.Entities
{
    public sealed class Flight : Entity
    {
        public string FlightNumber { get; private set; }
        public string DepartureCountry { get; private set; }
        public string DestinationCountry { get; private set; }
        public string DepartureAirport { get; private set; }
        public string ArrivalAirport { get; private set; }
        public DateTime DepartureDate { get; private set; }
        public DateTime ArrivalDate { get; private set; }

  
        public decimal EconomyPrice { get; private set; }
        public decimal BusinessPrice { get; private set; }
        public decimal FirstClassPrice { get; private set; }

        public int EconomySeats { get; private set; }
        public int BusinessSeats { get; private set; }
        public int FirstClassSeats { get; private set; }

        private Flight()
        {
            FlightNumber = string.Empty;
            DepartureCountry = string.Empty;
            DestinationCountry = string.Empty;
            DepartureAirport = string.Empty;
            ArrivalAirport = string.Empty;
        }

        public Flight(
            string flightNumber,
            string departureCountry,
            string destinationCountry,
            string departureAirport,
            string arrivalAirport,
            DateTime departureDate,
            DateTime arrivalDate,
            decimal economyPrice,
            decimal businessPrice,
            decimal firstClassPrice,
            int economySeats,
            int businessSeats,
            int firstClassSeats)
        {
            if (arrivalDate <= departureDate)
                throw new ArgumentException("Arrival date must be after departure date.");

            if (economyPrice < 0 || businessPrice < 0 || firstClassPrice < 0)
                throw new ArgumentException("Prices cannot be negative.");

            if (economySeats < 0 || businessSeats < 0 || firstClassSeats < 0)
                throw new ArgumentException("Seats cannot be negative.");

            FlightNumber = flightNumber;
            DepartureCountry = departureCountry;
            DestinationCountry = destinationCountry;
            DepartureAirport = departureAirport;
            ArrivalAirport = arrivalAirport;
            DepartureDate = departureDate;
            ArrivalDate = arrivalDate;
            EconomyPrice = economyPrice;
            BusinessPrice = businessPrice;
            FirstClassPrice = firstClassPrice;
            EconomySeats = economySeats;
            BusinessSeats = businessSeats;
            FirstClassSeats = firstClassSeats;
        }

        public decimal GetPrice(TravelClass travelClass)
        {
            return travelClass switch
            {
                TravelClass.Economy => EconomyPrice,
                TravelClass.Business => BusinessPrice,
                TravelClass.FirstClass => FirstClassPrice,
                _ => throw new ArgumentOutOfRangeException(nameof(travelClass))
            };
        }

        public bool HasAvailableSeat(TravelClass travelClass)
        {
            return GetAvailableSeats(travelClass) > 0;
        }

        public void ReserveSeat(TravelClass travelClass)
        {
            if (!HasAvailableSeat(travelClass))
                throw new InvalidOperationException("No available seats for the selected class.");

            switch (travelClass)
            {
                case TravelClass.Economy:
                    EconomySeats--;
                    break;

                case TravelClass.Business:
                    BusinessSeats--;
                    break;

                case TravelClass.FirstClass:
                    FirstClassSeats--;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(travelClass));
            }
        }

        public int GetAvailableSeats(TravelClass travelClass)
        {
            return travelClass switch
            {
                TravelClass.Economy => EconomySeats,
                TravelClass.Business => BusinessSeats,
                TravelClass.FirstClass => FirstClassSeats,
                _ => throw new ArgumentOutOfRangeException(nameof(travelClass))
            };
        }

        public void ReleaseSeat(TravelClass travelClass)
        {
            switch (travelClass)
            {
                case TravelClass.Economy:
                    EconomySeats++;
                    break;
                case TravelClass.Business:
                    BusinessSeats++;
                    break;
                case TravelClass.FirstClass:
                    FirstClassSeats++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(travelClass));
            }
        }
    }
}
