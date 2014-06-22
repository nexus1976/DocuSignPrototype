using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Prototype.DocuSign.DAL
{
    public class DNAContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
    }

    public class DNAContextFactory
    {
        public static DNAContext CreateDNAContext()
        {
            return new DNAContext();
        }
    }
}