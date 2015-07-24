using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class Conversions
    {
        public static int GetDraftYearFromBirthYear(DateTime birthday)
        {
            DateTime cutOffDate = new DateTime(birthday.Year, 08, 15);

            if (DateTime.Compare(birthday, cutOffDate) <= 0)
            {
                return cutOffDate.Year + 18;
            }

            return cutOffDate.Year + 19;
        }
    }
}
