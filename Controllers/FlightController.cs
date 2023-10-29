using Flights.ReadModel;
using Flights.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Flights.Dtos;
using Flights.Domain.Errors;
using Flights.Data;
using Microsoft.EntityFrameworkCore;

namespace Flights.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [ProducesResponseType(typeof(IEnumerable<FlightRm>), 200)]

    public class FlightController : ControllerBase
    {
        private readonly ILogger<FlightController> _logger;

        private readonly Entities _entities;

        public FlightController(ILogger<FlightController> logger, Entities entities)
        {
            _logger = logger;
            _entities = entities;
        }


        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(typeof(IEnumerable<FlightRm>), 200)]
        public IEnumerable<FlightRm> Search([FromQuery]FlightSearchParameters parameters)
        {
            _logger.LogInformation("Searching for a flight for: {Destination}", parameters.Destination);

            IQueryable<Flight> flights = _entities.Flights;

            if (!string.IsNullOrWhiteSpace(parameters.Destination))
                flights = flights.Where(f => f.Arrival.Place.Contains(parameters.Destination));

            if (!string.IsNullOrWhiteSpace(parameters.From))
                flights = flights.Where(f => f.Departure.Place.Contains(parameters.From));

            if (parameters.FromDate != null)
                flights = flights.Where(f => f.Departure.Time >= parameters.FromDate.Value.Date);

            if (parameters.ToDate != null)
                flights = flights.Where(f => f.Departure.Time >= parameters.ToDate.Value.Date.AddDays(1).AddTicks(-1));

            if (parameters.NumberOfPassengers != 0 && parameters.NumberOfPassengers != null)
                flights = flights.Where(f => f.RemainingNumberOfSeats >= parameters.NumberOfPassengers);
            else
                flights = flights.Where(f => f.RemainingNumberOfSeats >= 1);

            var flightRmList = flights.Select(flight => new FlightRm(
                flight.Id,
                flight.Airline,
                flight.Price,
                new TimePlaceRm(flight.Departure.Place.ToString(), flight.Departure.Time),
                new TimePlaceRm(flight.Arrival.Place.ToString(), flight.Arrival.Time),
                flight.RemainingNumberOfSeats
                ));
            return flightRmList;
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]

        [HttpGet("{id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(typeof(FlightRm), 200)]
        public ActionResult<FlightRm> Find(Guid id)
        {
            var flight = _entities.Flights.SingleOrDefault(f => f.Id == id);
            if (flight == null)
                return NotFound();

            var readModel = new FlightRm(
                flight.Id,
                flight.Airline,
                flight.Price,
                new TimePlaceRm(flight.Departure.Place.ToString(), flight.Departure.Time),
                new TimePlaceRm(flight.Arrival.Place.ToString(), flight.Arrival.Time),
                flight.RemainingNumberOfSeats
                );
            return Ok(readModel);
        }

        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(200)]


        public IActionResult Book(BookDto dto)
        {
            System.Diagnostics.Debug.WriteLine($"Booking a new flight {dto.FlightId}");

            var flight = _entities.Flights.SingleOrDefault(f => f.Id == dto.FlightId);

            if (flight == null)
                return NotFound();

            var error = flight.MakeBooking(dto.PassengerEmail, dto.NumberOfSeats);

            if (error is OverbookError)
                return Conflict(new { message = "Sorry, Not Enough Seats" });
            try
            {
                _entities.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(new { message = "An error occurred while booking. Try again!" });

            }

            return CreatedAtAction(nameof(Find), new { id = dto.FlightId });
        }



    }
}