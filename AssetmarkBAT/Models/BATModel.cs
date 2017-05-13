using AssetmarkBAT.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AssetmarkBAT.Models
{
    public class BATModel
    {      
        public string UserId { get; set; }

        //drop downs
        [Required(ErrorMessage = "Please make a selection")]
        public string Year { get; set; }
        public SelectList Years { get; set; }

       
        public string Month { get; set; }
        public SelectList Months { get; set; }

        [Required(ErrorMessage = "Please make a selection")]
        public string PracticeType { get; set; }
        public SelectList PracticeTypes { get; set; }

       
        public string AffiliationMode { get; set; }
        public SelectList AffiliationModes { get; set; }

        public string FirmType { get; set; }
        public SelectList FirmTypes { get; set; }





        [Display(Name = "Total Firm Assets Under Management")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid dollar value")]
        public string Ff_TotalFirmAsset { get; set; }
        public string Ff_TotalFirmAssetAnnualized { get; set; }

        [Display(Name = "Non-Recurring Revenue")]      
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid dollar value")]
        public string Ff_NonRecurringRevenue { get; set; }
        public string Ff_NonRecurringRevenueAnnualized { get; set; }

        [Display(Name = "Recurring Revenue")]      
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid dollar value")]
        public string Ff_RecurringRevenue { get; set; }
        public string Ff_RecurringRevenueAnnualized { get; set; }

        //**********Calculated************
        public string Ff_TotalRevenue { get; set; }
        public string Ff_TotalRevenueAnnualized { get; set; }

        [Display(Name = "Direct Expenses")]       
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid dollar value")]
        public string Ff_DirectExpenses { get; set; }
        public string Ff_DirectExpensesAnnualized { get; set; }

        [Display(Name = "Indirect Expenses")]      
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid dollar value")]
        public string Ff_IndirecteExpenses { get; set; }
        public string Ff_IndirecteExpensesAnnualized { get; set; }

        //**********Calculated************
        public string Ff_OperatingProfit { get; set; }
        public string Ff_OperatingProfitAnnualized { get; set; }

        [Display(Name = "Projected Growth Rate")]      
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid dollar value")]
        public string Ff_ProjectedGrowthRate { get; set; }
        public string Ff_ProjectedGrowthRateAnnualized { get; set; }

        [Display(Name = "Client Relationships")]      
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid dollar value")]
        public string Ff_ClientRelationships { get; set; }
        public string Ff_ClientRelationshipsAnnualized { get; set; }

        [Display(Name = "Full-Time Equivalent Non Advisors")]    
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid dollar value")]
        public string Ff_FullTimeNonAdvisors { get; set; }
        public string Ff_FullTimeNonAdvisorsAnnualized { get; set; }

        [Display(Name = "Full-Time Equivalent Advisors")]       
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid dollar value")]
        public string Ff_FullTimeAdvisors { get; set; }
        public string Ff_FullTimeAdvisorsAnnualized { get; set; }

        [Display(Name = "New Clients")]      
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid dollar value")]
        public string Ff_NewClients { get; set; }
        public string Ff_NewClientsAnnualized { get; set; }

        public string Ff_Complete { get; set; }


        //Managing section
        public string Vmi_Man_Written_Plan { get; set; }    
        public string Vmi_Man_Track { get; set; }      
        public string Vmi_Man_Phase { get; set; }    
        public string Vmi_Man_Revenue { get; set; }   
        public string Vmi_Man_Practice { get; set; }

        //Marketing section
        public string Vmi_Mar_Value_Proposition { get; set; }
        public string Vmi_Mar_Materials { get; set; }
        public string Vmi_Mar_Plan { get; set; }
        public string Vmi_Mar_Prospects { get; set; }
        public string Vmi_Mar_New_Business { get; set; }

        //Optimizing section
        public string Vmi_Opt_Automate { get; set; }
        public string Vmi_Opt_Procedures { get; set; }
        public string Vmi_Opt_Segment { get; set; }
        public string Vmi_Opt_Model { get; set; }
        public string Vmi_Opt_Schedule { get; set; }

        //Empowering section
        public string Vmi_Emp_Human { get; set; }
        public string Vmi_Emp_Compensation { get; set; }
        public string Vmi_Emp_Responsibilities { get; set; }
        public string Vmi_Emp_Staff { get; set; }
        public string Vmi_Emp_Emp_Retention { get; set; }

        public string Vmi_Complete { get; set; }

        public ValuationModel BATValuationModel { get; set; }

        public BATModel()
        {
            Vmi_Emp_Compensation = null;
            Vmi_Emp_Emp_Retention = null;
            Vmi_Emp_Human = null;
            Vmi_Emp_Responsibilities = null;
            Vmi_Emp_Staff = null;

            Vmi_Man_Phase = null;
            Vmi_Man_Practice = null;
            Vmi_Man_Revenue = null;
            Vmi_Man_Track = null;
            Vmi_Man_Written_Plan = null;

            Vmi_Mar_Materials = null;
            Vmi_Mar_New_Business = null;
            Vmi_Mar_Plan = null;
            Vmi_Mar_Prospects = null;
            Vmi_Mar_Value_Proposition = null;

            Vmi_Opt_Automate = null;
            Vmi_Opt_Model = null;
            Vmi_Opt_Procedures = null;
            Vmi_Opt_Schedule = null;
            Vmi_Opt_Segment = null;
        }
    }

    public class AgreeToTermsModel
    {
        [Display(Name = "Agree to the terms of use")]
        [BooleanRequired(ErrorMessage = "Please agree to terms to proceed")]
        public bool AgreedToTerms { get; set; }

    }
}


