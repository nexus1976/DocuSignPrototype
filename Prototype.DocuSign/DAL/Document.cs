using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prototype.DocuSign.DAL
{
    public class Document
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string RequestedBy { get; set; }
        public DateTime RequestedDate { get; set; }
        public string SentToName { get; set; }
        public string SentToEmail { get; set; }
        public string DocuSignEnvelopId { get; set; }
        public string Status { get; set; }
        public byte[] DocumentPDF { get; set; }
    }
}