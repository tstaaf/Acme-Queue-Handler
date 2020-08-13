using System;
using System.Collections.Generic;
using System.Windows;
using Domino_Queue_Handler.Model;
using FirebirdSql.Data.FirebirdClient;
using System.Windows.Media;
using Domino_Queue_Handler.Class;
using System.Threading;
using System.Globalization;

namespace Domino_Queue_Handler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool ProgramActive = true;
        List<ScannerData> data = new List<ScannerData>();
        private TCPCom ScanCom = new TCPCom(0, "192.168.10.21", 10001, false);
        private Thread ScanComThread;
        private TCPCom PrintCom = new TCPCom(1, "192.168.10.20", 9100, false);
        private Thread PrintComThread;
        private bool palletInPosition;
        private bool printSent = false;
        private System.Windows.Threading.DispatcherTimer ACCStatusTimer = new System.Windows.Threading.DispatcherTimer();
        private System.Windows.Threading.DispatcherTimer ConStatusTimer = new System.Windows.Threading.DispatcherTimer();
        public bool tick = false;
        public DateTime PrintDate = new DateTime();

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closed;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ScanCom.SetupCom();
            ScanComThread = new Thread(new ParameterizedThreadStart(ScannerThreadCom));
            ScanComThread.Start(ScanCom);
            PrintCom.SetupCom();
            PrintComThread = new Thread(new ParameterizedThreadStart(PrinterThreadCom));
            PrintComThread.Start(PrintCom);

            ACCStatusTimer.Tick += ACCStatusTimer_Tick;
            ACCStatusTimer.Interval = new TimeSpan(0, 0, 2);
            ACCStatusTimer.Start();

            ConStatusTimer.Tick += ConStatusTimer_Tick;
            ConStatusTimer.Interval = new TimeSpan(0, 0, 2);
            ConStatusTimer.Start();

            PrintDate = DateTime.Now;

            datePick.SelectedDate = PrintDate;
        }

        private void ConStatusTimer_Tick(object sender, EventArgs e)
        {
            if (PrintCom.GetStatus() == 1)
            {
                PrinterStatus.Dispatcher.Invoke(() =>
                {
                    PrinterStatus.Icon = FontAwesome.WPF.FontAwesomeIcon.CheckCircle;
                    PrinterStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009d58"));
                });
            }
            else
            {
                PrinterStatus.Dispatcher.Invoke(() =>
                {
                    PrinterStatus.Icon = FontAwesome.WPF.FontAwesomeIcon.TimesCircle;
                    PrinterStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ca0b00"));
                });
            }
            if (ScanCom.GetStatus() == 1)
            {
                ScannerStatus.Dispatcher.Invoke(() =>
                {
                    ScannerStatus.Icon = FontAwesome.WPF.FontAwesomeIcon.CheckCircle;
                    ScannerStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009d58"));
                });
            }
            else
            {
                ScannerStatus.Dispatcher.Invoke(() =>
                {
                    ScannerStatus.Icon = FontAwesome.WPF.FontAwesomeIcon.TimesCircle;
                    ScannerStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ca0b00"));
                });
            }
        }

        private void ACCStatusTimer_Tick(object sender, EventArgs e)
        {
            tick = true;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }


        private void PrinterThreadCom(object obj)
        {
            TCPCom ThreadCom = (TCPCom)obj;
            string ReceivedData;
            

            if (!ThreadCom.SetupCom())
            {
                //DebugWrite("Error setting up communication with printer.");
            }
            else
            {
                while (ProgramActive)
                {
                    
                    if (tick)
                    {
                        tick = false;
                        ReceivedData = ThreadCom.GetACC();
                        if (ReceivedData != "" && ReceivedData != null && ReceivedData.Length > 0)
                        {
                            //StatusBar.Dispatcher.Invoke(() =>
                            //{
                            //    StatusBar.Content = ReceivedData;
                            //});

                        }
                        if (ReceivedData.Contains("0A44") && ReceivedData.Length > 15)
                        {
                            if (ReceivedData[14].ToString() == "1")
                            {
                                palletInPosition = true;
                                PalletStatus.Dispatcher.Invoke(() =>
                                {
                                    PalletStatus.Icon = FontAwesome.WPF.FontAwesomeIcon.CheckCircle;
                                    PalletStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009d58"));
                                });
                            }
                            else if(ReceivedData[14].ToString() == "0")
                            {
                                palletInPosition = false;
                                printSent = false;
                                PalletStatus.Dispatcher.Invoke(() =>
                                {
                                    PalletStatus.Icon = FontAwesome.WPF.FontAwesomeIcon.TimesCircle;
                                    PalletStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ca0b00"));
                                });
                            }
                        }
                        if (palletInPosition && !printSent)
                        {
                            SendPrint();
                        }
                    }                  
                }
            }
        }

        private void ScannerThreadCom(object obj)
        {
            TCPCom ThreadCom = (TCPCom)obj;

            if (!ThreadCom.SetupCom())
            {
                //DebugWrite("Error setting up communication with Scanner.");
            }
            else
            {
                while (ProgramActive)
                {
                    string ReceivedData = ThreadCom.UseCom();
                    if (ReceivedData != "" && ReceivedData != null && ReceivedData.Length > 0)
                    {
                        var newString = ReceivedData.Replace("\u0002", "").Replace("\u0003", "");
                        StatusBar.Dispatcher.Invoke(() =>
                        {
                            StatusBar.Content = ReceivedData;
                        });
                        PopulateList(newString);
                    }
                }
            }
        }


        //För Excel

        //public void PopulateList(string scanData)
        //{
        //    try
        //    {
        //        data.Add(Excel.CompareXMLWithData(scanData));
        //        QueueList.Dispatcher.Invoke(() =>
        //        {
        //            QueueList.ItemsSource = null;
        //            QueueList.ItemsSource = data;
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        StatusBar.Dispatcher.Invoke(() =>
        //        {
        //            StatusBar.Content = e.Message;
        //        });
        //    }
        //}

        //För QD-Databas

        public void PopulateList(string scanData)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;
            try
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if(scanData == "\u0018")
                    {
                        ScannerData product = new ScannerData
                        {
                            ArticleNumber = "No Read",
                            ArticleName = "No Read"
                        };
                        data.Add(product);
                    }
                    else 
                    { 
                        DBCom db = new DBCom();
                        ScannerData product = db.DBGet(scanData);

                        if (product == null)
                        {
                            ScannerData notFound = new ScannerData
                            {
                                ArticleNumber = scanData,
                                ArticleName = "Kunde ej hitta artikel"
                            };
                            data.Add(notFound);
                            StatusBar.Dispatcher.Invoke(() =>
                            {
                                StatusBar.Content = "Kunde ej hitta artikel: " + scanData;
                            });
                        }
                        else
                        {
                            DateTime printDate = datePick.SelectedDate.Value;
                            string weekETI = cal.GetWeekOfYear(printDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek).ToString("00");
                            string yearETI = printDate.ToString("yy");
                            string printETI = yearETI + weekETI;

                            product.PrintDate = printETI;
                            data.Add(product);
                        }
                    }

                });

                QueueList.Dispatcher.Invoke(() =>
                {
                    QueueList.ItemsSource = null;
                    QueueList.ItemsSource = data;
                });
            }
            catch (Exception e)
            {
                StatusBar.Dispatcher.Invoke(() =>
                {
                    StatusBar.Content = e.Message + scanData;
                });
            }
        }

        private void SendPrint()
        {
            try
            {
                if (data.Count >= 1)
                {
                    if(data[0].ArticleNumber.ToString() == "No Read" || data[0].ArticleName.ToString() == "Kunde ej hitta artikel" || data[0].ArticleName.ToString() == "Tom artikel")
                    {
                        PrintCom.SendError41(data[0].ArticleName);
                        data.RemoveAt(0);
                    }
                    else
                    {
                        string articleBC = data[0].ArticleNumber.Replace(".", "");
                        PrintCom.SendCMD41(data[0].UnitLoadStackingCapacity, data[0].UnitLoadFootprint1, data[0].UnitLoadFootprint2, data[0].ArticleNumber, articleBC, data[0].Supplier, data[0].Quantity, data[0].GrossWeight, data[0].PrintDate);
                        data.RemoveAt(0);
                    }
                    
                }
                QueueList.Dispatcher.Invoke(() =>
                {
                    QueueList.ItemsSource = null;
                    QueueList.ItemsSource = data;
                });
                printSent = true;
            }
            catch(Exception e)
            {
                StatusBar.Dispatcher.Invoke(() =>
                {
                    StatusBar.Content = "Fel: " + e.Message;
                });
            }
            
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ScannerData emptyProd = new ScannerData()
            {
                ArticleName = "Tom artikel",
                ArticleNumber = "Tom artikel"
            };

            data.Insert(0, emptyProd);

            QueueList.Dispatcher.Invoke(() =>
            {
                QueueList.ItemsSource = null;
                QueueList.ItemsSource = data;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                data.RemoveAt(QueueList.SelectedIndex);

                QueueList.Dispatcher.Invoke(() =>
                {
                    QueueList.ItemsSource = null;
                    QueueList.ItemsSource = data;
                });
            }
            catch(Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }
    }
}
