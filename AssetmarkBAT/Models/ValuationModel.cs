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

    public class BenchmarkValuationMOdel
    {
        List<BenchmarkGroup> Groups = new List<BenchmarkGroup>
        {
                new BenchmarkGroup
                    { GroupRangeMin = 0, GroupRangeMax = 250000, ValuationMin = 318000, ValuationMax = 416000,
                        RecRevPerClient = 2345, RecRevPerAdvisor = 130333, TotalRevPerClient = 2558, TotalAUMPerClient = 600093,
                            TotalAUMPerAdvisor = 30773333, ProfitPerClient = 299, ProfitAsPercentOfRevenue = 17.7, ClientsPerAdvisor = 137, RevenutAsPBSOnAssets = 1
                },
                new BenchmarkGroup
                    { GroupRangeMin = 250000, GroupRangeMax = 499000, ValuationMin = 834000, ValuationMax = 1096000,
                        RecRevPerClient = 4380, RecRevPerAdvisor = 316373, TotalRevPerClient = 5227, TotalAUMPerClient = 515349,
                            TotalAUMPerAdvisor = 43081637, ProfitPerClient = 1883, ProfitAsPercentOfRevenue = 39.6, ClientsPerAdvisor = 140, RevenutAsPBSOnAssets = 1
                },
                new BenchmarkGroup
                    { GroupRangeMin = 500000, GroupRangeMax = 749000, ValuationMin = 1011000, ValuationMax = 1282000,
                        RecRevPerClient = 5452, RecRevPerAdvisor = 450347, TotalRevPerClient = 6167, TotalAUMPerClient = 1023483,
                            TotalAUMPerAdvisor = 90621951, ProfitPerClient = 2359, ProfitAsPercentOfRevenue = 39.3, ClientsPerAdvisor = 124, RevenutAsPBSOnAssets = 1
                },






                new BenchmarkGroup
                    { GroupRangeMin = 0, GroupRangeMax = 250000, ValuationMin = 318000, ValuationMax = 416000,
                        RecRevPerClient = 2345, RecRevPerAdvisor = 130333, TotalRevPerClient = 2558, TotalAUMPerClient = 600093,
                            TotalAUMPerAdvisor = 30773333, ProfitPerClient = 299, ProfitAsPercentOfRevenue = 17.7, ClientsPerAdvisor = 137, RevenutAsPBSOnAssets = 1
                }
        };




    }

    public class BenchmarkGroup
    {
        //Valuation Metrics
        public double GroupRangeMin;
        public double GroupRangeMax;
        public double ValuationMin;
        public double ValuationMax;
        //KPI's
        public double RecRevPerClient;
        public double RecRevPerAdvisor;
        public double TotalRevPerClient;
        public double TotalAUMPerClient;
        public double TotalAUMPerAdvisor;
        public double ProfitPerClient;
        public double ProfitAsPercentOfRevenue;
        public double ClientsPerAdvisor;
        public double RevenutAsPBSOnAssets;
    }
}