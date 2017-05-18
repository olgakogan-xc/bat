using AssetmarkBAT.Models;
using AssetmarkBATDbConnector;
using Microsoft.Win32;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
//using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Xfinium.Pdf;
using Xfinium.Pdf.Actions;
using Xfinium.Pdf.Core;
using Xfinium.Pdf.Graphics;
using Xfinium.Pdf.Graphics.FormattedContent;


namespace AssetmarkBAT.Controllers
{
    public class AssetmarkBATController : Controller
    {

        private string _BATCookieName = "assetmarkBAT";
        private string _EloquaCookieName = "eloquaCookie";
        private string _TermsViewName = "Terms";
        private string _Page1QuestionsViewName = "Page1Questions";
        private string _Page2QuestionsViewName = "Page2Questions";
        private string _ReportViewName = "Report";
        private string _ValuationOptimizer = "ValuationOptimizer";
        private string _EloquaQueryStringParamName = "eloqua";
        //private string _UserId = string.Empty;
        //public ValuationModel _ValModel = new ValuationModel();

        public string KnownUserId()
        {
            if (HttpContext.Request.Cookies[_BATCookieName] != null && !string.IsNullOrEmpty(HttpContext.Request.Cookies[_BATCookieName].Value))
            {
                return HttpContext.Request.Cookies[_BATCookieName].Value;
            }
            else if (Request.Url.AbsoluteUri.Contains(_EloquaQueryStringParamName))
            {
                return Request.QueryString[_EloquaQueryStringParamName];
            }
            else if (HttpContext.Request.Cookies[_EloquaCookieName] != null && !string.IsNullOrEmpty(HttpContext.Request.Cookies[_EloquaCookieName].Value))
            {
                return Request.QueryString[_EloquaCookieName];
            }
            return null;
        }


        /// <summary>
        /// Action Method to initiate the BAT tool
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //FileStream htmlStream = System.IO.File.OpenRead("C:\\InputShort.html");
            //byte[] byteArray = Encoding.UTF8.GetBytes("<html><body><p>Some Text Here</p><p><strong><u><font color='#A00000'>DOCUMENT FEATURES></font></u></strong></p><ul><li>Create and load PDF documents from files and streams </li><li>Save PDF files to disk and streams </li><li>Save PDF files in PDF / A - 1B format </li></ul></body></html>");
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            //MemoryStream stream = new MemoryStream(byteArray);
            //SimpleHtmlToPdf htmlToPdf = new SimpleHtmlToPdf();
            //PdfFixedDocument document = htmlToPdf.Convert(stream);
            //document.Save("C:\\output.pdf");

            //string test = GetValuationMetrics("", "", "").ToString();

            BATModel model = new BATModel();
            InitializeDropDowns(model);

            if (!string.IsNullOrEmpty(KnownUserId()))
            {
                model.UserId = KnownUserId();

                if (PopulateModelFromDatabase(model))
                {
                    if (model.Page2Complete)
                    {
                        return View(_ValuationOptimizer, model);
                    }
                    else if (model.Page1Complete)
                    {
                        return View(_Page2QuestionsViewName, model);
                    }
                    else
                    {
                        return View(_Page1QuestionsViewName, model);
                    }
                }
                else
                    return View(_TermsViewName);
            }

            return View("Terms");

            //return View("Eloqua");
        }

        /// <summary>
        /// Action method to handle user input on Terms page
        /// </summary>
        /// <param name="mymodel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Page1Questions(AgreeToTermsModel mymodel)
        {
            if (!mymodel.AgreedToTerms)
            {
                return View(_TermsViewName);
            }

            BATModel model = new BATModel();
            InitializeDropDowns(model);

            if (string.IsNullOrEmpty(KnownUserId()))
            {
                model.UserId = Guid.NewGuid().ToString();
                HttpCookie assetmarkBATCookie = new HttpCookie(_BATCookieName);
                assetmarkBATCookie.Value = model.UserId;
                assetmarkBATCookie.Expires = DateTime.Now.AddYears(10);
                HttpContext.Response.Cookies.Add(assetmarkBATCookie);
            }
            else
            {
                model.UserId = KnownUserId();
                PopulateModelFromDatabase(model);
            }

            return View(_Page1QuestionsViewName, model);
        }

