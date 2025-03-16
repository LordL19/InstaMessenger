using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstantMessenger.Models
{
    public class User
    {
        public string Username { get; set; }
        public string IpAddress { get; set; }

        public User(string username, string ipAddress)
        {
            Username = username;
            IpAddress = ipAddress;
        }

        public override string ToString()
        {
            return Username;
        }
    }
}