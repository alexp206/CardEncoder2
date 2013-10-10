using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZMOTIFPRINTERLib;
using System.Threading;
using System.Text.RegularExpressions;

namespace CardEncoder2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int actionID1;
        string printerName;
        string alarm;
        string uuidJob;
        string printingStatus;
        string cardPosition;
        int errorCode; 
        int copiesCompleted;
        int copiesRequested;
        string magStatus;
        string contactStatus;
        string contactlessStatus;
        
        public MainWindow()
        {
            InitializeComponent();
            Job job = new Job();
            try
            {
                object objPrinterList = null;
                job.GetPrinters(ConnectionTypeEnum.USB, out objPrinterList);
                if (objPrinterList != null)
                {
                    Array array = (Array)objPrinterList;
                    string[] prnList = new string[array.GetLength(0)];
                    for (int i = 0; i < array.GetLength(0); i++)
                        prnList[i] = (string)array.GetValue(i);
                }
                else
                {
                    MessageBox.Show("No Printers Found, power on/connect the printer and restart the program.");
                }
                cbPrinterList.ItemsSource = (Array)objPrinterList;
                
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                MessageBox.Show("No Printers Found, power on/connect the printer and restart the program.");
            }
            job = null;
        }

        private void bnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               //Open Connection to Printer
               Job job = new Job();
               printerName = cbPrinterList.SelectedItem.ToString();
               job.Open(printerName);
               //Set Card Source and Destination
               job.JobControl.FeederSource = FeederSourceEnum.CardFeeder; //Take Card From Feeder

               if (cxEject.IsChecked == true) //Determine Checkbox State
               {
                   job.JobControl.Destination = DestinationTypeEnum.Eject; //Eject Card When Finished
               }
               else
               {
                   job.JobControl.Destination = DestinationTypeEnum.Hold; //Leave Card in Printer When Finished
               }

               //Validate GNumber
                if (Regex.IsMatch(tbGNumber.Text, "^[0-9]{8}$"))
                {


               // Magnetically Encode Card
               actionID1 = 0;
               string GNumPrefix = "0";
               string GNumSuffix = "1180=";
               string dataToEncode = GNumPrefix + tbGNumber.Text + GNumSuffix;
               job.MagDataOnly(1, "", dataToEncode, "", out actionID1);

               //Report Job Status
               Task.Factory.StartNew(() =>
               {
                   while (job.IsOpen)
                   {
                       this.Dispatcher.BeginInvoke(new ThreadStart(() =>
                       {
                           short alarm = job.GetJobStatus(actionID1, out uuidJob, out printingStatus, out cardPosition, out errorCode, out copiesCompleted, out copiesRequested, out magStatus, out contactStatus, out contactlessStatus);
                           tbMagStatus.Text = magStatus;
                           tbPrinterStatus.Text = printingStatus;
                       }));
                       if (printingStatus == "done_ok")
                       {
                           job.Close(); //Finish Up, Close Connection
                           break;
                       }
                   }
               });
                }else{
                    job.Close();
                    MessageBox.Show("Please double check the G-Number and try again.");
                }

            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                MessageBox.Show("You Must First Select a Printer");
                e.Handled = true;

            }
         
        }

        private void bnEject_Click(object sender, RoutedEventArgs e)
        {
            Job job = new Job();
            try
            {
                printerName = cbPrinterList.SelectedItem.ToString();
                job.Open(printerName);
                job.EjectCard();
                job.Close();
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                MessageBox.Show("You Must First Select a Printer");
            }
        }
    }
}
