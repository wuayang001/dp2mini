using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.ChargingAnalysis
{
    // 借过的书
    public class BorrowedItem
    {
        /*
            "Item": {
                "Action": "return",
                "BiblioRecPath": null,
                "ClientAddress": "localhost",
                "Id": "634cc393c0e85810342d5845",
                "ItemBarcode": "B001",
                "LibraryCode": "",
                "No": null,
                "OperTime": "2022/10/17 10:53:07",
                "Operation": "return",
                "Operator": "ryh",
                "PatronBarcode": "P001",
                "Period": null,
                "Volume": null
            },
         */
        public ChargingItem chargingItem { get; set; }

        /*
    "RelatedItem": {
        "Action": "borrow",
        "BiblioRecPath": null,
        "ClientAddress": "localhost",
        "Id": "631feda0c0e8580ff0ee2e1c",
        "ItemBarcode": "B001",
        "LibraryCode": "",
        "No": "0",
        "OperTime": "2022/9/13 10:40:32",
        "Operation": "borrow",
        "Operator": "supervisor",
        "PatronBarcode": "P001",
        "Period": "31day",
        "Volume": null
    }
 */
        public ChargingItem relatedItem { get; set; }


        public BorrowedItem(ChargingItemWrapper itemWrapper)
        {
            this.chargingItem = itemWrapper.Item;
            this.relatedItem = itemWrapper.RelatedItem;

            // 从还书记录中获取信息
            this.ReturnTime = this.chargingItem.OperTime;
            DateTime day = DateTimeUtil.ParseFreeTimeString(this.ReturnTime);
            this.ReturnYear = day.ToString("yyyy");//DateTimeUtil.ToYearString(day);
            this.ReturnMonth = day.ToString("yyyy/MM"); //DateTimeUtil.ToMonthString(day);
            this.ReturnDay = day.ToString("yyyy/MM/dd");

            this.ItemBarcode = this.chargingItem.ItemBarcode;
            this.PatronBarcode = this.chargingItem.PatronBarcode;


            // 从借书记录中获取信息
            this.BorrowTime = this.relatedItem.OperTime;
            day = DateTimeUtil.ParseFreeTimeString(this.BorrowTime);
            this.BorrowYear = day.ToString("yyyy");// DateTimeUtil.ToYearString(day);
            this.BorrowMonth = day.ToString("yyyy/MM");// DateTimeUtil.ToMonthString(day);
            this.BorrowDay= day.ToString("yyyy/MM/dd");//

            this.BorrowPeriod = this.relatedItem.Period;


        }

        // 还书时间
        public string ReturnTime { get; set; }
        public string ReturnYear { get; set; }
        public string ReturnMonth { get; set; }
        public string ReturnDay { get; set; }

        // 册条码
        public string ItemBarcode { get; set; }

        // 读者证条码
        public string PatronBarcode { get; set; }


        // 借书时间
        public string BorrowTime { get; set; }
        public string BorrowYear { get; set; }
        public string BorrowMonth { get; set; }
        public string BorrowDay { get; set; }

        // 借期
        public string BorrowPeriod { get; set; }
        public string AccessNo { get; internal set; }
        public string BigClass { get; internal set; }
        public string Title { get; internal set; }
        public string ErrorInfo { get; internal set; }
        public string Location { get; internal set; }

        public string Dump()
        {
            return "ItemBarcode=" + ItemBarcode + ";"
                + "Title=" + Title + ";"
                //+ "BorrowYear=" + BorrowYear + ";"
                //+ "BorrowMonth=" + BorrowMonth + ";"
                + "BorrowDay=" + BorrowDay + ";"
                //+ "BorrowTime=" + BorrowTime + ";"
                //+ "ReturnYear=" + ReturnYear + ";"
                //+ "ReturnMonth=" + ReturnMonth + ";"
                + "ReturnDay=" + ReturnDay + ";"
                //+ "ReturnTime=" + ReturnTime + ";";
                + "AccessNo=" + AccessNo + ";"
                + "BigClass=" + BigClass + ";"
                + "Location=" + Location + ";"
                + "Error="+ErrorInfo;

        }


    }
}
