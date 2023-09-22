using System.ComponentModel.DataAnnotations;

namespace Flights.Domain.Entities
{
    public record Booking(
        Guid FlightId,
        string PassengerEmail,
        byte NumberOfSeats);
}
