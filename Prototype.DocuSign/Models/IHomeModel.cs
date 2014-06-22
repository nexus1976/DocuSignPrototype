using System;
using System.Collections.Generic;
namespace Prototype.DocuSign.Models
{
    public interface IHomeModel
    {
        string CurrentUser { get; set; }
        bool IsLoggedIn { get; set; }
        string FileName { get; set; }
        byte[] Document { get; set; }
        string SendToEmail { get; set; }
        string SendToName { get; set; }
        string StatusMessage { get; set; }
        string ErrorMessage { get; set; }
        decimal? SignatureX { get; set; }
        decimal? SignatureY { get; set; }
        int? SignaturePageNumber { get; set; }
        bool UseSignatureXY { get; set; }
        string DocuSignPopoutURL { get; set; }
        void UploadDocumentToDocuSign();
        void RefreshDocuments();
        void DeleteDocument(string envelopeId);
        IEnumerable<DAL.Document> Documents { get; set; }
    }
}
