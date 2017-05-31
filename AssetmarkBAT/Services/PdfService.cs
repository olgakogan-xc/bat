using AssetmarkBAT.Models;
using AssetmarkBAT.Utilities;
using System;
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
        public void DrawPdf(BATModel model)
        {
            try
            {
                PdfFixedDocument document = Load(HttpContext.Current.Server.MapPath(@"~\UserPDF\PdfTemplate.pdf"));
                PdfPage page = document.Pages[0];

                // Fonts and Brushes
                PdfStandardFont helvetica = new PdfStandardFont(PdfStandardFontFace.Helvetica, 8);
                PdfStandardFont helveticaBold = new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 8.5);

                PdfBrush textBrush = new PdfBrush((PdfRgbColor.Black));
                PdfBrush whiteBrush = new PdfBrush((PdfRgbColor.White));
                PdfBrush blackBrush = new PdfBrush(PdfRgbColor.Black);
                PdfBrush textBlueBrush = new PdfBrush((new PdfRgbColor(5, 79, 124)));              

              
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

                page.Graphics.DrawString(group, helvetica, blackBrush, 500, 94);

                //============================================================== Firm Financials Table =============================================================
                page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_TotalFirmAsset) ? (_Helpers.ConvertToDouble(model.Ff_TotalFirmAsset)).ToString("C0") : "N/A", helvetica, blackBrush, 170, 128);
                page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_ClientRelationships) ? model.Ff_ClientRelationships : "N/A", helvetica, blackBrush, 170, 147);
                page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_RecurringRevenue) ? (Convert.ToInt32(_Helpers.ConvertToDouble(model.Ff_RecurringRevenue))).ToString("C0") : "N/A", helvetica, blackBrush, 170, 165);
                page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_TotalRevenue) ? (Convert.ToInt32(_Helpers.ConvertToDouble(model.Ff_TotalRevenue))).ToString("C0") : "N/A", helvetica, textBrush, 170, 183);

                if (!string.IsNullOrEmpty(model.Ff_DirectExpenses) && !string.IsNullOrEmpty(model.Ff_IndirecteExpenses))
                {
                    page.Graphics.DrawString(Convert.ToInt32((_Helpers.ConvertToDouble(model.Ff_DirectExpenses) + _Helpers.ConvertToDouble(model.Ff_IndirecteExpenses))).ToString("C0"), helvetica, blackBrush, 170, 201);
                }
                else if (string.IsNullOrEmpty(model.Ff_DirectExpenses))
                {
                    page.Graphics.DrawString((Convert.ToInt32(_Helpers.ConvertToDouble(model.Ff_IndirecteExpenses)).ToString("C0")), helvetica, blackBrush, 170, 201);
                }
                else
                {
                    page.Graphics.DrawString((Convert.ToInt32(_Helpers.ConvertToDouble(model.Ff_DirectExpenses)).ToString("C0")), helvetica, blackBrush, 170, 201);
                }

                page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_OperatingProfit) ? Convert.ToInt32(_Helpers.ConvertToDouble(model.Ff_OperatingProfit)).ToString("C0") : "N/A", helvetica, blackBrush, 170, 218);
                page.Graphics.DrawString(!string.IsNullOrEmpty(model.Ff_ProjectedGrowthRate) ? model.Ff_ProjectedGrowthRate + "%" : "N/A", helvetica, blackBrush, 170, 236);

                page.Graphics.DrawString(peerGroup.AUM.ToString("C0"), helvetica, blackBrush, 290, 128);
                page.Graphics.DrawString(peerGroup.ClientRelationships.ToString(), helvetica, blackBrush, 290, 147);
                page.Graphics.DrawString(peerGroup.RecurringRevenue.ToString("C0"), helvetica, blackBrush, 290, 165);
                page.Graphics.DrawString(peerGroup.TotalRevenue.ToString("C0"), helvetica, blackBrush, 290, 183);
                page.Graphics.DrawString(peerGroup.TotalExpenses.ToString("C0"), helvetica, blackBrush, 290, 201);
                page.Graphics.DrawString(peerGroup.OperatingProfit.ToString("C0"), helvetica, blackBrush, 290, 218);
                page.Graphics.DrawString(peerGroup.ProjectedAnnualGrowthRate.ToString() + "%", helvetica, blackBrush, 290, 236);


                //============================================================== KPI's Table =============================================================    
                string year = model.Year;
                
                if(model.Year.ToLower().Contains("previous"))
                {
                    year = "Jan - Dec " + (DateTime.Now.Year - 1).ToString();
                }
                else
                {
                    DateTime dt = new DateTime(DateTime.Now.Year, model.Month, 31);
                    year = "Jan - " + dt.ToString("MMM") + " " + dt.Year;
                }

                page.Graphics.DrawString(year, helveticaBold, blackBrush, 165, 539);

                page.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.RecurringRevenuePerClient).ToString("c0"), helvetica, blackBrush, 170, 557);
                page.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.RecurringRevenuePerAdvisor).ToString("C0"), helvetica, blackBrush, 170, 576);
                page.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.TotalRevenuePerClient).ToString("C0"), helvetica, blackBrush, 170, 595);
                page.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.TotalAUMperClient).ToString("C0"), helvetica, blackBrush, 170, 611);
                page.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.TotalAUMperAdvisor).ToString("C0"), helvetica, blackBrush, 170, 627);
                page.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.ProfitPerClient).ToString("C0"), helvetica, blackBrush, 170, 644);
                page.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.ProfitAsPercentOfRevenut).ToString("0.0") + " %", helvetica, blackBrush, 170, 661);
                page.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.ClientsPerAdvisor).ToString("C0"), helvetica, blackBrush, 170, 680);
                page.Graphics.DrawString(_Helpers.ConvertToDouble(model.ClientValuationModel.RevenueAsBPSOnAssets).ToString("C0"), helvetica, blackBrush, 170, 699);

                page.Graphics.DrawString(Math.Ceiling(peerGroup.RecRevPerClient).ToString("C0"), helvetica, blackBrush, 290, 557);
                page.Graphics.DrawString(Math.Ceiling(peerGroup.RecRevPerAdvisor).ToString("C0"), helvetica, blackBrush, 290, 576);
                page.Graphics.DrawString(Math.Ceiling(peerGroup.TotalRevPerClient).ToString("C0"), helvetica, blackBrush, 290, 595);
                page.Graphics.DrawString(Math.Ceiling(peerGroup.TotalAUMPerClient).ToString("C0"), helvetica, blackBrush, 290, 611);
                page.Graphics.DrawString(Math.Ceiling(peerGroup.TotalAUMPerAdvisor).ToString("C0"), helvetica, blackBrush, 290, 627);
                page.Graphics.DrawString(peerGroup.ProfitPerClient.ToString("C0"), helvetica, blackBrush, 290, 644);
                page.Graphics.DrawString(peerGroup.ProfitAsPercentOfRevenue.ToString("0.0") + " %", helvetica, blackBrush, 290, 661);
                page.Graphics.DrawString("$" + peerGroup.ClientsPerAdvisor.ToString(), helvetica, blackBrush, 290, 680);
                page.Graphics.DrawString("$" + peerGroup.RevenutAsPBSOnAssets.ToString(), helvetica, blackBrush, 290, 699);




                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////                    GRAPHS                     //////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                
                //Graph brushes
                PdfBrush graphBrush1 = new PdfBrush((new PdfRgbColor(0, 74, 129))); //#004b81 Darkest
                PdfBrush graphBrush2 = new PdfBrush((new PdfRgbColor(0, 126, 187))); // #007ebb
                PdfBrush graphBrush3 = new PdfBrush((new PdfRgbColor(109, 198, 233))); //#6dc6e7;
                PdfBrush graphBrush4 = new PdfBrush((new PdfRgbColor(176, 216, 235))); //#b0d8eb;



                // ===================================================== VMI Graph =======================================================================
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 300), new PdfPoint(35, 415)); //vertical
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 300), new PdfPoint(255, 300));
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 327), new PdfPoint(255, 327));
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 349), new PdfPoint(255, 349));
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 371), new PdfPoint(255, 371));
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(35, 393), new PdfPoint(255, 393));
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 1), new PdfPoint(35, 415), new PdfPoint(255, 415)); //horizontal

                page.Graphics.DrawString("1000", helvetica, textBrush, 15, 298);
                page.Graphics.DrawString("800", helvetica, textBrush, 19, 321);
                page.Graphics.DrawString("600", helvetica, textBrush, 19, 343);
                page.Graphics.DrawString("400", helvetica, textBrush, 19, 365);
                page.Graphics.DrawString("200", helvetica, textBrush, 19, 387);
                page.Graphics.DrawString("0", helvetica, textBrush, 27, 409);
                page.Graphics.DrawString("Your Firm", helvetica, textBlueBrush, 70, 419);
                page.Graphics.DrawString("Benchmark Index", helvetica, textBlueBrush, 160, 419);

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

                page.Graphics.DrawRectangle(graphBrush4, x, y, 55, fourthBlock);
                page.Graphics.DrawString(model.ClientValuationModel.ManagingYourPracticeScore.ToString(), helvetica, whiteBrush, x + 20, y + ((fourthBlock / 2) - 2)); //score

                y = y + fourthBlock;
                page.Graphics.DrawRectangle(graphBrush3, x, y, 55, thirdBlock);
                page.Graphics.DrawString(model.ClientValuationModel.MarketingYourBusinessScore.ToString(), helvetica, whiteBrush, x + 20, y + ((thirdBlock / 2) - 2)); //score

                y = y + thirdBlock;
                page.Graphics.DrawRectangle(graphBrush2, x, y, 55, secondBlock);
                page.Graphics.DrawString(model.ClientValuationModel.OptimizingYourOperationsScore.ToString(), helvetica, whiteBrush, x + 20, y + ((secondBlock / 2) - 2)); //score

                y = y + secondBlock;
                page.Graphics.DrawRectangle(graphBrush1, x, y, 55, firstBlock);
                page.Graphics.DrawString(model.ClientValuationModel.EmpoweringYourTeamScore.ToString(), helvetica, whiteBrush, x + 20, y + ((firstBlock / 2) - 2)); //score

               


                //Calculate blocks height for Benchmarks                
                firstBlock = peerGroup.EYT * pixel;
                secondBlock = peerGroup.OYO * pixel;
                thirdBlock = peerGroup.MYB * pixel;
                fourthBlock = peerGroup.MYP * pixel;

                x = 160;
                y = 415 - (firstBlock + secondBlock + thirdBlock + fourthBlock);

                page.Graphics.DrawRectangle(graphBrush4, x, y, 55, fourthBlock);
                page.Graphics.DrawString(peerGroup.MYP.ToString(), helvetica, whiteBrush, x + 20, y + ((fourthBlock / 2) - 2)); //score

                y = y + fourthBlock;
                page.Graphics.DrawRectangle(graphBrush3, x, y, 55, thirdBlock);
                page.Graphics.DrawString(peerGroup.MYB.ToString(), helvetica, whiteBrush, x + 20, y + ((thirdBlock / 2) - 2)); //score

                y = y + thirdBlock;
                page.Graphics.DrawRectangle(graphBrush2, x, y, 55, secondBlock);
                page.Graphics.DrawString(peerGroup.OYO.ToString(), helvetica, whiteBrush, x + 20, y + ((secondBlock / 2) - 2)); //score

                y = y + secondBlock;
                page.Graphics.DrawRectangle(graphBrush1, x, y, 55, firstBlock);
                page.Graphics.DrawString(peerGroup.EYT.ToString(), helvetica, whiteBrush, x + 20, y + ((firstBlock / 2) - 2)); //score



                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                /////////////// VALUATION RANGE GRAPH   /////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                double axisMax = peerGroup.ValuationMax + (peerGroup.ValuationMax / 4);
                pixel = axisMax / 135;

                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.LightGray, 0.5), new PdfPoint(353, 300), new PdfPoint(353, 435)); //vertical
                page.Graphics.DrawLine(new PdfPen(PdfRgbColor.Black, 1), new PdfPoint(353, 435), new PdfPoint(550, 435)); //horizontal

                page.Graphics.DrawString("Your Firm", helvetica, textBlueBrush, 390, 440);
                page.Graphics.DrawString("Benchmark Index", helvetica, textBlueBrush, 470, 440);


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
                    page.Graphics.DrawRectangle(graphBrush2, x, y, 55, firstBlock);
                    page.Graphics.DrawString(model.ClientValuationModel.ValuationMax.ToString("C0"), helvetica, textBrush, x, (y - 9));
                    page.Graphics.DrawString(model.ClientValuationModel.ValuationMin.ToString("C0"), helvetica, textBrush, x, (y + firstBlock + 3));
                }
                else
                {
                    x = 400;
                    page.Graphics.DrawString("No Data", helvetica, textBrush, x, 350);
                }



                //Benchmark Valuation Range
                double dollarsInPixel = axisMax / 135;
                double range = peerGroup.ValuationMax - peerGroup.ValuationMin;
                double height = range / dollarsInPixel; //height in pixels

                x = 470;
                y = peerGroup.ValuationMax / dollarsInPixel;
                y = 435 - y; //bottom of range

                page.Graphics.DrawString(peerGroup.ValuationMax.ToString("C0"), helvetica, textBrush, x, (y - 9));
                page.Graphics.DrawRectangle(graphBrush3, x, y, 55, height);
                page.Graphics.DrawString(peerGroup.ValuationMin.ToString("C0"), helvetica, textBrush, x, (y + height + 3));

                //Axis values
                PdfStringLayoutOptions layout = new PdfStringLayoutOptions() { HorizontalAlign = PdfStringHorizontalAlign.Right, X = 351, Y = 300 };
                PdfPen pen = new PdfPen(PdfRgbColor.Black, 0.007);
                PdfStringAppearanceOptions appearance = new PdfStringAppearanceOptions(helvetica, pen, textBrush);

                page.Graphics.DrawString("$0", helvetica, textBrush, 346, 430);
                page.Graphics.DrawString((Convert.ToInt32(axisMax)).ToString("C0"), appearance, layout);

                double incrementHeight = 135 / 4;
                double incrementValue = axisMax / 4;

                layout.Y = layout.Y + incrementHeight;
                page.Graphics.DrawString((axisMax - incrementValue).ToString("C0"), appearance, layout);

                layout.Y = layout.Y + incrementHeight;
                page.Graphics.DrawString((axisMax - (axisMax / 2)).ToString("C0"), appearance, layout);

                layout.Y = layout.Y + incrementHeight;
                page.Graphics.DrawString((axisMax - (incrementValue * 3)).ToString("C0"), appearance, layout);
               


                //Upload the PDF to Azure storage
                MemoryStream stream = new MemoryStream();
                document.Save(stream);

                document.Save("C:\\Olga\\PdfCustom.pdf");



                //// Converts the PdfDocument object to byte form.
                //byte[] docBytes = stream.ToArray();
                ////Loads the byte array in PdfLoadedDocument

                //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]); //connection string is copied from Azure storage account's Settings
                //CloudBlobClient client = storageAccount.CreateCloudBlobClient();
                //CloudBlobContainer myContainer = client.GetContainerReference("assetmarkbat");
                //var permissions = myContainer.GetPermissions();
                //permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                //myContainer.SetPermissions(permissions);

                //CloudBlockBlob blockBlob = myContainer.GetBlockBlobReference(model.UserId + ".pdf");
                //blockBlob.Properties.ContentType = "application/pdf";
                ////blockBlob.UploadFromStream(stream);
                //blockBlob.UploadFromByteArray(docBytes, 0, docBytes.Count());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error creating a PDF document", e.Message);
            }
        }

        private static PdfFixedDocument Load(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
                return new PdfFixedDocument(stream);
        }
    }
}