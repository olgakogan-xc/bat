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

        public ActionResult Pdf()
        {
            return View("PdfShell");
        }


        /// <summary>
        /// Action Method to initiate the BAT tool
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //return View("PdfShell");

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
                model.BATValuationModel = new ClientValuationModel();
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
                new SelectListItem { Text = "Time Range", Value = "0" },
                new SelectListItem { Text = "YTD 2017", Value = "YTD 2017" },
                new SelectListItem { Text = "Previous Year", Value = "Previous" }
            };

            batModel.Years = new SelectList(years, "Value", "Text");          

            //months
            int fullMonths = DateTime.Now.Month - 1;
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
                new SelectListItem { Text = "Practice Type", Value = "Practice Type" },
                new SelectListItem { Text = "Single advisor practice: Solo practitioner or providing data on your book of business only1", Value = "Single advisor practice" },
                new SelectListItem { Text = "Multiple advisor practice:  Multiple advisor practice providing firm level data", Value = "Multiple advisor practice" },
                new SelectListItem { Text = "Third party: Broker-dealer, wholesaler, consultant ", Value = "Third party" },              
                new SelectListItem { Text = "Other", Value = "Other" }
            };

            batModel.PracticeTypes = new SelectList(types, "Value", "Text", batModel.PracticeType);

            //Affiliation Modes
            List<SelectListItem> modes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Affiliation Mode", Value = "N/A" },
                new SelectListItem { Text = "BD (Broker-Dealer): Affiliated with a full-service BD/wirehouse, independent BD, insurance BD or own BD", Value = "BD (Broker-Dealer)" },
                new SelectListItem { Text = "RIA only (Registered Investment Advisor): Registered as an investment advisor with the SEC or state", Value = "RIA only (Registered Investment Advisor)" },
                new SelectListItem { Text = "Hybrid BD/RAI: Have your own RIA, as well as a BD affiliation", Value = "Hybrid BD/RAI" },             
                new SelectListItem { Text = "Other", Value = "Other" }
            };

            batModel.AffiliationModes = new SelectList(modes, "Value", "Text");

            // Firm Types
            List<SelectListItem> firmTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Firm Types", Value = "N/A" },
                new SelectListItem { Text = "Financial Planning Firm: Focused on providing financial planning services / process", Value = "Financial Planning Firm" },
                new SelectListItem { Text = "Investment Advisory Firm: Focused on providing investment strategy and manager selection2", Value = "Investment Advisory Firm" },
                new SelectListItem { Text = "Investment Management Firm: Focused on investment recommendations and discretionary investment management of client assets", Value = "Investment Management Firm" },
                new SelectListItem { Text = "Wealth Management Firm: Provide holistic advice to clients, including integrated tax, estate and financial planning in addition to investment services", Value = "Wealth Management Firm" },
                new SelectListItem { Text = "Other", Value = "Other" }
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
                    model.BATValuationModel = new ClientValuationModel();
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
                        PracticeType = (!string.IsNullOrEmpty(model.PracticeTypeOther))? model.PracticeTypeOther : model.PracticeType,
                        AffiliationModel = (!string.IsNullOrEmpty(model.AffiliationModeOther))? model.AffiliationModeOther : model.AffiliationMode,
                        FirmType = (!string.IsNullOrEmpty(model.FirmTypeOther))? model.FirmTypeOther : model.FirmType,
                        TimeRange = (model.Year.Contains("Previous"))? model.Year : "YTD " + model.Month,                        

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
                    model.Ff_TotalRevenue = original.Ff_TotalRevenue;

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

        private BATModel GetClientValuationRanges()
        {
            BATModel model = new BATModel();
            model.BATValuationModel = new ClientValuationModel();
            model.BenchmarkModel = new BenchmarksValuationModel();

            if (!string.IsNullOrEmpty(KnownUserId()))
            {
                model.UserId = KnownUserId();

                if (PopulateModelFromDatabase(model))
                {
                    model.BATValuationModel.ValuationMin = 3092000;
                    model.BATValuationModel.ValuationMax = 3618000;
                }
            }

            return model;
        }


        public ActionResult GetValuationMetrics(double PAGR, double PM, double VMI)
        {
            BATModel model = GetClientValuationRanges();
            BenchmarkGroup peerGroup = model.BenchmarkModel.PeerGroups.FirstOrDefault(x => ConvertToDouble(model.Ff_TotalRevenue) > x.GroupRangeMin && ConvertToDouble(model.Ff_TotalRevenue) < x.GroupRangeMax);

            double comparativeValuationMin = peerGroup.ValuationMin;
            double comparativeValuationMax = peerGroup.ValuationMax;

            //Determine max axis value
            double maxValueForClient = model.BATValuationModel.ValuationMax + (model.BATValuationModel.ValuationMax / 4);
            double maxValueForComparative = comparativeValuationMax + (comparativeValuationMax / 4);

            return Json(new { operatingprofit=model.Ff_OperatingProfit, totalrevenue=model.Ff_TotalRevenue, maxvalue = (maxValueForClient > maxValueForComparative) ? maxValueForClient : maxValueForComparative, currentmax = model.BATValuationModel.ValuationMax, currentmin = model.BATValuationModel.ValuationMin, calculatedmax = comparativeValuationMax, calculatedmin = comparativeValuationMin, top_pagr_max = 11, top_pagr_min = 8, top_pm_max = 23, top_pm_min = 20, top_vmi_max = 90, top_vmi_min = 70 }, JsonRequestBehavior.AllowGet);

            //if params are blank return current with benchmark
            //return Json(new { maxvalue = 60000000, currentmax = 46678564, currentmin = 33567234, calculatedmax = 13000000, calculatedmin = 7000000, top_pagr_max = 11, top_pagr_min = 8, top_pm_max = 23, top_pm_min = 20, top_vmi_max = 90, top_vmi_min = 70 }, JsonRequestBehavior.AllowGet);
        }

        public static PdfFixedDocument Load(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
                return new PdfFixedDocument(stream);
        }


        private string CreatePdf(BATModel model)
        {
            //try
            //{
            //PdfFixedDocument document = new PdfFixedDocument();
            //PdfPage page = document.Pages.Add();
            //document.Save("empty.pdf");

            

            //PdfFixedDocument document = Load("C:\\Olga\\PdfTemplate.pdf");
            PdfFixedDocument document = Load(HttpContext.Server.MapPath(@"~\UserPDF\PdfTemplate.pdf"));
            PdfPage page = document.Pages[0];

            // Create a standard font with Helvetica face and 24 point size
            PdfStandardFont helvetica = new PdfStandardFont(PdfStandardFontFace.Helvetica, 12);
            // Create a solid RGB red brush.
            PdfBrush backgroundBrush = new PdfBrush(PdfRgbColor.Aqua);
            PdfBrush darkBlueBrush = new PdfBrush();
            darkBlueBrush.Color = new PdfRgbColor(123, 123, 123);
            PdfBrush textBrush = new PdfBrush((PdfRgbColor.Black));

            PdfBrush redBrush = new PdfBrush(PdfRgbColor.Red);

            //page.Graphics.DrawLine(new PdfPen(), new PdfPoint(50, 70), new PdfPoint(50, 700));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(100, 0), new PdfPoint(100, 800));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(200, 0), new PdfPoint(200, 800));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(300, 0), new PdfPoint(300, 800));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(400, 0), new PdfPoint(400, 800));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(500, 0), new PdfPoint(500, 800));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(600, 0), new PdfPoint(600, 800));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(700, 0), new PdfPoint(700, 800));

            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 100), new PdfPoint(800, 100));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 200), new PdfPoint(800, 200));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 300), new PdfPoint(800, 300));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 400), new PdfPoint(800, 400));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 500), new PdfPoint(800, 500));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 600), new PdfPoint(800, 600));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 700), new PdfPoint(800, 700));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 800), new PdfPoint(800, 800));
            page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 900), new PdfPoint(800, 900));


            //page.Graphics.DrawLine(new PdfPen(), new PdfPoint(50, 70), new PdfPoint(500, 700));
            //page.Graphics.DrawRectangle(backgroundBrush, 20, 20, 500, 150);
            //page.Graphics.DrawRectangle(darkBlueBrush, 50, 60, 50, 25);

            PdfBrush textBlueBrush = new PdfBrush((new PdfRgbColor(5, 79, 124)));
            PdfStandardFont largeTitleFont = new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 36);
            //page.Graphics.DrawString("Business Assessment Report", largeTitleFont, textBlueBrush, 50, 35);

            //Blue table header
            //PdfBrush aquaBrush = new PdfBrush((new PdfRgbColor(240, 248, 255)));
            //page.Graphics.DrawLine(redBrush, 0, 0, 100, 90);
            //page.Graphics.DrawRectangle(redBrush, 0, 100, 100, 90);
            //page.Graphics.DrawRectangle(redBrush, 0, 200, 100, 90);
            //page.Graphics.DrawRectangle(redBrush, 0, 300, 100, 90);
            //page.Graphics.DrawRectangle(redBrush, 0, 400, 100, 90);

            //PdfBrush tableRowTextBlueBrush = new PdfBrush((new PdfRgbColor(225, 225, 225)));
            //PdfStandardFont tableRowTextFont = new PdfStandardFont(PdfStandardFontFace.Helvetica, 16);
            //page.Graphics.DrawString("Firm Financials", tableRowTextFont, tableRowTextBlueBrush, 50, 55);
            //page.Graphics.DrawString("Your Firm", tableRowTextFont, tableRowTextBlueBrush, 100, 55);
            //page.Graphics.DrawString("Benchmarks", tableRowTextFont, tableRowTextBlueBrush, 150, 55);

            if (PopulateModelFromDatabase(model))
            {
                model.BenchmarkModel = new BenchmarksValuationModel();
                BenchmarkGroup peerGroup = model.BenchmarkModel.PeerGroups.FirstOrDefault(x => ConvertToDouble(model.Ff_TotalRevenue) > x.GroupRangeMin && ConvertToDouble(model.Ff_TotalRevenue) < x.GroupRangeMax);

                //first row
                //page.Graphics.DrawString("Total Firm Aum", tableRowTextFont, tableRowTextBlueBrush, 50, 95);
                //page.Graphics.DrawString("------", tableRowTextFont, tableRowTextBlueBrush, 100, 95);
                //page.Graphics.DrawString(peerGroup.TotalAUMPerClient.ToString(), tableRowTextFont, tableRowTextBlueBrush, 150, 95);

                int groupNumber = model.BenchmarkModel.PeerGroups.IndexOf(peerGroup);

                if (groupNumber == 4)
                {
                    page.Graphics.DrawString("$1M - $3M", helvetica, redBrush, 700, 140);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 184);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 210);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 236);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 262);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 288);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 314);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 340);

                }
                else
                {
                    page.Graphics.DrawString(peerGroup.GroupRangeMin + " - " + peerGroup.GroupRangeMax, helvetica, redBrush, 700, 140);
                }

                //Client Metrics
                page.Graphics.DrawString("?????", helvetica, redBrush, 240, 184);
                page.Graphics.DrawString("?????", helvetica, redBrush, 240, 210);
                page.Graphics.DrawString(model.Ff_RecurringRevenue, helvetica, redBrush, 240, 236);
                page.Graphics.DrawString(model.Ff_TotalRevenue, helvetica, redBrush, 240, 262);
                page.Graphics.DrawString("?????", helvetica, redBrush, 240, 314);
                page.Graphics.DrawString("sdf", helvetica, redBrush, 240, 288);               
                page.Graphics.DrawString("?????", helvetica, redBrush, 240, 340);


                //Client KPI's
                page.Graphics.DrawString("?????", helvetica, redBrush, 240, 832);
                page.Graphics.DrawString("?????", helvetica, redBrush, 240, 858);
                page.Graphics.DrawString("?????", helvetica, redBrush, 240, 884);
                page.Graphics.DrawString("sdfdsf", helvetica, redBrush, 240, 910);
                page.Graphics.DrawString("?????", helvetica, redBrush, 240, 936);
                page.Graphics.DrawString("3345", helvetica, redBrush, 240, 962);
                page.Graphics.DrawString("?????", helvetica, redBrush, 240, 988);
                page.Graphics.DrawString("3345", helvetica, redBrush, 240, 1014);
                page.Graphics.DrawString("?????", helvetica, redBrush, 240, 1040);

                //Benchmark KPI's
                page.Graphics.DrawString("?????", helvetica, redBrush, 400, 832);
                page.Graphics.DrawString("?????", helvetica, redBrush, 400, 858);
                page.Graphics.DrawString("?????", helvetica, redBrush, 400, 884);
                page.Graphics.DrawString("sdfdsf", helvetica, redBrush, 400, 910);
                page.Graphics.DrawString("?????", helvetica, redBrush, 400, 936);
                page.Graphics.DrawString("3345", helvetica, redBrush, 400, 962);
                page.Graphics.DrawString("?????", helvetica, redBrush, 400, 988);
                page.Graphics.DrawString("3345", helvetica, redBrush, 400, 1014);
                page.Graphics.DrawString("?????", helvetica, redBrush, 400, 1040);

                //string imagePath = HttpContext.Server.MapPath(@"~\Styles\Images\" + "Lock.png");
                //PdfImage lockImage = new PdfImage(imagePath);
                //page.Graphics.DrawImage(lockImage, 50, 100, 25, 25);

                //VMI Graph ------------------------------------------------
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 1), new PdfPoint(50, 420), new PdfPoint(50, 620));
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 2), new PdfPoint(50, 620), new PdfPoint(350, 620));
                page.Graphics.DrawString("1000", helvetica, redBrush, 30, 420);
                page.Graphics.DrawString("0", helvetica, redBrush, 30, 600);

                //Graph brushes
                PdfBrush graphBrush1 = new PdfBrush((new PdfRgbColor(0, 74, 129))); //#004b81
                PdfBrush graphBrush2 = new PdfBrush((new PdfRgbColor(0, 126, 187))); // #007ebb
                PdfBrush graphBrush3 = new PdfBrush((new PdfRgbColor(109, 198, 233))); //#6dc6e7;
                PdfBrush graphBrush4 = new PdfBrush((new PdfRgbColor(176, 216, 235))); //#b0d8eb;





                //Benchmarks
                page.Graphics.DrawRectangle(graphBrush1, 220, 620, 60, peerGroup.EYT * 200 / 1000);
                page.Graphics.DrawRectangle(graphBrush2, 220, 620 - peerGroup.OYO * 200 / 1000, 60, peerGroup.OYO * 200 / 1000);
                page.Graphics.DrawRectangle(graphBrush3, 220, 620 - peerGroup.OYO * 200 / 1000 - peerGroup.MYB * 200 / 1000, 60, peerGroup.MYB * 200 / 1000);
                page.Graphics.DrawRectangle(graphBrush4, 220, 580, peerGroup.OYO * 200 / 1000 - peerGroup.MYB * 200 / 1000 - peerGroup.MYP * 200 / 1000, peerGroup.MYP * 200 / 1000);


                //Valuation Graph --------------------------------------------
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 1), new PdfPoint(450, 420), new PdfPoint(450, 620));
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 2), new PdfPoint(450, 620), new PdfPoint(750, 620));
              
                page.Graphics.DrawString("0", helvetica, redBrush, 497, 620);

            }






            MemoryStream stream = new MemoryStream();
            // Saves the document as stream
            document.Save(stream);

            document.Save("C:\\Olga\\PdfCustom.pdf");



            // Converts the PdfDocument object to byte form.
            byte[] docBytes = stream.ToArray();
            //Loads the byte array in PdfLoadedDocument

            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]); //connection string is copied from Azure storage account's Settings
            //CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            //CloudBlobContainer myContainer = client.GetContainerReference("assetmarkbat");
            //var permissions = myContainer.GetPermissions();
            //permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            //myContainer.SetPermissions(permissions);

            //CloudBlockBlob blockBlob = myContainer.GetBlockBlobReference(model.UserId + ".pdf");
            //blockBlob.Properties.ContentType = "application/pdf";
            ////blockBlob.UploadFromStream(stream);
            //blockBlob.UploadFromByteArray(docBytes, 0, docBytes.Count());

            //return blockBlob.StorageUri.PrimaryUri.ToString();
            return "sdfsdf";
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