using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AssetmarkBAT.Models
{
    public class ValuationModel
    {
        //BAT Admin Utility Constants
        public double _TaxRate = 0.25; //25%
        public double _PerpetualGrowthRateMax = 0.05; // 5%
        public double _PerpetualGrowthRateMin = 0.03; // 3%
        public double _ValuationRiskRate = 0.14; // 14%
        public double _MaxVMIRiskRate = 0.15; //15%
        public double _MinVMIRiskRate = 0.05; //5%

        //Non-Advisor Cash Flow
        public double NonAdvisorCashFlowYear1 { get; set; }
        public double NonAdvisorCashFlowYear2 { get; set; }
        public double NonAdvisorCashFlowYear3 { get; set; }
        public double NonAdvisorCashFlowYear4 { get; set; }
        public double NonAdvisorCashFlowYear5 { get; set; }

        //Discounted Cash Flow Min
        public double DiscountedCashFlowYear1Min { get; set; }
        public double DiscountedCashFlowYear2Min { get; set; }
        public double DiscountedCashFlowYear3Min { get; set; }
        public double DiscountedCashFlowYear4Min { get; set; }
        public double DiscountedCashFlowYear5Min { get; set; }

        //Discounted Cash Flow Max
        public double DiscountedCashFlowYear1Max { get; set; }
        public double DiscountedCashFlowYear2Max { get; set; }
        public double DiscountedCashFlowYear3Max { get; set; }
        public double DiscountedCashFlowYear4Max { get; set; }
        public double DiscountedCashFlowYear5Max { get; set; }

        //Ranges
        public double DiscountedCashFlowMin { get; set; }
        public double DiscountedCashFlowMax { get; set; }
        public double NonAdvisorCashFlowMin { get; set; }
        public double NonAdvisorCashFlowMax { get; set; }

        public double PerpetualGrowthRateCashFlowMin { get; set; }
        public double PerpetualGrowthRateCashFlowMax { get; set; }


        //VMI  
        public double ValuationIndex { get; set; }
        public double VmiRiskRate { get; set; }
        public double UserPerpetualGrowthRate { get; set; }

        //Value Optimizer variables
        public double ProfitMargin { get; set; }
        public double ProjectedAnnualGrowthRate { get; set; }

        //KPI's
        public double RecurringRevenuePerClient { get; set; }
        public double RecurringRevenuePerAdvisor { get; set; }
        public double TotalRevenuePerClient { get; set; }
        public double TotalAUMperClient { get; set; }
        public double TotalAUMperAdvisor { get; set; }
        public double ProfitPerClient { get; set; }
        public double ProfitAsPercentOfRevenut { get; set; }
        public double ClientsPerAdvisor { get; set; }


    }
}