using System.ComponentModel;

namespace Flights.Dtos
{
    public record FlightSearchParameters(
        [DefaultValue("12/31/2023 05:50:00 PM")] 
        DateTime?  FromDate,

        [DefaultValue("12/31/2023 05:50:00 PM")]
        DateTime? ToDate,

        [DefaultValue("Seattle")]
        string? From,

        [DefaultValue("Los Angeles")]
        string? Destination,


        [DefaultValue(1)]
        int? NumberOfPassengers);
}

