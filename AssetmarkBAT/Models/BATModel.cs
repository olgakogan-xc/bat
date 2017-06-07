using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AssetmarkBAT.Models
{
    public class BATModel
    {      
        public bool AgreedToTerms { get; set; }
        public string UserId { get; set; }

        //Eloqua fields
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string emailAddress { get; set; }
        public string busPhone { get; set; }
        public string zipPostal { get; set; }
        public string brokerDealer1 { get; set; }
        public string EloquaId { get; set; }
        public string sFDCContactID { get; set; }
        //End of Eloqua fields
        
        //drop downs
        [Required(ErrorMessage = "Please make a selection")]
        public string Year { get; set; }
        public SelectList Years { get; set; }
       
        public int Month { get; set; }
        public SelectList Months { get; set; }

        [Required(ErrorMessage = "Please make a selection")]
        public string PracticeType { get; set; }

        public string PracticeTypeOther { get; set; }
        public SelectList PracticeTypes { get; set; }
       
        public string AffiliationMode { get; set; }
        public string AffiliationModeOther { get; set; }
        public SelectList AffiliationModes { get; set; }

        public string FirmType { get; set; }
        public string FirmTypeOther { get; set; }
        public SelectList FirmTypes { get; set; }

        public string DateStarted { get; set; }
        public string Message { get; set; }
        public string PDFPath { get; set; }
        public bool Page1Complete { get; set; }
        public bool Page2Complete { get; set; }


        [Display(Name = "Total Firm Assets Under Management")]
        public string Ff_TotalFirmAsset { get; set; }

        [Display(Name = "Non-Recurring Revenue")]    
        public string Ff_NonRecurringRevenue { get; set; }
        public string Ff_NonRecurringRevenueAnnualized { get; set; }

        [Display(Name = "Recurring Revenue")]   
        public string Ff_RecurringRevenue { get; set; }
        public string Ff_RecurringRevenueAnnualized { get; set; }

        
        public string Ff_TotalRevenue { get; set; }
        public string Ff_TotalRevenueAnnualized { get; set; }

        [Display(Name = "Direct Expenses")]             
        public string Ff_DirectExpenses { get; set; }
        public string Ff_DirectExpensesAnnualized { get; set; }

        [Display(Name = "Indirect Expenses")]        
        public string Ff_IndirecteExpenses { get; set; }
        public string Ff_IndirecteExpensesAnnualized { get; set; }

        
        public string Ff_OperatingProfit { get; set; }
        public string Ff_OperatingProfitAnnualized { get; set; }

        [Display(Name = "Projected Growth Rate")]     
        public string Ff_ProjectedGrowthRate { get; set; }

        [Display(Name = "Client Relationships")]  
        public string Ff_ClientRelationships { get; set; }

        [Display(Name = "Full-Time Equivalent Non Advisors")]  
        public string Ff_FullTimeNonAdvisors { get; set; }

        [Display(Name = "Full-Time Equivalent Advisors")]    
        public string Ff_FullTimeAdvisors { get; set; }

        [Display(Name = "New Clients")]      
        public string Ff_NewClients { get; set; }
        public string Ff_NewClientsAnnualized { get; set; }

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

        public string Vmi_Index { get; set; }       

        public ClientValuationModel ClientValuationModel { get; set; }

        public BenchmarksValuationModel BenchmarksValuationModel { get; set; }       

        public BATModel()
        {
            ClientValuationModel = new ClientValuationModel();
            BenchmarksValuationModel = new BenchmarksValuationModel();

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

            Vmi_Index = "N/A";
        }
    }
}


