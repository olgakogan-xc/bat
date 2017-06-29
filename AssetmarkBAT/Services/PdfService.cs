using AssetmarkBAT.Models;
using AssetmarkBAT.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;

namespace AssetmarkBAT.Services
{
    public class PdfService
    {
        private Helpers _Helpers = new Helpers();

        // Fonts and Brushes
        PdfStandardFont helvetica = new PdfStandardFont(PdfStandardFontFace.Helvetica, 8);
        PdfStandardFont helveticaBold = new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 8.5);

        PdfBrush textBrush = new PdfBrush((PdfRgbColor.Black));
        PdfBrush whiteBrush = new PdfBrush((PdfRgbColor.White));
        PdfBrush grayBrush = new PdfBrush((PdfRgbColor.Gray));
        PdfBrush blackBrush = new PdfBrush(PdfRgbColor.Black);
        PdfBrush textBlueBrush = new PdfBrush((new PdfRgbColor(5, 79, 124)));

        private void DrawVMIPage(PdfPage page, int pageNumber, int decrement, string str1, string str2, string str3, string str4, string str5, BATModel model)
        {
            PdfImage slider = LoadImage(HttpContext.Current.Server.MapPath(@"~\Images\slider.png"));             

            double value1 = _Helpers.ConvertToDouble(str1);
            double value2 = _Helpers.ConvertToDouble(str2);
            double value3 = _Helpers.ConvertToDouble(str3);
            double value4 = _Helpers.ConvertToDouble(str4);
            double value5 = _Helpers.ConvertToDouble(str5);

            double yDifference = 80;
            double xDifference = 25;
            double startingY = 132 - decrement;

            //First slider
            double x = 179 + (xDifference * value1) - 7;
            double y = startingY;
            page.Graphics.DrawImage(slider, x, y, 15, 27);

            //Second slider
            x = 179 + (xDifference * value2) - 7;
            y = startingY + yDifference;
            page.Graphics.DrawImage(slider, x, y, 15, 27);

            //Third slider
            x = 179 + (xDifference * value3) - 7;
            y = startingY + (2 * yDifference);
            page.Graphics.DrawImage(slider, x, y, 15, 27);

            //Fourth slider
            x = 179 + (xDifference * value4) - 7;
            y = startingY + (3 * yDifference);
            page.Graphics.DrawImage(slider, x, y, 15, 27);

            //Fifth slider
            x = 179 + (xDifference * value5) - 7;
            y = startingY + (4 * yDifference);
            page.Graphics.DrawImage(slider, x, y, 15, 27);

            page.Graphics.DrawString(5 * (value1 + value2 + value3 + value4 + value5) + " out of 250", new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 10), textBrush, 490, (pageNumber == 4 || pageNumber == 5 || pageNumber == 6)? 463 : 516 - decrement);

