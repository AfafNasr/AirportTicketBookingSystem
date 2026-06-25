using System;
using System.Collections.Generic;
using System.Text;

using AirportTicketBookingSystem.Application.DTOs.Auth;
using AirportTicketBookingSystem.Application.Services;
using AirportTicketBookingSystem.ConsoleApp.Input;
using AirportTicketBookingSystem.Domain.Enums;

namespace AirportTicketBookingSystem.ConsoleApp.Menus
{
    public sealed class MainMenu
    {
        private readonly AuthService _authService;
        private readonly PassengerMenu _passengerMenu;
        private readonly ManagerMenu _managerMenu;

        public MainMenu(
            AuthService authService,
            PassengerMenu passengerMenu,
            ManagerMenu managerMenu)
        {
            _authService = authService;
            _passengerMenu = passengerMenu;
            _managerMenu = managerMenu;
        }

        public async Task ShowAsync()
        {
            while (true)
            {
                ConsoleUi.Header("AIRPORT TICKET BOOKING SYSTEM");

                Console.WriteLine("1. Login");
                Console.WriteLine("2. Register as Passenger");
                Console.WriteLine("3. Exit");
                Console.WriteLine();

                var choice = ConsoleUi.Prompt("Choose option");

                switch (choice)
                {
                    case "1":
                        await LoginAsync();
                        break;

                    case "2":
                        await RegisterAsync();
                        break;

                    case "3":
                        return;

                    default:
                        ConsoleUi.Error("Invalid option.");
                        ConsoleUi.Pause();
                        break;
                }
            }
        }

        private async Task LoginAsync()
        {
            ConsoleUi.Header("LOGIN");

            var result = await _authService.LoginAsync(new LoginRequest
            {
                Email = ConsoleUi.Prompt("Email"),
                Password = ConsoleUi.Prompt("Password")
            });

            if (result.IsFailure)
            {
                ConsoleUi.Error(result.Error);
                ConsoleUi.Pause();
                return;
            }

            ConsoleUi.Success($"Welcome, {result.Value!.FullName}");

            if (result.Value.Role == UserRole.Passenger)
                await _passengerMenu.ShowAsync();
            else
                await _managerMenu.ShowAsync();
        }

        private async Task RegisterAsync()
        {
            ConsoleUi.Header("PASSENGER REGISTRATION");

            var result = await _authService.RegisterPassengerAsync(new RegisterPassengerRequest
            {
                FullName = ConsoleUi.Prompt("Full Name"),
                Email = ConsoleUi.Prompt("Email"),
                Password = ConsoleUi.Prompt("Password"),
                PassportNumber = ConsoleUi.Prompt("Passport Number")
            });

            if (result.IsFailure)
            {
                ConsoleUi.Error(result.Error);
                ConsoleUi.Pause();
                return;
            }

            ConsoleUi.Success("Registration completed successfully.");
            await _passengerMenu.ShowAsync();
        }
    }
}
