using Abp.Dependency;
using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KonbiCloud.Products.Helpers
{
    public  class SKUHelper: ISKUHelper
    {
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static Random random = new Random();
        public string Generate(int length = 16)
        {           
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public bool Validate(string input)
        {
            Regex r = new Regex("^[a-zA-Z0-9]*$");
            if (r.IsMatch(input))
            {
                return true;
            }
            return false;
        }
        
    }
}
