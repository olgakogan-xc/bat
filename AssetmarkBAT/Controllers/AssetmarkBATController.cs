using AssetmarkBAT.Models;
using AssetmarkBATDbConnector;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;
//using System.Data.SqlClient;
using System.Linq;
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

        private string _CookieName = "assetmarkBAT";
        private string _TermsViewName = "Terms";
        private string _Page1QuestionsViewName = "Page1Questions";
        private string _Page2QuestionsViewName = "Page2Questions";
        private string _ReportViewName = "Report";
        private string _ValuationOptimizer = "ValuationOptimizer";
        private string _UserId = string.Empty;
        //public ValuationModel _ValModel = new ValuationModel();
        

        /// <summary>
        /// Action Method to initiate the BAT tool
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //FileStream htmlStream = System.IO.File.OpenRead("C:\\InputShort.html");

            byte[] byteArray = Encoding.UTF8.GetBytes("<html><body><p>Some Text Here</p><p><strong><u><font color='#A00000'>DOCUMENT FEATURES></font></u></strong></p><ul><li>Create and load PDF documents from files and streams </li><li>Save PDF files to disk and streams </li><li>Save PDF files in PDF / A - 1B format </li></ul></body></html>");

            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            MemoryStream stream = new MemoryStream(byteArray);

            //SimpleHtmlToPdf htmlToPdf = new SimpleHtmlToPdf();
            //PdfFixedDocument document = htmlToPdf.Convert(stream);
            //document.Save("C:\\output.pdf");



            BATModel model = new BATModel();
            InitializeDropDowns(model);

            if (HttpContext.Request.Cookies[_CookieName] != null && !string.IsNullOrEmpty(HttpContext.Request.Cookies[_CookieName].Value))
            {
                model.UserId = HttpContext.Request.Cookies[_CookieName].Value;
                if (PopulateModelFromDatabase(model))
                {
                    //if (!string.IsNullOrEmpty(model.Vmi_Emp_Emp_Retention))
                    //{
                    //    return View(_ReportViewName, model);
                    //}
                    ////else if (!string.IsNullOrEmpty(model.Ff_NewClients))
                    ////{
                    ////    return View(_Page2QuestionsViewName, model);
                    ////}
                    //else
                    //{
                    return View(_Page1QuestionsViewName, model);
                    //}
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




            if (HttpContext.Request.Cookies[_CookieName] == null || string.IsNullOrEmpty(HttpContext.Request.Cookies[_CookieName].Value))
            {
                model.UserId = Guid.NewGuid().ToString();
                _UserId = model.UserId;
                CreatePdf();

                HttpCookie assetmarkBATCookie = new HttpCookie(_CookieName);
                assetmarkBATCookie.Value = model.UserId;
                assetmarkBATCookie.Expires = DateTime.Now.AddYears(10);
                HttpContext.Response.Cookies.Add(assetmarkBATCookie);
            }
            else
            {
                model.UserId = HttpContext.Request.Cookies[_CookieName].Value;
                CreatePdf();
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
        /// Action method to handle user input on Page 2 (VMI)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="submit"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Report(BATModel model, string submit)
        {
            //var errors = ModelState.Values.SelectMany(v => v.Errors);

            InitializeDropDowns(model);

            if (submit == "Next")
            {
                //PopulateEntityFromModel(model);
                //CalculateVMIScore(model);
                return View(_ReportViewName, model);
            }
            else if (submit == "Previous")
            {
                return View(_Page1QuestionsViewName, model);
            }
            else
                return View(_Page2QuestionsViewName, model);
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

        

        private void PopulateEntityFromModel(BATModel model)
        {
            try
            {
                model.BATValuationModel.ProfitMargin = Convert.ToDouble(model.Ff_NonRecurringRevenue) + Convert.ToDouble(model.Ff_RecurringRevenue) - (Convert.ToDouble(model.Ff_DirectExpenses) + Convert.ToDouble(model.Ff_IndirecteExpenses));
                model.BATValuationModel.ProjectedAnnualGrowthRate = Convert.ToInt32(model.Ff_ProjectedGrowthRate);
               
                //using (AssetmarkBATEntities db = new AssetmarkBATEntities())
                //{
                //    am_bat user = new am_bat()
                //    {
                //        UserId = model.UserId,
                //        //Firm Financials
                //        Ff_TotalFirmAsset = model.Ff_TotalFirmAsset,
                //        Ff_NonRecurringRevenue = model.Ff_NonRecurringRevenue,
                //        Ff_RecurringRevenue = model.Ff_RecurringRevenue,
                //        Ff_DirectExpenses = model.Ff_DirectExpenses,
                //        Ff_IndirectExpenses = model.Ff_IndirecteExpenses,
                //        Ff_Client_Relationships = model.Ff_ClientRelationships,
                //        Ff_Fte_Advisors = model.Ff_FullTimeAdvisors,
                //        Ff_Fte_Non_Advisors = model.Ff_FullTimeNonAdvisors,
                //        Ff_New_Clients = model.Ff_NewClients,
                //        Ff_Projected_Growth = model.Ff_ProjectedGrowthRate,
                //        //VMI
                //        Vmi_Man_Phase = model.Vmi_Man_Phase,
                //        Vmi_Man_Practice = model.Vmi_Man_Practice,
                //        Vmi_Man_Revenue = model.Vmi_Man_Revenue,
                //        Vmi_Man_Track = model.Vmi_Man_Track,
                //        Vmi_Man_Written_Plan = model.Vmi_Man_Written_Plan,
                //        Vmi_Mar_Materials = model.Vmi_Mar_Materials,
                //        Vmi_Mar_New_Business = model.Vmi_Mar_New_Business,
                //        Vmi_Mar_Plan = model.Vmi_Mar_Plan,
                //        Vmi_Mar_Prospects = model.Vmi_Mar_Prospects,
                //        Vmi_Mar_Value_Proposition = model.Vmi_Mar_Value_Proposition,
                //        Vmi_Emp_Compensation = model.Vmi_Emp_Compensation,
                //        Vmi_Emp_Emp_Retention = model.Vmi_Emp_Emp_Retention,
                //        Vmi_Emp_Human = model.Vmi_Emp_Human,
                //        Vmi_Emp_Responsibilities = model.Vmi_Emp_Responsibilities,
                //        Vmi_Emp_Staff = model.Vmi_Emp_Staff,
                //        Vmi_Opt_Automate = model.Vmi_Opt_Automate,
                //        Vmi_Opt_Model = model.Vmi_Opt_Model,
                //        Vmi_Opt_Procedures = model.Vmi_Opt_Procedures,
                //        Vmi_Opt_Schedule = model.Vmi_Opt_Schedule,
                //        Vmi_Opt_Segment = model.Vmi_Opt_Segment,
                //        VmiIndex = "1000"
                //    };

                //    var original = db.am_bat.Find(user.UserId);

                //    if (original != null)
                //    {
                //        db.Entry(original).CurrentValues.SetValues(user);
                //    }
                //    else
                //    {
                //        db.am_bat.Add(user);
                //    }

                //    db.SaveChanges();
                //}



            }
            catch (Exception ex)
            {

            }
        }

        private bool PopulateModelFromDatabase(BATModel model)
        {
            //using (AssetmarkBATEntities db = new AssetmarkBATEntities())
            //{
            //    var original = db.am_bat.Find(model.UserId);

            //    if (original != null)
            //    {
            //        //Firm Financials
            //        model.Ff_TotalFirmAsset = original.Ff_TotalFirmAsset;
            //        model.Ff_NonRecurringRevenue = original.Ff_NonRecurringRevenue;
            //        model.Ff_RecurringRevenue = original.Ff_RecurringRevenue;
            //        model.Ff_DirectExpenses = original.Ff_DirectExpenses;
            //        model.Ff_IndirecteExpenses = original.Ff_IndirectExpenses;
            //        model.Ff_ProjectedGrowthRate = original.Ff_Projected_Growth;
            //        model.Ff_ClientRelationships = original.Ff_Client_Relationships;
            //        model.Ff_FullTimeNonAdvisors = original.Ff_Fte_Non_Advisors;
            //        model.Ff_FullTimeAdvisors = original.Ff_Fte_Advisors;
            //        model.Ff_NewClients = original.Ff_New_Clients;

            //        //// VMI OLD CODE
            //        //model.Vmi_Man_Phase = (string.IsNullOrEmpty(original.Vmi_Man_Phase)) ? 5 : Convert.ToInt32(original.Vmi_Man_Phase);
            //        //model.Vmi_Man_Practice = (string.IsNullOrEmpty(original.Vmi_Man_Practice)) ? 5 : Convert.ToInt32(original.Vmi_Man_Practice);
            //        //model.Vmi_Man_Revenue = (string.IsNullOrEmpty(original.Vmi_Man_Revenue)) ? 5 : Convert.ToInt32(original.Vmi_Man_Revenue);
            //        //model.Vmi_Man_Track = (string.IsNullOrEmpty(original.Vmi_Man_Track)) ? 5 : Convert.ToInt32(original.Vmi_Man_Track);
            //        //model.Vmi_Man_Written_Plan = (string.IsNullOrEmpty(original.Vmi_Man_Written_Plan)) ? 5 : Convert.ToInt32(original.Vmi_Man_Written_Plan);

            //        //model.Vmi_Mar_Materials = (string.IsNullOrEmpty(original.Vmi_Mar_Materials)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Materials);
            //        //model.Vmi_Mar_New_Business = (string.IsNullOrEmpty(original.Vmi_Mar_New_Business)) ? 5 : Convert.ToInt32(original.Vmi_Mar_New_Business);
            //        //model.Vmi_Mar_Plan = (string.IsNullOrEmpty(original.Vmi_Mar_Plan)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Plan);
            //        //model.Vmi_Mar_Prospects = (string.IsNullOrEmpty(original.Vmi_Mar_Prospects)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Prospects);
            //        //model.Vmi_Mar_Value_Proposition = (string.IsNullOrEmpty(original.Vmi_Mar_Value_Proposition)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Value_Proposition);

            //        //model.Vmi_Opt_Automate = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Automate);
            //        //model.Vmi_Opt_Model = (string.IsNullOrEmpty(original.Vmi_Opt_Model)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Model);
            //        //model.Vmi_Opt_Procedures = (string.IsNullOrEmpty(original.Vmi_Opt_Procedures)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Procedures);
            //        //model.Vmi_Opt_Schedule = (string.IsNullOrEmpty(original.Vmi_Opt_Schedule)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Schedule);
            //        //model.Vmi_Opt_Segment = (string.IsNullOrEmpty(original.Vmi_Opt_Segment)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Segment);

            //        //model.Vmi_Emp_Compensation = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Emp_Compensation);
            //        //model.Vmi_Emp_Emp_Retention = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Emp_Emp_Retention);
            //        //model.Vmi_Emp_Human = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Emp_Human);
            //        //model.Vmi_Emp_Responsibilities = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Emp_Responsibilities);
            //        //model.Vmi_Emp_Staff = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Emp_Staff);

            //        //VMI END OF OLD CODE

            //        model.Vmi_Man_Phase = original.Vmi_Man_Phase;
            //        model.Vmi_Man_Practice = original.Vmi_Man_Practice;
            //        model.Vmi_Man_Revenue = original.Vmi_Man_Revenue;
            //        model.Vmi_Man_Track = original.Vmi_Man_Track;
            //        model.Vmi_Man_Written_Plan = original.Vmi_Man_Written_Plan;

            //        model.Vmi_Mar_Materials = original.Vmi_Mar_Materials;
            //        model.Vmi_Mar_New_Business = original.Vmi_Mar_New_Business;
            //        model.Vmi_Mar_Plan = original.Vmi_Mar_Plan;
            //        model.Vmi_Mar_Prospects = original.Vmi_Mar_Prospects;
            //        model.Vmi_Mar_Value_Proposition = original.Vmi_Mar_Value_Proposition;

            //        model.Vmi_Opt_Automate = original.Vmi_Opt_Automate;
            //        model.Vmi_Opt_Model = original.Vmi_Opt_Model;
            //        model.Vmi_Opt_Procedures = original.Vmi_Opt_Procedures;
            //        model.Vmi_Opt_Schedule = original.Vmi_Opt_Schedule;
            //        model.Vmi_Opt_Segment = original.Vmi_Opt_Segment;

            //        model.Vmi_Emp_Compensation = original.Vmi_Emp_Compensation;
            //        model.Vmi_Emp_Emp_Retention = original.Vmi_Emp_Emp_Retention;
            //        model.Vmi_Emp_Human = original.Vmi_Emp_Human;
            //        model.Vmi_Emp_Responsibilities = original.Vmi_Emp_Responsibilities;
            //        model.Vmi_Emp_Staff = original.Vmi_Emp_Staff;

            //        return true;
            //    }

                return false;
            //}
        }

        private void CreatePdf()
        {
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

            page.Graphics.DrawLine(new PdfPen(), new PdfPoint(50, 70), new PdfPoint(50, 700));
            page.Graphics.DrawLine(new PdfPen(), new PdfPoint(50, 70), new PdfPoint(500, 700));



            page.Graphics.DrawRectangle(backgroundBrush, 20, 20, 500, 150);
            page.Graphics.DrawRectangle(darkBlueBrush, 50, 60, 50, 25);

            page.Graphics.DrawString("Valuation Range", helvetica, textBrush, 50, 35);

            string imagePath = HttpContext.Server.MapPath(@"~\UserPDF\" + "Lock.png");
            PdfImage lockImage = new PdfImage(imagePath);
            page.Graphics.DrawImage(lockImage, 50, 100, 25, 25);
            string path = HttpContext.Server.MapPath(@"~\UserPDF\" + _UserId + ".pdf");
            document.Save(path);
            //PdfFile.Save(doc, HttpContext.Current.Server.MapPath(@"~\UserPDF\" + id + ".pdf"));


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

    //public class SimpleHtmlToPdf
    //{
    //    // Stack of fonts
    //    private Stack<PdfFont> fonts = new Stack<PdfFont>();

    //    /// <summary>
    //    /// Gets the active font.
    //    /// </summary>
    //    public PdfFont ActiveFont
    //    {
    //        get { return fonts.Peek(); }
    //    }

    //    private Stack<PdfBrush> textColors = new Stack<PdfBrush>();
    //    /// <summary>
    //    /// Gets the active text color.
    //    /// </summary>
    //    public PdfBrush ActiveTextColor
    //    {
    //        get { return textColors.Peek(); }
    //    }

    //    /// <summary>
    //    /// Converts simple XHTML code to a PDF document.
    //    /// </summary>
    //    /// <param name="html"></param>
    //    /// <returns></returns>
    //    public PdfFixedDocument Convert(Stream html)
    //    {
    //        PdfFixedDocument document = new PdfFixedDocument();

    //        PdfFormattedContent fc = ConvertHtmlToFormattedContent(html);
    //        DrawFormattedContent(document, fc);

    //        return document;
    //    }

    //    /// <summary>
    //    /// Converts simple XHTML to a formatted content object.
    //    /// </summary>
    //    /// <param name="html"></param>
    //    /// <returns></returns>
    //    private PdfFormattedContent ConvertHtmlToFormattedContent(Stream html)
    //    {
    //        PdfFormattedContent fc = new PdfFormattedContent();
    //        PdfFormattedParagraph currentParagraph = null;
    //        PdfUriAction currentLinkAction = null;
    //        PdfFormattedTextBlock bullet = null;

    //        // Create a default font.
    //        fonts.Push(new PdfStandardFont(PdfStandardFontFace.Helvetica, 10));
    //        // Create a default text color.
    //        textColors.Push(new PdfBrush(PdfRgbColor.Black));

    //        System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(html);
    //        xmlReader.MoveToContent();

    //        while (xmlReader.Read())
    //        {
    //            switch (xmlReader.NodeType)
    //            {
    //                case System.Xml.XmlNodeType.Element:
    //                    switch (xmlReader.Name)
    //                    {
    //                        case "p":
    //                            currentParagraph = new PdfFormattedParagraph();
    //                            currentParagraph.SpacingBefore = 3;
    //                            currentParagraph.SpacingAfter = 3;
    //                            fc.Paragraphs.Add(currentParagraph);
    //                            break;
    //                        case "br":
    //                            if (currentParagraph.Blocks.Count > 0)
    //                            {
    //                                PdfFormattedTextBlock textBlock =
    //                                    currentParagraph.Blocks[currentParagraph.Blocks.Count - 1] as PdfFormattedTextBlock;
    //                                textBlock.Text = textBlock.Text + "\r\n";
    //                            }
    //                            else
    //                            {
    //                                PdfFormattedTextBlock textBlock = new PdfFormattedTextBlock("\r\n", ActiveFont);
    //                                currentParagraph.Blocks.Add(textBlock);
    //                            }
    //                            break;
    //                        case "a":
    //                            while (xmlReader.MoveToNextAttribute())
    //                            {
    //                                if (xmlReader.Name == "href")
    //                                {
    //                                    currentLinkAction = new PdfUriAction();
    //                                    currentLinkAction.URI = xmlReader.Value;
    //                                }
    //                            }
    //                            break;
    //                        case "font":
    //                            while (xmlReader.MoveToNextAttribute())
    //                            {
    //                                if (xmlReader.Name == "color")
    //                                {
    //                                    PdfBrush color = ActiveTextColor;
    //                                    string colorCode = xmlReader.Value;
    //                                    // #RRGGBB
    //                                    if (colorCode.StartsWith("#") && (colorCode.Length == 7))
    //                                    {
    //                                        byte r = byte.Parse(colorCode.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
    //                                        byte g = byte.Parse(colorCode.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
    //                                        byte b = byte.Parse(colorCode.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
    //                                        color = new PdfBrush(new PdfRgbColor(r, g, b));
    //                                    }

    //                                    textColors.Push(color);
    //                                }
    //                            }
    //                            break;
    //                        case "ul":
    //                            bullet = new PdfFormattedTextBlock("\x95 ", ActiveFont);
    //                            break;
    //                        case "li":
    //                            currentParagraph = new PdfFormattedParagraph();
    //                            currentParagraph.SpacingBefore = 3;
    //                            currentParagraph.SpacingAfter = 3;
    //                            currentParagraph.Bullet = bullet;
    //                            currentParagraph.LeftIndentation = 18;
    //                            fc.Paragraphs.Add(currentParagraph);
    //                            break;
    //                        case "b":
    //                        case "strong":
    //                            PdfFont boldFont = CopyFont(ActiveFont);
    //                            boldFont.Bold = true;
    //                            fonts.Push(boldFont);
    //                            break;
    //                        case "i":
    //                        case "em":
    //                            PdfFont italicFont = CopyFont(ActiveFont);
    //                            italicFont.Italic = true;
    //                            fonts.Push(italicFont);
    //                            break;
    //                        case "u":
    //                            PdfFont underlineFont = CopyFont(ActiveFont);
    //                            underlineFont.Underline = true;
    //                            fonts.Push(underlineFont);
    //                            break;
    //                    }
    //                    break;
    //                case System.Xml.XmlNodeType.EndElement:
    //                    switch (xmlReader.Name)
    //                    {
    //                        case "a":
    //                            currentLinkAction = null;
    //                            break;
    //                        case "ul":
    //                            bullet = null;
    //                            break;
    //                        case "b":
    //                        case "strong":
    //                        case "i":
    //                        case "em":
    //                        case "u":
    //                            fonts.Pop();
    //                            break;
    //                        case "font":
    //                            textColors.Pop();
    //                            break;
    //                    }
    //                    break;
    //                case System.Xml.XmlNodeType.Text:
    //                    string text = xmlReader.Value;
    //                    // Remove spaces from text that do not have meaning in HTML.
    //                    text = text.Replace("\r", "");
    //                    text = text.Replace("\n", "");
    //                    text = text.Replace("\t", " ");
    //                    PdfFormattedTextBlock block = new PdfFormattedTextBlock(text, ActiveFont);
    //                    block.TextColor = ActiveTextColor;
    //                    if (currentLinkAction != null)
    //                    {
    //                        block.Action = currentLinkAction;
    //                        // Make the links blue.
    //                        block.TextColor = new PdfBrush(PdfRgbColor.Blue);
    //                    }
    //                    currentParagraph.Blocks.Add(block);
    //                    break;
    //            }
    //        }



    //        return fc;
    //    }



    //    /// <summary>
    //    /// Creates a bold copy of the given font.
    //    /// </summary>
    //    /// <param name="font"></param>
    //    /// <returns></returns>
    //    private PdfFont CopyFont(PdfFont font)
    //    {
    //        PdfFont copy = null;
    //        PdfStandardFont standardFont = font as PdfStandardFont;
    //        if (standardFont != null)
    //        {
    //            copy = new PdfStandardFont(standardFont);
    //        }

    //        return copy;
    //    }

    //    /// <summary>
    //    /// Draws the formatted content on document's pages.
    //    /// </summary>
    //    /// <param name="document"></param>
    //    /// <param name="fc"></param>
    //    private void DrawFormattedContent(PdfFixedDocument document, PdfFormattedContent fc)
    //    {
    //        double leftMargin, topMargin, rightMargin, bottomMargin;
    //        leftMargin = topMargin = rightMargin = bottomMargin = 36;

    //        PdfPage page = document.Pages.Add();
    //        PdfFormattedContent fragment = fc.SplitByBox(page.Width - leftMargin - rightMargin, page.Height - topMargin - bottomMargin);
    //        while (fragment != null)
    //        {
    //            page.Graphics.DrawFormattedContent(fragment,
    //                leftMargin, topMargin, page.Width - leftMargin - rightMargin, page.Height - topMargin - bottomMargin);
    //            page.Graphics.CompressAndClose();

    //            fragment = fc.SplitByBox(page.Width - leftMargin - rightMargin, page.Height - topMargin - bottomMargin);
    //            if (fragment != null)
    //            {
    //                page = document.Pages.Add();
    //            }
    //        }
    //    }


    //}
}