using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Application.Abstractions.Repositories;
using AirportTicketBookingSystem.Domain.Entities;

namespace AirportTicketBookingSystem.Infrastructure.Persistence
{
    public sealed class BookingRepository : JsonFileRepository<Booking>, IBookingRepository
    {
        public BookingRepository() : base("Bookings.json")
        {
        }

        public async Task<IReadOnlyList<Booking>> GetAllAsync()
        {
            return await ReadAllAsync();
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            var bookings = await ReadAllAsync();

            return bookings.FirstOrDefault(booking => booking.Id == id);
        }

        public async Task AddAsync(Booking booking)
        {
            var bookings = (await ReadAllAsync()).ToList();

            bookings.Add(booking);

            await WriteAllAsync(bookings);
        }

        public async Task UpdateAsync(Booking booking)
        {
            var bookings = (await ReadAllAsync()).ToList();

            var index = bookings.FindIndex(existingBooking => existingBooking.Id == booking.Id);

            if (index == -1)
                return;

            bookings[index] = booking;

            await WriteAllAsync(bookings);
        }
    }
}
