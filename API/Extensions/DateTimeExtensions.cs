using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateTime dob)
        {
            var today = DateTime.Today;
            var age= today.Year-dob.Year;//someone's age
            if(dob.Date>today.AddYears(-age)) age--;//jesli nie skonczyl jeszcze dniowo
            return age;
        }
    }
}