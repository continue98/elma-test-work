using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models
{
    public class Users
    {
        public virtual int Id { get; set; }
        public virtual string HashPassword { get; set; }
        public virtual string Email { get; set; }
        public virtual string Name { get; set; }
        public virtual string Salt { get; set; }
    }
}