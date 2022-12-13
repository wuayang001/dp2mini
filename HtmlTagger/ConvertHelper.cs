using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalPlatform;
//using DigitalPlatform.Xml;
using DigitalPlatform.Xml;
using System.Xml;
using System.Xml.Linq;

namespace xml2html
{
    public class ConvertHelper
    {

        //  xml转html todo
        public static void Convert(
            string source_filename, // 源文件名
                  string target_filename)//目标文件名


        {
            XmlDocument dom = new XmlDocument();
            dom.Load(source_filename);  //打开源文件

            using (var sw = new StreamWriter(target_filename,
               false,  // append
               Encoding.UTF8))
            {
                var style_TheDebitBooks = new HtmlTagger("style")
                    .SetInnerText("  * {\r\n        margin: 0;\r\n        padding: 0;\r\n        background-color: #b2d6fd;\r\n        font-family: Microsoft YaHei;\r\n    }\r\n    .bookReview {\r\n        margin: 30px auto;\r\n        text-align: center;\r\n    }\r\n    .headline {\r\n        margin-left: 35px;\r\n        font-size: 20px;\r\n        margin-top: 10px;\r\n    }\r\n    table {\r\n        text-align: left;\r\n        background-color: white;\r\n        width: 95%;\r\n        margin: 0 auto;\r\n        border-left-width: 8px;\r\n        border-right-width: 8px;\r\n        border-color: white;\r\n        margin-top: 10px;\r\n    }\r\n    table tr td {\r\n        background-color: white;\r\n        line-height: 30px;\r\n        text-indent: 1rem;\r\n        border-top: 1px dotted #dddddd;\r\n        border-right: 1px dotted #dddddd;\r\n        font-size: 16px;\r\n    }\r\n    .title td {\r\n        background-color: #ccc;\r\n    }\r\n    .comment {\r\n        font-size: 16px;\r\n        margin-left: 35px;\r\n        margin-top: 10px;\r\n    }\r\n    .company {\r\n        text-align: center;\r\n        margin-top: 30px;\r\n        color: #999999;\r\n        padding-bottom: 30px;\r\n    }\r\n    .borrowInfo {\r\n        margin-left: 35px;\r\n        margin-top: 15px;\r\n        font-size: 16px;\r\n    }");

                sw.WriteLine(style_TheDebitBooks.ToString());
                //标题
                var H1_TheDebitBooks = new HtmlTagger("H1")  //添加一个H1标签
                    .AddCssClass("bookReview")
                   .SetInnerText("读者借阅报告");

                sw.WriteLine(H1_TheDebitBooks.ToString());

                //读者基本信息
                void parton()
                {
                    XmlNode patronNode = dom.DocumentElement.SelectSingleNode("patron");
                    var barcode = DomUtil.GetElementText(patronNode, "barcode");
                    var patronName = DomUtil.GetElementText(patronNode, "name");
                    var tel = DomUtil.GetElementText(patronNode, "tel");
                    var readerType = DomUtil.GetElementText(patronNode, "readerType");
                    var refID = DomUtil.GetElementText(patronNode, "refID");
                    var libraryCode = DomUtil.GetElementText(patronNode, "libraryCode");
                    var borrowedNum = DomUtil.GetElementText(patronNode, "info/item[@name='可借总册数']");
                    var canBorrowNum = DomUtil.GetElementText(patronNode, "info/item[@name='当前还可借']");

                    var H2_readersInformation = new HtmlTagger("H2")
                        .AddCssClass("headline")
                        .SetInnerText("读者基本信息");
                    sw.WriteLine(H2_readersInformation.ToString());
                    var table_readersInformation = new HtmlTagger("table").AddCssClass("readersInformationTable");
                    void getLabel(string a, string b, string c)
                    {
                        var tr = HtmlTagger.Create("tr")
                      .AddChild(HtmlTagger.Create("td", a))
                       .AddCssClass(b)
                      .AddChild(HtmlTagger.Create("td", c));
                        table_readersInformation.AddChild(tr);
                    };

                    getLabel("证条码号", "code", barcode);
                    getLabel("姓名", "name", patronName);
                    getLabel("电话", "tel", tel);
                    getLabel("读者类型", "readerType", readerType);
                    getLabel("refID", "refID", refID);
                    getLabel("图书馆代码", "libraryCode", libraryCode);
                    getLabel("可借总册数", "borrowNum", borrowedNum);
                    getLabel("当前还可借", "returnBookNum", canBorrowNum);
                    sw.WriteLine(table_readersInformation.ToString());

                    /*
                   var barcode_node = dom.DocumentElement.SelectSingleNode("patron/barcode"); //证条码号
                   var barcode = barcode_node.InnerText;
                   var name_node = dom.DocumentElement.SelectSingleNode("patron/name");   //名字
                   var Name = name_node.InnerText;
                   var tel_node = dom.DocumentElement.SelectSingleNode("patron/tel");  //电话
                   var tel = tel_node.InnerText;
                   var readerType_node = dom.DocumentElement.SelectSingleNode("patron/readerType");   //读者类型
                   var readerType = readerType_node.InnerText;
                   var refID_node = dom.DocumentElement.SelectSingleNode("patron/refID");  //refID
                   var refID = refID_node.InnerText;
                   var libraryCode_node = dom.DocumentElement.SelectSingleNode("patron/libraryCode");  //图书馆代码
                   var libraryCode = libraryCode_node.InnerText;
                   var borrowNum_node = dom.DocumentElement.SelectSingleNode("patron/info/item[@name='可借总册数']"); //可借总册数
                   var borrowNum = borrowNum_node.Attributes["value"].Value;
                   var returnBookNum_node = dom.DocumentElement.SelectSingleNode("patron/info/item[@name='当前还可借']"); //当前还可借
                   var returnBookNum = returnBookNum_node.Attributes["value"].Value;
                   
                    getLabel("证条码号", "code", barcode);
                    getLabel("姓名", "name", Name);
                    getLabel("电话", "tel", tel);
                    getLabel("读者类型", "readerType", readerType);
                    getLabel("refID", "refID", refID);
                    getLabel("图书馆代码", "libraryCode", libraryCode);
                    getLabel("可借总册数", "borrowNum", borrowNum);
                    getLabel("当前还可借", "returnBookNum", returnBookNum);
                    sw.WriteLine(table_readersInformation.ToString());
                    */

                }
                parton();

                /*
                var tr_code = HtmlTagger.Create("tr")
                    .AddChild(HtmlTagger.Create("td", "证条码号"))
                     .AddCssClass("code")
                    .AddChild(HtmlTagger.Create("td", barcode));
                table_readersInformation.AddChild(tr_code);

                var tr_name = HtmlTagger.Create("tr")
                  .AddChild(HtmlTagger.Create("td", "姓名"))
                   .AddCssClass("name")
                  .AddChild(HtmlTagger.Create("td", Name));
                table_readersInformation.AddChild(tr_name);


                var tr_tel = HtmlTagger.Create("tr")
                 .AddChild(HtmlTagger.Create("td", "电话"))
                  .AddCssClass("tel")
                 .AddChild(HtmlTagger.Create("td", tel));
                table_readersInformation.AddChild(tr_tel);

                var tr_readerType = HtmlTagger.Create("tr")
                   .AddChild(HtmlTagger.Create("td", "读者类型"))
                    .AddCssClass("readerType")
                   .AddChild(HtmlTagger.Create("td", readerType));
                table_readersInformation.AddChild(tr_readerType);

                var tr_refID = HtmlTagger.Create("tr")
                   .AddChild(HtmlTagger.Create("td", "refID"))
                    .AddCssClass("refID")
                   .AddChild(HtmlTagger.Create("td", refID));
                table_readersInformation.AddChild(tr_refID);

                var tr_libraryCode = HtmlTagger.Create("tr")
                   .AddChild(HtmlTagger.Create("td", "图书馆代码"))
                    .AddCssClass("libraryCode")
                   .AddChild(HtmlTagger.Create("td", libraryCode));
                table_readersInformation.AddChild(tr_libraryCode);

                var tr_borrowNum = HtmlTagger.Create("tr")
                  .AddChild(HtmlTagger.Create("td", "可借总册数"))
                   .AddCssClass("libraryCode")
                  .AddChild(HtmlTagger.Create("td", borrowNum));
                table_readersInformation.AddChild(tr_borrowNum);

                var tr_returnBookNum = HtmlTagger.Create("tr")
                 .AddChild(HtmlTagger.Create("td", "当前还可借"))
                  .AddCssClass("returnBookNum")
                 .AddChild(HtmlTagger.Create("td", returnBookNum));
                table_readersInformation.AddChild(tr_returnBookNum);
                */


                //第一次借书时间
                void fast_borrow()
                {
                    var borrowInfo_node = dom.DocumentElement.SelectSingleNode("borrowInfo[@firstBorrowDate]");
                    var borrowInfotime = borrowInfo_node.Attributes["firstBorrowDate"].Value;
                    var borrowInfos_node = dom.DocumentElement.SelectSingleNode("borrowInfo[@totalBorrowedCount]");
                    var totalBorrowedCount = borrowInfos_node.Attributes["totalBorrowedCount"].Value;

                    //判断是否有借书
                    if (string.IsNullOrEmpty(borrowInfotime) == false)
                    {

                        var p_borrowInfo = new HtmlTagger("p")
                        .AddCssClass("borrowInfo")
                        .SetInnerText("您首次借阅时间为" + borrowInfotime + ",共借阅" + totalBorrowedCount + "册。");
                        sw.WriteLine(p_borrowInfo.ToString());
                    }
                }

                fast_borrow();

                var borrows_list = dom.DocumentElement.SelectNodes("borrows/borrowItem");
                //在借图书
                void borrow()
                {
                    var H2_TheDebitBooks = new HtmlTagger("H2")
                        .AddCssClass("headline")
                       .SetInnerText("在借图书");

                    sw.WriteLine(H2_TheDebitBooks.ToString());

                    var table_TheDebitBooks = new HtmlTagger("table");
                    var th_TheDebitBooks = HtmlTagger.Create("tr")
                        .AddCssClass("title")
                        .AddChild(HtmlTagger.Create("td", "册条码"))
                        .AddChild(HtmlTagger.Create("td", "题名与责任者"))
                        .AddChild(HtmlTagger.Create("td", "索取号"))
                        .AddChild(HtmlTagger.Create("td", "借书时间"))
                        .AddChild(HtmlTagger.Create("td", "还书时间"));

                    table_TheDebitBooks.AddChild(th_TheDebitBooks);

                    foreach (XmlElement item in borrows_list)
                    {
                        var itemBarcode = item.GetAttribute("itemBarcode");
                        var title = item.GetAttribute("title");
                        var accessNo = item.GetAttribute("accessNo");
                        var borrowTime = item.GetAttribute("borrowTime");
                        var returnTime = item.GetAttribute("returnTime");

                        var tr_TheDebitBooks = HtmlTagger.Create("tr")

                            .AddChild(HtmlTagger.Create("td", itemBarcode))
                            .AddChild(HtmlTagger.Create("td", title))
                            .AddChild(HtmlTagger.Create("td", accessNo))
                            .AddChild(HtmlTagger.Create("td", borrowTime))
                            .AddChild(HtmlTagger.Create("td", returnTime));

                        table_TheDebitBooks.AddChild(tr_TheDebitBooks);
                    }
                    //borrow();
                    sw.WriteLine(table_TheDebitBooks.ToString());

                }
                if (borrows_list.Count > 0)
                    borrow();

                //if(_borrow==1)
                //borrow();
                //借阅历史
                var history_list = dom.DocumentElement.SelectNodes("borrowHistory/borrowItem");

                void borrwHistory()
                {
                    var H2_BorrowHistory = new HtmlTagger("H2")
                        .AddCssClass("headline")
                      .SetInnerText("借阅历史");
                    sw.WriteLine(H2_BorrowHistory.ToString());
                    var table_BorrowHistory = new HtmlTagger("table");
                    var th_BorrowHistory = HtmlTagger.Create("tr")
                         .AddCssClass("title")
                          .AddChild(HtmlTagger.Create("td", "册条码"))
                          .AddChild(HtmlTagger.Create("td", "题名与责任者"))
                          .AddChild(HtmlTagger.Create("td", "索取号"))
                          .AddChild(HtmlTagger.Create("td", "借书时间"))
                          .AddChild(HtmlTagger.Create("td", "还书时间"));
                    table_BorrowHistory.AddChild(th_BorrowHistory);
                    foreach (XmlElement item in history_list)
                    {
                        var itemBarcode = item.GetAttribute("itemBarcode");
                        var title = item.GetAttribute("title");
                        var accessNo = item.GetAttribute("accessNo");
                        var borrowTime = item.GetAttribute("borrowTime");
                        var returnTime = item.GetAttribute("returnTime");

                        var tr_BorrowHistory = HtmlTagger.Create("tr")

                            .AddChild(HtmlTagger.Create("td").SetInnerText(itemBarcode)) // 这是复杂形态
                            .AddChild(HtmlTagger.Create("td", title))   // 这是简化的形态
                            .AddChild(HtmlTagger.Create("td", accessNo))
                            .AddChild(HtmlTagger.Create("td", borrowTime))
                            .AddChild(HtmlTagger.Create("td", returnTime));

                        table_BorrowHistory.AddChild(tr_BorrowHistory);
                    }
                    if (history_list != null)
                        sw.WriteLine(table_BorrowHistory.ToString());
                }
                if (history_list.Count > 0)
                    borrwHistory();

                var div_overall = new HtmlTagger("div").AddCssClass("overall");

                //中图法分类统计
                var clcGroup_list = dom.DocumentElement.SelectNodes("clcGroup/clcItem");

                void clc()
                {
                    var div_CLC = HtmlTagger.Create("div").AddCssClass("CLC");
                    var H2_CLC = HtmlTagger.Create("H2")
                        .AddCssClass("headline")
                    .SetInnerText("中图法分类统计");
                    div_CLC.AddChild(H2_CLC);
                    var table_CLC = HtmlTagger.Create("table");
                    var th_CLC = HtmlTagger.Create("tr")
                        .AddCssClass("title")
                         .AddChild(HtmlTagger.Create("td", "分类"))
                         .AddChild(HtmlTagger.Create("td", "分类名称"))
                         .AddChild(HtmlTagger.Create("td", "数量"));

                    table_CLC.AddChild(th_CLC);
                    foreach (XmlElement item in clcGroup_list)
                    {
                        var name = item.GetAttribute("name");
                        var caption = item.GetAttribute("caption");
                        var count = item.GetAttribute("count");

                        var tr_CLC = HtmlTagger.Create("tr")
                            .AddChild(HtmlTagger.Create("td", name))
                            .AddChild(HtmlTagger.Create("td", caption))
                            .AddChild(HtmlTagger.Create("td", count));
                        table_CLC.AddChild(tr_CLC);
                    }
                    div_CLC.AddChild(table_CLC);
                    div_overall.AddChild(div_CLC);
                }
                if (clcGroup_list.Count > 0)
                    clc();

                //按年份统计
                var yearGroup_list = dom.DocumentElement.SelectNodes("yearGroup/yearItem");

                void year()
                {
                    var div_year = HtmlTagger.Create("div").AddCssClass("year");
                    var H2_year = HtmlTagger.Create("H2")
                        .AddCssClass("headline")
                  .SetInnerText("按年份统计");
                    div_year.AddChild(H2_year);
                    var table_year = HtmlTagger.Create("table");
                    var th_year = HtmlTagger.Create("tr")
                         .AddCssClass("title")
                        .AddChild(HtmlTagger.Create("td", "年份"))
                        .AddChild(HtmlTagger.Create("td", "借阅量"));
                    table_year.AddChild(th_year);
                    foreach (XmlElement item in yearGroup_list)
                    {
                        var name = item.GetAttribute("name");
                        var count = item.GetAttribute("count");

                        var tr_year = HtmlTagger.Create("tr")
                            .AddChild(HtmlTagger.Create("td", name))
                            .AddChild(HtmlTagger.Create("td", count));
                        table_year.AddChild(tr_year);
                    }
                    div_year.AddChild(table_year);
                    div_overall.AddChild(div_year);

                }
                if (yearGroup_list.Count > 0)
                    year();

                //按季度统计
                var quarterGroup_list = dom.DocumentElement.SelectNodes("quarterGroup/quarterItem");

                void quarter()
                {

                    var div_quarter = HtmlTagger.Create("div").AddCssClass("quarter");
                    var H2_quarter = HtmlTagger.Create("H2")
                        .AddCssClass("headline")
                .SetInnerText("按季度统计");
                    div_quarter.AddChild(H2_quarter);
                    var table_quarter = HtmlTagger.Create("table");
                    var th_quarter = HtmlTagger.Create("tr")
                        .AddCssClass("title")
                       .AddChild(HtmlTagger.Create("td", "季度"))
                       .AddChild(HtmlTagger.Create("td", "借阅量"));
                    table_quarter.AddChild(th_quarter);
                    foreach (XmlElement item in quarterGroup_list)
                    {
                        var name = item.GetAttribute("name");
                        var count = item.GetAttribute("count");
                        var tr_quarter = HtmlTagger.Create("tr")
                            .AddChild(HtmlTagger.Create("td", name))
                            .AddChild(HtmlTagger.Create("td", count));
                        table_quarter.AddChild(tr_quarter);
                    }
                    div_quarter.AddChild(table_quarter);
                    div_overall.AddChild(div_quarter);
                }
                if (quarterGroup_list.Count > 0)
                    quarter();

                //按月份统计
                var monthGroupGroup_list = dom.DocumentElement.SelectNodes("monthGroup/monthItem");

                void month()
                {
                    var div_month = HtmlTagger.Create("div").AddCssClass("month");
                    var H2_month = HtmlTagger.Create("H2")
                        .AddCssClass("headline")
              .SetInnerText("按月份统计");
                    div_month.AddChild(H2_month);
                    var table_month = HtmlTagger.Create("table");
                    var th_month = HtmlTagger.Create("tr")
                        .AddCssClass("title")
                       .AddChild(HtmlTagger.Create("td", "月份"))
                       .AddChild(HtmlTagger.Create("td", "借阅量"));


                    table_month.AddChild(th_month);
                    foreach (XmlElement item in monthGroupGroup_list)
                    {
                        var name = item.GetAttribute("name");
                        var count = item.GetAttribute("count");
                        var tr_month = HtmlTagger.Create("tr")
                            .AddChild(HtmlTagger.Create("td", name))
                            .AddChild(HtmlTagger.Create("td", count));
                        table_month.AddChild(tr_month);
                    }
                    div_month.AddChild(table_month);
                    div_overall.AddChild(div_month);
                    sw.WriteLine(div_overall.ToString());
                }
                if (monthGroupGroup_list.Count > 0)
                    month();

                //馆长评语
                var comment_node = dom.DocumentElement.SelectSingleNode("comment");

                void remark()
                {
                    var H2_remark = new HtmlTagger("H2")
                       .AddCssClass("headline")
                        .SetInnerText("馆长评语");
                    sw.WriteLine(H2_remark.ToString());
                    var comment = comment_node.InnerText;
                    var p_remark = HtmlTagger.Create("p")
                        .AddCssClass("comment")
                        .SetInnerText(comment);
                    sw.WriteLine(p_remark.ToString());
                }
                //if(_remark==1)
                if (comment_node != null)
                    remark();

                //公司名称
                void company()
                {
                    var p_company = new HtmlTagger("p")
                        .AddCssClass("company")
                        .SetInnerText("数字平台（北京）软件有限责任公司");
                    sw.WriteLine(p_company.ToString());
                }
                //if(_company==1)
                company();
            }
        }
    }
}