        /// <summary>
        /// Action method to handle user input on Page 1 (Firm Financials)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Page2Questions(BATModel model, string submit)
        {
            InitializeDropDowns(model);




            //if (ModelState.IsValid)
            //{
            if (submit == "Save Your Inputs")
            {
                model.BATValuationModel = new ValuationModel();
                //model.PDFPath = "On save";
                model.DateStarted = DateTime.Now.ToString("MM/dd/yy H:mm:ss");
                model.Page1Complete = true;
                PopulateEntityFromModel(model);
                return View(_Page1QuestionsViewName, model);
            }
            else
            {
                PopulateModelFromDatabase(model);

                if (string.IsNullOrEmpty(model.Vmi_Man_Written_Plan))
                {
                    model.Vmi_Man_Written_Plan = "5";
                    model.Vmi_Man_Phase = "5";
                    model.Vmi_Man_Practice = "5";
                    model.Vmi_Man_Revenue = "5";
                    model.Vmi_Man_Track = "5";

                    model.Vmi_Mar_Materials = "5";
                    model.Vmi_Mar_New_Business = "5";
                    model.Vmi_Mar_Plan = "5";
                    model.Vmi_Mar_Prospects = "5";
                    model.Vmi_Mar_Value_Proposition = "5";

                    model.Vmi_Opt_Automate = "5";
                    model.Vmi_Opt_Model = "5";
                    model.Vmi_Opt_Procedures = "5";
                    model.Vmi_Opt_Schedule = "5";
                    model.Vmi_Opt_Segment = "5";

                    model.Vmi_Emp_Compensation = "5";
                    model.Vmi_Emp_Emp_Retention = "5";
                    model.Vmi_Emp_Human = "5";
                    model.Vmi_Emp_Responsibilities = "5";
                    model.Vmi_Emp_Staff = "5";
                }


                return View(_Page2QuestionsViewName, model);
            }
            //}
            //else
            //{
            //    return View(_Page1QuestionsViewName, model);
            //}
        }

        /// <summary>
        /// Action method to handle user input on Page 2 (VMI sliders)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="submit"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Report(BATModel model, string submit)
        {
            //var errors = ModelState.Values.SelectMany(v => v.Errors);

            InitializeDropDowns(model);

            if (submit == "Save Your Inputs")
            {
                model.PDFPath = CreatePdf(model);
                model.Page2Complete = true;
                //model.PDFPath = HttpContext.Server.MapPath(@"~\UserPDF\");
                PopulateEntityFromModel(model);
                //CalculateVMIScore(model);

                return View(_Page2QuestionsViewName, model);
            }
            else if (submit == "Previous Firm Financials")
            {
                PopulateModelFromDatabase(model);
                return View(_Page1QuestionsViewName, model);
            }
            else
            {
                PopulateModelFromDatabase(model);
                return View(_ReportViewName, model);
            }
        }

        [HttpPost]
        public ActionResult Optimizer(BATModel model)
        {
            //var errors = ModelState.Values.SelectMany(v => v.Errors);


            InitializeDropDowns(model);

            return View(_ValuationOptimizer, model);
        }

        #region PrivateMethods

        private void InitializeDropDowns(BATModel batModel)
        {
            //years

            List<SelectListItem> years = new List<SelectListItem>
            {
                new SelectListItem { Text = "YTD", Value = "YTD" },
                new SelectListItem { Text = "Previous Year", Value = "Previous" }
            };

            batModel.Years = new SelectList(years, "Value", "Text");

            //months

            //TODO: return below value later
            //int fullMonths = DateTime.Now.Month - 1;

            int fullMonths = 9;

            List<SelectListItem> months = new List<SelectListItem>();

            for (int x = 1; x <= fullMonths; x++)
            {
                string monthName = new DateTime(2010, x, 1).ToString("MMM");
                months.Add(new SelectListItem { Text = monthName, Value = x.ToString() });
            }


            batModel.Months = new SelectList(months, "Value", "Text");

            //Practice Types

            List<SelectListItem> types = new List<SelectListItem>
            {
                new SelectListItem { Text = "Type 1", Value = "Type 1" },
                new SelectListItem { Text = "Type 2", Value = "Type 2" },
                new SelectListItem { Text = "Type 3", Value = "Type 3" },
                new SelectListItem { Text = "Type 4", Value = "Type 4" }
            };

            batModel.PracticeTypes = new SelectList(types, "Value", "Text", batModel.PracticeType);

            //Affiliation Modes

            List<SelectListItem> modes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Affiliation Mode", Value = "0" },
                new SelectListItem { Text = "Mode 1", Value = "Mode 1" },
                new SelectListItem { Text = "Mode 2", Value = "Mode 2" },
                new SelectListItem { Text = "Mode 3", Value = "Mode 3" },
                new SelectListItem { Text = "Mode 4", Value = "Mode 4" }
            };

