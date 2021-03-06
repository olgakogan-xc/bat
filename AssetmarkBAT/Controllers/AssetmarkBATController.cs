﻿using AssetmarkBAT.Models;
using AssetmarkBAT.Services;
using AssetmarkBAT.Utilities;
using AssetmarkBATDbConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        private Helpers _Helpers = new Helpers();
        private PdfService _PdfService = new PdfService();

        #region ActionMethods

        /// <summary>
        /// Action Method to initiate the BAT tool
        /// </summary>     

        public ActionResult Index()
        {
            BATModel model = new BATModel();
            InitializeDropDowns(model);

            //Eloqua ID            
            if (Request.Url.AbsoluteUri.Contains(_EloquaQueryStringParamName))
            {
                model.EloquaId = Request.QueryString[_EloquaQueryStringParamName];
            }
            else if (HttpContext.Request.Cookies[_EloquaCookieName] != null && !string.IsNullOrEmpty(HttpContext.Request.Cookies[_EloquaCookieName].Value))
            {
                model.EloquaId = Request.QueryString[_EloquaCookieName];
            }

            if (!string.IsNullOrEmpty(KnownUserId()))
            {
                model.UserId = KnownUserId();

                if (PopulateModelFromDatabase(model))
                {
                    if (Request.QueryString["redo"] != null && !string.IsNullOrEmpty(Request.QueryString["redo"]) && PopulateModelFromDatabase(model))
                    {
                        return View(_Page1QuestionsViewName, model);
                    }
                    else
                    {
                        if (model.Vmi_Index != "N/A") //VMI's filled out
                        {
                            if (!string.IsNullOrEmpty(model.firstName) && !string.IsNullOrEmpty(model.lastName))
                            {
                                GetVMISectionScores(model);
                                GetBenchmarkGroup(model);

                                if(string.IsNullOrEmpty(model.results))
                                {
                                    model.results = "https://assetmarkstdstor.blob.core.windows.net/assetmarkbat/" + model.UserId + ".pdf";
                                }

                                return View(_ValuationOptimizer, model);
                            }
                            else
                            {
                                return View(_ReportViewName, model);
                            }
                        }
                        else if (model.Page1Complete)
                        {
                            if (model.Vmi_Index == null || model.Vmi_Index == "N/A")
                            {
                                PrepopulateVMIs(model);
                            }

                            return View(_Page2QuestionsViewName, model);
                        }
                        else
                        {
                            return View(_Page1QuestionsViewName, model);
                        }
                    }
                }
                else
                {
                    return View(_TermsViewName, model);
                }
            }

            return View(_TermsViewName, model);
        }

        /// <summary>
        /// Action method to handle user input on Terms page
        /// </summary>  
        [HttpPost]
        public ActionResult Page1Questions(BATModel model)
        {
            if (!model.AgreedToTerms)
            {
                return View(_TermsViewName);
            }

            InitializeDropDowns(model);

            if (string.IsNullOrEmpty(KnownUserId()))
            {
                model.UserId = Guid.NewGuid().ToString();
                HttpCookie assetmarkBATCookie = new HttpCookie(_BATCookieName);
                assetmarkBATCookie.Value = model.UserId;
                assetmarkBATCookie.Expires = DateTime.Now.AddYears(10);
                HttpContext.Response.Cookies.Add(assetmarkBATCookie);
                SaveAnswers(model);
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

        /// <summary>
        /// Action method to handle user input on Page 2 (VMI sliders)
        /// </summary>      
        [HttpPost]
        public ActionResult Report(BATModel model, string submit)
        {
            InitializeDropDowns(model);

            if (submit == "Save Your Inputs")
            {
                model.results = "https://assetmarkstdstor.blob.core.windows.net/assetmarkbat/" + model.UserId + ".pdf";
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
                model.results = "https://assetmarkstdstor.blob.core.windows.net/assetmarkbat/" + model.UserId + ".pdf";
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
            BATModel savedModel = new BATModel() { UserId = model.UserId };

            if (PopulateModelFromDatabase(savedModel))
            {
                savedModel.firstName = model.firstName;
                savedModel.lastName = model.lastName;
                savedModel.zipPostal = model.zipPostal;
                savedModel.busPhone = model.busPhone;
                savedModel.emailAddress = model.emailAddress;
                savedModel.brokerDealer1 = model.brokerDealer1;

                SaveAnswers(savedModel);
                GetVMISectionScores(savedModel);
                GetBenchmarkGroup(savedModel);

                CalculateKPIs(savedModel);
                CalculateValuation(savedModel, false);

                _PdfService.DrawPdf(savedModel);
            }

            return View("Eloqua", savedModel);
        }

        /// <summary>
        /// Service call to be consumed by the front end to get some valuation metrics for graphs
        /// </summary>       
        public ActionResult GetValuationMetrics(double PAGR, double PM, int VMI, bool recalculate, bool report)
        {
            BATModel clientModel = new BATModel();
            BATModel comparativeModel = clientModel;
            double comparativeValuationMin;
            double comparativeValuationMax;

            clientModel = GetClientValuationRanges(false, null, -1, -1, -1);

            if (recalculate) //call made from Valuation Optimizer page after slider(s) selections. Build comparative range with optimizer values
            {
                //Call to recalculate and build comparative range
                comparativeModel = GetClientValuationRanges(true, null, PAGR, PM, VMI);
                comparativeValuationMin = comparativeModel.ClientValuationModel.ValuationMin;
                comparativeValuationMax = comparativeModel.ClientValuationModel.ValuationMax;
            }
            else
            {
                if (report) //call made from the report page. Build comparative range from benchmarks
                {
                    BenchmarkGroup peerGroup = clientModel.BenchmarksValuationModel.PeerGroups.FirstOrDefault(x => _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) > x.GroupRangeMin && _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) < x.GroupRangeMax);

                    if (peerGroup == null && _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) > 0)
                    {
                        peerGroup = clientModel.BenchmarksValuationModel.PeerGroups.Last();
                    }
                    else if (peerGroup == null && _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) == 0)
                    {
                        peerGroup = clientModel.BenchmarksValuationModel.PeerGroups.First();
                    }

                    comparativeValuationMin = peerGroup.ValuationMin;
                    comparativeValuationMax = peerGroup.ValuationMax;
                }
                else //call from the optimizer page on load. Build comparative range identical to client range
                {
                    comparativeValuationMin = clientModel.ClientValuationModel.ValuationMin;
                    comparativeValuationMax = clientModel.ClientValuationModel.ValuationMax;
                }
            }

            double maxValueForClient = clientModel.ClientValuationModel.ValuationMax + (clientModel.ClientValuationModel.ValuationMax / 4);
            double maxValueForComparative = comparativeValuationMax + (comparativeValuationMax / 4);

            string opProfAnnual = (recalculate) ? (_Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) * PM).ToString() : _Helpers.ConvertToDouble(clientModel.Ff_OperatingProfitAnnualized).ToString();

            if (!recalculate)
            {
                PM = Math.Round((_Helpers.ConvertToDouble(clientModel.Ff_OperatingProfitAnnualized) / _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized)), 2);
            }

            return Json(new
            {
                operatingprofitannual = (_Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) * PM).ToString(),
                profitmarginannual = PM,
                maxvalue = (maxValueForClient > maxValueForComparative) ? maxValueForClient.ToString() : maxValueForComparative.ToString(),
                currentmin = clientModel.ClientValuationModel.ValuationMin,
                currentmax = clientModel.ClientValuationModel.ValuationMax,

                calculatedmax = comparativeValuationMax,
                calculatedmin = comparativeValuationMin,
                pagr = _Helpers.ConvertToDouble(clientModel.Ff_ProjectedGrowthRate) / 100,
                vmi = clientModel.Vmi_Index,

                top_pagr_max = 12,
                top_pagr_min = 8,
                top_pm_max = 40,
                top_pm_min = 20,
                top_vmi_max = 900,
                top_vmi_min = 700
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetVMIMetrics()
        {
            BATModel clientModel = new BATModel();

            clientModel.UserId = KnownUserId();

            if (PopulateModelFromDatabase(clientModel))
            {
                BenchmarkGroup peerGroup = clientModel.BenchmarksValuationModel.PeerGroups.FirstOrDefault(x => _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) > x.GroupRangeMin && _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) < x.GroupRangeMax);

                if (peerGroup == null && _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) > 0)
                {
                    peerGroup = clientModel.BenchmarksValuationModel.PeerGroups.Last();
                }
                else if (peerGroup == null && _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) == 0)
                {
                    peerGroup = clientModel.BenchmarksValuationModel.PeerGroups.First();
                }

                GetVMISectionScores(clientModel);

                return Json(new
                {
                    bm_managing = peerGroup.MYP,
                    bm_marketing = peerGroup.MYB,
                    bm_optimizing = peerGroup.OYO,
                    bm_empowering = peerGroup.EYT,
                    cl_managing = clientModel.ClientValuationModel.ManagingYourPracticeScore,
                    cl_marketing = clientModel.ClientValuationModel.MarketingYourBusinessScore,
                    cl_optimizing = clientModel.ClientValuationModel.OptimizingYourOperationsScore,
                    cl_empowering = clientModel.ClientValuationModel.EmpoweringYourTeamScore
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    result = "No result"
                }, JsonRequestBehavior.AllowGet);
            }
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
            try
            {
                //years
                List<SelectListItem> years = new List<SelectListItem>
            {
                new SelectListItem { Text = "Previous Year", Value = "Previous Year" },
                new SelectListItem { Text = "YTD " + DateTime.Now.Year, Value = "YTD " + DateTime.Now.Year }
            };

                batModel.Years = new SelectList(years, "Value", "Text", 0);

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
                new SelectListItem { Text = "Single advisor practice: Solo practitioner or providing data on your book of business only", Value = "Single advisor practice" },
                new SelectListItem { Text = "Multiple advisor practice:  Multiple advisor practice providing firm level data", Value = "Multiple advisor practice" },
                new SelectListItem { Text = "Third party: Broker-dealer, wholesaler, consultant ", Value = "Third party" },
                new SelectListItem { Text = "Other", Value = "Other" }
            };

                batModel.PracticeTypes = new SelectList(types, "Value", "Text", batModel.PracticeType);

                //Affiliation Modes
                List<SelectListItem> modes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Affiliation Model", Value = "N/A" },
                new SelectListItem { Text = "BD (Broker-Dealer): Affiliated with a full-service BD/wirehouse, independent BD, insurance BD or own BD", Value = "BD (Broker-Dealer)" },
                new SelectListItem { Text = "RIA only (Registered Investment Advisor): Registered as an investment advisor registered with the SEC or state", Value = "RIA only (Registered Investment Advisor)" },
                new SelectListItem { Text = "Hybrid BD/RIA: Have your own RIA, as well as a BD affiliation", Value = "Hybrid BD/RIA" },
                new SelectListItem { Text = "Other", Value = "Other" }
            };

                batModel.AffiliationModes = new SelectList(modes, "Value", "Text");

                // Firm Types
                List<SelectListItem> firmTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Firm Types", Value = "N/A" },
                new SelectListItem { Text = "Financial Planning Firm: Focused on providing financial planning services / process", Value = "Financial Planning Firm" },
                new SelectListItem { Text = "Investment Advisory Firm: Focused on providing investment strategy and manager selection", Value = "Investment Advisory Firm" },
                new SelectListItem { Text = "Investment Management Firm: Focused on investment recommendations and discretionary investment management of client assets", Value = "Investment Management Firm" },
                new SelectListItem { Text = "Wealth Management Firm: Provide holistic advice to clients, including integrated tax, estate and financial planning in addition to investment services", Value = "Wealth Management Firm" },
                new SelectListItem { Text = "Other", Value = "Other" }
            };

                batModel.FirmTypes = new SelectList(firmTypes, "Value", "Text");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error initializing dropdowns", e.Message);
            }
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
                        //Eloqua Data
                        FirstName = model.firstName,
                        LastName = model.lastName,
                        Phone = model.busPhone,
                        Email = model.emailAddress,
                        Zip = model.zipPostal,
                        BrokerOrIRA = model.brokerDealer1,
                        SalesforceId = model.sFDCContactID,
                        EloquaId = model.EloquaId,

                        PracticeType = model.PracticeType,
                        PracticeTypeOther = model.PracticeTypeOther,
                        AffiliationModel = model.AffiliationMode,
                        AffiliationModeOther = model.AffiliationModeOther,
                        FirmType = model.FirmType,
                        FirmTypeOther = model.FirmTypeOther,
                        TimeRange = model.Year,
                        Month = (model.Year != null && model.Year.Contains("Previous")) ? 12 : Convert.ToInt32(model.Month),

                        PDF = model.results,
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
                        model.sFDCContactID = original.SalesforceId;
                        model.EloquaId = original.EloquaId;
                        model.Year = original.TimeRange;
                        model.Month = original.Month.Value;
                        model.results = original.PDF;
                        model.DateStarted = original.DateStarted;

                        model.PracticeType = original.PracticeType;
                        model.PracticeTypeOther = original.PracticeTypeOther;
                        model.AffiliationMode = original.AffiliationModel;
                        model.AffiliationModeOther = original.AffiliationModeOther;
                        model.FirmType = original.FirmType;
                        model.FirmTypeOther = original.FirmTypeOther;

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
                        model.Ff_ClientRelationships = original.Ff_Client_Relationships;
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

        private void SaveAnswers(BATModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.PracticeType) && model.PracticeType != "Other") { model.PracticeTypeOther = null; }

                if (!string.IsNullOrEmpty(model.AffiliationMode) && model.AffiliationMode != "Other") { model.AffiliationModeOther = null; }

                if (!string.IsNullOrEmpty(model.FirmType) && model.FirmType != "Other") { model.FirmTypeOther = null; }


                if (model.Year != null && !model.Year.Contains("Previous"))
                {
                    model.Year = "YTD " + DateTime.Now.Year;
                }



                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                DateTime pacificTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);

                model.DateStarted = pacificTime.ToString();

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
                    model.Ff_TotalRevenue = (_Helpers.ConvertToDouble(model.Ff_NonRecurringRevenue) + _Helpers.ConvertToDouble(model.Ff_RecurringRevenue)).ToString("C", new System.Globalization.CultureInfo("en-US"));

                    model.Ff_OperatingProfit = (_Helpers.ConvertToDouble(model.Ff_NonRecurringRevenue) + _Helpers.ConvertToDouble(model.Ff_RecurringRevenue) - _Helpers.ConvertToDouble(model.Ff_IndirecteExpenses) - _Helpers.ConvertToDouble(model.Ff_DirectExpenses)).ToString("C", new System.Globalization.CultureInfo("en-US"));
                    model.Ff_OperatingProfitAnnualized = (model.Month < 12) ? (_Helpers.ConvertToDouble(model.Ff_OperatingProfit) / model.Month * 12).ToString("C", new System.Globalization.CultureInfo("en-US")) : model.Ff_OperatingProfit;

                    model.Ff_NonRecurringRevenueAnnualized = ((_Helpers.ConvertToDouble(model.Ff_NonRecurringRevenue) / model.Month * 12)).ToString("C", new System.Globalization.CultureInfo("en-US"));
                    model.Ff_RecurringRevenueAnnualized = ((_Helpers.ConvertToDouble(model.Ff_RecurringRevenue) / model.Month * 12)).ToString("C", new System.Globalization.CultureInfo("en-US"));
                    model.Ff_TotalRevenueAnnualized = (_Helpers.ConvertToDouble(model.Ff_NonRecurringRevenueAnnualized) + _Helpers.ConvertToDouble(model.Ff_RecurringRevenueAnnualized)).ToString("C", new System.Globalization.CultureInfo("en-US"));

                    model.Ff_DirectExpensesAnnualized = (_Helpers.ConvertToDouble(model.Ff_DirectExpenses) / model.Month * 12).ToString("C", new System.Globalization.CultureInfo("en-US"));
                    model.Ff_IndirecteExpensesAnnualized = (_Helpers.ConvertToDouble(model.Ff_IndirecteExpenses) / model.Month * 12).ToString("C", new System.Globalization.CultureInfo("en-US"));

                    model.Ff_NewClientsAnnualized = (_Helpers.ConvertToDouble(model.Ff_NewClients) / model.Month * 12).ToString();
                    model.Page1Complete = true;
                }
                else
                {
                    model.Page1Complete = false;
                }



                PopulateEntityFromModel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving to Azure database " + model.UserId, ex.Message);
            }
        }

        private void CalculateValuation(BATModel model, bool recalculate)
        {
            try
            {
                CalculateValuationVariables(model, recalculate);

                string pgr = model.Ff_ProjectedGrowthRate;
                string vmi = model.Vmi_Index;
                string profit = model.Ff_OperatingProfit;
                string profilAnnual = model.Ff_OperatingProfitAnnualized;

                bool nonAdvisorCashFlow = CalculateNonAdvisorTaxFreeCashFlow(model, recalculate);
                bool discountedCashFlow = CalculateDiscountedCashFlow(model);

                if (nonAdvisorCashFlow && discountedCashFlow)
                {
                    CalculateValuationRanges(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calculating valuation ", ex.Message);
            }
        }

        public string KnownUserId()
        {
            if (HttpContext.Request.Cookies[_BATCookieName] != null && !string.IsNullOrEmpty(HttpContext.Request.Cookies[_BATCookieName].Value))
            {
                return HttpContext.Request.Cookies[_BATCookieName].Value;
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
                    model.Ff_ProjectedGrowthRate = (PAGR * 100).ToString();

                if (PM != -1)
                    model.Ff_OperatingProfitAnnualized = (_Helpers.ConvertToDouble(model.Ff_TotalRevenueAnnualized) * PM).ToString(); //Operating Profit $ = Total Revenue*0.25 (I.E.)
                else
                {
                    string check = model.Ff_OperatingProfitAnnualized;
                }

                if (VMI != -1)
                    model.Vmi_Index = VMI.ToString();


                //Do all calculations here
                CalculateValuation(model, recalculate);
                CalculateKPIs(model);
            }

            return model;
        }

        /// <summary>
        /// Calculates VMI related values and puts the results in the BAT model
        /// </summary>
        /// <param name="model"></param>
        private void CalculateValuationVariables(BATModel model, bool recalculate)
        {
            try
            {
                if (!recalculate)
                {
                    GetVMISectionScores(model);

                    int total = model.ClientValuationModel.ManagingYourPracticeScore + model.ClientValuationModel.MarketingYourBusinessScore + model.ClientValuationModel.EmpoweringYourTeamScore + model.ClientValuationModel.OptimizingYourOperationsScore;
                    double temp = total / 10000.00;

                    model.ClientValuationModel.VmiRiskRate = 0.15 - temp;
                    model.Vmi_Index = total.ToString();
                    model.ClientValuationModel.UserPerpetualGrowthRate = (Convert.ToInt32(model.Vmi_Index) >= 700) ? model.ClientValuationModel._PerpetualGrowthRateMax : model.ClientValuationModel._PerpetualGrowthRateMax - 0.01;
                }
                else
                {
                    model.ClientValuationModel.VmiRiskRate = 0.15 - (Convert.ToInt32(model.Vmi_Index) / 10000.00);
                    model.ClientValuationModel.UserPerpetualGrowthRate = (Convert.ToInt32(model.Vmi_Index) >= 700) ? model.ClientValuationModel._PerpetualGrowthRateMax : model.ClientValuationModel._PerpetualGrowthRateMax - 0.01;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calculating valuation variables ", ex.Message);
            }
        }

        private bool CalculateNonAdvisorTaxFreeCashFlow(BATModel model, bool recalculate)
        {
            try
            {
                model.ClientValuationModel.ProfitMargin = ((_Helpers.ConvertToDouble(model.Ff_NonRecurringRevenue) + _Helpers.ConvertToDouble(model.Ff_RecurringRevenue)) - (_Helpers.ConvertToDouble(model.Ff_IndirecteExpenses) + _Helpers.ConvertToDouble(model.Ff_DirectExpenses)));

                //year 1
                if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
                {
                    //blank
                    model.ClientValuationModel.NonAdvisorCashFlowYear1 = 0;
                }
                else
                {
                    model.ClientValuationModel.NonAdvisorCashFlowYear1 = _Helpers.ConvertToDouble(model.Ff_OperatingProfitAnnualized) * (1 + _Helpers.ConvertToDouble(model.Ff_ProjectedGrowthRate) / 100) * (1 - model.ClientValuationModel._TaxRate);
                }

                //year2
                if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
                {
                    //blank
                    model.ClientValuationModel.NonAdvisorCashFlowYear2 = 0;
                }
                else
                {
                    model.ClientValuationModel.NonAdvisorCashFlowYear2 = model.ClientValuationModel.NonAdvisorCashFlowYear1 * (1 + _Helpers.ConvertToDouble(model.Ff_ProjectedGrowthRate) / 100);
                }

                //year3
                if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
                {
                    //blank
                    model.ClientValuationModel.NonAdvisorCashFlowYear3 = 0;
                }
                else
                {
                    model.ClientValuationModel.NonAdvisorCashFlowYear3 = model.ClientValuationModel.NonAdvisorCashFlowYear2 * (1 + _Helpers.ConvertToDouble(model.Ff_ProjectedGrowthRate) / 100);
                }

                //year4
                if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
                {
                    //blank
                    model.ClientValuationModel.NonAdvisorCashFlowYear4 = 0;
                }
                else
                {
                    model.ClientValuationModel.NonAdvisorCashFlowYear4 = model.ClientValuationModel.NonAdvisorCashFlowYear3 * (1 + _Helpers.ConvertToDouble(model.Ff_ProjectedGrowthRate) / 100);
                }

                //year5
                if (string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate))
                {
                    //blank
                    model.ClientValuationModel.NonAdvisorCashFlowYear5 = 0;
                }
                else
                {
                    model.ClientValuationModel.NonAdvisorCashFlowYear5 = model.ClientValuationModel.NonAdvisorCashFlowYear4 * (1 + _Helpers.ConvertToDouble(model.Ff_ProjectedGrowthRate) / 100);
                }

                return true;
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                return false;
            }
        }

        private void CalculateValuationRanges(BATModel model)
        {
            try
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

                model.ClientValuationModel.ValuationMin = RoundDown(model.ClientValuationModel.DiscountedCashFlowMin + model.ClientValuationModel.PerpetualGrowthRateCashFlowMin);
                model.ClientValuationModel.ValuationMax = RoundUp(model.ClientValuationModel.DiscountedCashFlowMax + model.ClientValuationModel.PerpetualGrowthRateCashFlowMax);
            }
            catch (Exception e)
            {
                model.ClientValuationModel.ValuationMin = 0;
                model.ClientValuationModel.ValuationMax = 0;
            }
        }

        private int RoundUp(double input)
        {
            double start = (int)input / 1000.00;
            int result = (int)(Math.Ceiling(start) * 1000.00);
            return result;
        }

        private int RoundDown(double input)
        {
            double start = (int)input / 1000.00;
            int result = (int)(Math.Floor(start) * 1000.00);
            return result;
        }

        private void CalculateKPIs(BATModel model)
        {
            try
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
                    model.ClientValuationModel.RecurringRevenuePerClient = (_Helpers.ConvertToDouble(model.Ff_RecurringRevenueAnnualized) / _Helpers.ConvertToDouble(model.Ff_ClientRelationships)).ToString();
                }
                //D12 and D20 - Recurring  Revenue per Advisor
                if (string.IsNullOrEmpty(model.Ff_RecurringRevenueAnnualized) || string.IsNullOrEmpty(model.Ff_FullTimeAdvisors))
                {
                    model.ClientValuationModel.RecurringRevenuePerAdvisor = "N/A";
                }
                else
                {
                    model.ClientValuationModel.RecurringRevenuePerAdvisor = (_Helpers.ConvertToDouble(model.Ff_RecurringRevenueAnnualized) / _Helpers.ConvertToDouble(model.Ff_FullTimeAdvisors)).ToString();
                }
                //D13 and D18 - Total Revenue per Client
                if (string.IsNullOrEmpty(model.Ff_TotalRevenueAnnualized) || string.IsNullOrEmpty(model.Ff_ClientRelationships))
                {
                    model.ClientValuationModel.TotalRevenuePerClient = "N/A";
                }
                else
                {
                    model.ClientValuationModel.TotalRevenuePerClient = (_Helpers.ConvertToDouble(model.Ff_TotalRevenueAnnualized) / _Helpers.ConvertToDouble(model.Ff_ClientRelationships)).ToString();
                }
                //D10 and D18 - Total AUM per Client
                if (string.IsNullOrEmpty(model.Ff_TotalFirmAsset) || string.IsNullOrEmpty(model.Ff_ClientRelationships))
                {
                    model.ClientValuationModel.TotalAUMperClient = "N/A";
                }
                else
                {
                    model.ClientValuationModel.TotalAUMperClient = (_Helpers.ConvertToDouble(model.Ff_TotalFirmAsset) / _Helpers.ConvertToDouble(model.Ff_ClientRelationships)).ToString();
                }
                //D10 and D20 - Total AUM per Advisor
                if (string.IsNullOrEmpty(model.Ff_TotalFirmAsset) || string.IsNullOrEmpty(model.Ff_FullTimeAdvisors))
                {
                    model.ClientValuationModel.TotalAUMperAdvisor = "N/A";
                }
                else
                {
                    model.ClientValuationModel.TotalAUMperAdvisor = (_Helpers.ConvertToDouble(model.Ff_TotalFirmAsset) / _Helpers.ConvertToDouble(model.Ff_FullTimeAdvisors)).ToString();
                }
                //D16 and D18 - Profit per Client
                if (string.IsNullOrEmpty(model.Ff_OperatingProfitAnnualized) || string.IsNullOrEmpty(model.Ff_ClientRelationships))
                {
                    model.ClientValuationModel.ProfitPerClient = "N/A";
                }
                else
                {
                    model.ClientValuationModel.ProfitPerClient = (_Helpers.ConvertToDouble(model.Ff_OperatingProfitAnnualized) / _Helpers.ConvertToDouble(model.Ff_ClientRelationships)).ToString();
                }
                //D16 and D13 - Profit per Client
                if (string.IsNullOrEmpty(model.Ff_OperatingProfitAnnualized) || string.IsNullOrEmpty(model.Ff_TotalRevenueAnnualized))
                {
                    model.ClientValuationModel.ProfitAsPercentOfRevenue = "N/A";
                }
                else
                {
                    model.ClientValuationModel.ProfitAsPercentOfRevenue = (_Helpers.ConvertToDouble(model.Ff_OperatingProfitAnnualized) / _Helpers.ConvertToDouble(model.Ff_TotalRevenueAnnualized) * 100).ToString();
                }
                //D18 and D20 - Clients per Advisor
                if (string.IsNullOrEmpty(model.Ff_ClientRelationships) || string.IsNullOrEmpty(model.Ff_FullTimeAdvisors))
                {
                    model.ClientValuationModel.ClientsPerAdvisor = "N/A";
                }
                else
                {
                    model.ClientValuationModel.ClientsPerAdvisor = (_Helpers.ConvertToDouble(model.Ff_ClientRelationships) / _Helpers.ConvertToDouble(model.Ff_FullTimeAdvisors)).ToString();
                }
                //D13 and D10 - Revenue as BPS on Assets
                if (string.IsNullOrEmpty(model.Ff_TotalRevenueAnnualized) || string.IsNullOrEmpty(model.Ff_TotalFirmAsset))
                {
                    model.ClientValuationModel.RevenueAsBPSOnAssets = "N/A";
                }
                else
                {
                    model.ClientValuationModel.RevenueAsBPSOnAssets = (_Helpers.ConvertToDouble(model.Ff_TotalRevenueAnnualized) / _Helpers.ConvertToDouble(model.Ff_TotalFirmAsset) * 10000).ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calculating KPI's", ex.Message);
            }
        }

        private void GetVMISectionScores(BATModel model)
        {
            model.ClientValuationModel.ManagingYourPracticeScore = (Convert.ToInt32(model.Vmi_Man_Written_Plan) + Convert.ToInt32(model.Vmi_Man_Track) + Convert.ToInt32(model.Vmi_Man_Phase) + Convert.ToInt32(model.Vmi_Man_Revenue) + Convert.ToInt32(model.Vmi_Man_Practice)) * 5;
            model.ClientValuationModel.MarketingYourBusinessScore = (Convert.ToInt32(model.Vmi_Mar_Value_Proposition) + Convert.ToInt32(model.Vmi_Mar_Materials) + Convert.ToInt32(model.Vmi_Mar_Plan) + Convert.ToInt32(model.Vmi_Mar_Prospects) + Convert.ToInt32(model.Vmi_Mar_New_Business)) * 5;
            model.ClientValuationModel.EmpoweringYourTeamScore = (Convert.ToInt32(model.Vmi_Emp_Human) + Convert.ToInt32(model.Vmi_Emp_Compensation) + Convert.ToInt32(model.Vmi_Emp_Responsibilities) + Convert.ToInt32(model.Vmi_Emp_Staff) + Convert.ToInt32(model.Vmi_Emp_Emp_Retention)) * 5;
            model.ClientValuationModel.OptimizingYourOperationsScore = (Convert.ToInt32(model.Vmi_Opt_Automate) + Convert.ToInt32(model.Vmi_Opt_Procedures) + Convert.ToInt32(model.Vmi_Opt_Segment) + Convert.ToInt32(model.Vmi_Opt_Model) + Convert.ToInt32(model.Vmi_Opt_Schedule)) * 5;
        }

        private void GetBenchmarkGroup(BATModel clientModel)
        {
            BenchmarkGroup peerGroup = clientModel.BenchmarksValuationModel.PeerGroups.FirstOrDefault(x => _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) > x.GroupRangeMin && _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) < x.GroupRangeMax);

            if (peerGroup == null && _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) > 0)
            {
                peerGroup = clientModel.BenchmarksValuationModel.PeerGroups.Last();
            }
            else if (peerGroup == null && _Helpers.ConvertToDouble(clientModel.Ff_TotalRevenueAnnualized) == 0)
            {
                peerGroup = clientModel.BenchmarksValuationModel.PeerGroups.First();
            }

            clientModel.BenchmarksValuationModel.CurrentPeerGroup = peerGroup;
        }

        #endregion
    }
}