﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using Telegram.Bot;

namespace KV4S.AmateurRadio.IRLP.StationTracking
{
    class Program
    {
        public static string URL = "http://status.irlp.net/index.php?PSTART=9";

        //load from App.config

        //telegram
        public static TelegramBotClient bot = new TelegramBotClient(ConfigurationManager.AppSettings["BotToken"]);
        public static string destinationID = ConfigurationManager.AppSettings["DestinationID"];
        public static int intSleepTime = 2000;

        //email
        public static MailAddress from = new MailAddress(ConfigurationManager.AppSettings["EmailFrom"]);
        public static string toConfig = ConfigurationManager.AppSettings["EmailTo"];
        public static string smtpHost = ConfigurationManager.AppSettings["SMTPHost"];
        public static string smtpPort = ConfigurationManager.AppSettings["SMTPPort"];
        public static string smtpUser = ConfigurationManager.AppSettings["SMTPUser"];
        public static string smtpPswrd = ConfigurationManager.AppSettings["SMTPPassword"];

        private static List<string> _callsignList = null;
        private static string CallsignListString
        {
            set
            {
                string[] callsignArray = value.ToUpper().Split(',');
                _callsignList = new List<string>(callsignArray.Length);
                _callsignList.AddRange(callsignArray);
            }
        }

