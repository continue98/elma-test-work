using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models
{
    public class DocsModel
    {
        public virtual string NameDoc { set; get; }
        public virtual DateTime Date { set; get; }
        public virtual string Author { set; get; }
        public virtual byte[] BinaryFile { get; set; }
    }
}