using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLib
{
   public class UserManager
    {
        public bool Add(string userId,string phone,string email)
        {
            if (userId.Length < 4)
                throw new Exception("userId Exception");
            if (phone.Contains("9"))
                throw new Exception("phone Exception");
            if (!email.Contains("@"))
                throw new Exception("email Exception");

            return true;
        }
    }
}
