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
        private string _EloquaCookieName = "ELOQUA";
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
                    return View(_Page1QuestionsViewName, model);
                    //}
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

        private void SaveAnswers(BATModel model)
        {
            if (!string.IsNullOrEmpty(model.PracticeTypeOther))
            {
                model.PracticeType = model.PracticeTypeOther;
            }

            if (!string.IsNullOrEmpty(model.AffiliationModeOther))
            {
                model.AffiliationMode = model.AffiliationModeOther;
            }

            if (!string.IsNullOrEmpty(model.FirmTypeOther))
            {
                model.FirmType = model.FirmTypeOther;
            }

            if (!model.Year.Contains("Previous"))
            {
                model.Year = "YTD " + DateTime.Now.Year;
            }

            model.DateStarted = DateTime.Now.ToString();

            if (HttpContext.Request.Cookies[_EloquaCookieName] != null && !string.IsNullOrEmpty(HttpContext.Request.Cookies[_EloquaCookieName].Value))
            {
                model.EloquaId = HttpContext.Request.Cookies[_EloquaCookieName].Value;
            }

            //If all fields filled out calculate annuals, valuation metrics, and mark Page1Complete as true
            if (!string.IsNullOrEmpty(model.Ff_TotalFirmAsset) && !string.IsNullOrEmpty(model.Ff_RecurringRevenue) && !string.IsNullOrEmpty(model.Ff_NonRecurringRevenue)
                && !string.IsNullOrEmpty(model.Ff_DirectExpenses) && !string.IsNullOrEmpty(model.Ff_IndirecteExpenses)
                && !string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate) && !string.IsNullOrEmpty(model.Ff_ClientRelationships) && !string.IsNullOrEmpty(model.Ff_FullTimeAdvisors) && !string.IsNullOrEmpty(model.Ff_FullTimeNonAdvisors)
                && !string.IsNullOrEmpty(model.Ff_NewClients))
            {
                model.Ff_TotalRevenue = (ConvertToDouble(model.Ff_NonRecurringRevenue) + ConvertToDouble(model.Ff_RecurringRevenue)).ToString("C", new System.Globalization.CultureInfo("en-US"));

                model.Ff_OperatingProfit = (ConvertToDouble(model.Ff_NonRecurringRevenue) + ConvertToDouble(model.Ff_RecurringRevenue) - ConvertToDouble(model.Ff_IndirecteExpenses) - ConvertToDouble(model.Ff_DirectExpenses)).ToString("C", new System.Globalization.CultureInfo("en-US"));
                model.Ff_OperatingProfitAnnualized = (model.Month < 12) ? (ConvertToDouble(model.Ff_OperatingProfit) / model.Month * 12).ToString("C", new System.Globalization.CultureInfo("en-US")) : model.Ff_OperatingProfit;

                model.Ff_NonRecurringRevenueAnnualized = ((ConvertToDouble(model.Ff_NonRecurringRevenue) / model.Month * 12)).ToString("C", new System.Globalization.CultureInfo("en-US"));
                model.Ff_RecurringRevenueAnnualized = ((ConvertToDouble(model.Ff_RecurringRevenue) / model.Month * 12)).ToString("C", new System.Globalization.CultureInfo("en-US"));
                model.Ff_TotalRevenueAnnualized = (ConvertToDouble(model.Ff_RecurringRevenueAnnualized) + ConvertToDouble(model.Ff_RecurringRevenueAnnualized)).ToString("C", new System.Globalization.CultureInfo("en-US"));

                model.Ff_DirectExpensesAnnualized = (ConvertToDouble(model.Ff_DirectExpenses) / model.Month * 12).ToString("C", new System.Globalization.CultureInfo("en-US"));
                model.Ff_IndirecteExpensesAnnualized = (ConvertToDouble(model.Ff_IndirecteExpenses) / model.Month * 12).ToString("C", new System.Globalization.CultureInfo("en-US"));

                model.Ff_NewClientsAnnualized = (ConvertToDouble(model.Ff_NewClients) / model.Month * 12).ToString();
                model.Page1Complete = true;
            }
            else
            {
                model.Page1Complete = false;
            }

            PopulateEntityFromModel(model);
        }

        /// <summary>
        /// Action method to handle user input on Page 1 (Firm Financials)
        /// </summary>      
        [HttpPost]
        public ActionResult Page2Questions(BATModel model, string submit)
        {
            InitializeDropDowns(model);

            if (string.IsNullOrEmpty(model.PracticeTypeOther) && model.PracticeType == "Practice Type")
            {
                model.Message = "Practice Type is required";
                return View(_Page1QuestionsViewName, model);
            }

            if (submit == "Save Your Inputs")
            {
                SaveAnswers(model);
                return View(_Page1QuestionsViewName, model);
            }
            else
            {
                SaveAnswers(model);

                if (model.Vmi_Index == null || model.Vmi_Index == "N/A")
                {
                    PrepopulateVMIs(model);
                }

                return View(_Page2QuestionsViewName, model);
            }
        }

        private void CalculateValuation(BATModel model, bool recalculate)
        {
            CalculateValuationVariables(model, recalculate);

            bool nonAdvisorCashFlow = CalculateNonAdvisorTaxFreeCashFlow(model);
            bool discountedCashFlow = CalculateDiscountedCashFlow(model);

            if (nonAdvisorCashFlow && discountedCashFlow)
            {
                CalculateValuationRanges(model);
            }
        }

        /// <summary>
        /// Action method to handle user input on Page 2 (VMI sliders)
        /// </summary>      
        [HttpPost]
        public ActionResult Report(BATModel model, string submit)
        {  
            InitializeDropDowns(model);

            if (submit == "Save Your Inputs")
            {
                model.PDFPath = "https://assetmarkstdstor.blob.core.windows.net/assetmarkbat/" + model.UserId + ".pdf";
                model.Vmi_Index = ((Convert.ToInt32(model.Vmi_Man_Written_Plan) + Convert.ToInt32(model.Vmi_Man_Track) + Convert.ToInt32(model.Vmi_Man_Phase) + Convert.ToInt32(model.Vmi_Man_Revenue) + Convert.ToInt32(model.Vmi_Man_Practice)
                    + Convert.ToInt32(model.Vmi_Mar_Value_Proposition) + Convert.ToInt32(model.Vmi_Mar_Materials) + Convert.ToInt32(model.Vmi_Mar_Plan) + Convert.ToInt32(model.Vmi_Mar_Prospects) + Convert.ToInt32(model.Vmi_Mar_New_Business)
                    + Convert.ToInt32(model.Vmi_Emp_Human) + Convert.ToInt32(model.Vmi_Emp_Compensation) + Convert.ToInt32(model.Vmi_Emp_Responsibilities) + Convert.ToInt32(model.Vmi_Emp_Staff) + Convert.ToInt32(model.Vmi_Emp_Emp_Retention)
                    + Convert.ToInt32(model.Vmi_Opt_Automate) + Convert.ToInt32(model.Vmi_Opt_Procedures) + Convert.ToInt32(model.Vmi_Opt_Segment) + Convert.ToInt32(model.Vmi_Opt_Model) + Convert.ToInt32(model.Vmi_Opt_Schedule)) * 5).ToString();
                model.Page2Complete = true;
                PopulateEntityFromModel(model); 

                return View(_Page2QuestionsViewName, model);
            }
            else if (submit == "Previous Firm Financials")
            {
                PopulateModelFromDatabase(model);
                return View(_Page1QuestionsViewName, model);
            }
            else
            {
                model.PDFPath = "https://assetmarkstdstor.blob.core.windows.net/assetmarkbat/" + model.UserId + ".pdf";
                model.Vmi_Index = ((Convert.ToInt32(model.Vmi_Man_Written_Plan) + Convert.ToInt32(model.Vmi_Man_Track) + Convert.ToInt32(model.Vmi_Man_Phase) + Convert.ToInt32(model.Vmi_Man_Revenue) + Convert.ToInt32(model.Vmi_Man_Practice)
                    + Convert.ToInt32(model.Vmi_Mar_Value_Proposition) + Convert.ToInt32(model.Vmi_Mar_Materials) + Convert.ToInt32(model.Vmi_Mar_Plan) + Convert.ToInt32(model.Vmi_Mar_Prospects) + Convert.ToInt32(model.Vmi_Mar_New_Business)
                    + Convert.ToInt32(model.Vmi_Emp_Human) + Convert.ToInt32(model.Vmi_Emp_Compensation) + Convert.ToInt32(model.Vmi_Emp_Responsibilities) + Convert.ToInt32(model.Vmi_Emp_Staff) + Convert.ToInt32(model.Vmi_Emp_Emp_Retention)
                    + Convert.ToInt32(model.Vmi_Opt_Automate) + Convert.ToInt32(model.Vmi_Opt_Procedures) + Convert.ToInt32(model.Vmi_Opt_Segment) + Convert.ToInt32(model.Vmi_Opt_Model) + Convert.ToInt32(model.Vmi_Opt_Schedule)) * 5).ToString();
                model.Page2Complete = true;
                PopulateEntityFromModel(model);
                PopulateModelFromDatabase(model);
                CalculateKPIs(model);
                return View(_ReportViewName, model);
            }
        }

        /// <summary>
        /// Action method to handle to proceed to Optimizer page
        /// </summary>     
        [HttpPost]
        public ActionResult Optimizer(BATModel model)
        {
            PopulateModelFromDatabase(model);
            CalculateKPIs(model);
            DrawPdf(model);
            return View(_ValuationOptimizer, model);
        }

        /// <summary>
        /// Service call to be consumed by the front end to get some valuation metrics for graphs
        /// </summary>       
        public ActionResult GetValuationMetrics(double PAGR, double PM, int VMI, bool recalculate)
        {
            BATModel clientModel = new BATModel();
            BATModel comparativeModel = clientModel;
            double comparativeValuationMin;
            double comparativeValuationMax;

            clientModel = GetClientValuationRanges(false, null, -1, -1, -1);

            if (recalculate) //call made from Valuation Optimizer page after slider(s) selections. Build comparative range with optimizer values
            {
                //model = GetClientValuationRanges(null, PAGR, PM, VMI);
                //CalculateValuation(model, false);               

                //Call to recalculate and build comparative range
                comparativeModel = GetClientValuationRanges(true, null, PAGR, PM, VMI);
                comparativeValuationMin = comparativeModel.ClientValuationModel.ValuationMin;
                comparativeValuationMax = comparativeModel.ClientValuationModel.ValuationMax;
            }
            else //call made from the Report page or Optimizer page on load. Get benchmarks
            {
                BenchmarkGroup peerGroup = clientModel.BenchmarksValuationModel.PeerGroups.FirstOrDefault(x => ConvertToDouble(clientModel.Ff_TotalRevenue) > x.GroupRangeMin && ConvertToDouble(clientModel.Ff_TotalRevenue) < x.GroupRangeMax);

                if (peerGroup == null)
                {
                    peerGroup = clientModel.BenchmarksValuationModel.PeerGroups.Last();
                }

                comparativeValuationMin = peerGroup.ValuationMin;
                comparativeValuationMax = peerGroup.ValuationMax;
            }

            double maxValueForClient = clientModel.ClientValuationModel.ValuationMax + (clientModel.ClientValuationModel.ValuationMax / 4);
            double maxValueForComparative = comparativeValuationMax + (comparativeValuationMax / 4);

            double CLIENT_MIN = clientModel.ClientValuationModel.ValuationMin;
            double CLIENT_MAX = clientModel.ClientValuationModel.ValuationMax;

            double COMP_MIN = comparativeValuationMin;
            double COMP_MAX = comparativeValuationMax;

            return Json(new
            {
                operatingprofit = (ConvertToDouble(clientModel.Ff_OperatingProfitAnnualized) / ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized)).ToString(),
                totalrevenue = clientModel.Ff_TotalRevenue,
                maxvalue = (maxValueForClient > maxValueForComparative) ? maxValueForClient : maxValueForComparative, //Determine max axis values
                                                                                                                      //client range
                currentmax = clientModel.ClientValuationModel.ValuationMax,
                currentmin = clientModel.ClientValuationModel.ValuationMin,
                //comparative range (either benchmark or optimized)
                calculatedmax = comparativeValuationMax,
                calculatedmin = comparativeValuationMin,
                //metrics used in calculations
                pagr = clientModel.ClientValuationModel.ProjectedAnnualGrowthRate,
                pm = clientModel.ClientValuationModel.ProfitMargin,
                vmi = clientModel.Vmi_Index,
                profitannualized = ConvertToDouble(clientModel.Ff_OperatingProfitAnnualized),
                top_pagr_max = 12,
                top_pagr_min = 8,
                top_pm_max = 25.2,
                top_pm_min = 20,
                top_vmi_max = 900,
                top_vmi_min = 700
            }, JsonRequestBehavior.AllowGet);

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
                new SelectListItem { Text = "Previous Year", Value = "Previous Year" },
                new SelectListItem { Text = "YTD " + DateTime.Now.Year, Value = "YTD " + DateTime.Now.Year }
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
                return Convert.ToDouble(input.Replace("$", "").Replace(",", "").Replace(" ", ""));
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
                using (AssetmarkBATEntities db = new AssetmarkBATEntities())
                {
                    am_bat user = new am_bat()
                    {
                        //User info
                        UserId = model.UserId,
                        FirstName = model.firstName,
                        LastName = model.lastName,
                        Phone = model.busPhone,
                        Email = model.emailAddress,
                        Zip = model.zipPostal,
                        BrokerOrIRA = model.brokerDealer1,
                        EloquaId = model.EloquaId,

                        PracticeType = model.PracticeType,
                        AffiliationModel = model.AffiliationMode,
                        FirmType = model.FirmType,
                        TimeRange = model.Year,
                        Month = (model.Year.Contains("Previous")) ? 12 : Convert.ToInt32(model.Month),

                        PDF = model.PDFPath,
                        DateStarted = model.DateStarted,
                        Page2Complete = model.Page2Complete,
                        Page1Complete = model.Page1Complete,

                        //Firm Financials
                        Ff_TotalFirmAsset = model.Ff_TotalFirmAsset,
                        Ff_NonRecurringRevenue = model.Ff_NonRecurringRevenue,
                        Ff_NonRecurringRevenue_Annualized = model.Ff_NonRecurringRevenueAnnualized,
                        Ff_RecurringRevenue = model.Ff_RecurringRevenue,
                        Ff_RecurringRevenue_Annualized = model.Ff_RecurringRevenueAnnualized,
                        Ff_TotalRevenue = model.Ff_TotalRevenue,
                        Ff_TotalRevenue_Annualized = model.Ff_TotalRevenueAnnualized,
                        Ff_DirectExpenses = model.Ff_DirectExpenses,
                        Ff_DirectExpenses_Annualized = model.Ff_DirectExpensesAnnualized,
                        Ff_IndirectExpenses = model.Ff_IndirecteExpenses,
                        Ff_IndirectExpenses_Annualized = model.Ff_IndirecteExpensesAnnualized,
                        Ff_OperatingProfit = model.Ff_OperatingProfit,
                        Ff_OperaintProfit_Annualized = model.Ff_OperatingProfitAnnualized,
                        Ff_Client_Relationships = model.Ff_ClientRelationships,
                        Ff_Fte_Advisors = model.Ff_FullTimeAdvisors,
                        Ff_Fte_Non_Advisors = model.Ff_FullTimeNonAdvisors,
                        Ff_New_Clients = model.Ff_NewClients,
                        Ff_New_Clients_Annualized = model.Ff_NewClientsAnnualized,
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
                        VmiIndex = model.Vmi_Index,
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
            try
            {
                using (AssetmarkBATEntities db = new AssetmarkBATEntities())
                {
                    var original = db.am_bat.Find(model.UserId);

                    if (original != null)
                    {
                        model.firstName = original.FirstName;
                        model.lastName = original.LastName;
                        model.emailAddress = original.Email;
                        model.busPhone = original.Phone;
                        model.zipPostal = original.Zip;
                        model.brokerDealer1 = original.BrokerOrIRA;
                        model.EloquaId = original.EloquaId;
                        model.Year = original.TimeRange;
                        model.Month = original.Month.Value;
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
                        model.Ff_NonRecurringRevenueAnnualized = original.Ff_NonRecurringRevenue_Annualized;
                        model.Ff_RecurringRevenue = original.Ff_RecurringRevenue;
                        model.Ff_RecurringRevenueAnnualized = original.Ff_RecurringRevenue_Annualized;
                        model.Ff_DirectExpenses = original.Ff_DirectExpenses;
                        model.Ff_DirectExpensesAnnualized = original.Ff_DirectExpenses_Annualized;
                        model.Ff_OperatingProfit = original.Ff_OperatingProfit;
                        model.Ff_OperatingProfitAnnualized = original.Ff_OperaintProfit_Annualized;
                        model.Ff_IndirecteExpenses = original.Ff_IndirectExpenses;
                        model.Ff_IndirecteExpensesAnnualized = original.Ff_IndirectExpenses_Annualized;
                        model.Ff_ProjectedGrowthRate = original.Ff_Projected_Growth;
                        //model.Ff_ProjectedGrowthRateAnnualized = original.Ff_OperaintProfit_Annualized;
                        model.Ff_ClientRelationships = original.Ff_Client_Relationships;
                        //model.Ff_ClientRelationshipsAnnualized = original.Ff_New_Clients_Annualized;
                        model.Ff_FullTimeNonAdvisors = original.Ff_Fte_Non_Advisors;
                        model.Ff_FullTimeAdvisors = original.Ff_Fte_Advisors;
                        model.Ff_NewClients = original.Ff_New_Clients;
                        model.Ff_NewClientsAnnualized = original.Ff_New_Clients_Annualized;
                        model.Ff_TotalRevenue = original.Ff_TotalRevenue;
                        model.Ff_TotalRevenueAnnualized = original.Ff_TotalRevenue_Annualized;



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

                        model.Vmi_Index = original.VmiIndex;

                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading from Azure database. ", e.Message);
                return false;
            }
        }

        private static PdfFixedDocument Load(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
                return new PdfFixedDocument(stream);
        }

        private void DrawPdf(BATModel model)
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
                PdfStandardFont helvetica = new PdfStandardFont(PdfStandardFontFace.Helvetica, 8);
                // Create a solid RGB red brush.
                PdfBrush backgroundBrush = new PdfBrush(PdfRgbColor.Aqua);
                PdfBrush darkBlueBrush = new PdfBrush();
                darkBlueBrush.Color = new PdfRgbColor(123, 123, 123);
                PdfBrush textBrush = new PdfBrush((PdfRgbColor.Black));
                PdfBrush whiteBrush = new PdfBrush((PdfRgbColor.White));
                PdfBrush redBrush = new PdfBrush(PdfRgbColor.Black);
                PdfBrush textBlueBrush = new PdfBrush((new PdfRgbColor(5, 79, 124)));
                PdfStandardFont largeTitleFont = new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 36);



                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(400, 0), new PdfPoint(400, 500));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Red, 1), new PdfPoint(500, 0), new PdfPoint(500, 500));
                //page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Green, 1), new PdfPoint(0, 400), new PdfPoint(800, 400));

                if (PopulateModelFromDatabase(model))
                {
                    CalculateValuation(model, false);
                    model.BenchmarksValuationModel = new BenchmarksValuationModel();
                    BenchmarkGroup peerGroup = model.BenchmarksValuationModel.PeerGroups.FirstOrDefault(b => ConvertToDouble(model.Ff_TotalRevenue) > b.GroupRangeMin && ConvertToDouble(model.Ff_TotalRevenue) < b.GroupRangeMax);

                    if (peerGroup == null)
                    {
                        peerGroup = model.BenchmarksValuationModel.PeerGroups.Last();
                    }

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

                    page.Graphics.DrawString(group, helvetica, redBrush, 500, 95);

                    //Firm Financials Table--------------------------------------------------
                    page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_TotalFirmAsset) ? (ConvertToDouble(model.Ff_TotalFirmAsset)).ToString("C0") : "N/A", helvetica, redBrush, 170, 128);
                    page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_ClientRelationships) ? model.Ff_ClientRelationships : "N/A", helvetica, redBrush, 170, 147);
                    page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_RecurringRevenue) ? (Convert.ToInt32(ConvertToDouble(model.Ff_RecurringRevenue))).ToString("C0") : "N/A", helvetica, redBrush, 170, 165);
                    page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_TotalRevenue) ? (Convert.ToInt32(ConvertToDouble(model.Ff_TotalRevenue))).ToString("C0") : "N/A", helvetica, textBrush, 170, 183);

                    if (!string.IsNullOrEmpty(model.Ff_DirectExpenses) && !string.IsNullOrEmpty(model.Ff_IndirecteExpenses))
                    {
                        page.Graphics.DrawString(Convert.ToInt32((ConvertToDouble(model.Ff_DirectExpenses) + ConvertToDouble(model.Ff_IndirecteExpenses))).ToString("C0"), helvetica, redBrush, 170, 201);
                    }
                    else if (string.IsNullOrEmpty(model.Ff_DirectExpenses))
                    {
                        page.Graphics.DrawString((Convert.ToInt32(ConvertToDouble(model.Ff_IndirecteExpenses)).ToString("C0")), helvetica, redBrush, 170, 201);
                    }
                    else
                    {
                        page.Graphics.DrawString((Convert.ToInt32(ConvertToDouble(model.Ff_DirectExpenses)).ToString("C0")), helvetica, redBrush, 170, 201);
                    }


                    page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_OperatingProfit) ? Convert.ToInt32(ConvertToDouble(model.Ff_OperatingProfit)).ToString("C0") : "N/A", helvetica, redBrush, 170, 219);
                    page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate) ? model.Ff_ProjectedGrowthRate : "N/A", helvetica, redBrush, 170, 236);

                    page.Graphics.DrawString(peerGroup.AUM.ToString("C0"), helvetica, redBrush, 290, 128);
                    page.Graphics.DrawString(peerGroup.ClientRelationships.ToString(), helvetica, redBrush, 290, 147);
                    page.Graphics.DrawString(peerGroup.RecurringRevenue.ToString("C0"), helvetica, redBrush, 290, 165);
                    page.Graphics.DrawString(peerGroup.TotalRevenue.ToString("C0"), helvetica, redBrush, 290, 183);
                    page.Graphics.DrawString(peerGroup.TotalExpenses.ToString("C0"), helvetica, redBrush, 290, 201);
                    page.Graphics.DrawString(peerGroup.OperatingProfit.ToString("C0"), helvetica, redBrush, 290, 219);
                    page.Graphics.DrawString(peerGroup.ProjectedAnnualGrowthRate.ToString() + "%", helvetica, redBrush, 290, 236);


                    //KPI's Table ------------------------------------------------------------ 
                    page.Graphics.DrawString(ConvertToDouble(model.ClientValuationModel.RecurringRevenuePerClient).ToString("c0"), helvetica, redBrush, 170, 557);
                    page.Graphics.DrawString(ConvertToDouble(model.ClientValuationModel.RecurringRevenuePerAdvisor).ToString("C0"), helvetica, redBrush, 170, 576);
                    page.Graphics.DrawString(ConvertToDouble(model.ClientValuationModel.TotalRevenuePerClient).ToString("C0"), helvetica, redBrush, 170, 595);
                    page.Graphics.DrawString(ConvertToDouble(model.ClientValuationModel.TotalAUMperClient).ToString("C0"), helvetica, redBrush, 170, 611);
                    page.Graphics.DrawString(ConvertToDouble(model.ClientValuationModel.TotalAUMperAdvisor).ToString("C0"), helvetica, redBrush, 170, 626);
                    page.Graphics.DrawString(ConvertToDouble(model.ClientValuationModel.ProfitPerClient).ToString("C0"), helvetica, redBrush, 170, 644);
                    page.Graphics.DrawString(ConvertToDouble(model.ClientValuationModel.ProfitAsPercentOfRevenut).ToString("0.0") + " %", helvetica, redBrush, 170, 661);
                    page.Graphics.DrawString(ConvertToDouble(model.ClientValuationModel.ClientsPerAdvisor).ToString("C0"), helvetica, redBrush, 170, 680);
                    page.Graphics.DrawString(ConvertToDouble(model.ClientValuationModel.RevenueAsBPSOnAssets).ToString("C0"), helvetica, redBrush, 170, 699);

                    page.Graphics.DrawString(Math.Ceiling(peerGroup.RecRevPerClient).ToString("C0"), helvetica, redBrush, 290, 557);
                    page.Graphics.DrawString(Math.Ceiling(peerGroup.RecRevPerAdvisor).ToString("C0"), helvetica, redBrush, 290, 576);
                    page.Graphics.DrawString(Math.Ceiling(peerGroup.TotalRevPerClient).ToString("C0"), helvetica, redBrush, 290, 595);
                    page.Graphics.DrawString(Math.Ceiling(peerGroup.TotalAUMPerClient).ToString("C0"), helvetica, redBrush, 290, 611);
                    page.Graphics.DrawString(Math.Ceiling(peerGroup.TotalAUMPerAdvisor).ToString("C0"), helvetica, redBrush, 290, 626);
                    page.Graphics.DrawString(peerGroup.ProfitPerClient.ToString("C0"), helvetica, redBrush, 290, 644);
                    page.Graphics.DrawString(peerGroup.ProfitAsPercentOfRevenue.ToString("0.0") + " %", helvetica, redBrush, 290, 661);

                    //page.Graphics.DrawString(((int)Math.Floor(peerGroup.ProfitAsPercentOfRevenue)).ToString() + " %", helvetica, redBrush, 290, 661);

                    //(int)Math.floor(10.99999)
                    page.Graphics.DrawString(peerGroup.ClientsPerAdvisor.ToString(), helvetica, redBrush, 290, 680);
                    page.Graphics.DrawString(peerGroup.RevenutAsPBSOnAssets.ToString(), helvetica, redBrush, 290, 699);



                    //Graph brushes
                    PdfBrush graphBrush1 = new PdfBrush((new PdfRgbColor(0, 74, 129))); //#004b81 Darkest
                    PdfBrush graphBrush2 = new PdfBrush((new PdfRgbColor(0, 126, 187))); // #007ebb
                    PdfBrush graphBrush3 = new PdfBrush((new PdfRgbColor(109, 198, 233))); //#6dc6e7;
                    PdfBrush graphBrush4 = new PdfBrush((new PdfRgbColor(176, 216, 235))); //#b0d8eb;



                    //VMI Graph-------------------------------------------------------------------------------- -
                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 300), new PdfPoint(35, 415));

                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 300), new PdfPoint(255, 300));
                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 327), new PdfPoint(255, 327));
                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 349), new PdfPoint(255, 349));
                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 371), new PdfPoint(255, 371));
                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 393), new PdfPoint(255, 393));

                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 1), new PdfPoint(35, 415), new PdfPoint(255, 415));
                    page.Graphics.DrawString("1000", helvetica, textBrush, 17, 298);
                    page.Graphics.DrawString("800", helvetica, textBrush, 19, 320);
                    page.Graphics.DrawString("600", helvetica, textBrush, 19, 342);
                    page.Graphics.DrawString("400", helvetica, textBrush, 19, 364);
                    page.Graphics.DrawString("200", helvetica, textBrush, 19, 386);
                    page.Graphics.DrawString("0", helvetica, textBrush, 25, 408);
                    page.Graphics.DrawString("Your Firm", helvetica, textBlueBrush, 70, 419);
                    page.Graphics.DrawString("Benchmark Index", helvetica, textBlueBrush, 160, 419);

                    //Calculate blocks height for Your Firm -------------------------------------------------------------------------------
                    model.ClientValuationModel.ManagingYourPracticeScore = (Convert.ToInt32(model.Vmi_Man_Written_Plan) + Convert.ToInt32(model.Vmi_Man_Track) + Convert.ToInt32(model.Vmi_Man_Phase) + Convert.ToInt32(model.Vmi_Man_Revenue) + Convert.ToInt32(model.Vmi_Man_Practice)) * 5;
                    model.ClientValuationModel.MarketingYourBusinessScore = (Convert.ToInt32(model.Vmi_Mar_Value_Proposition) + Convert.ToInt32(model.Vmi_Mar_Materials) + Convert.ToInt32(model.Vmi_Mar_Plan) + Convert.ToInt32(model.Vmi_Mar_Prospects) + Convert.ToInt32(model.Vmi_Mar_New_Business)) * 5;
                    model.ClientValuationModel.EmpoweringYourTeamScore = (Convert.ToInt32(model.Vmi_Emp_Human) + Convert.ToInt32(model.Vmi_Emp_Compensation) + Convert.ToInt32(model.Vmi_Emp_Responsibilities) + Convert.ToInt32(model.Vmi_Emp_Staff) + Convert.ToInt32(model.Vmi_Emp_Emp_Retention)) * 5;
                    model.ClientValuationModel.OptimizingYourOperationsScore = (Convert.ToInt32(model.Vmi_Opt_Automate) + Convert.ToInt32(model.Vmi_Opt_Procedures) + Convert.ToInt32(model.Vmi_Opt_Segment) + Convert.ToInt32(model.Vmi_Opt_Model) + Convert.ToInt32(model.Vmi_Opt_Schedule)) * 5;
                    double pixel = 0.115;
                    double firstBlock = model.ClientValuationModel.EmpoweringYourTeamScore * pixel;
                    double secondBlock = model.ClientValuationModel.OptimizingYourOperationsScore * pixel;
                    double thirdBlock = model.ClientValuationModel.MarketingYourBusinessScore * pixel;
                    double fourthBlock = model.ClientValuationModel.ManagingYourPracticeScore * pixel;

                    double x = 70;
                    double y = 403;

                    page.Graphics.DrawRectangle(graphBrush1, x, y, 55, firstBlock);

                    y = y - firstBlock;
                    page.Graphics.DrawRectangle(graphBrush2, x, y, 55, secondBlock);

                    y = y - secondBlock;
                    page.Graphics.DrawRectangle(graphBrush3, x, y, 55, thirdBlock);

                    y = y - thirdBlock;
                    page.Graphics.DrawRectangle(graphBrush4, x, y, 55, fourthBlock);




                    y = 403;

                    page.Graphics.DrawString(model.ClientValuationModel.EmpoweringYourTeamScore.ToString(), helvetica, whiteBrush, x + 20, y + 3); //score

                    y = y - firstBlock;
                    page.Graphics.DrawString(model.ClientValuationModel.OptimizingYourOperationsScore.ToString(), helvetica, whiteBrush, x + 20, y + 3); //score

                    y = y - secondBlock;
                    page.Graphics.DrawString(model.ClientValuationModel.MarketingYourBusinessScore.ToString(), helvetica, whiteBrush, x + 20, y + 3); //score

                    y = y - thirdBlock;
                    page.Graphics.DrawString(model.ClientValuationModel.ManagingYourPracticeScore.ToString(), helvetica, whiteBrush, x + 20, y + 3); //score


                    //page.Graphics.DrawString()

                    //Calculate blocks height for Benchmarks    ----------------------------------------------------------------------------                
                    firstBlock = peerGroup.EYT * pixel;
                    secondBlock = peerGroup.OYO * pixel;
                    thirdBlock = peerGroup.MYB * pixel;
                    fourthBlock = peerGroup.MYP * pixel;

                    x = 160;
                    y = 392;
                    page.Graphics.DrawRectangle(graphBrush1, x, y, 55, firstBlock);

                    y = y - secondBlock;
                    page.Graphics.DrawRectangle(graphBrush2, x, y, 55, secondBlock);

                    y = y - thirdBlock;
                    page.Graphics.DrawRectangle(graphBrush3, x, y, 55, thirdBlock);

                    y = y - fourthBlock;
                    page.Graphics.DrawRectangle(graphBrush4, x, y, 55, fourthBlock);




                    y = 392;
                    page.Graphics.DrawString(peerGroup.EYT.ToString(), helvetica, whiteBrush, x + 20, y + 3); //score

                    y = y - firstBlock;
                    page.Graphics.DrawString(peerGroup.OYO.ToString(), helvetica, whiteBrush, x + 20, y + 3); //score

                    y = y - secondBlock;
                    page.Graphics.DrawString(peerGroup.MYB.ToString(), helvetica, whiteBrush, x + 20, y + 3); //score

                    y = y - thirdBlock;
                    page.Graphics.DrawString(peerGroup.MYP.ToString(), helvetica, whiteBrush, x + 20, y + 3); //score


                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /////////////// VALUATION RANGE GRAPH   /////////////////////
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    double axisMax = peerGroup.ValuationMax + (peerGroup.ValuationMax / 4);
                    pixel = axisMax / 135;

                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(353, 300), new PdfPoint(353, 435)); //vertical
                    page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 1), new PdfPoint(353, 435), new PdfPoint(550, 435)); //horizontal
                    //page.Graphics.DrawString();

                    

                    //Client Valuation Range
                    if (model.ClientValuationModel.ValuationMax != 0 && model.ClientValuationModel.ValuationMin != 0)
                    {
                        if(model.ClientValuationModel.ValuationMax > peerGroup.ValuationMax)
                        {
                            axisMax = model.ClientValuationModel.ValuationMax + (model.ClientValuationModel.ValuationMax / 4);
                            pixel = axisMax / 135;
                        }
                       
                        firstBlock = (model.ClientValuationModel.ValuationMax - model.ClientValuationModel.ValuationMin) / pixel;
                        

                        x = 390;
                        y = 435 - (model.ClientValuationModel.ValuationMax / pixel);
                        page.Graphics.DrawRectangle(graphBrush2, x, y, 55, firstBlock);
                        page.Graphics.DrawString(model.ClientValuationModel.ValuationMax.ToString("C0"), helvetica, textBrush, x, (y - 9));
                        page.Graphics.DrawString(model.ClientValuationModel.ValuationMin.ToString("C0"), helvetica, textBrush, x, (y + firstBlock + 3));
                    }
                    else
                    {
                        x = 400;
                        page.Graphics.DrawString("No Data", helvetica, textBrush, x, 350);
                    }

                   

                    //Benchmark Valuation Range

                    double dollarsInPixel = axisMax / 135;
                    double range = peerGroup.ValuationMax - peerGroup.ValuationMin;
                    double height = range / dollarsInPixel; //height in pixels

                    x = 470;
                    y = peerGroup.ValuationMax / dollarsInPixel;
                    y = 435 - y; //bottom of range

                    page.Graphics.DrawString(peerGroup.ValuationMax.ToString("C0"), helvetica, textBrush, x, (y - 9));
                    page.Graphics.DrawRectangle(graphBrush3, x, y, 55, height);
                    page.Graphics.DrawString(peerGroup.ValuationMin.ToString("C0"), helvetica, textBrush, x, (y + height + 3));

                    //Axis values
                    PdfStringLayoutOptions layout = new PdfStringLayoutOptions() { HorizontalAlign = PdfStringHorizontalAlign.Right, X = 351, Y = 300 };
                    PdfPen pen = new PdfPen(PdfRgbColor.Black, 0.1);
                    PdfStringAppearanceOptions appearance = new PdfStringAppearanceOptions(helvetica, pen, textBrush);
                   
                    page.Graphics.DrawString("0", helvetica, textBrush, 346, 430);
                    page.Graphics.DrawString((Convert.ToInt32(axisMax)).ToString("C0"), appearance, layout);

                    double incrementHeight = 135 / 4;
                    double incrementValue = axisMax / 4;

                    layout.Y = layout.Y + incrementHeight;
                    page.Graphics.DrawString((axisMax - incrementValue).ToString("C0"), appearance, layout);

                    layout.Y = layout.Y + incrementHeight;
                    page.Graphics.DrawString((axisMax - (axisMax / 2)).ToString("C0"), appearance, layout);

                    layout.Y = layout.Y + incrementHeight;
                    page.Graphics.DrawString((axisMax - (incrementValue * 3)).ToString("C0"), appearance, layout);
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
            }
            catch (Exception e)
            {
                Console.WriteLine("Error creating a PDF document", e.Message);
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

        private BATModel GetClientValuationRanges(bool recalculate, BATModel model = null, double PAGR = -1, double PM = -1, int VMI = -1)
        {
            if (model == null)
            {
                if (!string.IsNullOrEmpty(KnownUserId()))
                {
                    model = new BATModel();
                    model.UserId = KnownUserId();
                }
            }

            if (PopulateModelFromDatabase(model))
            {
                //any of the Optimizer values can be sent separately and all together, need the check here
                if (PAGR != -1)
                    model.Ff_ProjectedGrowthRate = PAGR.ToString();

                if (PM != -1)
                    model.Ff_OperatingProfitAnnualized = (ConvertToDouble(model.Ff_TotalRevenueAnnualized) * (PM / 1000 / 100)).ToString(); //Operating Profit $ = Total Revenue*0.25 (I.E.)
                else
                {
                    string check = model.Ff_OperatingProfitAnnualized;
                }

                if (VMI != -1)
                    model.Vmi_Index = VMI.ToString();


                //Do all calculations here
                CalculateValuation(model, recalculate);
                CalculateKPIs(model);

                //model.ClientValuationModel.ValuationMin = model.ClientValuationModel.ValuationMin;
                //model.ClientValuationModel.ValuationMax = model.ClientValuationModel.ValuationMax;
            }

            return model;
        }

        /// <summary>
        /// Calculates VMI related values and puts the results in the BAT model
        /// </summary>
        /// <param name="model"></param>
        private void CalculateValuationVariables(BATModel model, bool recalculate)
        {
            if (!recalculate)
            {
                model.ClientValuationModel.ManagingYourPracticeScore = (Convert.ToInt32(model.Vmi_Man_Written_Plan) + Convert.ToInt32(model.Vmi_Man_Track) + Convert.ToInt32(model.Vmi_Man_Phase) + Convert.ToInt32(model.Vmi_Man_Revenue) + Convert.ToInt32(model.Vmi_Man_Practice)) * 5;
                model.ClientValuationModel.MarketingYourBusinessScore = (Convert.ToInt32(model.Vmi_Mar_Value_Proposition) + Convert.ToInt32(model.Vmi_Mar_Materials) + Convert.ToInt32(model.Vmi_Mar_Plan) + Convert.ToInt32(model.Vmi_Mar_Prospects) + Convert.ToInt32(model.Vmi_Mar_New_Business)) * 5;
                model.ClientValuationModel.EmpoweringYourTeamScore = (Convert.ToInt32(model.Vmi_Emp_Human) + Convert.ToInt32(model.Vmi_Emp_Compensation) + Convert.ToInt32(model.Vmi_Emp_Responsibilities) + Convert.ToInt32(model.Vmi_Emp_Staff) + Convert.ToInt32(model.Vmi_Emp_Emp_Retention)) * 5;
                model.ClientValuationModel.OptimizingYourOperationsScore = (Convert.ToInt32(model.Vmi_Opt_Automate) + Convert.ToInt32(model.Vmi_Opt_Procedures) + Convert.ToInt32(model.Vmi_Opt_Segment) + Convert.ToInt32(model.Vmi_Opt_Model) + Convert.ToInt32(model.Vmi_Opt_Schedule)) * 5;

                int total = model.ClientValuationModel.ManagingYourPracticeScore + model.ClientValuationModel.MarketingYourBusinessScore + model.ClientValuationModel.EmpoweringYourTeamScore + model.ClientValuationModel.OptimizingYourOperationsScore;
                double temp = total / 5 / 2000;

                model.ClientValuationModel.VmiRiskRate = 0.15 - temp;
                model.Vmi_Index = total.ToString();
                model.ClientValuationModel.UserPerpetualGrowthRate = (Convert.ToInt32(model.Vmi_Index) >= 700) ? model.ClientValuationModel._PerpetualGrowthRateMax : model.ClientValuationModel._PerpetualGrowthRateMax - 0.01;
            }
            else
            {
                model.ClientValuationModel.VmiRiskRate = 0.15 - (Convert.ToInt32(model.Vmi_Index) / 5 / 2000);
                model.ClientValuationModel.UserPerpetualGrowthRate = (Convert.ToInt32(model.Vmi_Index) >= 700) ? model.ClientValuationModel._PerpetualGrowthRateMax : model.ClientValuationModel._PerpetualGrowthRateMax - 0.01;
            }
        }

        private bool CalculateNonAdvisorTaxFreeCashFlow(BATModel model)
        {
            try
            {
                model.ClientValuationModel.ProfitMargin = ((ConvertToDouble(model.Ff_NonRecurringRevenue) + ConvertToDouble(model.Ff_RecurringRevenue)) - (ConvertToDouble(model.Ff_IndirecteExpenses) + ConvertToDouble(model.Ff_DirectExpenses)));

                model.ClientValuationModel.ProjectedAnnualGrowthRate = ConvertToDouble(model.Ff_ProjectedGrowthRate.Replace("%", "").Replace(" ", "")) / 100;

                //year 1
                if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
                {
                    //blank
                    model.ClientValuationModel.NonAdvisorCashFlowYear1 = 0;
                }
                else
                {
                    model.ClientValuationModel.NonAdvisorCashFlowYear1 = ConvertToDouble(model.Ff_OperatingProfitAnnualized) * (1 + model.ClientValuationModel.ProjectedAnnualGrowthRate) * (1 - model.ClientValuationModel._TaxRate);
                }

                //year2
                if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
                {
                    //blank
                    model.ClientValuationModel.NonAdvisorCashFlowYear2 = 0;
                }
                else
                {
                    model.ClientValuationModel.NonAdvisorCashFlowYear2 = model.ClientValuationModel.NonAdvisorCashFlowYear1 * (1 + model.ClientValuationModel.ProjectedAnnualGrowthRate);
                }

                //year3
                if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
                {
                    //blank
                    model.ClientValuationModel.NonAdvisorCashFlowYear3 = 0;
                }
                else
                {
                    model.ClientValuationModel.NonAdvisorCashFlowYear3 = model.ClientValuationModel.NonAdvisorCashFlowYear2 * (1 + model.ClientValuationModel.ProjectedAnnualGrowthRate);
                }

                //year4
                if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
                {
                    //blank
                    model.ClientValuationModel.NonAdvisorCashFlowYear4 = 0;
                }
                else
                {
                    model.ClientValuationModel.NonAdvisorCashFlowYear4 = model.ClientValuationModel.NonAdvisorCashFlowYear3 * (1 + model.ClientValuationModel.ProjectedAnnualGrowthRate);
                }

                //year5
                if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
                {
                    //blank
                    model.ClientValuationModel.NonAdvisorCashFlowYear5 = 0;
                }
                else
                {
                    model.ClientValuationModel.NonAdvisorCashFlowYear5 = model.ClientValuationModel.NonAdvisorCashFlowYear4 * (1 + model.ClientValuationModel.ProjectedAnnualGrowthRate);
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool CalculateDiscountedCashFlow(BATModel model)
        {
            try
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

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void CalculateValuationRanges(BATModel model)
        {
            try
            {
                //Discounted Cash Flow Range
                model.ClientValuationModel.DiscountedCashFlowMin = Math.Ceiling(model.ClientValuationModel.DiscountedCashFlowYear1Min + model.ClientValuationModel.DiscountedCashFlowYear2Min + model.ClientValuationModel.DiscountedCashFlowYear3Min + model.ClientValuationModel.DiscountedCashFlowYear4Min + model.ClientValuationModel.DiscountedCashFlowYear5Min);
                model.ClientValuationModel.DiscountedCashFlowMax = Math.Ceiling(model.ClientValuationModel.DiscountedCashFlowYear1Max + model.ClientValuationModel.DiscountedCashFlowYear2Max + model.ClientValuationModel.DiscountedCashFlowYear3Max + model.ClientValuationModel.DiscountedCashFlowYear4Max + model.ClientValuationModel.DiscountedCashFlowYear5Max);

                //Perpetual Growth Rate Cash FLow
                model.ClientValuationModel.PerpetualGrowthRateCashFlowMin = Math.Ceiling(model.ClientValuationModel.NonAdvisorCashFlowYear5 * (1 + model.ClientValuationModel._PerpetualGrowthRateMin) /
                    ((model.ClientValuationModel.VmiRiskRate + model.ClientValuationModel._ValuationRiskRate) - model.ClientValuationModel._PerpetualGrowthRateMin) /
                    Math.Pow(1 + model.ClientValuationModel.VmiRiskRate + model.ClientValuationModel._ValuationRiskRate, 5));

                model.ClientValuationModel.PerpetualGrowthRateCashFlowMax = Math.Ceiling(model.ClientValuationModel.NonAdvisorCashFlowYear5 * (1 + model.ClientValuationModel.UserPerpetualGrowthRate) /
                    ((model.ClientValuationModel._MinVMIRiskRate + model.ClientValuationModel._ValuationRiskRate) - model.ClientValuationModel.UserPerpetualGrowthRate) /
                    Math.Pow(1 + model.ClientValuationModel._MinVMIRiskRate + model.ClientValuationModel._ValuationRiskRate, 5));

                model.ClientValuationModel.ValuationMin = model.ClientValuationModel.DiscountedCashFlowMin + model.ClientValuationModel.PerpetualGrowthRateCashFlowMin;
                model.ClientValuationModel.ValuationMax = model.ClientValuationModel.DiscountedCashFlowMax + model.ClientValuationModel.PerpetualGrowthRateCashFlowMax;
            }
            catch (Exception e)
            {
                model.ClientValuationModel.ValuationMin = 0;
                model.ClientValuationModel.ValuationMax = 0;
            }
        }

        private void CalculateKPIs(BATModel model)
        {
            //D10 - Total Assets
            //D12 - RecurringRevenueAnnualized
            //D18 - ClientRelationships
            //D20 - FTE Advisors
            //D13 - Total RevenueAnnualized
            //D16 - 

            //D12 and D18 - Recurring Revenue per Client
            if (string.IsNullOrEmpty(model.Ff_RecurringRevenueAnnualized) || string.IsNullOrEmpty(model.Ff_ClientRelationships))
            {
                model.ClientValuationModel.RecurringRevenuePerClient = "N/A";
            }
            else
            {
                model.ClientValuationModel.RecurringRevenuePerClient = (ConvertToDouble(model.Ff_RecurringRevenueAnnualized) / ConvertToDouble(model.Ff_ClientRelationships)).ToString();
            }
            //D12 and D20 - Recurring  Revenue per Advisor
            if (string.IsNullOrEmpty(model.Ff_RecurringRevenueAnnualized) || string.IsNullOrEmpty(model.Ff_FullTimeAdvisors))
            {
                model.ClientValuationModel.RecurringRevenuePerAdvisor = "N/A";
            }
            else
            {
                model.ClientValuationModel.RecurringRevenuePerAdvisor = (ConvertToDouble(model.Ff_RecurringRevenueAnnualized) / ConvertToDouble(model.Ff_FullTimeAdvisors)).ToString();
            }
            //D13 and D18 - Total Revenue per Client
            if (string.IsNullOrEmpty(model.Ff_TotalRevenueAnnualized) || string.IsNullOrEmpty(model.Ff_ClientRelationships))
            {
                model.ClientValuationModel.TotalRevenuePerClient = "N/A";
            }
            else
            {
                model.ClientValuationModel.TotalRevenuePerClient = (ConvertToDouble(model.Ff_TotalRevenueAnnualized) / ConvertToDouble(model.Ff_ClientRelationships)).ToString();
            }
            //D10 and D18 - Total AUM per Client
            if (string.IsNullOrEmpty(model.Ff_TotalFirmAsset) || string.IsNullOrEmpty(model.Ff_ClientRelationships))
            {
                model.ClientValuationModel.TotalAUMperClient = "N/A";
            }
            else
            {
                model.ClientValuationModel.TotalAUMperClient = (ConvertToDouble(model.Ff_TotalFirmAsset) / ConvertToDouble(model.Ff_ClientRelationships)).ToString();
            }
            //D10 and D20 - Total AUM per Advisor
            if (string.IsNullOrEmpty(model.Ff_TotalFirmAsset) || string.IsNullOrEmpty(model.Ff_FullTimeAdvisors))
            {
                model.ClientValuationModel.TotalAUMperAdvisor = "N/A";
            }
            else
            {
                model.ClientValuationModel.TotalAUMperAdvisor = (ConvertToDouble(model.Ff_TotalFirmAsset) / ConvertToDouble(model.Ff_FullTimeAdvisors)).ToString();
            }
            //D16 and D18 - Profit per Client
            if (string.IsNullOrEmpty(model.Ff_OperatingProfitAnnualized) || string.IsNullOrEmpty(model.Ff_ClientRelationships))
            {
                model.ClientValuationModel.ProfitPerClient = "N/A";
            }
            else
            {
                model.ClientValuationModel.ProfitPerClient = (ConvertToDouble(model.Ff_OperatingProfitAnnualized) / ConvertToDouble(model.Ff_ClientRelationships)).ToString();
            }
            //D16 and D13 - Profit per Client
            if (string.IsNullOrEmpty(model.Ff_OperatingProfitAnnualized) || string.IsNullOrEmpty(model.Ff_TotalRevenueAnnualized))
            {
                model.ClientValuationModel.ProfitAsPercentOfRevenut = "N/A";
            }
            else
            {
                model.ClientValuationModel.ProfitAsPercentOfRevenut = (ConvertToDouble(model.Ff_OperatingProfitAnnualized) / ConvertToDouble(model.Ff_TotalRevenueAnnualized)).ToString();
            }
            //D18 and D20 - Clients per Advisor
            if (string.IsNullOrEmpty(model.Ff_ClientRelationships) || string.IsNullOrEmpty(model.Ff_FullTimeAdvisors))
            {
                model.ClientValuationModel.ClientsPerAdvisor = "N/A";
            }
            else
            {
                model.ClientValuationModel.ClientsPerAdvisor = (ConvertToDouble(model.Ff_ClientRelationships) / ConvertToDouble(model.Ff_FullTimeAdvisors)).ToString();
            }
            //D13 and D10 - Revenue as BPS on Assets
            if (string.IsNullOrEmpty(model.Ff_TotalRevenueAnnualized) || string.IsNullOrEmpty(model.Ff_TotalFirmAsset))
            {
                model.ClientValuationModel.RevenueAsBPSOnAssets = "N/A";
            }
            else
            {
                model.ClientValuationModel.RevenueAsBPSOnAssets = (ConvertToDouble(model.Ff_TotalRevenueAnnualized) / ConvertToDouble(model.Ff_TotalFirmAsset) * 10000).ToString();
            }
        }

        #endregion
    }
}