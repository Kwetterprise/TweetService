using System;
using System.Collections.Generic;
using System.Text;

namespace Business
{
    public class JwtConfiguration
    {
        public JwtConfiguration(string issuer, string key)
        {
            this.Issuer = issuer;
            this.Key = Encoding.UTF8.GetBytes(key);
        }

        public string Issuer { get; }

        public byte[] Key { get; }
    }
}