            //Draw grand totals
            if(pageNumber == 6)
            {
                model.ClientValuationModel.ManagingYourPracticeScore = (Convert.ToInt32(model.Vmi_Man_Written_Plan) + Convert.ToInt32(model.Vmi_Man_Track) + Convert.ToInt32(model.Vmi_Man_Phase) + Convert.ToInt32(model.Vmi_Man_Revenue) + Convert.ToInt32(model.Vmi_Man_Practice)) * 5;
                model.ClientValuationModel.MarketingYourBusinessScore = (Convert.ToInt32(model.Vmi_Mar_Value_Proposition) + Convert.ToInt32(model.Vmi_Mar_Materials) + Convert.ToInt32(model.Vmi_Mar_Plan) + Convert.ToInt32(model.Vmi_Mar_Prospects) + Convert.ToInt32(model.Vmi_Mar_New_Business)) * 5;
                model.ClientValuationModel.EmpoweringYourTeamScore = (Convert.ToInt32(model.Vmi_Emp_Human) + Convert.ToInt32(model.Vmi_Emp_Compensation) + Convert.ToInt32(model.Vmi_Emp_Responsibilities) + Convert.ToInt32(model.Vmi_Emp_Staff) + Convert.ToInt32(model.Vmi_Emp_Emp_Retention)) * 5;
                model.ClientValuationModel.OptimizingYourOperationsScore = (Convert.ToInt32(model.Vmi_Opt_Automate) + Convert.ToInt32(model.Vmi_Opt_Procedures) + Convert.ToInt32(model.Vmi_Opt_Segment) + Convert.ToInt32(model.Vmi_Opt_Model) + Convert.ToInt32(model.Vmi_Opt_Schedule)) * 5;

                page.Graphics.DrawString(model.ClientValuationModel.ManagingYourPracticeScore.ToString(), new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 10), textBrush, 490, 505);
                page.Graphics.DrawString(model.ClientValuationModel.MarketingYourBusinessScore.ToString(), new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 10), textBrush, 490, 525);
                page.Graphics.DrawString(model.ClientValuationModel.OptimizingYourOperationsScore.ToString(), new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 10), textBrush, 490, 545);
                page.Graphics.DrawString(model.ClientValuationModel.EmpoweringYourTeamScore.ToString(), new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 10), textBrush, 490, 565);

                page.Graphics.DrawString(model.Vmi_Index + " out of 1000", new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 10), textBrush, 490, 618);
            }
        }

        public static PdfPngImage LoadImage(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
                return new PdfPngImage(stream);
        }

        public void DrawFirmFinancialsPage(PdfPage page, BATModel model)
        {
            //Time Period Column
            int column1Left = 300;
            page.Graphics.DrawString(GetTimeRange(model), helveticaBold, blackBrush, column1Left, 132);            

            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_TotalFirmAsset) ? (_Helpers.ConvertToDouble(model.Ff_TotalFirmAsset)).ToString("C0") : "N/A",
                new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column1Left, 168);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_NonRecurringRevenue) ? (_Helpers.ConvertToDouble(model.Ff_NonRecurringRevenue)).ToString("C0") : "N/A",
                new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column1Left, 188);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_RecurringRevenue) ? (_Helpers.ConvertToDouble(model.Ff_RecurringRevenue)).ToString("C0") : "N/A",
                new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column1Left, 208);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_TotalRevenue) ? (_Helpers.ConvertToDouble(model.Ff_TotalRevenue)).ToString("C0") : "N/A",
                new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 10), textBrush, column1Left, 228);


            //Expense
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_DirectExpenses) ? (_Helpers.ConvertToDouble(model.Ff_DirectExpenses)).ToString("C0") : "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column1Left, 248);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_IndirecteExpenses) ? (_Helpers.ConvertToDouble(model.Ff_IndirecteExpenses)).ToString("C0") : "N/A",
                new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column1Left, 268);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_OperatingProfit) ? (_Helpers.ConvertToDouble(model.Ff_OperatingProfit)).ToString("C0") : "N/A",
                new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 10), textBrush, column1Left, 288);
          
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate) ? (_Helpers.ConvertToDouble(model.Ff_ProjectedGrowthRate)).ToString() + " %": "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column1Left, 308);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_ClientRelationships) ? (_Helpers.ConvertToDouble(model.Ff_ClientRelationships)).ToString() : "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column1Left, 328);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_FullTimeNonAdvisors) ? (_Helpers.ConvertToDouble(model.Ff_FullTimeNonAdvisors)).ToString() : "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column1Left, 348);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_FullTimeAdvisors) ? (_Helpers.ConvertToDouble(model.Ff_FullTimeAdvisors)).ToString() : "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column1Left, 368);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_NewClients) ? (_Helpers.ConvertToDouble(model.Ff_NewClients)).ToString() : "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column1Left, 388);



            //Annualized Column
            int column2Left = 470;

            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_TotalFirmAsset) ? (_Helpers.ConvertToDouble(model.Ff_TotalFirmAsset)).ToString("C0") : "N/A", 
                new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column2Left, 168);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_NonRecurringRevenueAnnualized) ? (_Helpers.ConvertToDouble(model.Ff_NonRecurringRevenueAnnualized)).ToString("C0") : "N/A", 
                new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column2Left, 188);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_RecurringRevenueAnnualized) ? (_Helpers.ConvertToDouble(model.Ff_RecurringRevenueAnnualized)).ToString("C0") : "N/A",
                new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column2Left, 208);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_TotalRevenueAnnualized) ? (_Helpers.ConvertToDouble(model.Ff_TotalRevenueAnnualized)).ToString("C0") : "N/A",
                new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 10), textBrush, column2Left, 228);

            //Expense
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_DirectExpensesAnnualized) ? (_Helpers.ConvertToDouble(model.Ff_DirectExpensesAnnualized)).ToString("C0") : "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column2Left, 248);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_IndirecteExpensesAnnualized) ? (_Helpers.ConvertToDouble(model.Ff_IndirecteExpensesAnnualized)).ToString("C0") : "N/A",
                new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column2Left, 268);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_OperatingProfitAnnualized) ? (_Helpers.ConvertToDouble(model.Ff_OperatingProfitAnnualized)).ToString("C0") : "N/A",
                new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 10), textBrush, column2Left, 288);
           
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate) ? (_Helpers.ConvertToDouble(model.Ff_ProjectedGrowthRate)).ToString() + " %" : "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column2Left, 308);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_ClientRelationships) ? (_Helpers.ConvertToDouble(model.Ff_ClientRelationships)).ToString() : "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column2Left, 328);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_FullTimeNonAdvisors) ? (_Helpers.ConvertToDouble(model.Ff_FullTimeNonAdvisors)).ToString() : "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column2Left, 348);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_FullTimeAdvisors) ? (_Helpers.ConvertToDouble(model.Ff_FullTimeAdvisors)).ToString() : "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column2Left, 368);
            page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_NewClientsAnnualized) ? (_Helpers.ConvertToDouble(model.Ff_NewClientsAnnualized)).ToString() : "N/A",
               new PdfStandardFont(PdfStandardFontFace.Helvetica, 10), textBrush, column2Left, 388);
        }


        public void DrawPdf(BATModel model)
        {
            try
            {
                PdfFixedDocument document = Load(HttpContext.Current.Server.MapPath(@"~\UserPDF\PdfTemplateLong.pdf"));
                PdfPage page1 = document.Pages[0];
                PdfPage page2 = document.Pages[1];
                DrawFirmFinancialsPage(page2, model);

                //VMI pages
                PdfPage page3 = document.Pages[2];
                DrawVMIPage(page3, 1, 0, model.Vmi_Man_Written_Plan, model.Vmi_Man_Track, model.Vmi_Man_Phase, model.Vmi_Man_Revenue, model.Vmi_Man_Practice, null);

                double width = page3.Width;

                PdfPage page4 = document.Pages[3];
                DrawVMIPage(page4, 4, 63, model.Vmi_Mar_Value_Proposition, model.Vmi_Mar_Materials, model.Vmi_Mar_Plan, model.Vmi_Mar_Prospects, model.Vmi_Mar_New_Business, null);

                PdfPage page5 = document.Pages[4];
                //DrawVMIPage(page5, 5, 63, model.Vmi_Opt_Automate, model.Vmi_Opt_Model, model.Vmi_Opt_Procedures, model.Vmi_Opt_Schedule, model.Vmi_Opt_Segment, null);
                DrawVMIPage(page5, 5, 63, model.Vmi_Opt_Automate, model.Vmi_Opt_Procedures, model.Vmi_Opt_Segment, model.Vmi_Opt_Model, model.Vmi_Opt_Schedule, null);

                PdfPage page6 = document.Pages[5];
                DrawVMIPage(page6, 6, 63, model.Vmi_Emp_Human, model.Vmi_Emp_Compensation, model.Vmi_Emp_Responsibilities, model.Vmi_Emp_Staff, model.Vmi_Emp_Emp_Retention, model);


                string timeRange = "Previous Year";

                if (!model.Year.ToLower().Contains("previous"))
                {
                    timeRange = "YTD " + DateTime.Now.Year;
                }

                page1.Graphics.DrawString(model.firstName + " " + model.lastName, new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 16), textBlueBrush, 21, 20);
                page1.Graphics.DrawString(timeRange + ", " + "Created on " + DateTime.Now.ToString("d"), new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 12.5), textBlueBrush, 370, 70);


                model.BenchmarksValuationModel = new BenchmarksValuationModel();
                BenchmarkGroup peerGroup = model.BenchmarksValuationModel.PeerGroups.FirstOrDefault(p => _Helpers.ConvertToDouble(model.Ff_TotalRevenueAnnualized) > p.GroupRangeMin && _Helpers.ConvertToDouble(model.Ff_TotalRevenueAnnualized) < p.GroupRangeMax);

                if (peerGroup == null && _Helpers.ConvertToDouble(model.Ff_TotalRevenueAnnualized) > 0)
                {
                    peerGroup = model.BenchmarksValuationModel.PeerGroups.Last();
                }
                else if (peerGroup == null && _Helpers.ConvertToDouble(model.Ff_TotalRevenueAnnualized) == 0)
                {
                    peerGroup = model.BenchmarksValuationModel.PeerGroups.First();
                }

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

                page1.Graphics.DrawString(group, helvetica, blackBrush, 500, 94);

                //============================================================== Firm Financials Table =============================================================
                page1.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_TotalFirmAsset) ? (_Helpers.ConvertToDouble(model.Ff_TotalFirmAsset)).ToString("C0") : "N/A", helvetica, blackBrush, 170, 128);
                page1.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_ClientRelationships) ? model.Ff_ClientRelationships : "N/A", helvetica, blackBrush, 170, 147);
                page1.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_RecurringRevenue) ? (Convert.ToInt32(_Helpers.ConvertToDouble(model.Ff_RecurringRevenueAnnualized))).ToString("C0") : "N/A", helvetica, blackBrush, 170, 165);
                page1.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_TotalRevenue) ? (Convert.ToInt32(_Helpers.ConvertToDouble(model.Ff_TotalRevenueAnnualized))).ToString("C0") : "N/A", helvetica, textBrush, 170, 183);

                if (!string.IsNullOrEmpty(model.Ff_DirectExpensesAnnualized) && !string.IsNullOrEmpty(model.Ff_IndirecteExpensesAnnualized))
                {
                    page1.Graphics.DrawString(Convert.ToInt32((_Helpers.ConvertToDouble(model.Ff_DirectExpensesAnnualized) + _Helpers.ConvertToDouble(model.Ff_IndirecteExpensesAnnualized))).ToString("C0"), helvetica, blackBrush, 170, 201);
                }
                else if (string.IsNullOrEmpty(model.Ff_DirectExpensesAnnualized))
                {
                    page1.Graphics.DrawString((Convert.ToInt32(_Helpers.ConvertToDouble(model.Ff_IndirecteExpensesAnnualized)).ToString("C0")), helvetica, blackBrush, 170, 201);
                }
                else
                {
                    page1.Graphics.DrawString((Convert.ToInt32(_Helpers.ConvertToDouble(model.Ff_DirectExpensesAnnualized)).ToString("C0")), helvetica, blackBrush, 170, 201);
                }

                page1.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_OperatingProfitAnnualized) ? Convert.ToInt32(_Helpers.ConvertToDouble(model.Ff_OperatingProfitAnnualized)).ToString("C0") : "N/A", helvetica, blackBrush, 170, 218);
                page1.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate) ? model.Ff_ProjectedGrowthRate + "%" : "N/A", helvetica, blackBrush, 170, 236);

                page1.Graphics.DrawString(peerGroup.AUM.ToString("C0"), helvetica, blackBrush, 290, 128);
                page1.Graphics.DrawString(peerGroup.ClientRelationships.ToString(), helvetica, blackBrush, 290, 147);
                page1.Graphics.DrawString(peerGroup.RecurringRevenue.ToString("C0"), helvetica, blackBrush, 290, 165);
                page1.Graphics.DrawString(peerGroup.TotalRevenue.ToString("C0"), helvetica, blackBrush, 290, 183);
                page1.Graphics.DrawString(peerGroup.TotalExpenses.ToString("C0"), helvetica, blackBrush, 290, 201);
                page1.Graphics.DrawString(peerGroup.OperatingProfit.ToString("C0"), helvetica, blackBrush, 290, 218);
                page1.Graphics.DrawString(peerGroup.ProjectedAnnualGrowthRate.ToString("0") + "%", helvetica, blackBrush, 290, 236);


                //============================================================== KPI's Table =============================================================    
               
                page1.Graphics.DrawString(GetTimeRange(model), helveticaBold, blackBrush, 165, 539);

                page1.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.RecurringRevenuePerClient).ToString("c0"), helvetica, blackBrush, 170, 557);
                page1.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.RecurringRevenuePerAdvisor).ToString("C0"), helvetica, blackBrush, 170, 576);
                page1.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.TotalRevenuePerClient).ToString("C0"), helvetica, blackBrush, 170, 595);
                page1.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.TotalAUMperClient).ToString("C0"), helvetica, blackBrush, 170, 611);
                page1.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.TotalAUMperAdvisor).ToString("C0"), helvetica, blackBrush, 170, 627);
                page1.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.ProfitPerClient).ToString("C0"), helvetica, blackBrush, 170, 644);
                page1.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.ProfitAsPercentOfRevenue).ToString("0") + " %", helvetica, blackBrush, 170, 661);
                page1.Graphics.DrawString(((int)_Helpers.ConvertToDouble(model.ClientValuationModel.ClientsPerAdvisor)).ToString(), helvetica, blackBrush, 170, 680);
                page1.Graphics.DrawString(((int)_Helpers.ConvertToDouble(model.ClientValuationModel.RevenueAsBPSOnAssets)).ToString(), helvetica, blackBrush, 170, 699);

                page1.Graphics.DrawString(Math.Ceiling(peerGroup.RecRevPerClient).ToString("C0"), helvetica, blackBrush, 290, 557);
                page1.Graphics.DrawString(Math.Ceiling(peerGroup.RecRevPerAdvisor).ToString("C0"), helvetica, blackBrush, 290, 576);
                page1.Graphics.DrawString(Math.Ceiling(peerGroup.TotalRevPerClient).ToString("C0"), helvetica, blackBrush, 290, 595);
                page1.Graphics.DrawString(Math.Ceiling(peerGroup.TotalAUMPerClient).ToString("C0"), helvetica, blackBrush, 290, 611);
                page1.Graphics.DrawString(Math.Ceiling(peerGroup.TotalAUMPerAdvisor).ToString("C0"), helvetica, blackBrush, 290, 627);
                page1.Graphics.DrawString(peerGroup.ProfitPerClient.ToString("C0"), helvetica, blackBrush, 290, 644);
                page1.Graphics.DrawString(peerGroup.ProfitAsPercentOfRevenue.ToString("0") + " %", helvetica, blackBrush, 290, 661);
                page1.Graphics.DrawString(peerGroup.ClientsPerAdvisor.ToString(), helvetica, blackBrush, 290, 680);
                page1.Graphics.DrawString(peerGroup.RevenueAsPBSOnAssets.ToString(), helvetica, blackBrush, 290, 699);




                
                //Graph brushes
                PdfBrush graphBrush1 = new PdfBrush((new PdfRgbColor(0, 74, 129))); //#004b81 Darkest
                PdfBrush graphBrush2 = new PdfBrush((new PdfRgbColor(0, 126, 187))); // #007ebb
                PdfBrush graphBrush3 = new PdfBrush((new PdfRgbColor(109, 198, 233))); //#6dc6e7;
                PdfBrush graphBrush4 = new PdfBrush((new PdfRgbColor(176, 216, 235))); //#b0d8eb;
                               
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////                                                      VMI GRAPH                                                           /////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 300), new PdfPoint(35, 415)); //vertical
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 300), new PdfPoint(255, 300));
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 327), new PdfPoint(255, 327));
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 349), new PdfPoint(255, 349));
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 371), new PdfPoint(255, 371));
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 393), new PdfPoint(255, 393));
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 1), new PdfPoint(35, 415), new PdfPoint(255, 415)); //horizontal

                page1.Graphics.DrawString("1000", helvetica, textBrush, 15, 298);
                page1.Graphics.DrawString("800", helvetica, textBrush, 19, 321);
                page1.Graphics.DrawString("600", helvetica, textBrush, 19, 343);
                page1.Graphics.DrawString("400", helvetica, textBrush, 19, 365);
                page1.Graphics.DrawString("200", helvetica, textBrush, 19, 387);
                page1.Graphics.DrawString("0", helvetica, textBrush, 27, 409);
                page1.Graphics.DrawString("Your Firm", helvetica, textBlueBrush, 70, 419);
                page1.Graphics.DrawString("Benchmark Index", helvetica, textBlueBrush, 160, 419);

                //Calculate blocks height for Your Firm 
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
                double y = 415 - (firstBlock + secondBlock + thirdBlock + fourthBlock);

                page1.Graphics.DrawString(model.Vmi_Index , helvetica, grayBrush, x + 20, y - 15); //VMI Index

                page1.Graphics.DrawRectangle(graphBrush4, x, y, 55, fourthBlock);
                page1.Graphics.DrawString(model.ClientValuationModel.ManagingYourPracticeScore.ToString(), helvetica, whiteBrush, x + 20, y + ((fourthBlock / 2) - 2)); //score

                y = y + fourthBlock;
                page1.Graphics.DrawRectangle(graphBrush3, x, y, 55, thirdBlock);
                page1.Graphics.DrawString(model.ClientValuationModel.MarketingYourBusinessScore.ToString(), helvetica, whiteBrush, x + 20, y + ((thirdBlock / 2) - 2)); //score

                y = y + thirdBlock;
                page1.Graphics.DrawRectangle(graphBrush2, x, y, 55, secondBlock);
                page1.Graphics.DrawString(model.ClientValuationModel.OptimizingYourOperationsScore.ToString(), helvetica, whiteBrush, x + 20, y + ((secondBlock / 2) - 2)); //score

                y = y + secondBlock;
                page1.Graphics.DrawRectangle(graphBrush1, x, y, 55, firstBlock);
                page1.Graphics.DrawString(model.ClientValuationModel.EmpoweringYourTeamScore.ToString(), helvetica, whiteBrush, x + 20, y + ((firstBlock / 2) - 2)); //score

               


                //Calculate blocks height for Benchmarks                
                firstBlock = peerGroup.EYT * pixel;
                secondBlock = peerGroup.OYO * pixel;
                thirdBlock = peerGroup.MYB * pixel;
                fourthBlock = peerGroup.MYP * pixel;

                x = 160;
                y = 415 - (firstBlock + secondBlock + thirdBlock + fourthBlock);

                page1.Graphics.DrawString((peerGroup.MYB + peerGroup.MYP + peerGroup.EYT + peerGroup.OYO).ToString(), helvetica, grayBrush, x + 20, y - 15); //VMI Index

                page1.Graphics.DrawRectangle(graphBrush4, x, y, 55, fourthBlock);
                page1.Graphics.DrawString(peerGroup.MYP.ToString(), helvetica, whiteBrush, x + 20, y + ((fourthBlock / 2) - 2)); //score

                y = y + fourthBlock;
                page1.Graphics.DrawRectangle(graphBrush3, x, y, 55, thirdBlock);
                page1.Graphics.DrawString(peerGroup.MYB.ToString(), helvetica, whiteBrush, x + 20, y + ((thirdBlock / 2) - 2)); //score

                y = y + thirdBlock;
                page1.Graphics.DrawRectangle(graphBrush2, x, y, 55, secondBlock);
                page1.Graphics.DrawString(peerGroup.OYO.ToString(), helvetica, whiteBrush, x + 20, y + ((secondBlock / 2) - 2)); //score

                y = y + secondBlock;
                page1.Graphics.DrawRectangle(graphBrush1, x, y, 55, firstBlock);
                page1.Graphics.DrawString(peerGroup.EYT.ToString(), helvetica, whiteBrush, x + 20, y + ((firstBlock / 2) - 2)); //score



                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///////////////                                                  VALUATION RANGE GRAPH                                                           /////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                double axisMax = peerGroup.ValuationMax + (peerGroup.ValuationMax / 4);
                pixel = axisMax / 135;

                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(353, 300), new PdfPoint(353, 435)); //vertical
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 1), new PdfPoint(353, 435), new PdfPoint(550, 435)); //horizontal

                //Gray lines
                y = 300;
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(353, y), new PdfPoint(550, y)); //horizontal
                y = y + 34;
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(353, y), new PdfPoint(550, y)); //horizontal
                y = y + 34;
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(353, y), new PdfPoint(550, y)); //horizontal
                y = y + 34;
                page1.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(353, y), new PdfPoint(550, y)); //horizontal


                page1.Graphics.DrawString("Your Firm", helvetica, textBlueBrush, 390, 440);
                page1.Graphics.DrawString("Benchmark Index", helvetica, textBlueBrush, 470, 440);


                //Client Valuation Range
                if (model.ClientValuationModel.ValuationMax != 0 && model.ClientValuationModel.ValuationMin != 0)
                {
                    if (model.ClientValuationModel.ValuationMax > peerGroup.ValuationMax)
                    {
                        axisMax = model.ClientValuationModel.ValuationMax + (model.ClientValuationModel.ValuationMax / 4);
                        pixel = axisMax / 135;
                    }

                    firstBlock = (model.ClientValuationModel.ValuationMax - model.ClientValuationModel.ValuationMin) / pixel;


                    x = 390;
                    y = 435 - (model.ClientValuationModel.ValuationMax / pixel);
                    page1.Graphics.DrawRectangle(graphBrush2, x, y, 55, firstBlock);
                    page1.Graphics.DrawString(((int)Math.Round(model.ClientValuationModel.ValuationMax / 1000) * 1000).ToString("C0"), helvetica, textBrush, x, (y - 9));
                    page1.Graphics.DrawString(((int)Math.Round(model.ClientValuationModel.ValuationMin / 1000) * 1000).ToString("C0"), helvetica, textBrush, x, (y + firstBlock + 3));
                }
                else
                {
                    x = 400;
                    page1.Graphics.DrawString("No Data", helvetica, textBrush, x, 350);
                }



                //Benchmark Valuation Range
                double dollarsInPixel = axisMax / 135;
                double range = peerGroup.ValuationMax - peerGroup.ValuationMin;
                double height = range / dollarsInPixel; //height in pixels

                x = 470;
                y = peerGroup.ValuationMax / dollarsInPixel;
                y = 435 - y; //bottom of range

                page1.Graphics.DrawString(peerGroup.ValuationMax.ToString("C0"), helvetica, textBrush, x, (y - 9));
                page1.Graphics.DrawRectangle(graphBrush3, x, y, 55, height);
                page1.Graphics.DrawString(peerGroup.ValuationMin.ToString("C0"), helvetica, textBrush, x, (y + height + 3));

                //Axis values
                PdfStringLayoutOptions layout = new PdfStringLayoutOptions() { HorizontalAlign = PdfStringHorizontalAlign.Right, X = 351, Y = 300 };
                PdfPen pen = new PdfPen(PdfRgbColor.Black, 0.007);
                PdfStringAppearanceOptions appearance = new PdfStringAppearanceOptions(helvetica, pen, textBrush);

                page1.Graphics.DrawString("$0", helvetica, textBrush, 342, 430);              
                page1.Graphics.DrawString(((int)Math.Round(axisMax / 1000) * 1000).ToString("C0"), appearance, layout);

                double incrementHeight = 135 / 4;
                double incrementValue = axisMax / 4;

                layout.Y = layout.Y + incrementHeight;
                page1.Graphics.DrawString(((int)Math.Round((axisMax - incrementValue) / 1000) * 1000).ToString("C0"), appearance, layout);

                layout.Y = layout.Y + incrementHeight;
                page1.Graphics.DrawString(((int)Math.Round((axisMax / 2) / 1000) * 1000).ToString("C0"), appearance, layout);

                layout.Y = layout.Y + incrementHeight;
                page1.Graphics.DrawString(((int)Math.Round((axisMax - (incrementValue * 3)) / 1000) * 1000).ToString("C0"), appearance, layout);

                page1.Width = 612;

                //Upload the PDF to Azure storage
                MemoryStream stream = new MemoryStream();
                document.Save(stream);

                //document.Save("C:\\Olga\\PdfCustom.pdf");




                byte[] docBytes = stream.ToArray();
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]); //connection string is copied from Azure storage account's Settings
                CloudBlobClient client = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer myContainer = client.GetContainerReference("assetmarkbat");
                var permissions = myContainer.GetPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                myContainer.SetPermissions(permissions);
                CloudBlockBlob blockBlob = myContainer.GetBlockBlobReference(model.UserId + ".pdf");
                blockBlob.Properties.ContentType = "application/pdf";
                blockBlob.UploadFromByteArray(docBytes, 0, docBytes.Count());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error creating a PDF document", e.Message);
            }
        }

        private string GetTimeRange(BATModel model)
        {
            string year = model.Year;

            if (model.Year.ToLower().Contains("previous"))
            {
                year = "Jan - Dec " + (DateTime.Now.Year - 1).ToString();
            }
            else
            {
                DateTime dt = new DateTime(DateTime.Now.Year, model.Month, 1);
                year = "Jan - " + dt.ToString("MMM") + " " + dt.Year;
            }

            return year;
        }

        private static PdfFixedDocument Load(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
                return new PdfFixedDocument(stream);
        }
    }
}