        private static List<string> _emailAddressList = null;
        private static string EmailAddressListString
        {
            set
            {
                string[] emailAddressArray = value.Split(',');
                _emailAddressList = new List<string>(emailAddressArray.Length);
                _emailAddressList.AddRange(emailAddressArray);
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
                    
                    if (irlpHTML.Contains("9050") &&        //seeing if the html includes the largest reflector. sometimes the data isn't loaded when the html is loaded.
                        irlpHTML.Contains("<hr><center>"))  //when writing html file out to disk saw bad symbols or blank spaces on some downloads and represents a bad download.
                    {
                        CallsignListString = ConfigurationManager.AppSettings["Callsigns"].ToUpper();
                        foreach (string callsign in _callsignList)
                        {
                            Console.WriteLine("Checking station " + callsign);
                            //callsign split
                            string[] strCallsignSplit = new string[] { callsign };
                            string[] strRowSplit = irlpHTML.Split(strCallsignSplit, StringSplitOptions.RemoveEmptyEntries);

                            //status end split
                            int intSplitIndex = strRowSplit.Count() - 1;
                            string[] strRowEndSplit = new string[] { "</td></tr>" };
                            string[] strStatusendSplit = strRowSplit[intSplitIndex].Split(strRowEndSplit, StringSplitOptions.RemoveEmptyEntries);

                            //status begin split
                            string[] strHtmlSplit = new string[] { "<td>" };
                            string[] strStatusSplit = strStatusendSplit[0].Split(strHtmlSplit, StringSplitOptions.RemoveEmptyEntries);

                            //status
                            intSplitIndex = strStatusSplit.Count() - 1;
                            string status = strStatusSplit[intSplitIndex];

                            if (status == "Status")
                            {
                                Console.WriteLine("Station " + callsign + " is not listed on the IRLP website for tracking.");
                                continue;
                            }

                            if (File.Exists(callsign + ".txt"))
                            {
                                bool updated = false;
                                using (StreamReader sr = File.OpenText(callsign + ".txt"))
                                {
                                    String s = "";

                                    while ((s = sr.ReadLine()) != null)
                                    {
                                        if (!status.Contains("<td"))
                                        {
                                            if (status != s)
                                            {
                                                Console.WriteLine("Station " + callsign + " has changed to " + status);
                                                updated = true;
                                                if (ConfigurationManager.AppSettings["StatusEmails"] == "Y")
                                                {
                                                    Email(callsign, status);
                                                }
                                                if (ConfigurationManager.AppSettings["TelegramStatus"] == "Y")
                                                {
                                                    bot.SendTextMessageAsync(destinationID, "IRLP.StationTracking - Station " + callsign + " has changed to " + status);
                                                    Thread.Sleep(intSleepTime);
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("Station " + callsign + " has not changed. Still " + status);
                                            }
                                        }
                                    }
                                }
                                if (updated)
                                {
                                    if (!status.Contains("<td"))
                                    {
                                        File.Delete(callsign + ".txt");
                                        FileStream fs = null;
                                        fs = new FileStream(callsign + ".txt", FileMode.Append);
                                        StreamWriter log = new StreamWriter(fs);
                                        log.WriteLine(status);
                                        log.Close();
                                        fs.Close();

                                        //debugging/testing only - adding code to write html to file to try to figure out if malformed html is reporting as a disconnect false positive.
                                        //FileStream fs1 = null;
                                        //fs1 = new FileStream("irlpHTML_" + DateTime.Now.ToString("HH_mm_ss") + ".txt", FileMode.Append);
                                        //StreamWriter html = new StreamWriter(fs1);
                                        //html.WriteLine(irlpHTML.ToString());
                                        //html.Close();
                                        //fs1.Close();
                                    }
                                }
                            }
                            else
                            {
                                if (!status.Contains("<td"))
                                {
                                    FileStream fs = null;
                                    fs = new FileStream(callsign + ".txt", FileMode.Append);
                                    StreamWriter log = new StreamWriter(fs);
                                    log.WriteLine(status);
                                    log.Close();
                                    fs.Close();
                                    Console.WriteLine("Station " + callsign + " is now being tracked on the IRLP website. Current status " + status);
                                    if (ConfigurationManager.AppSettings["StatusEmails"] == "Y")
                                    {
                                        Email(callsign, status);
                                    }
                                    if (ConfigurationManager.AppSettings["TelegramStatus"] == "Y")
                                    {
                                        bot.SendTextMessageAsync(destinationID, "IRLP.StationTracking - Station " + callsign + " has changed to " + status);
                                        Thread.Sleep(intSleepTime);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Program encountered and error:");
                Console.WriteLine(ex.Message);
                LogError(ex.Message, ex.Source);
                if (ConfigurationManager.AppSettings["EmailError"] == "Y")
                {
                    EmailError(ex.Message, ex.Source);
                }
                if (ConfigurationManager.AppSettings["TelegramError"] == "Y")
                {
                    bot.SendTextMessageAsync(destinationID, "IRLP.StationTracking Error - Message: " + ex.Message + " Source: " + ex.Source);
                    Thread.Sleep(intSleepTime);
                }
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

        private static void EmailError(string Message, string Source)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.Subject = "IRLP.StationTracking Error";
                mail.From = from;

                EmailAddressListString = toConfig;
                foreach (string emailAddress in _emailAddressList)
                {
                    mail.To.Add(emailAddress);
                }

                mail.Body = "Message: " + Message + " Source: " + Source;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = smtpHost;
                smtp.Port = Convert.ToInt32(smtpPort);

                smtp.Credentials = new NetworkCredential(smtpUser, smtpPswrd);
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Program encountered and an error sending email:");
                Console.WriteLine(ex.Message);
                LogError(ex.Message, ex.Source);
            }
        }

        private static void Email(string callSign, string Status)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.Subject = "IRLP.StationTracking";
                mail.From = from;

                EmailAddressListString = toConfig;
                foreach (string emailAddress in _emailAddressList)
                {
                    mail.To.Add(emailAddress);
                }

                mail.Body = "Station " + callSign + " has changed to " + Status;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = smtpHost;
                smtp.Port = Convert.ToInt32(smtpPort);

                smtp.Credentials = new NetworkCredential(smtpUser, smtpPswrd);
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email:");
                Console.WriteLine(ex.Message);
                LogError(ex.Message, ex.Source);
            }
        }

        private static void LogError(string Message, string source)
        {
            try
            {
                FileStream fs = null;
                fs = new FileStream("ErrorLog.txt", FileMode.Append);
                StreamWriter log = new StreamWriter(fs);
                log.WriteLine(DateTime.Now + " Error: " + Message + " Source: " + source);
                log.Close();
                fs.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Error logging previous error.");
                Console.WriteLine("Make sure the Error log is not open.");
            }
        }
    }
}
