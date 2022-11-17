using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
            // 借阅历史中的记录
            this.Type = "1";

            this.chargingItem = itemWrapper.Item;
            this.relatedItem = itemWrapper.RelatedItem;



            // 从还书记录中获取信息
            this.ReturnTimeOriginal = this.chargingItem.OperTime;
            this.ReturnDate = new DateItem(DateTimeUtil.ParseFreeTimeString(this.ReturnTimeOriginal));
            //DateTime day = DateTimeUtil.ParseFreeTimeString(this.ReturnTimeOriginal);
            //this.ReturnYear = day.ToString("yyyy");//DateTimeUtil.ToYearString(day);
            //this.ReturnMonth = day.ToString("yyyy/MM"); //DateTimeUtil.ToMonthString(day);
            //this.ReturnDay = day.ToString("yyyy/MM/dd");
            //this.ReturnTime = day.ToString("yyyy-MM-dd HH:mm:ss");  //转换成统计的格式，方便排序

            // 季度
            // day.AddMonths(0 - ((day.Month - 1) % 3)).ToString("yyyy-MM-01");

            this.ItemBarcode = this.chargingItem.ItemBarcode;
            this.PatronBarcode = this.chargingItem.PatronBarcode;


            // 从借书记录中获取信息
            this.BorrowTimeOriginal = this.relatedItem.OperTime;
            this.BorrowDate = new DateItem(DateTimeUtil.ParseFreeTimeString(this.BorrowTimeOriginal));
            //day = DateTimeUtil.ParseFreeTimeString(this.BorrowTimeOriginal);
            //this.BorrowYear = day.ToString("yyyy");// DateTimeUtil.ToYearString(day);
            //this.BorrowMonth = day.ToString("yyyy/MM");// DateTimeUtil.ToMonthString(day);
            //this.BorrowDay= day.ToString("yyyy/MM/dd");//
            //this.BorrowTime = day.ToString("yyyy-MM-dd HH:mm:ss"); //转换成统计的格式，方便排序

            this.BorrowPeriod = this.relatedItem.Period;


        }


        public BorrowedItem(string patronBarcode,XmlNode borrowNode)
        {
            // 在借图书
            this.Type = "0";

            /*
              <borrow barcode="B001" oi="" recPath="中文图书实体/13" biblioRecPath="中文图书/6" 
            location="流通库" borrowDate="Thu, 10 Nov 2022 16:30:31 +0800" borrowPeriod="31day" 
            borrowID="082c2aad-f092-448f-94b5-4291df159e85" returningDate="Sun, 11 Dec 2022 12:00:00 +0800" 
            operator="supervisor" type="普通" price="CNY198.00" /> 
             */

            this.ItemBarcode = DomUtil.GetAttr(borrowNode, "barcode");// this.chargingItem.ItemBarcode;
            this.PatronBarcode = patronBarcode;

            // 从借书记录中获取信息
            this.BorrowTimeOriginal =   DateTimeUtil.LocalTime(DomUtil.GetAttr(borrowNode, "borrowDate"));
            this.BorrowDate = new DateItem(DateTimeUtil.ParseFreeTimeString(this.BorrowTimeOriginal));
           //DateTime day = DateTimeUtil.ParseFreeTimeString(this.BorrowTimeOriginal);
           // this.BorrowYear = day.ToString("yyyy");// DateTimeUtil.ToYearString(day);
           // this.BorrowMonth = day.ToString("yyyy/MM");// DateTimeUtil.ToMonthString(day);
           // this.BorrowDay = day.ToString("yyyy/MM/dd");//
           // this.BorrowTime = day.ToString("yyyy-MM-dd HH:mm:ss"); //转换成统计的格式，方便排序

            string strPeriod = DomUtil.GetAttr(borrowNode, "borrowPeriod");

            // 从还书记录中获取信息
            //this.ReturnTime = this.chargingItem.OperTime;
            //DateTime day = DateTimeUtil.ParseFreeTimeString(this.ReturnTime);
            //this.ReturnYear = day.ToString("yyyy");//DateTimeUtil.ToYearString(day);
            //this.ReturnMonth = day.ToString("yyyy/MM"); //DateTimeUtil.ToMonthString(day);
            //this.ReturnDay = day.ToString("yyyy/MM/dd");







        }

        // 还书时间
        public string ReturnTimeOriginal { get; set; }  //从服务器获取的原始时间

        // 还书日期对象，用一个日期对象，里面有年/季度/月/日期/时间 属性
        public DateItem ReturnDate { get; set; }

        //public string ReturnTime { get; set; }
        //public string ReturnYear { get; set; }
        //public string ReturnMonth { get; set; }
        //public string ReturnDay { get; set; }

        // 册条码
        public string ItemBarcode { get; set; }

        // 读者证条码
        public string PatronBarcode { get; set; }


        // 借书时间
        public string BorrowTimeOriginal { get; set; }  //从服务器获取的原始时间
                                                        
        // 借书日期对象，用一个日期对象，里面有年/季度/月/日期/时间 属性
        public DateItem BorrowDate { get; set; }
        //public string BorrowTime { get; set; }
        //public string BorrowYear { get; set; }
        //public string BorrowMonth { get; set; }
        //public string BorrowDay { get; set; }

        // 借期
        public string BorrowPeriod { get; set; }
        public string AccessNo { get; internal set; }
        public string BigClass { get; internal set; }
        public string Title { get; internal set; }
        public string ErrorInfo { get; internal set; }
        public string Location { get; internal set; }



        // 类型，0表示在借未还的，1表示借阅历史中的，排序的时候，先按type排，再按借书时倒序排
        public string Type { get; set; }

        public string Dump()
        {
            return "ItemBarcode=" + ItemBarcode + ";"
                + "Title=" + Title + ";"
                //+ "BorrowYear=" + BorrowYear + ";"
                //+ "BorrowMonth=" + BorrowMonth + ";"
                + "BorrowDate=" + BorrowDate.Date + ";"
                //+ "BorrowTime=" + BorrowTime + ";"
                //+ "ReturnYear=" + ReturnYear + ";"
                //+ "ReturnMonth=" + ReturnMonth + ";"
                + "ReturnDate=" + ReturnDate.Date + ";"
                //+ "ReturnTime=" + ReturnTime + ";";
                + "AccessNo=" + AccessNo + ";"
                + "BigClass=" + BigClass + ";"
                + "Location=" + Location + ";"
                + "Error="+ErrorInfo;

        }


    }

    public class DateItem
    {

        public DateItem(DateTime day)
        {
            this.Year = day.ToString("yyyy");//DateTimeUtil.ToYearString(day);
            this.Quarter=DateTimeUtil.GetQuarter(day);
            this.Month = day.ToString("yyyy/MM"); //DateTimeUtil.ToMonthString(day);
            this.Date = day.ToString("yyyy/MM/dd");
            this.Time = day.ToString("yyyy-MM-dd HH:mm:ss");  //转换成统计的格式，方便排序

        }

        public string Year { get; set; }
        public string Quarter { get; set; }  //季度
        public string Month { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
    }
}
