using Prototype.DocuSign.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.DocuSign.Models
{
    public interface IDocuSignAPI
    {
        string DocuSignURL { get; set; }
        string DocuSignUserName { get; set; }
        string DocuSignPWD { get; set; }
        string DocuSignIntegratorKey { get; set; }
        string SenderViewURI { get; set; }
        Uri Login();
        string SendSingleDocumentSenderView(string emailBody, string emailSubject, string documentName, byte[] document, string senderEmail, string senderName);
        string SendSingleDocument(string emailBody, string emailSubject, string documentName, byte[] document, string senderEmail, string senderName, decimal X, decimal Y, int pageNumber);
        string GetSenderView(string envelopeId);
        IEnumerable<Document> GetStatusUpdate(IEnumerable<Document> listOfDocumentsToCheck);
        byte[] GetDocument(string envelopeId);
    }
}
