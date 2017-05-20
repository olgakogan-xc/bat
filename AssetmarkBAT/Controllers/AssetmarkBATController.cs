using AssetmarkBAT.Models;
using AssetmarkBATDbConnector;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;

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

        #region ActionMethods

        /// <summary>
        /// Action Method to initiate the BAT tool
        /// </summary>     
        public ActionResult Index()
        {
            BATModel model = new BATModel();
            InitializeDropDowns(model);

            if (!string.IsNullOrEmpty(KnownUserId()))
            {
                model.UserId = KnownUserId();

                if (PopulateModelFromDatabase(model))
                {
                    //TODO: uncomment for QA

                    //if (model.Page2Complete)
                    //{
                    //    return View(_ValuationOptimizer, model);
                    //}
                    //else if (model.Page1Complete)
                    //{
                    //    if (model.Page2Complete == false)
                    //    {
                    //        PrepopulateVMIs(model);
                    //    }
                    //    return View(_Page2QuestionsViewName, model);
                    //}
                    //else
                    //{
                    //    return View(_Page1QuestionsViewName, model);
                    //}
                    return View(_Page1QuestionsViewName, model);

                }
                else
                    return View(_TermsViewName);
            }

            return View("Terms");
        }

        /// <summary>
        /// Action method to handle user input on Terms page
        /// </summary>  
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
        [HttpPost]
        public ActionResult Page2Questions(BATModel model, string submit)
        {
            InitializeDropDowns(model);




            //if (ModelState.IsValid)
            //{
            if (submit == "Save Your Inputs")
            {
                model.ClientValuationModel = new ClientValuationModel();
                //model.PDFPath = "On save";
                model.DateStarted = DateTime.Now.ToString("MM/dd/yy H:mm:ss");
                model.Page1Complete = true;
                PopulateEntityFromModel(model);
                return View(_Page1QuestionsViewName, model);
            }
            else
            {
                PopulateModelFromDatabase(model);

                if (model.Page2Complete == false)
                {
                    PrepopulateVMIs(model);
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
        [HttpPost]
        public ActionResult Report(BATModel model, string submit)
        {
            //var errors = ModelState.Values.SelectMany(v => v.Errors);

            InitializeDropDowns(model);

            if (submit == "Save Your Inputs")
            {
                model.PDFPath = model.UserId + ".pdf";
                model.Page2Complete = true;
                PopulateEntityFromModel(model);
                //model.PDFPath = CreatePdf(model);
                CreatePdf(model);
                
                //model.PDFPath = HttpContext.Server.MapPath(@"~\UserPDF\");
                
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

                //Do all calculations here
                CalculateValuationVariables(model);
                CalculateNonAdvisorTaxFreeCashFlow(model);
                CalculateDiscountedCashFlow(model);
                CalculateValuationRanges(model);
                //CalculateKPIs(model);

                return View(_ReportViewName, model);
            }
        }

        /// <summary>
        /// Action method to handle to proceed to Optimizer page
        /// </summary>     
        [HttpPost]
        public ActionResult Optimizer(BATModel model)
        {
            return View(_ValuationOptimizer, model);
        }

        /// <summary>
        /// Service call to be consumed by the front end to get some valuation metrics for graphs
        /// </summary>       
        public ActionResult GetValuationMetrics(double PAGR, double PM, double VMI)
        {
            BATModel model = GetClientValuationRanges();
            BenchmarkGroup peerGroup = model.BenchmarksValuationModel.PeerGroups.FirstOrDefault(x => ConvertToDouble(model.Ff_TotalRevenue) > x.GroupRangeMin && ConvertToDouble(model.Ff_TotalRevenue) < x.GroupRangeMax);

            double comparativeValuationMin = peerGroup.ValuationMin;
            double comparativeValuationMax = peerGroup.ValuationMax;

            //Determine max axis value
            double maxValueForClient = model.ClientValuationModel.ValuationMax + (model.ClientValuationModel.ValuationMax / 4);
            double maxValueForComparative = comparativeValuationMax + (comparativeValuationMax / 4);

            if (PAGR < 15)
            {
                return Json(new { operatingprofit = model.Ff_OperatingProfit, totalrevenue = model.Ff_TotalRevenue, maxvalue = (maxValueForClient > maxValueForComparative) ? maxValueForClient : maxValueForComparative, currentmax = model.ClientValuationModel.ValuationMax, currentmin = model.ClientValuationModel.ValuationMin, calculatedmax = 5678423, calculatedmin = 4000786, top_pagr_max = 11, top_pagr_min = 8, top_pm_max = 23, top_pm_min = 20, top_vmi_max = 90, top_vmi_min = 70 }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { operatingprofit = model.Ff_OperatingProfit, totalrevenue = model.Ff_TotalRevenue, maxvalue = (maxValueForClient > maxValueForComparative) ? maxValueForClient : maxValueForComparative, currentmax = model.ClientValuationModel.ValuationMax, currentmin = model.ClientValuationModel.ValuationMin, calculatedmax = comparativeValuationMax, calculatedmin = comparativeValuationMin, top_pagr_max = 11, top_pagr_min = 8, top_pm_max = 23, top_pm_min = 20, top_vmi_max = 90, top_vmi_min = 70 }, JsonRequestBehavior.AllowGet);

            //if params are blank return current with benchmark
            //return Json(new { maxvalue = 60000000, currentmax = 46678564, currentmin = 33567234, calculatedmax = 13000000, calculatedmin = 7000000, top_pagr_max = 11, top_pagr_min = 8, top_pm_max = 23, top_pm_min = 20, top_vmi_max = 90, top_vmi_min = 70 }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Not used in tool directly. This is hmtl for conversion to PDF template
        /// </summary>      
        public ActionResult Pdf()
        {
            return View("PdfShell");
        }

        #endregion ActionMethods

        #region PrivateMethods

        private void InitializeDropDowns(BATModel batModel)
        {
            //years
            List<SelectListItem> years = new List<SelectListItem>
            {                 
                new SelectListItem { Text = "Previous Year", Value = "Previous" },
                new SelectListItem { Text = "YTD 2017", Value = "YTD 2017" }
            };

            batModel.Years = new SelectList(years, "Value", "Text", 0);

            //var selected = years.Where(x => x.Value == "Previous Year").First();
            //selected.Selected = true;

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

        private void PrepopulateVMIs(BATModel model)
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
                //model.ClientValuationModel.ProfitMargin = Convert.ToDouble(model.Ff_NonRecurringRevenue) + Convert.ToDouble(model.Ff_RecurringRevenue) - (Convert.ToDouble(model.Ff_DirectExpenses) + Convert.ToDouble(model.Ff_IndirecteExpenses));
                //model.ClientValuationModel.ProjectedAnnualGrowthRate = Convert.ToInt32(model.Ff_ProjectedGrowthRate);

                if (model.ClientValuationModel == null)
                {
                    model.ClientValuationModel = new ClientValuationModel();
                }

                model.ClientValuationModel.ProfitMargin = 225000;
                model.ClientValuationModel.ProjectedAnnualGrowthRate = 0.04;
                model.Ff_TotalRevenue = (ConvertToDouble(model.Ff_NonRecurringRevenue) + ConvertToDouble(model.Ff_RecurringRevenue)).ToString();
                model.Ff_OperatingProfit = (ConvertToDouble(model.Ff_NonRecurringRevenue) + ConvertToDouble(model.Ff_RecurringRevenue) - ConvertToDouble(model.Ff_IndirecteExpenses) - ConvertToDouble(model.Ff_DirectExpenses)).ToString();
                //TODO
                model.Month = 7;
                model.Ff_OperatingProfitAnnualized = (model.Month < 12)? (ConvertToDouble(model.Ff_OperatingProfit) / model.Month * 12).ToString() : model.Ff_OperatingProfit;

                using (AssetmarkBATEntities db = new AssetmarkBATEntities())
                {
                    am_bat user = new am_bat()
                    {
                        //User info
                        UserId = model.UserId,
                        FirstName = model.firstName,
                        LastName = model.lastName,
                        Phone = model.phone,
                        Email = model.email,
                        Zip = model.zip,
                        BrokerOrIRA = model.brokerorira,
                        EloquaUser = model.EloquaUser,

                        PracticeType = (!string.IsNullOrEmpty(model.PracticeTypeOther)) ? model.PracticeTypeOther : model.PracticeType,
                        AffiliationModel = (!string.IsNullOrEmpty(model.AffiliationModeOther)) ? model.AffiliationModeOther : model.AffiliationMode,
                        FirmType = (!string.IsNullOrEmpty(model.FirmTypeOther)) ? model.FirmTypeOther : model.FirmType,
                        TimeRange = (model.Year.Contains("Previous")) ? model.Year : "YTD ",
                        //TODO: remove this temporary value
                        //Month = (model.Year.Contains("Previous")) ? 12 : Convert.ToInt32(model.Month),
                        Month = 7,

                        PDF = model.PDFPath,
                        DateStarted = model.DateStarted,
                        Page2Complete = model.Page2Complete,
                        Page1Complete = model.Page1Complete,

                        //Firm Financials
                        Ff_TotalFirmAsset = (model.Ff_TotalFirmAsset != null) ? model.Ff_TotalFirmAsset.Replace("$", "") : model.Ff_TotalFirmAsset,
                        Ff_NonRecurringRevenue = (model.Ff_NonRecurringRevenue != null) ? model.Ff_NonRecurringRevenue.Replace("$", "") : model.Ff_NonRecurringRevenue,
                        Ff_RecurringRevenue = (model.Ff_RecurringRevenue != null) ? model.Ff_RecurringRevenue.Replace("$", "") : model.Ff_RecurringRevenue,
                        Ff_TotalRevenue = model.Ff_TotalRevenue,
                        Ff_DirectExpenses = (model.Ff_DirectExpenses != null) ? model.Ff_DirectExpenses.Replace("$", "") : model.Ff_DirectExpenses,
                        Ff_IndirectExpenses = (model.Ff_IndirecteExpenses != null) ? model.Ff_IndirecteExpenses.Replace("$", "") : model.Ff_IndirecteExpenses,
                        Ff_OperatingProfit = model.Ff_OperatingProfit,
                        Ff_OperaintProfit_Annualized = model.Ff_OperatingProfitAnnualized,
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
                        VmiIndex = model.ClientValuationModel.VMIScore.ToString(),

                        
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
                    model.firstName = original.FirstName;
                    model.lastName = original.LastName;
                    model.email = original.Email;
                    model.phone = original.Phone;
                    model.zip = original.Zip;
                    model.brokerorira = original.BrokerOrIRA;
                    model.EloquaUser = (original.EloquaUser.HasValue) ? true : false;
                    model.Year = original.TimeRange;
                    //model.Month = original.Month ?? 12; TODO
                    model.Month = 7;
                    model.PDFPath = original.PDF;
                    model.DateStarted = original.DateStarted;

                    model.PracticeType = original.PracticeType;
                    model.AffiliationMode = original.AffiliationModel;
                    model.FirmType = original.FirmType;


                    model.Page2Complete = (original.Page2Complete.HasValue && original.Page2Complete == true) ? true : false;
                    model.Page1Complete = (original.Page1Complete.HasValue && original.Page1Complete == true) ? true : false;

                    //Firm Financials
                    model.Ff_TotalFirmAsset = original.Ff_TotalFirmAsset;
                    model.Ff_NonRecurringRevenue = original.Ff_NonRecurringRevenue;
                    model.Ff_RecurringRevenue = original.Ff_RecurringRevenue;
                    model.Ff_DirectExpenses = original.Ff_DirectExpenses;
                    model.Ff_OperatingProfit = original.Ff_OperatingProfit;
                    model.Ff_OperatingProfitAnnualized = original.Ff_OperaintProfit_Annualized;
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



                    return true;
                }

                return false;
            }
        }

        private static PdfFixedDocument Load(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
                return new PdfFixedDocument(stream);
        }

        private void CreatePdf(BATModel model)
        {
            try
            {
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


                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(100, 0), new PdfPoint(100, 800));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(200, 0), new PdfPoint(200, 800));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(300, 0), new PdfPoint(300, 800));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(400, 0), new PdfPoint(400, 800));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(500, 0), new PdfPoint(500, 800));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(600, 0), new PdfPoint(600, 800));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(700, 0), new PdfPoint(700, 800));

                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 100), new PdfPoint(800, 100));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 200), new PdfPoint(800, 200));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 300), new PdfPoint(800, 300));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 400), new PdfPoint(800, 400));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 500), new PdfPoint(800, 500));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 600), new PdfPoint(800, 600));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 700), new PdfPoint(800, 700));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 800), new PdfPoint(800, 800));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 900), new PdfPoint(800, 900));




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
                    model.BenchmarksValuationModel = new BenchmarksValuationModel();
                    BenchmarkGroup peerGroup = model.BenchmarksValuationModel.PeerGroups.FirstOrDefault(x => ConvertToDouble(model.Ff_TotalRevenue) > x.GroupRangeMin && ConvertToDouble(model.Ff_TotalRevenue) < x.GroupRangeMax);

                    //first row
                    //page.Graphics.DrawString("Total Firm Aum", tableRowTextFont, tableRowTextBlueBrush, 50, 95);
                    //page.Graphics.DrawString("------", tableRowTextFont, tableRowTextBlueBrush, 100, 95);
                    //page.Graphics.DrawString(peerGroup.TotalAUMPerClient.ToString(), tableRowTextFont, tableRowTextBlueBrush, 150, 95);

                    int groupNumber = model.BenchmarksValuationModel.PeerGroups.IndexOf(peerGroup);
                    string group = "$0 - $250K";

                    if (groupNumber == 1)
                    {
                        group = "$250K - $499K";
                    }
                    else if (groupNumber == 2)
                    {
                        group = "$500K - $749K";
                    }
                    else if (groupNumber == 3)
                    {
                        group = "$750K - $999K";
                    }
                    else if (groupNumber == 4)
                    {
                        group = "$1M - $3M";
                    }



                    page.Graphics.DrawString(group, helvetica, redBrush, 700, 135);

                    page.Graphics.DrawString("Bench", helvetica, redBrush, 400, 184);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 210);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 236);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 262);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 288);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 314);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 400, 340);

                    //Client Metrics
                    page.Graphics.DrawString("?????", helvetica, redBrush, 240, 184);
                    page.Graphics.DrawString("?????", helvetica, redBrush, 240, 210);
                    page.Graphics.DrawString(model.Ff_RecurringRevenue, helvetica, redBrush, 240, 236);
                    page.Graphics.DrawString("$" + string.Format("{0:n0}", model.Ff_TotalRevenue), helvetica, textBrush, 240, 262);
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



                    //Graph brushes
                    PdfBrush graphBrush1 = new PdfBrush((new PdfRgbColor(0, 74, 129))); //#004b81 Darkest
                    PdfBrush graphBrush2 = new PdfBrush((new PdfRgbColor(0, 126, 187))); // #007ebb
                    PdfBrush graphBrush3 = new PdfBrush((new PdfRgbColor(109, 198, 233))); //#6dc6e7;
                    PdfBrush graphBrush4 = new PdfBrush((new PdfRgbColor(176, 216, 235))); //#b0d8eb;



                    //First Graph - VMI ---------------------------------------------------------------------------------
                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 1), new PdfPoint(50, 420), new PdfPoint(50, 620));
                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 2), new PdfPoint(50, 620), new PdfPoint(350, 620));
                    page.Graphics.DrawString("1000", helvetica, textBrush, 20, 425);
                    page.Graphics.DrawString("0", helvetica, textBrush, 38, 613);

                    page.Graphics.DrawRectangle(graphBrush1, 220, 575, 60, peerGroup.EYT * 200 / 1000);
                    page.Graphics.DrawRectangle(graphBrush2, 220, 575 - peerGroup.OYO * 200 / 1000, 60, peerGroup.OYO * 200 / 1000);
                    page.Graphics.DrawRectangle(graphBrush3, 220, 575 - peerGroup.OYO * 200 / 1000 - peerGroup.MYB * 200 / 1000, 60, peerGroup.MYB * 200 / 1000);
                    page.Graphics.DrawRectangle(graphBrush4, 220, 575 - peerGroup.OYO * 200 / 1000 - peerGroup.MYB * 200 / 1000 - peerGroup.MYP * 200 / 1000, 60, peerGroup.MYP * 200 / 1000);












                    //Second Graph - Valuation ---------------------------------------------------------------------------------
                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 1), new PdfPoint(450, 420), new PdfPoint(450, 620));
                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 2), new PdfPoint(450, 620), new PdfPoint(750, 620));

                    page.Graphics.DrawString("0", helvetica, textBrush, 440, 613);

                }






                MemoryStream stream = new MemoryStream();
                // Saves the document as stream
                document.Save(stream);

                //document.Save("C:\\Olga\\PdfCustom.pdf");



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

                //return blockBlob.StorageUri.PrimaryUri.ToString();
                //return "sdfsdf";
            }
            catch(Exception ex)
            {
                //return "blah";
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

        private BATModel GetClientValuationRanges()
        {
            BATModel model = new BATModel();
            model.ClientValuationModel = new ClientValuationModel();
            model.BenchmarksValuationModel = new BenchmarksValuationModel();

            if (!string.IsNullOrEmpty(KnownUserId()))
            {
                model.UserId = KnownUserId();

                if (PopulateModelFromDatabase(model))
                {
                    //model.ClientValuationModel.ValuationMin = 2092000;
                    //model.ClientValuationModel.ValuationMax = 3101000;
                    model.ClientValuationModel.ValuationMin = model.ClientValuationModel.ValuationMin;
                    model.ClientValuationModel.ValuationMax = model.ClientValuationModel.ValuationMax;
                }
            }

            return model;
        }

        private void CalculateValuationVariables(BATModel model)
        {
            model.ClientValuationModel = new ClientValuationModel();
            double total = (Convert.ToInt32(model.Vmi_Man_Written_Plan) + Convert.ToInt32(model.Vmi_Man_Track) + Convert.ToInt32(model.Vmi_Man_Phase) + Convert.ToInt32(model.Vmi_Man_Revenue) + Convert.ToInt32(model.Vmi_Man_Practice) +
                   Convert.ToInt32(model.Vmi_Mar_Value_Proposition) + Convert.ToInt32(model.Vmi_Mar_Materials) + Convert.ToInt32(model.Vmi_Mar_Plan) + Convert.ToInt32(model.Vmi_Mar_Prospects) + Convert.ToInt32(model.Vmi_Mar_New_Business) +
                   Convert.ToInt32(model.Vmi_Opt_Automate) + Convert.ToInt32(model.Vmi_Opt_Procedures) + Convert.ToInt32(model.Vmi_Opt_Segment) + Convert.ToInt32(model.Vmi_Opt_Model) + Convert.ToInt32(model.Vmi_Opt_Schedule) +
                   Convert.ToInt32(model.Vmi_Emp_Human) + Convert.ToInt32(model.Vmi_Emp_Compensation) + Convert.ToInt32(model.Vmi_Emp_Responsibilities) + Convert.ToInt32(model.Vmi_Emp_Staff) + Convert.ToInt32(model.Vmi_Emp_Emp_Retention));


            double temp = total / 2000;

            model.ClientValuationModel.VmiRiskRate = 0.15 - temp;
            model.ClientValuationModel.VMIScore = temp * 10000;
            model.ClientValuationModel.UserPerpetualGrowthRate = (model.ClientValuationModel.VMIScore >= 700) ? model.ClientValuationModel._PerpetualGrowthRateMax : model.ClientValuationModel._PerpetualGrowthRateMax - 0.01;

            //returned as whole number
        }

        private void CalculateNonAdvisorTaxFreeCashFlow(BATModel model)
        {
            model.ClientValuationModel.ProfitMargin = Convert.ToDouble((Convert.ToDouble(model.Ff_NonRecurringRevenue) + Convert.ToDouble(model.Ff_RecurringRevenue)) - (Convert.ToDouble(model.Ff_IndirecteExpenses) + Convert.ToDouble(model.Ff_DirectExpenses)));
            model.ClientValuationModel.ProjectedAnnualGrowthRate = ConvertToDouble(model.Ff_ProjectedGrowthRate.Replace("%","").Replace(" ", "")) / 100;

            //year 1
            if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
            {
                //TODO: blank
                model.ClientValuationModel.NonAdvisorCashFlowYear1 = 0;
            }
            else
            {
                model.ClientValuationModel.NonAdvisorCashFlowYear1 = ConvertToDouble(model.Ff_OperatingProfitAnnualized) * (1 + model.ClientValuationModel.ProjectedAnnualGrowthRate) * (1 - model.ClientValuationModel._TaxRate);
            }

            //year2
            if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
            {
                //TODO: blank
                model.ClientValuationModel.NonAdvisorCashFlowYear2 = 0;
            }
            else
            {
                model.ClientValuationModel.NonAdvisorCashFlowYear2 = model.ClientValuationModel.NonAdvisorCashFlowYear1 * (1 + model.ClientValuationModel.ProjectedAnnualGrowthRate);
            }

            //year3
            if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
            {
                //TODO: blank
                model.ClientValuationModel.NonAdvisorCashFlowYear3 = 0;
            }
            else
            {
                model.ClientValuationModel.NonAdvisorCashFlowYear3 = model.ClientValuationModel.NonAdvisorCashFlowYear2 * (1 + model.ClientValuationModel.ProjectedAnnualGrowthRate);
            }

            //year4
            if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
            {
                //TODO: blank
                model.ClientValuationModel.NonAdvisorCashFlowYear4 = 0;
            }
            else
            {
                model.ClientValuationModel.NonAdvisorCashFlowYear4 = model.ClientValuationModel.NonAdvisorCashFlowYear3 * (1 + model.ClientValuationModel.ProjectedAnnualGrowthRate);
            }

            //year5
            if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
            {
                //TODO: blank
                model.ClientValuationModel.NonAdvisorCashFlowYear5 = 0;
            }
            else
            {
                model.ClientValuationModel.NonAdvisorCashFlowYear5 = model.ClientValuationModel.NonAdvisorCashFlowYear4 * (1 + model.ClientValuationModel.ProjectedAnnualGrowthRate);
            }
        }

        private void CalculateDiscountedCashFlow(BATModel model)
        {
            //Max
            model.ClientValuationModel.DiscountedCashFlowYear1Max = model.ClientValuationModel.NonAdvisorCashFlowYear1 / Math.Pow((1 + model.ClientValuationModel._MinVMIRiskRate + model.ClientValuationModel._ValuationRiskRate), 1);
            model.ClientValuationModel.DiscountedCashFlowYear2Max = model.ClientValuationModel.NonAdvisorCashFlowYear2 / Math.Pow((1 + model.ClientValuationModel._MinVMIRiskRate + model.ClientValuationModel._ValuationRiskRate), 2);
            model.ClientValuationModel.DiscountedCashFlowYear3Max = model.ClientValuationModel.NonAdvisorCashFlowYear3 / Math.Pow((1 + model.ClientValuationModel._MinVMIRiskRate + model.ClientValuationModel._ValuationRiskRate), 3);
            model.ClientValuationModel.DiscountedCashFlowYear4Max = model.ClientValuationModel.NonAdvisorCashFlowYear4 / Math.Pow((1 + model.ClientValuationModel._MinVMIRiskRate + model.ClientValuationModel._ValuationRiskRate), 4);
            model.ClientValuationModel.DiscountedCashFlowYear5Max = model.ClientValuationModel.NonAdvisorCashFlowYear5 / Math.Pow((1 + model.ClientValuationModel._MinVMIRiskRate + model.ClientValuationModel._ValuationRiskRate), 5);

            //Min
            model.ClientValuationModel.DiscountedCashFlowYear1Min = model.ClientValuationModel.NonAdvisorCashFlowYear1 / Math.Pow((1 + model.ClientValuationModel.VmiRiskRate + model.ClientValuationModel._ValuationRiskRate), 1);
            model.ClientValuationModel.DiscountedCashFlowYear2Min = model.ClientValuationModel.NonAdvisorCashFlowYear2 / Math.Pow((1 + model.ClientValuationModel.VmiRiskRate + model.ClientValuationModel._ValuationRiskRate), 2);
            model.ClientValuationModel.DiscountedCashFlowYear3Min = model.ClientValuationModel.NonAdvisorCashFlowYear3 / Math.Pow((1 + model.ClientValuationModel.VmiRiskRate + model.ClientValuationModel._ValuationRiskRate), 3);
            model.ClientValuationModel.DiscountedCashFlowYear4Min = model.ClientValuationModel.NonAdvisorCashFlowYear4 / Math.Pow((1 + model.ClientValuationModel.VmiRiskRate + model.ClientValuationModel._ValuationRiskRate), 4);
            model.ClientValuationModel.DiscountedCashFlowYear5Min = model.ClientValuationModel.NonAdvisorCashFlowYear5 / Math.Pow((1 + model.ClientValuationModel.VmiRiskRate + model.ClientValuationModel._ValuationRiskRate), 5);
        }

        private void CalculateValuationRanges(BATModel model)
        {
            //Discounted Cash Flow Range
            model.ClientValuationModel.DiscountedCashFlowMin = model.ClientValuationModel.DiscountedCashFlowYear1Min + model.ClientValuationModel.DiscountedCashFlowYear2Min + model.ClientValuationModel.DiscountedCashFlowYear3Min + model.ClientValuationModel.DiscountedCashFlowYear4Min + model.ClientValuationModel.DiscountedCashFlowYear5Min;
            model.ClientValuationModel.DiscountedCashFlowMax = model.ClientValuationModel.DiscountedCashFlowYear1Max + model.ClientValuationModel.DiscountedCashFlowYear2Max + model.ClientValuationModel.DiscountedCashFlowYear3Max + model.ClientValuationModel.DiscountedCashFlowYear4Max + model.ClientValuationModel.DiscountedCashFlowYear5Max;

            //Perpetual Growth Rate Cash FLow
            model.ClientValuationModel.PerpetualGrowthRateCashFlowMin = model.ClientValuationModel.NonAdvisorCashFlowYear5 * (1 + model.ClientValuationModel._PerpetualGrowthRateMin) /
                ((model.ClientValuationModel.VmiRiskRate + model.ClientValuationModel._ValuationRiskRate) - model.ClientValuationModel._PerpetualGrowthRateMin) /
                Math.Pow(1 + model.ClientValuationModel.VmiRiskRate + model.ClientValuationModel._ValuationRiskRate, 5);

            model.ClientValuationModel.PerpetualGrowthRateCashFlowMax = model.ClientValuationModel.NonAdvisorCashFlowYear5 * (1 + model.ClientValuationModel.UserPerpetualGrowthRate) /
                ((model.ClientValuationModel._MinVMIRiskRate + model.ClientValuationModel._ValuationRiskRate) - model.ClientValuationModel.UserPerpetualGrowthRate) /
                Math.Pow(1 + model.ClientValuationModel._MinVMIRiskRate + model.ClientValuationModel._ValuationRiskRate, 5);

        }

        private void CalculateKPIs(BATModel model)
        {
            //TODO: blanks for D12 and D18
            model.ClientValuationModel.RecurringRevenuePerClient = (Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) == 0 || Convert.ToDouble(model.Ff_ClientRelationships) == 0) ?
                0 : Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) / Convert.ToDouble(model.Ff_ClientRelationships);

            //TODO: blanks for D12  and D20
            model.ClientValuationModel.RecurringRevenuePerClient = (Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) == 0 || Convert.ToDouble(model.Ff_FullTimeAdvisorsAnnualized) == 0) ?
                0 : Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) / Convert.ToDouble(model.Ff_FullTimeAdvisorsAnnualized);

            //TODO: blanks for D13 and D18 
            model.ClientValuationModel.RecurringRevenuePerClient = (Convert.ToDouble(model.Ff_TotalRevenueAnnualized) == 0 || Convert.ToDouble(model.Ff_ClientRelationships) == 0) ?
                0 : Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) / Convert.ToDouble(model.Ff_ClientRelationships);








            //TODO: blanks for D12 
            model.ClientValuationModel.RecurringRevenuePerClient = (Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) == 0) ?
                0 : Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) / Convert.ToDouble(model.Ff_ClientRelationships);

            //TODO: blanks for D12 
            model.ClientValuationModel.RecurringRevenuePerClient = (Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) == 0) ?
                0 : Convert.ToDouble(model.Ff_RecurringRevenueAnnualized) / Convert.ToDouble(model.Ff_ClientRelationships);
        }

        #endregion
    }
}