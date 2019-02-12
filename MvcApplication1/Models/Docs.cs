using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models
{
    public class Docs : DocsModel
    {
        public virtual int Id { get; set; }
        public virtual int UserId { get; set; }
        public virtual string PathToFile { get; set; }
        public virtual Users User { get; set; }
        protected byte BinaryFile;
    }
}