using AssetmarkBAT.Models;
using AssetmarkBATDbConnector;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AssetmarkBAT.Controllers
{
    public class AssetmarkBATController : Controller
    {
        // Terms page
        public ActionResult Index()
        {
            //SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            //builder.DataSource = "am-dol.database.windows.net";
            //builder.UserID = "am_bvs";
            //builder.Password = "gE^B{rG8X/rQg+83";
            //builder.InitialCatalog = "am_bvs.database";

            ////insert
            //using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            //{
            //    connection.Open();
            //    StringBuilder sb = new StringBuilder();
            //    sb.Append("INSERT INTO am_bat (RespondentId) ");
            //    sb.Append("VALUES (@RespondentId);");
            //    String sql = sb.ToString();
            //    using (SqlCommand command = new SqlCommand(sql, connection))
            //    {
            //        command.Parameters.AddWithValue("@RespondentId", "MyTestId3");
            //        int rowsAffected = command.ExecuteNonQuery();
            //    }
            //}

            BATModel model = new BATModel();

            if (HttpContext.Request.Cookies["assetmarkBAT"] != null && !string.IsNullOrEmpty(HttpContext.Request.Cookies["assetmarkBAT"].Value))
            {
                model.UserId = HttpContext.Request.Cookies["assetmarkBAT"].Value;
            }

            using (AssetmarkBATEntities db = new AssetmarkBATEntities())
            {
                var original = db.am_bat.Find(model.UserId);

                if (original != null)
                {
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


                    model.Vmi_Man_Phase = (string.IsNullOrEmpty(original.Vmi_Man_Phase))? 5 : Convert.ToInt32(original.Vmi_Man_Phase);
                    model.Vmi_Man_Practice = (string.IsNullOrEmpty(original.Vmi_Man_Practice)) ? 5 : Convert.ToInt32(original.Vmi_Man_Practice);
                    model.Vmi_Man_Revenue = (string.IsNullOrEmpty(original.Vmi_Man_Revenue)) ? 5 : Convert.ToInt32(original.Vmi_Man_Revenue);
                    model.Vmi_Man_Track = (string.IsNullOrEmpty(original.Vmi_Man_Track)) ? 5 : Convert.ToInt32(original.Vmi_Man_Track);
                    model.Vmi_Man_Written_Plan = (string.IsNullOrEmpty(original.Vmi_Man_Written_Plan)) ? 5 : Convert.ToInt32(original.Vmi_Man_Written_Plan);

                    model.Vmi_Mar_Materials = (string.IsNullOrEmpty(original.Vmi_Mar_Materials)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Materials);
                    model.Vmi_Mar_New_Business = (string.IsNullOrEmpty(original.Vmi_Mar_New_Business)) ? 5 : Convert.ToInt32(original.Vmi_Mar_New_Business);
                    model.Vmi_Mar_Plan = (string.IsNullOrEmpty(original.Vmi_Mar_Plan)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Plan);
                    model.Vmi_Mar_Prospects = (string.IsNullOrEmpty(original.Vmi_Mar_Prospects)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Prospects);
                    model.Vmi_Mar_Value_Proposition = (string.IsNullOrEmpty(original.Vmi_Mar_Value_Proposition)) ? 5 : Convert.ToInt32(original.Vmi_Mar_Value_Proposition);

                    model.Vmi_Opt_Automate = (string.IsNullOrEmpty(original.Vmi_Opt_Automate)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Automate);
                    model.Vmi_Opt_Model = (string.IsNullOrEmpty(original.Vmi_Opt_Model)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Model);
                    model.Vmi_Opt_Procedures = (string.IsNullOrEmpty(original.Vmi_Opt_Procedures)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Procedures);
                    model.Vmi_Opt_Schedule = (string.IsNullOrEmpty(original.Vmi_Opt_Schedule)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Schedule);
                    model.Vmi_Opt_Segment = (string.IsNullOrEmpty(original.Vmi_Opt_Segment)) ? 5 : Convert.ToInt32(original.Vmi_Opt_Segment);
                    model.Vmi_Complete = original.Vmi_Complete;
                    model.Ff_Complete = original.Ff_Complete;

                    if (!string.IsNullOrEmpty(original.Ff_Complete) && !string.IsNullOrEmpty(original.Vmi_Complete))
                    {
                        return View("Report", model);
                    }
                    else if (string.IsNullOrEmpty(original.Ff_Complete))
                    {
                        InitializeDropDowns(model);

                        

                        return View("Page1Questions", model);
                    }
                    else
                    {
                        return View("Page2Questions", model);
                    }

                }
            }


                return View("Terms");
        }

        //Page one of BAT took with entry fields
        [HttpPost]
        public ActionResult Page1Questions(AgreeToTermsModel mymodel)
        {
            if (!mymodel.AgreedToTerms)
            {
                return View("Terms");
            }

            BATModel model = new BATModel();

            InitializeDropDowns(model);           


            if (HttpContext.Request.Cookies["assetmarkBAT"] == null || string.IsNullOrEmpty(HttpContext.Request.Cookies["assetmarkBAT"].Value))
            {
                model.UserId = Guid.NewGuid().ToString();

                HttpCookie assetmarkBATCookie = new HttpCookie("assetmarkBAT");
                assetmarkBATCookie.Value = model.UserId;
                assetmarkBATCookie.Expires = DateTime.Now.AddYears(10);
                HttpContext.Response.Cookies.Add(assetmarkBATCookie);
            }
            else
            {
                model.UserId = HttpContext.Request.Cookies["assetmarkBAT"].Value;

                using (AssetmarkBATEntities db = new AssetmarkBATEntities())
                {
                    var original = db.am_bat.Find(model.UserId);

                    if (original != null)
                    {
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
                        model.Ff_Complete = original.Ff_Complete;


                        model.Vmi_Man_Phase = Convert.ToInt32(original.Vmi_Man_Phase);
                        model.Vmi_Man_Practice = Convert.ToInt32(original.Vmi_Man_Practice);
                        model.Vmi_Man_Revenue = Convert.ToInt32(original.Vmi_Man_Revenue);
                        model.Vmi_Man_Track = Convert.ToInt32(original.Vmi_Man_Track);
                        model.Vmi_Man_Written_Plan = Convert.ToInt32(original.Vmi_Man_Written_Plan);

                        model.Vmi_Mar_Materials = Convert.ToInt32(original.Vmi_Mar_Materials);
                        model.Vmi_Mar_New_Business = Convert.ToInt32(original.Vmi_Mar_New_Business);
                        model.Vmi_Mar_Plan = Convert.ToInt32(original.Vmi_Mar_Plan);
                        model.Vmi_Mar_Prospects = Convert.ToInt32(original.Vmi_Mar_Prospects);
                        model.Vmi_Mar_Value_Proposition = Convert.ToInt32(original.Vmi_Mar_Value_Proposition);

                        model.Vmi_Opt_Automate = Convert.ToInt32(original.Vmi_Opt_Automate);
                        model.Vmi_Opt_Model = Convert.ToInt32(original.Vmi_Opt_Model);
                        model.Vmi_Opt_Procedures = Convert.ToInt32(original.Vmi_Opt_Procedures);
                        model.Vmi_Opt_Schedule = Convert.ToInt32(original.Vmi_Opt_Schedule);
                        model.Vmi_Opt_Segment = Convert.ToInt32(original.Vmi_Opt_Segment);
                        model.Vmi_Complete = original.Vmi_Complete;
                    }
                }
            }            

            return View("Page1Questions", model);
        }

        //Page two of BAT took with sliders
        [HttpPost]
        public ActionResult Page2Questions(BATModel model, FormCollection values)
        {
            //BATModel model = new BATModel();
            InitializeDropDowns(model);

            if (ModelState.IsValid)
            {   
                //Save to database
                using (AssetmarkBATEntities db = new AssetmarkBATEntities())
                {
                    
                    am_bat user = new am_bat()
                    {
                        //UserId = values["UserId"],
                        //Ff_TotalFirmAsset = values["Ff_TotalFirmAsset"],
                        //Ff_NonRecurringRevenue = values["Ff_NonRecurringRevenue"],
                        //Ff_RecurringRevenue = values["Ff_RecurringRevenue"]

                        UserId = model.UserId,
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
                        Ff_Complete = DateTime.Now.ToString()

                        //TODO: save remaining fields
                    };

                    //model = PopulateModel(user);
                    var original = db.am_bat.Find(user.UserId);

                    if (original != null)
                    {
                        db.Entry(original).CurrentValues.SetValues(user);
                    }
                    else
                    {
                        db.am_bat.Add(user);
                    }

                    

                    try
                    {
                        // Your code...
                        // Could also be before try if you know the exception occurs in SaveChanges

                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            string text = "";
                        }
                    }

                    //Load defaul VMI values
                    model.Vmi_Man_Written_Plan = 5;
                    model.Vmi_Man_Track = 5;
                    model.Vmi_Man_Phase = 5;
                    model.Vmi_Man_Revenue = 5;
                    model.Vmi_Man_Practice = 5;

                    model.Vmi_Mar_Prospects = 5;
                    model.Vmi_Mar_Materials = 5;
                    model.Vmi_Mar_Plan = 5;
                    model.Vmi_Mar_New_Business = 5;
                    model.Vmi_Mar_Value_Proposition = 5;

                    model.Vmi_Opt_Automate = 5;
                    model.Vmi_Opt_Procedures = 5;
                    model.Vmi_Opt_Segment = 5;
                    model.Vmi_Opt_Model = 5;
                    model.Vmi_Opt_Schedule = 5;

                    model.Vmi_Emp_Human = 5;
                    model.Vmi_Emp_Compensation = 5;
                    model.Vmi_Emp_Responsibilities = 5;
                    model.Vmi_Emp_Staff = 5;
                    model.Vmi_Emp_Emp_Retention = 5;

                }

                return View("Page2Questions", model);
            }
            else
            {
                return View("Page1Questions", model);
            }
        }

        //Page two of BAT took with sliders
        [HttpPost]
        public ActionResult Report(BATModel model, string submit)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            InitializeDropDowns(model);

            if (submit == "Next")
            {
                using (AssetmarkBATEntities db = new AssetmarkBATEntities())
                {
                    am_bat user = new am_bat()
                    {
                        //UserId = values["UserId"],
                        //Ff_TotalFirmAsset = values["Ff_TotalFirmAsset"],
                        //Ff_NonRecurringRevenue = values["Ff_NonRecurringRevenue"],
                        //Ff_RecurringRevenue = values["Ff_RecurringRevenue"]

                        UserId = model.UserId,
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
                        Vmi_Man_Phase = model.Vmi_Man_Phase.ToString(),
                        Vmi_Man_Practice = model.Vmi_Man_Practice.ToString(),
                        Vmi_Man_Revenue = model.Vmi_Man_Revenue.ToString(),
                        Vmi_Man_Track = model.Vmi_Man_Track.ToString(),
                        Vmi_Man_Written_Plan = model.Vmi_Man_Written_Plan.ToString(),
                        Vmi_Mar_Materials = model.Vmi_Mar_Materials.ToString(),
                        Vmi_Mar_New_Business = model.Vmi_Mar_New_Business.ToString(),
                        Vmi_Mar_Plan = model.Vmi_Mar_Plan.ToString(),
                        Vmi_Mar_Prospects = model.Vmi_Mar_Prospects.ToString(),
                        Vmi_Mar_Value_Proposition = model.Vmi_Mar_Value_Proposition.ToString(),
                        Vmi_Emp_Compensation = model.Vmi_Emp_Compensation.ToString(),
                        Vmi_Emp_Emp_Retention = model.Vmi_Emp_Emp_Retention.ToString(),
                        Vmi_Emp_Human = model.Vmi_Emp_Human.ToString(),
                        Vmi_Emp_Responsibilities = model.Vmi_Emp_Responsibilities.ToString(),
                        Vmi_Emp_Staff = model.Vmi_Emp_Staff.ToString(),
                        Vmi_Opt_Automate = model.Vmi_Opt_Automate.ToString(),
                        Vmi_Opt_Model = model.Vmi_Opt_Model.ToString(),
                        Vmi_Opt_Procedures = model.Vmi_Opt_Procedures.ToString(),
                        Vmi_Opt_Schedule = model.Vmi_Opt_Schedule.ToString(),
                        Vmi_Opt_Segment = model.Vmi_Opt_Segment.ToString(),
                        VmiIndex = "1000",
                        Vmi_Complete = DateTime.Today.ToString(),
                        Ff_Complete = DateTime.Today.ToString()

                        //TODO: save remaining fields
                    };

                    //model = PopulateModel(user);
                    var original = db.am_bat.Find(user.UserId);

                    if (original != null)
                    {
                        db.Entry(original).CurrentValues.SetValues(user);
                    }
                    else
                    {
                        db.am_bat.Add(user);
                    }


                    try
                    {
                        // Your code...
                        // Could also be before try if you know the exception occurs in SaveChanges

                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            string text = "";
                        }
                    }
                }

                    return View("Report", model);
            }
            else if(submit == "Previous")
            {
                return View("Page1Questions", model);
            }
            else
                return View("Page2Questions", model);
        }

      

        

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

        private BATModel PopulateModel(am_bat user)
        {
            BATModel model = new BATModel()
            {
                UserId = user.UserId,
                Ff_TotalFirmAsset = user.Ff_TotalFirmAsset,
                Ff_NonRecurringRevenue = user.Ff_NonRecurringRevenue,
                Ff_RecurringRevenue = user.Ff_RecurringRevenue
            };

            return model;
        }
    }
}