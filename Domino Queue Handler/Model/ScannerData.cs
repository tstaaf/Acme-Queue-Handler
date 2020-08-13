using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domino_Queue_Handler.Model
{
    public class ScannerData
    {
        public string EAN { get; set; }
        public string UnitLoadFootprint1 { get; set; }
        public string UnitLoadFootprint2 { get; set; }
        public string UnitLoadStackingCapacity { get; set; }
        public string ArticleNumber { get; set; }
        public string ArticleNumberBC { get; set; }
        public string ArticleName { get; set; }
        public string PrintDate { get; set; }
        public string Supplier { get; set; }
        public string Quantity { get; set; }
        public string GrossWeight { get; set; }

    }

}
