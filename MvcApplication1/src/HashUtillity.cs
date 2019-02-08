using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1
{
    public class HashUtillity
    {
        public bool compare_password(string password, string hash_password, string salt)
        {
            string hashsed_password = Convert.ToBase64String(CryptSharp.Utility.SCrypt.ComputeDerivedKey(System.Text.UTF8Encoding.UTF8.GetBytes(password), Convert.FromBase64String(salt), 16384, 8, 1, null, 128));

            if (hashsed_password == hash_password)
                return true;
            return false;
        }
    }
}