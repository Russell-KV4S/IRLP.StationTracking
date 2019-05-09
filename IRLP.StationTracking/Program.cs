using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IRLP.StationTracking
{
    class Program
    {
        public static string URL = "http://status.irlp.net/index.php?PSTART=9";

        private static List<string> _callsignList = null;
        private static string CallsignListString
        {
            set
            {
                string[] callsignArray = value.Split(',');
                _callsignList = new List<string>(callsignArray.Length);
                _callsignList.AddRange(callsignArray);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Welcome to the IRLP Station Tracker Application by KV4S!");
                Console.WriteLine(" ");
                Console.WriteLine("Beginning download from " + URL);
                Console.WriteLine("Please Stand by.....");
                Console.WriteLine(" ");
                using (WebClient wc = new WebClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var irlpHTML = wc.DownloadString(URL);

                    CallsignListString = ConfigurationManager.AppSettings["Callsigns"];                    
                    foreach (string callsign in _callsignList)
                    {
                        //callsign split
                        string[] strCallsignSplit = new string[] { callsign };
                        string[] strRowSplit = irlpHTML.Split(strCallsignSplit, StringSplitOptions.RemoveEmptyEntries);

                        //status end split
                        string[] strRowEndSplit = new string[] { "</td></tr>" };
                        string[] strStatusendSplit = strRowSplit[1].Split(strRowEndSplit, StringSplitOptions.RemoveEmptyEntries);

                        //status begin split
                        string[] strHtmlSplit = new string[] { "<td>" };
                        string[] strStatusSplit = strStatusendSplit[0].Split(strHtmlSplit, StringSplitOptions.RemoveEmptyEntries);

                        //status
                        string status = strStatusSplit[4];
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Program encountered and error:");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (ConfigurationManager.AppSettings["Unattended"] == "N")
                {
                    Console.WriteLine("Press any key on your keyboard to quit...");
                    Console.ReadKey();
                }
            }
        }
    }
}