            batModel.AffiliationModes = new SelectList(modes, "Value", "Text");

            // Firm Types

            List<SelectListItem> firmTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Firm Types", Value = "0" },
                new SelectListItem { Text = "Type 1", Value = "Type 1" },
                new SelectListItem { Text = "Type 2", Value = "Type 2" },
                new SelectListItem { Text = "Type 3", Value = "Type 3" },
                new SelectListItem { Text = "Type 4", Value = "Type 4" }
            };

            batModel.FirmTypes = new SelectList(firmTypes, "Value", "Text");
        }

        private double ConvertToDouble(string input)
        {
            try
            {
                return Convert.ToDouble(input.Replace("$", "").Replace(",", ""));
            }
            catch
            {
                return 0;
            }
        }

        private void PopulateEntityFromModel(BATModel model)
        {
            try
            {
                //TODO calculations
                //model.BATValuationModel.ProfitMargin = Convert.ToDouble(model.Ff_NonRecurringRevenue) + Convert.ToDouble(model.Ff_RecurringRevenue) - (Convert.ToDouble(model.Ff_DirectExpenses) + Convert.ToDouble(model.Ff_IndirecteExpenses));
                //model.BATValuationModel.ProjectedAnnualGrowthRate = Convert.ToInt32(model.Ff_ProjectedGrowthRate);

                if (model.BATValuationModel == null)
                {
                    model.BATValuationModel = new ValuationModel();
                }

                model.BATValuationModel.ProfitMargin = 225000;
                model.BATValuationModel.ProjectedAnnualGrowthRate = 0.04;
                model.Ff_TotalRevenue = (ConvertToDouble(model.Ff_NonRecurringRevenue) + ConvertToDouble(model.Ff_RecurringRevenue)).ToString();
                model.Ff_OperatingProfit = (ConvertToDouble(model.Ff_NonRecurringRevenue) + ConvertToDouble(model.Ff_RecurringRevenue) - ConvertToDouble(model.Ff_IndirecteExpenses) - ConvertToDouble(model.Ff_DirectExpenses)).ToString();

                using (AssetmarkBATEntities db = new AssetmarkBATEntities())
                {
                    am_bat user = new am_bat()
                    {
                        UserId = model.UserId,
                        //Firm Financials
                        Ff_TotalFirmAsset = (model.Ff_TotalFirmAsset != null) ? model.Ff_TotalFirmAsset.Replace("$", "") : model.Ff_TotalFirmAsset,
                        Ff_NonRecurringRevenue = (model.Ff_NonRecurringRevenue != null) ? model.Ff_NonRecurringRevenue.Replace("$", "") : model.Ff_NonRecurringRevenue,
                        Ff_RecurringRevenue = (model.Ff_RecurringRevenue != null) ? model.Ff_RecurringRevenue.Replace("$", "") : model.Ff_RecurringRevenue,
                        Ff_TotalRevenue = model.Ff_TotalRevenue,
                        Ff_DirectExpenses = (model.Ff_DirectExpenses != null) ? model.Ff_DirectExpenses.Replace("$", "") : model.Ff_DirectExpenses,
                        Ff_IndirectExpenses = (model.Ff_IndirecteExpenses != null) ? model.Ff_IndirecteExpenses.Replace("$", "") : model.Ff_IndirecteExpenses,
                        Ff_OperatingProfit = model.Ff_OperatingProfit,
                        Ff_Client_Relationships = model.Ff_ClientRelationships,
                        Ff_Fte_Advisors = model.Ff_FullTimeAdvisors,
                        Ff_Fte_Non_Advisors = model.Ff_FullTimeNonAdvisors,
                        Ff_New_Clients = model.Ff_NewClients,
                        Ff_Projected_Growth = model.Ff_ProjectedGrowthRate,
                        //VMI
                        Vmi_Man_Phase = model.Vmi_Man_Phase,
                        Vmi_Man_Practice = model.Vmi_Man_Practice,
                        Vmi_Man_Revenue = model.Vmi_Man_Revenue,
                        Vmi_Man_Track = model.Vmi_Man_Track,
                        Vmi_Man_Written_Plan = model.Vmi_Man_Written_Plan,
                        Vmi_Mar_Materials = model.Vmi_Mar_Materials,
                        Vmi_Mar_New_Business = model.Vmi_Mar_New_Business,
                        Vmi_Mar_Plan = model.Vmi_Mar_Plan,
                        Vmi_Mar_Prospects = model.Vmi_Mar_Prospects,
                        Vmi_Mar_Value_Proposition = model.Vmi_Mar_Value_Proposition,
                        Vmi_Emp_Compensation = model.Vmi_Emp_Compensation,
                        Vmi_Emp_Emp_Retention = model.Vmi_Emp_Emp_Retention,
                        Vmi_Emp_Human = model.Vmi_Emp_Human,
                        Vmi_Emp_Responsibilities = model.Vmi_Emp_Responsibilities,
                        Vmi_Emp_Staff = model.Vmi_Emp_Staff,
                        Vmi_Opt_Automate = model.Vmi_Opt_Automate,
                        Vmi_Opt_Model = model.Vmi_Opt_Model,
                        Vmi_Opt_Procedures = model.Vmi_Opt_Procedures,
                        Vmi_Opt_Schedule = model.Vmi_Opt_Schedule,
                        Vmi_Opt_Segment = model.Vmi_Opt_Segment,
                        VmiIndex = "1000",
                        PDF = model.PDFPath,
                        DateStarted = model.DateStarted
                    };

                    var original = db.am_bat.Find(user.UserId);

                    if (original != null)
                    {
                        db.Entry(original).CurrentValues.SetValues(user);
                    }
                    else
                    {
                        db.am_bat.Add(user);
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving to Azure database " + model.UserId, e.Message);
            }
        }

        private bool PopulateModelFromDatabase(BATModel model)
        {
            using (AssetmarkBATEntities db = new AssetmarkBATEntities())
            {
                var original = db.am_bat.Find(model.UserId);

                if (original != null)
                {
                    //Firm Financials
                    model.Ff_TotalFirmAsset = original.Ff_TotalFirmAsset;
                    model.Ff_NonRecurringRevenue = original.Ff_NonRecurringRevenue;
                    model.Ff_RecurringRevenue = original.Ff_RecurringRevenue;
                    model.Ff_DirectExpenses = original.Ff_DirectExpenses;
                    model.Ff_IndirecteExpenses = original.Ff_IndirectExpenses;
                    model.Ff_ProjectedGrowthRate = original.Ff_Projected_Growth;
                    model.Ff_ClientRelationships = original.Ff_Client_Relationships;
                    model.Ff_FullTimeNonAdvisors = original.Ff_Fte_Non_Advisors;
                    model.Ff_FullTimeAdvisors = original.Ff_Fte_Advisors;
                    model.Ff_NewClients = original.Ff_New_Clients;

                    //VMI's
                    model.Vmi_Man_Phase = original.Vmi_Man_Phase;
                    model.Vmi_Man_Practice = original.Vmi_Man_Practice;
                    model.Vmi_Man_Revenue = original.Vmi_Man_Revenue;
                    model.Vmi_Man_Track = original.Vmi_Man_Track;
                    model.Vmi_Man_Written_Plan = original.Vmi_Man_Written_Plan;

                    model.Vmi_Mar_Materials = original.Vmi_Mar_Materials;
                    model.Vmi_Mar_New_Business = original.Vmi_Mar_New_Business;
                    model.Vmi_Mar_Plan = original.Vmi_Mar_Plan;
                    model.Vmi_Mar_Prospects = original.Vmi_Mar_Prospects;
                    model.Vmi_Mar_Value_Proposition = original.Vmi_Mar_Value_Proposition;

                    model.Vmi_Opt_Automate = original.Vmi_Opt_Automate;
                    model.Vmi_Opt_Model = original.Vmi_Opt_Model;
                    model.Vmi_Opt_Procedures = original.Vmi_Opt_Procedures;
                    model.Vmi_Opt_Schedule = original.Vmi_Opt_Schedule;
                    model.Vmi_Opt_Segment = original.Vmi_Opt_Segment;

                    model.Vmi_Emp_Compensation = original.Vmi_Emp_Compensation;
                    model.Vmi_Emp_Emp_Retention = original.Vmi_Emp_Emp_Retention;
                    model.Vmi_Emp_Human = original.Vmi_Emp_Human;
                    model.Vmi_Emp_Responsibilities = original.Vmi_Emp_Responsibilities;
                    model.Vmi_Emp_Staff = original.Vmi_Emp_Staff;

                    model.PDFPath = original.PDF;
                    model.DateStarted = original.DateStarted;

                    return true;
                }

                return false;
            }
        }

        private void DownloadUserPdf(string userId)
        {
            //string path = HttpContext.Server.MapPath(@"~\UserPDF\userPdf.pdf");           
            //WebClient webClient = new WebClient();

            //String dPath = String.Empty;
            //RegistryKey rKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main");
            //if (rKey != null)
            //    dPath = (String)rKey.GetValue("Default Download Directory");
            //if (String.IsNullOrEmpty(dPath))
            //    dPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\downloads";

            //string filePath = dPath + @"\userFileAgain.pdf";  

            //webClient.DownloadFile(new Uri(""), filePath);

            // Converts the PdfDocument object to byte form.
            //byte[] docBytes = stream.ToArray();
            //Loads the byte array in PdfLoadedDocument

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]); //connection string is copied from Azure storage account's Settings
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer myContainer = client.GetContainerReference("assetmarkbat");
            var permissions = myContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            myContainer.SetPermissions(permissions);

            CloudBlockBlob blockBlob = myContainer.GetBlockBlobReference(userId + ".pdf");
            blockBlob.Properties.ContentType = "application/pdf";
            //blockBlob.UploadFromStream(stream);
            //blockBlob.UploadFromByteArray(docBytes, 0, docBytes.Count());
            // Save blob contents to a file.
            using (var fileStream = System.IO.File.OpenWrite(@"C:\" + userId + ".pdf"))
            {
                blockBlob.DownloadToStream(fileStream);
            }

        }


        public ActionResult GetValuationMetrics(double PAGR, double PM, double VMI)
        {
            if(PM > 20)
                return Json(new { maxvalue = 60000000, currentmax = 46678564, currentmin = 33567234, calculatedmax = 48000000, calculatedmin = 22000000, top_pagr_max = 11, top_pagr_min = 8, top_pm_max = 23, top_pm_min = 20, top_vmi_max = 90, top_vmi_min = 70 }, JsonRequestBehavior.AllowGet);

            //if params are blank return current with benchmark
            return Json(new { maxvalue = 60000000, currentmax = 46678564, currentmin = 33567234, calculatedmax = 13000000, calculatedmin = 7000000, top_pagr_max = 11, top_pagr_min = 8, top_pm_max = 23, top_pm_min = 20, top_vmi_max = 90, top_vmi_min = 70 }, JsonRequestBehavior.AllowGet);
        }


        private string CreatePdf(BATModel model)
        {
            //try
            //{
            PdfFixedDocument document = new PdfFixedDocument();
            PdfPage page = document.Pages.Add();
            //document.Save("empty.pdf");

            // Create a standard font with Helvetica face and 24 point size
            PdfStandardFont helvetica = new PdfStandardFont(PdfStandardFontFace.Helvetica, 14);
            // Create a solid RGB red brush.
            PdfBrush backgroundBrush = new PdfBrush(PdfRgbColor.Aqua);
            PdfBrush darkBlueBrush = new PdfBrush();
            darkBlueBrush.Color = new PdfRgbColor(123, 123, 123);
            PdfBrush textBrush = new PdfBrush((PdfRgbColor.Black));

            //page.Graphics.DrawLine(new PdfPen(), new PdfPoint(50, 70), new PdfPoint(50, 700));
            //page.Graphics.DrawLine(new PdfPen(), new PdfPoint(50, 70), new PdfPoint(500, 700));
            //page.Graphics.DrawRectangle(backgroundBrush, 20, 20, 500, 150);
            //page.Graphics.DrawRectangle(darkBlueBrush, 50, 60, 50, 25);
            page.Graphics.DrawString(model.Ff_TotalFirmAsset, helvetica, textBrush, 50, 35);

            string imagePath = HttpContext.Server.MapPath(@"~\Styles\Images\" + "Lock.png");
            PdfImage lockImage = new PdfImage(imagePath);
            page.Graphics.DrawImage(lockImage, 50, 100, 25, 25);

            MemoryStream stream = new MemoryStream();
            // Saves the document as stream
            document.Save(stream);



            // Converts the PdfDocument object to byte form.
            byte[] docBytes = stream.ToArray();
            //Loads the byte array in PdfLoadedDocument

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]); //connection string is copied from Azure storage account's Settings
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer myContainer = client.GetContainerReference("assetmarkbat");
            var permissions = myContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            myContainer.SetPermissions(permissions);

            CloudBlockBlob blockBlob = myContainer.GetBlockBlobReference(model.UserId + ".pdf");
            blockBlob.Properties.ContentType = "application/pdf";
            //blockBlob.UploadFromStream(stream);
            blockBlob.UploadFromByteArray(docBytes, 0, docBytes.Count());

            return blockBlob.StorageUri.PrimaryUri.ToString();
        }

        private void CalculateVMIScore(BATModel model)
        {
            double test = (Convert.ToInt32(model.Vmi_Man_Written_Plan) + Convert.ToInt32(model.Vmi_Man_Track) + Convert.ToInt32(model.Vmi_Man_Phase) + Convert.ToInt32(model.Vmi_Man_Revenue) + Convert.ToInt32(model.Vmi_Man_Practice) +
                   Convert.ToInt32(model.Vmi_Mar_Value_Proposition) + Convert.ToInt32(model.Vmi_Mar_Materials) + Convert.ToInt32(model.Vmi_Mar_Plan) + Convert.ToInt32(model.Vmi_Mar_Prospects) + Convert.ToInt32(model.Vmi_Mar_New_Business) +
                   Convert.ToInt32(model.Vmi_Opt_Automate) + Convert.ToInt32(model.Vmi_Opt_Procedures) + Convert.ToInt32(model.Vmi_Opt_Segment) + Convert.ToInt32(model.Vmi_Opt_Model) + Convert.ToInt32(model.Vmi_Opt_Schedule) +
                   Convert.ToInt32(model.Vmi_Emp_Human) + Convert.ToInt32(model.Vmi_Emp_Compensation) + Convert.ToInt32(model.Vmi_Emp_Responsibilities) + Convert.ToInt32(model.Vmi_Emp_Staff) + Convert.ToInt32(model.Vmi_Emp_Emp_Retention));


            double temp = test / 2000;

            model.BATValuationModel.VmiRiskRate = 0.15 - temp;
            model.BATValuationModel.ValuationIndex = temp * 5;
            model.BATValuationModel.UserPerpetualGrowthRate = (model.BATValuationModel.ValuationIndex >= 700) ? model.BATValuationModel._PerpetualGrowthRateMax : model.BATValuationModel._PerpetualGrowthRateMax - 0.01;
        }

        private void CalculateNonAdvisorTaxFreeCashFlow(BATModel model)
        {
            model.BATValuationModel.ProfitMargin = Convert.ToDouble((Convert.ToDouble(model.Ff_NonRecurringRevenue) + Convert.ToDouble(model.Ff_RecurringRevenue)) - (Convert.ToDouble(model.Ff_IndirecteExpenses) + Convert.ToDouble(model.Ff_DirectExpenses)));
            model.BATValuationModel.ProjectedAnnualGrowthRate = Convert.ToDouble(model.Ff_ProjectedGrowthRate);

            //year 1
            if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
            {
                //TODO: blank
                model.BATValuationModel.NonAdvisorCashFlowYear1 = 0;
            }
            else
            {
                model.BATValuationModel.NonAdvisorCashFlowYear1 = model.BATValuationModel.ProfitMargin * (1 + model.BATValuationModel.ProfitMargin) * (1 - 0.25);
            }

            //year2
            if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
            {
                //TODO: blank
                model.BATValuationModel.NonAdvisorCashFlowYear2 = 0;
            }
            else
            {
                model.BATValuationModel.NonAdvisorCashFlowYear2 = model.BATValuationModel.NonAdvisorCashFlowYear1 * (1 + model.BATValuationModel.ProjectedAnnualGrowthRate);
            }

            //year3
            if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
            {
                //TODO: blank
                model.BATValuationModel.NonAdvisorCashFlowYear3 = 0;
            }
            else
            {
                model.BATValuationModel.NonAdvisorCashFlowYear3 = model.BATValuationModel.NonAdvisorCashFlowYear2 * (1 + model.BATValuationModel.ProjectedAnnualGrowthRate);
            }

            //year4
            if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
            {
                //TODO: blank
                model.BATValuationModel.NonAdvisorCashFlowYear4 = 0;
            }
            else
            {
                model.BATValuationModel.NonAdvisorCashFlowYear4 = model.BATValuationModel.NonAdvisorCashFlowYear3 * (1 + model.BATValuationModel.ProjectedAnnualGrowthRate);
            }

            //year5
            if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
            {
                //TODO: blank
                model.BATValuationModel.NonAdvisorCashFlowYear5 = 0;
            }
            else
            {
                model.BATValuationModel.NonAdvisorCashFlowYear5 = model.BATValuationModel.NonAdvisorCashFlowYear4 * (1 + model.BATValuationModel.ProjectedAnnualGrowthRate);
            }
        }

        private void CalculateDiscountedCashFlow(BATModel model)
        {
            //Max
            model.BATValuationModel.DiscountedCashFlowYear1Max = model.BATValuationModel.NonAdvisorCashFlowYear1 / Math.Pow((1 + model.BATValuationModel._MinVMIRiskRate + model.BATValuationModel._ValuationRiskRate), 1);
            model.BATValuationModel.DiscountedCashFlowYear2Max = model.BATValuationModel.NonAdvisorCashFlowYear2 / Math.Pow((1 + model.BATValuationModel._MinVMIRiskRate + model.BATValuationModel._ValuationRiskRate), 2);
            model.BATValuationModel.DiscountedCashFlowYear3Max = model.BATValuationModel.NonAdvisorCashFlowYear3 / Math.Pow((1 + model.BATValuationModel._MinVMIRiskRate + model.BATValuationModel._ValuationRiskRate), 3);
            model.BATValuationModel.DiscountedCashFlowYear4Max = model.BATValuationModel.NonAdvisorCashFlowYear4 / Math.Pow((1 + model.BATValuationModel._MinVMIRiskRate + model.BATValuationModel._ValuationRiskRate), 4);
            model.BATValuationModel.DiscountedCashFlowYear5Max = model.BATValuationModel.NonAdvisorCashFlowYear5 / Math.Pow((1 + model.BATValuationModel._MinVMIRiskRate + model.BATValuationModel._ValuationRiskRate), 5);

            //Min
            model.BATValuationModel.DiscountedCashFlowYear1Min = model.BATValuationModel.NonAdvisorCashFlowYear1 / Math.Pow((1 + model.BATValuationModel.VmiRiskRate + model.BATValuationModel._ValuationRiskRate), 1);
            model.BATValuationModel.DiscountedCashFlowYear2Min = model.BATValuationModel.NonAdvisorCashFlowYear2 / Math.Pow((1 + model.BATValuationModel.VmiRiskRate + model.BATValuationModel._ValuationRiskRate), 2);
            model.BATValuationModel.DiscountedCashFlowYear3Min = model.BATValuationModel.NonAdvisorCashFlowYear3 / Math.Pow((1 + model.BATValuationModel.VmiRiskRate + model.BATValuationModel._ValuationRiskRate), 3);
            model.BATValuationModel.DiscountedCashFlowYear4Min = model.BATValuationModel.NonAdvisorCashFlowYear4 / Math.Pow((1 + model.BATValuationModel.VmiRiskRate + model.BATValuationModel._ValuationRiskRate), 4);
            model.BATValuationModel.DiscountedCashFlowYear5Min = model.BATValuationModel.NonAdvisorCashFlowYear5 / Math.Pow((1 + model.BATValuationModel.VmiRiskRate + model.BATValuationModel._ValuationRiskRate), 5);
        }

        private void CalculateValuationRanges(BATModel model)
        {
            //Discounted Cash Flow Range
            model.BATValuationModel.DiscountedCashFlowMin = model.BATValuationModel.DiscountedCashFlowYear1Min + model.BATValuationModel.DiscountedCashFlowYear2Min + model.BATValuationModel.DiscountedCashFlowYear3Min + model.BATValuationModel.DiscountedCashFlowYear4Min + model.BATValuationModel.DiscountedCashFlowYear5Min;
            model.BATValuationModel.DiscountedCashFlowMax = model.BATValuationModel.DiscountedCashFlowYear1Max + model.BATValuationModel.DiscountedCashFlowYear2Max + model.BATValuationModel.DiscountedCashFlowYear3Max + model.BATValuationModel.DiscountedCashFlowYear4Max + model.BATValuationModel.DiscountedCashFlowYear5Max;

            //Perpetual Growth Rate Cash FLow
            model.BATValuationModel.PerpetualGrowthRateCashFlowMin = model.BATValuationModel.NonAdvisorCashFlowYear1 * (1 + model.BATValuationModel._PerpetualGrowthRateMin) /
                ((model.BATValuationModel.VmiRiskRate + model.BATValuationModel._ValuationRiskRate) - model.BATValuationModel._PerpetualGrowthRateMin) /
                Math.Pow(1 + model.BATValuationModel.VmiRiskRate + model.BATValuationModel._ValuationRiskRate, 5);

            model.BATValuationModel.PerpetualGrowthRateCashFlowMax = model.BATValuationModel.NonAdvisorCashFlowYear1 * (1 + model.BATValuationModel.UserPerpetualGrowthRate) /
                ((model.BATValuationModel._MinVMIRiskRate + model.BATValuationModel._ValuationRiskRate) - model.BATValuationModel.UserPerpetualGrowthRate) /
                Math.Pow(1 + model.BATValuationModel._MinVMIRiskRate + model.BATValuationModel._ValuationRiskRate, 5);

        }

        private void CalculateKPIs(BATModel model)
        {
            //TODO: blanks for D12 and D18
            model.BATValuationModel.RecurringRevenuePerClient = (Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) == 0 || Convert.ToDouble(model.Ff_ClientRelationships) == 0) ?
                0 : Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) / Convert.ToDouble(model.Ff_ClientRelationships);

            //TODO: blanks for D12  and D20
            model.BATValuationModel.RecurringRevenuePerClient = (Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) == 0 || Convert.ToDouble(model.Ff_FullTimeAdvisorsAnnualized) == 0) ?
                0 : Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) / Convert.ToDouble(model.Ff_FullTimeAdvisorsAnnualized);

            //TODO: blanks for D13 and D18 
            model.BATValuationModel.RecurringRevenuePerClient = (Convert.ToDouble(model.Ff_TotalRevenueAnnualized) == 0 || Convert.ToDouble(model.Ff_ClientRelationships) == 0) ?
                0 : Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) / Convert.ToDouble(model.Ff_ClientRelationships);








            //TODO: blanks for D12 
            model.BATValuationModel.RecurringRevenuePerClient = (Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) == 0) ?
                0 : Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) / Convert.ToDouble(model.Ff_ClientRelationships);

            //TODO: blanks for D12 
            model.BATValuationModel.RecurringRevenuePerClient = (Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) == 0) ?
                0 : Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) / Convert.ToDouble(model.Ff_ClientRelationships);
        }

        #endregion
    }
}