using Domino_Queue_Handler.Class;
using Domino_Queue_Handler.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Domino_Queue_Handler.Windows
{
    /// <summary>
    /// Interaction logic for ignoreListWindow.xaml
    /// </summary>
    public partial class ignoreListWindow : Window
    {

        List<ScannerData> ignoreList = new List<ScannerData>();
        readonly string ignoreListPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public ignoreListWindow()
        {
            InitializeComponent();
            try
            {
                if (File.Exists(ignoreListPath + "\\Domino Queue Handler\\ignorelist.txt"))
                {
                    var lines = File.ReadAllLines(ignoreListPath + "\\Domino Queue Handler\\ignorelist.txt");
                    foreach (var line in lines)
                    {
                        ScannerData prod = new ScannerData
                        {
                            ArticleNumber = line
                        };
                        ignoreList.Add(prod);
                    }
                }
                ignoreListG.Dispatcher.Invoke(() =>
                {
                    ignoreListG.ItemsSource = null;
                    ignoreListG.ItemsSource = ignoreList;
                });
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void LaggTill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ignoreData = artTextBox.Text;

                DBCom db = new DBCom();
                ScannerData product = new ScannerData
                {
                    ArticleNumber = ignoreData
                };

                ignoreList.Add(product);

                ignoreListG.Dispatcher.Invoke(() =>
                {
                    ignoreListG.ItemsSource = null;
                    ignoreListG.ItemsSource = ignoreList;
                });
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message);
            }
            
        }

        private void TaBort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ignoreList.RemoveAt(ignoreListG.SelectedIndex);
                ignoreListG.ItemsSource = null;
                ignoreListG.ItemsSource = ignoreList;
            }
            catch (Exception err)
            {
                MessageBox.Show("Välj en artikel att ta bort.");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                using (TextWriter tw = new StreamWriter(ignoreListPath + "\\Domino Queue Handler\\ignorelist.txt"))
                {
                    foreach (var prod in ignoreList)
                    {
                        tw.WriteLine(prod.ArticleNumber);
                    }
                }
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
    }
}
