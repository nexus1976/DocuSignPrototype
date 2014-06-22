using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prototype.DocuSign.Models;
using Prototype.DocuSign.DAL;

namespace Prototype.DocuSign.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            IHomeModel model = HomeModelFactory.CreateHomeModel();
            return View("Index", model);
        }

        public ActionResult Logout()
        {
            if(Session["login"] != null)
            {
                Session.Remove("login");
            }
            IHomeModel model = HomeModelFactory.CreateHomeModel();
            return View("Index", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Index(HomeModel modelUp)
        {
            IHomeModel model = this.HydrateHomeModel(modelUp);
            model.RefreshDocuments();
            return View("Index", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(HomeModel modelUp)
        {
            if(string.IsNullOrWhiteSpace(modelUp.CurrentUser))
            {
                modelUp.ErrorMessage = "You must enter a value in order to login to the system.";
                return View("Index", modelUp);
            }

            IHomeModel model = this.HydrateHomeModel(modelUp);
            return View("Index", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SubmitDocusign(HomeModel modelUp, HttpPostedFileBase file)
        {
            IHomeModel model = this.HydrateHomeModel(modelUp);

            if(string.IsNullOrWhiteSpace(model.SendToEmail))
            {
                model.ErrorMessage = "You must enter a valid Send To Email value before attempting to send a document.";
                return View("Index", model);
            }
            if (string.IsNullOrWhiteSpace(model.SendToName))
            {
                model.ErrorMessage = "You must enter a valid Send To Name before attempting to send a document.";
                return View("Index", model);
            }
            if(model.UseSignatureXY)
            {
                if(!model.SignaturePageNumber.HasValue)
                {
                    model.ErrorMessage = "If directly specifying a signature location, you must enter a valid number into the Page Number field.";
                    return View("Index", model);
                }
                if (!model.SignatureX.HasValue)
                {
                    model.ErrorMessage = "If directly specifying a signature location, you must enter a valid number into the X Coordinate field.";
                    return View("Index", model);
                }
                if (!model.SignatureY.HasValue)
                {
                    model.ErrorMessage = "If directly specifying a signature location, you must enter a valid number into the Y Coordinate field.";
                    return View("Index", model);
                }
            }

            if(file != null && file.ContentLength > 0)
            {
                model.FileName = Path.GetFileName(file.FileName);
                using (MemoryStream ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    model.Document = ms.GetBuffer();
                }
                model.UploadDocumentToDocuSign();
            }
            else
            {
                model.ErrorMessage = "Either a file was not specified or it was empty. Please specify a valid file before attempting to send a document.";
            }

            return View("Index", model);
        }

        public RedirectResult ShowDocument(string envelopeId)
        {
            string url = HomeModel.GetDocumentURL(envelopeId);
            return Redirect(url);
        }

        [HttpPost]
        public JsonResult DeleteDocument(string envelopeId)
        {
            IHomeModel model = this.HydrateHomeModel(null);
            model.DeleteDocument(envelopeId);
            return Json(envelopeId);
        }

        private IHomeModel HydrateHomeModel(IHomeModel receivedModel)
        {
            IHomeModel model = HomeModelFactory.CreateHomeModel();
            if (receivedModel != null)
            {
                model.CurrentUser = receivedModel.CurrentUser;
                model.SendToEmail = receivedModel.SendToEmail;
                model.SendToName = receivedModel.SendToName;
                model.UseSignatureXY = receivedModel.UseSignatureXY;
                model.SignaturePageNumber = receivedModel.SignaturePageNumber;
                model.SignatureX = receivedModel.SignatureX;
                model.SignatureY = receivedModel.SignatureY;
            }
            if(string.IsNullOrWhiteSpace(model.CurrentUser))
            {
                model.CurrentUser = Session["login"] == null ? null : Session["login"].ToString();
            }
            else
            {
                Session.Add("login", model.CurrentUser);
            }
            return model;
        }
    }
}