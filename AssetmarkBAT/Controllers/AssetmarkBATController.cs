using AssetmarkBAT.Models;
using AssetmarkBATDbConnector;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
//using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AssetmarkBAT.Controllers
{
    public class AssetmarkBATController : Controller
    {
        private string _CookieName = "assetmarkBAT";
        private string _TermsViewName = "Terms";
        private string _Page1QuestionsViewName = "Page1Questions";
        private string _Page2QuestionsViewName = "Page2Questions";
        private string _ReportViewName = "Report";

        /// <summary>
        /// Action Method to initiate the BAT tool
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            BATModel model = new BATModel();
            InitializeDropDowns(model);

            if (HttpContext.Request.Cookies[_CookieName] != null && !string.IsNullOrEmpty(HttpContext.Request.Cookies[_CookieName].Value))
            {
                model.UserId = HttpContext.Request.Cookies[_CookieName].Value;
                if (PopulateModelFromDatabase(model))
                {
                    if (!string.IsNullOrEmpty(model.Vmi_Emp_Emp_Retention))
                    {
                        return View(_ReportViewName, model);
                    }
                    else if (!string.IsNullOrEmpty(model.Ff_NewClients))
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

                HttpCookie assetmarkBATCookie = new HttpCookie(_CookieName);
                assetmarkBATCookie.Value = model.UserId;
                assetmarkBATCookie.Expires = DateTime.Now.AddYears(10);
                HttpContext.Response.Cookies.Add(assetmarkBATCookie);
            }
            else
            {
                model.UserId = HttpContext.Request.Cookies[_CookieName].Value;
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
            

            if (ModelState.IsValid)
            {
                if (submit == "Save Your Inputs")
                {
                    PopulateEntityFromModel(model);
                    return View(_Page1QuestionsViewName, model);
                }
                else
                {
                    PopulateModelFromDatabase(model);

                    if(string.IsNullOrEmpty(model.Vmi_Man_Written_Plan))
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
            }
            else
            {
                return View(_Page1QuestionsViewName, model);
            }
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
                PopulateEntityFromModel(model);  
                return View(_ReportViewName, model);
            }
            else if (submit == "Previous")
            {
                return View(_Page1QuestionsViewName, model);
            }
            else
                return View(_Page2QuestionsViewName, model);
        }

        #region PrivateMethods

        private void InitializeDropDowns(BATModel batModel)
        {
            //years

            List<SelectListItem> years = new List<SelectListItem>
            {
                new SelectListItem { Text = "YTD", Value = "YTD" },
                new SelectListItem { Text = "Jan to Dec 2016", Value = "2016" },
                new SelectListItem { Text = "Jan to Dec 2015", Value = "2015" },
                new SelectListItem { Text = "Jan to Dec 2014", Value = "2014" }
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
            using (AssetmarkBATEntities db = new AssetmarkBATEntities())
            {
                am_bat user = new am_bat()
                {
                    UserId = model.UserId,
                    //Firm Financials
                    Ff_TotalFirmAsset = model.Ff_TotalFirmAsset,
                    Ff_NonRecurringRevenue = model.Ff_NonRecurringRevenue,
                    Ff_RecurringRevenue = model.Ff_RecurringRevenue,
                    Ff_DirectExpenses = model.Ff_DirectExpenses,
                    Ff_IndirectExpenses = model.Ff_IndirecteExpenses,
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
                    VmiIndex = "1000"
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

                    //// VMI
                    //model.Vmi_Man_Phase = (string.IsNullOrEmpty(original.Vmi_Man_Phase)) ? 5 : Convert.ToInt32(original.Vmi_Man_Phase);
                    //model.Vmi_Man_Practice = (string.IsNullOrEmpty(original.Vmi_Man_Practice)) ? 5 : Convert.ToInt32(original.Vmi_Man_Practice);
                    //model.Vmi_Man_Revenue = (string.IsNullOrEmpty(original.Vmi_Man_Revenue)) ? 5 : Convert.ToInt32(original.Vmi_Man_Revenue);
                    //model.Vmi_Man_Track = (string.IsNullOrEmpty(original.Vmi_Man_Track)) ? 5 : Convert.ToInt32(original.Vmi_Man_Track);
                    //model.Vmi_Man_Written_Plan = (string.IsNullOrEmpty(original.Vmi_Man_Written_Plan)) ? 5 : Convert.ToInt32(original.Vmi_Man_Written_Plan);

                    //model.Vmi_Mar_Materials = (string.IsNullOrEmpty(original.Vmi_Mar_Materials)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Materials);
                    //model.Vmi_Mar_New_Business = (string.IsNullOrEmpty(original.Vmi_Mar_New_Business)) ? 5 : Convert.ToInt32(original.Vmi_Mar_New_Business);
                    //model.Vmi_Mar_Plan = (string.IsNullOrEmpty(original.Vmi_Mar_Plan)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Plan);
                    //model.Vmi_Mar_Prospects = (string.IsNullOrEmpty(original.Vmi_Mar_Prospects)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Prospects);
                    //model.Vmi_Mar_Value_Proposition = (string.IsNullOrEmpty(original.Vmi_Mar_Value_Proposition)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Value_Proposition);

                    //model.Vmi_Opt_Automate = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Automate);
                    //model.Vmi_Opt_Model = (string.IsNullOrEmpty(original.Vmi_Opt_Model)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Model);
                    //model.Vmi_Opt_Procedures = (string.IsNullOrEmpty(original.Vmi_Opt_Procedures)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Procedures);
                    //model.Vmi_Opt_Schedule = (string.IsNullOrEmpty(original.Vmi_Opt_Schedule)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Schedule);
                    //model.Vmi_Opt_Segment = (string.IsNullOrEmpty(original.Vmi_Opt_Segment)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Segment);

                    //model.Vmi_Emp_Compensation = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Emp_Compensation);
                    //model.Vmi_Emp_Emp_Retention = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Emp_Emp_Retention);
                    //model.Vmi_Emp_Human = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Emp_Human);
                    //model.Vmi_Emp_Responsibilities = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Emp_Responsibilities);
                    //model.Vmi_Emp_Staff = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Emp_Staff);

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

        #endregion
    }
}