using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Extensions
{
    public static class ParsingExtensions
    {
        public static int ToInt(this string str) => int.TryParse(str, out int result) ? result : default(int);
    }
}
