using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prototype.DocuSign.DAL;
using System.ComponentModel.DataAnnotations;

namespace Prototype.DocuSign.Models
{
    public class HomeModel : IHomeModel
    {
        public virtual string CurrentUser {
            get { return this._currentUser; }
            set
            {
                this._currentUser = value;
                if(!string.IsNullOrEmpty(value))
                {
                    this.IsLoggedIn = true;
                    this.HydrateDocuments();
                }
                else
                {
                    this.IsLoggedIn = false;
                    ((List<Document>)this.Documents).Clear();
                }
            }
        }
        public virtual bool IsLoggedIn { get; set; }
        public virtual string FileName { get; set; }
        public virtual byte[] Document { get; set; }
        [DataType(DataType.EmailAddress)]
        public virtual string SendToEmail { get; set; }
        public virtual string SendToName { get; set; }
        public virtual string StatusMessage { get; set; }
        public virtual string ErrorMessage { get; set; }
        public virtual decimal? SignatureX { get; set; }
        public virtual decimal? SignatureY { get; set; }
        public virtual int? SignaturePageNumber { get; set; }
        public virtual bool UseSignatureXY { get; set; }
        public virtual IEnumerable<Document> Documents { get; set; }
        public virtual string DocuSignPopoutURL { get; set; }

        private string _currentUser = null;
        private IDocuSignAPI _docuSignAPI = null;

        public HomeModel()
        {
            this.Documents = new List<Document>();
        }
        public HomeModel(IDocuSignAPI docuSignAPI) 
        {
            this._docuSignAPI = docuSignAPI;
            this.Documents = new List<Document>();
        }

        public virtual void UploadDocumentToDocuSign()
        {
            try
            {
                this.StatusMessage = null;
                this.ErrorMessage = null;
                this.DocuSignPopoutURL = null;
                string envelopeId = null;
                string status = "SENT";
                if (this.UseSignatureXY)
                {
                    envelopeId = this._docuSignAPI.SendSingleDocument("Please sign the attached document", "DocuSign Test", this.FileName, this.Document, this.SendToEmail, this.SendToName, this.SignatureX.Value, this.SignatureY.Value, this.SignaturePageNumber.Value);
                    this.StatusMessage = string.Format("The document was successfully sent to DocuSign with an envelopeId of {0}", envelopeId);
                }
                else
                {
                    envelopeId = this._docuSignAPI.SendSingleDocumentSenderView("Please sign the attached document", "DocuSign Test", this.FileName, this.Document, this.SendToEmail, this.SendToName);
                    this.DocuSignPopoutURL = this._docuSignAPI.GetSenderView(envelopeId);
                    status = "CREATED";
                }
                
                this.PersistDocumentToQueue(envelopeId, status);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }
        public virtual void RefreshDocuments()
        {
            try
            {
                this.StatusMessage = null;
                this.ErrorMessage = null;
                this.Documents = this._docuSignAPI.GetStatusUpdate(this.Documents);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
        }
        public virtual void DeleteDocument(string envelopeId)
        {
            using (var context = DNAContextFactory.CreateDNAContext())
            {
                var doc = context.Documents.Where(d => d.DocuSignEnvelopId == envelopeId).FirstOrDefault();
                if (doc != null)
                {
                    context.Documents.Remove(doc);
                    context.SaveChanges();
                }
            }
            if(this.Documents != null)
            {
                var doc = this.Documents.Where(d => d.DocuSignEnvelopId == envelopeId).FirstOrDefault();
                if(doc != null)
                {
                    (this.Documents as List<Document>).Remove(doc);
                }
            }
        }

        public static byte[] GetDocumentByEnvelopeId(string envelopeId)
        {
            byte[] doc = new byte[] { };

            using (var context = DNAContextFactory.CreateDNAContext())
            {
                var document = context.Documents.Where(d => d.DocuSignEnvelopId == envelopeId).FirstOrDefault();
                if(document != null)
                {
                    doc = document.DocumentPDF;
                }
            }

            return doc;
        }
        public static string GetDocumentURL(string envelopeId)
        {
            IDocuSignAPI docuSignAPI = DocuSignAPIFactory.CreateDocuSignAPI();
            string url = docuSignAPI.GetSenderView(envelopeId);
            return url;
        }

        private void PersistDocumentToQueue(string envelopeId, string status)
        {
            using (var context = DNAContextFactory.CreateDNAContext())
            {
                var doc = new Document()
                {
                    DocumentName = this.FileName,
                    DocuSignEnvelopId = envelopeId,
                    DocumentPDF = this.Document,
                    RequestedBy = this.CurrentUser,
                    RequestedDate = DateTime.Now,
                    SentToEmail = this.SendToEmail,
                    SentToName = this.SendToName,
                    Status = status
                };
                context.Documents.Add(doc);
                context.SaveChanges();
            }
            this.HydrateDocuments();
        }
        private void HydrateDocuments()
        {
            using (var context = DNAContextFactory.CreateDNAContext())
            {
                List<Document> docs = new List<Document>();
                context.Documents.Where(d => d.RequestedBy == this.CurrentUser).OrderByDescending(d => d.RequestedDate).ToList().ForEach(d => docs.Add(d));
                this.Documents = docs;
            }
        }
    }

    public class HomeModelFactory
    {
        public static IHomeModel CreateHomeModel()
        {
            return new HomeModel(DocuSignAPIFactory.CreateDocuSignAPI());
        }
    }
}