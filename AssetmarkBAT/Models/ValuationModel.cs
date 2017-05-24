using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AssetmarkBAT.Models
{
    public class ClientValuationModel
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
        public double VmiRiskRate { get; set; }
        public double UserPerpetualGrowthRate { get; set; }
        public int VMIScore { get; set; }

        public int ManagingYourPracticeScore { get; set; }
        public int MarketingYourBusinessScore { get; set; }
        public int EmpoweringYourTeamScore { get; set; }
        public int OptimizingYourOperationsScore { get; set; }


        //Value Optimizer variables
        public double ProfitMargin { get; set; }
        public double ProjectedAnnualGrowthRate { get; set; }

        //KPI's
        public string RecurringRevenuePerClient { get; set; }
        public string RecurringRevenuePerAdvisor { get; set; }
        public string TotalRevenuePerClient { get; set; }
        public string TotalAUMperClient { get; set; }
        public string TotalAUMperAdvisor { get; set; }
        public string ProfitPerClient { get; set; }
        public string ProfitAsPercentOfRevenut { get; set; }
        public string ClientsPerAdvisor { get; set; }
        public string RevenueAsBPSOnAssets { get; set; }

        //Valulation Ranges
        public double ValuationMin { get; set; }
        public double ValuationMax { get; set; }

        public ClientValuationModel()
        {
            VMIScore = 0;

        }
    }



    public class BenchmarksValuationModel
    {
        public List<BenchmarkGroup> PeerGroups { get; set; }

        public BenchmarksValuationModel()
        {
            PeerGroups = new List<BenchmarkGroup>
            {
                // Peer group $0 - $249K
                    new BenchmarkGroup
                        { GroupRangeMin = 0, GroupRangeMax = 250000, ValuationMin = 1751000, ValuationMax = 2497000, 
                        //Firm Financials
                        RecurringRevenue = 130333, TotalRevenue = 165333, TotalExpenses = 139350, OperatingProfit = 25983, ProjectedAnnualGrowthRate = 98.6, AUM = 30773333, ClientRelationships = 137,
                            //KPI's
                            RecRevPerClient = 2345, RecRevPerAdvisor = 130333, TotalRevPerClient = 2558, TotalAUMPerClient = 600093,
                                TotalAUMPerAdvisor = 30773333, ProfitPerClient = 299, ProfitAsPercentOfRevenue = 17.7, ClientsPerAdvisor = 137, RevenutAsPBSOnAssets = 1,
                                //VMI's
                                MYP = 180, MYB = 185, OYO = 170, EYT = 165
                    },
                    // Peer group $250K - $499K
                    new BenchmarkGroup
                        { GroupRangeMin = 250000, GroupRangeMax = 499000, ValuationMin = 1487000, ValuationMax = 1955000, 
                         //Firm Financials
                        RecurringRevenue = 353872, TotalRevenue = 414670, TotalExpenses = 245453, OperatingProfit = 169217, ProjectedAnnualGrowthRate = 25.2, AUM = 46414970, ClientRelationships = 147,
                            //KPI's
                            RecRevPerClient = 4380, RecRevPerAdvisor = 316373, TotalRevPerClient = 5227, TotalAUMPerClient = 515349,
                                TotalAUMPerAdvisor = 43081637, ProfitPerClient = 1883, ProfitAsPercentOfRevenue = 39.6, ClientsPerAdvisor = 140, RevenutAsPBSOnAssets = 1,
                                MYP = 185, MYB = 190, OYO = 185, EYT = 170
                    },
                    // Peer group $500K - $749K
                    new BenchmarkGroup
                        { GroupRangeMin = 500000, GroupRangeMax = 749000, ValuationMin = 1842000, ValuationMax = 2342000, 
                         //Firm Financials
                        RecurringRevenue = 493998, TotalRevenue = 616152, TotalExpenses = 366423, OperatingProfit = 249729, ProjectedAnnualGrowthRate = 18.7, AUM = 98935835, ClientRelationships = 150,
                            //KPI's
                            RecRevPerClient = 5452, RecRevPerAdvisor = 450347, TotalRevPerClient = 6167, TotalAUMPerClient = 1023483,
                                TotalAUMPerAdvisor = 90621951, ProfitPerClient = 2359, ProfitAsPercentOfRevenue = 39.3, ClientsPerAdvisor = 124, RevenutAsPBSOnAssets = 1,
                                MYP = 190, MYB = 200, OYO = 195, EYT = 185
                    },
                    // Peer group $750K - $999K
                    new BenchmarkGroup
                        { GroupRangeMin = 750000, GroupRangeMax = 999999, ValuationMin = 2247000, ValuationMax = 2711000, 
                         //Firm Financials
                        RecurringRevenue = 837839, TotalRevenue = 898792, TotalExpenses = 565360, OperatingProfit = 333432, ProjectedAnnualGrowthRate = 15, AUM = 101028075, ClientRelationships = 200,
                            //KPI's
                            RecRevPerClient = 7022, RecRevPerAdvisor = 662202, TotalRevPerClient = 7182, TotalAUMPerClient = 868398,
                                TotalAUMPerAdvisor = 82293029, ProfitPerClient = 3204, ProfitAsPercentOfRevenue = 37.4, ClientsPerAdvisor = 173, RevenutAsPBSOnAssets = 1,
                                MYP = 200, MYB = 225, OYO = 215, EYT = 205
                    },
                    // Peer group $1M - $3M
                     new BenchmarkGroup
                        { GroupRangeMin = 1000000, GroupRangeMax = 3000000, ValuationMin = 4815000, ValuationMax = 5609000, 
                         //Firm Financials
                        RecurringRevenue = 333432, TotalRevenue = 1675328, TotalExpenses = 985392, OperatingProfit = 689936, ProjectedAnnualGrowthRate = 14.9, AUM = 276727265, ClientRelationships = 212,
                            //KPI's
                            RecRevPerClient = 25265, RecRevPerAdvisor = 620676, TotalRevPerClient = 25801, TotalAUMPerClient = 4840186,
                                TotalAUMPerAdvisor = 94938787, ProfitPerClient = 11202, ProfitAsPercentOfRevenue = 42.8, ClientsPerAdvisor = 86, RevenutAsPBSOnAssets = 1,
                                MYP = 220, MYB = 230, OYO = 230, EYT = 220
                    }
            };
        }
    }

    public class BenchmarkGroup
    {
        //Valuation Metrics
        public double GroupRangeMin;
        public double GroupRangeMax;
        public double ValuationMin;
        public double ValuationMax;
        //Firm Financials
        public double AUM;
        public double RecurringRevenue;
        public double TotalRevenue;
        public double TotalExpenses;
        public double OperatingProfit;
        public double ProjectedAnnualGrowthRate;
        public int ClientRelationships;
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
        //VMI's
        public int MYP;
        public int MYB;
        public int OYO;
        public int EYT;
    }
}