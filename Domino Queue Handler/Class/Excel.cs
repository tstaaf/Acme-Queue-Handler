using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using LinqToExcel;
using Domino_Queue_Handler.Model;

namespace Domino_Queue_Handler.Class
{
    class Excel
    {
        public static ScannerData CompareXMLWithData(string scanData)
        {
            var wb = new LinqToExcel.ExcelQueryFactory(@"C:\Domino\Listener\DB.xlsx");

            wb.AddMapping<ScannerData>(x => x.EAN, "Scannad Kod");
            wb.AddMapping<ScannerData>(x => x.UnitLoadFootprint1, "Unit Load footprint 1");
            wb.AddMapping<ScannerData>(x => x.UnitLoadFootprint2, "Unit Load footprint 2");
            wb.AddMapping<ScannerData>(x => x.UnitLoadStackingCapacity, "Unit Load stacking capacity");
            wb.AddMapping<ScannerData>(x => x.ArticleNumber, "Article Number");
            wb.AddMapping<ScannerData>(x => x.ArticleName, "Art Name");
            wb.AddMapping<ScannerData>(x => x.Supplier, "Supplier");
            wb.AddMapping<ScannerData>(x => x.Quantity, "Quantity");
            wb.AddMapping<ScannerData>(x => x.GrossWeight, "Gross Weight");
            var value = from x in wb.Worksheet<ScannerData>("Blad1")
                        where x.EAN == scanData
                        select x;

            return value.FirstOrDefault();

        }
    }
}
//let item = new ScannerData
//{
//    EAN = row["Scannad Kod"].Cast<string>(),
//    UnitLoadFootprint1 = row["Unit Load footprint 1"].Cast<string>(),
//    UnitLoadFootprint2 = row["Unit Load footprint 2"].Cast<string>(),
//    UnitLoadStackingCapacity = row["Unit Load stacking capacity"].Cast<string>(),
//    ArticleNumber = row["Article Number"].Cast<string>(),
//    Supplier = row["Supplier"].Cast<string>(),
//    Quantity = row["quantity"].Cast<string>(),
//    GrossWeight = row["Gross Weight"].Cast<string>()
//}
