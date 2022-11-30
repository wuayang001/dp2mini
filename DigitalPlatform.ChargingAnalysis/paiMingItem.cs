using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DigitalPlatform.ChargingAnalysis
{

    public class paiMingItem
    {
        public paiMingItem(string barcode, int count, string fileName, XmlDocument dom)
        {
            this.PatronBarcode = barcode;
            this.totalBorrowedCount = count;
            this.fileName = fileName;
            this.dom = dom;
        }

        public string PatronBarcode;
        public int totalBorrowedCount;

        public string fileName;
        public XmlDocument dom;
    }
}
