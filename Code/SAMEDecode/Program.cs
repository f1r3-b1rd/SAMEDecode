using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SAMEDecode
{
    class Program
    {
        // Declare smaller lookup lists
        private static List<KeyValuePair<string, string>> originators = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("PEP", "Primary Entry Point"),
            new KeyValuePair<string, string>("CIV", "Civil authorities (local government)"),
            new KeyValuePair<string, string>("WXR", "National Weather Service"),
            new KeyValuePair<string, string>("EAS", "Local EAS Participant Station (broadcasting station)"),
            new KeyValuePair<string, string>("EAN", "Emergency Action Notification Network (Presidential)")
        };

        private static List<KeyValuePair<string, string>> eventCodes = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("ADR", "Administrative Message"),
            new KeyValuePair<string, string>("AVA", "Avalanche Watch"),
            new KeyValuePair<string, string>("AVW", "Avalanche Warning"),
            new KeyValuePair<string, string>("BZW", "Blizzard Warning"),
            new KeyValuePair<string, string>("CAE", "Child Abduction Emergency"),
            new KeyValuePair<string, string>("CDW", "Civil Danger Warning"),
            new KeyValuePair<string, string>("CEM", "Civil Emergency Message"),
            new KeyValuePair<string, string>("CFA", "Coastal Flood Watch"),
            new KeyValuePair<string, string>("CFW", "Coastal Flood Warning"),
            new KeyValuePair<string, string>("DMO", "Practice/Demo Warning"),
            new KeyValuePair<string, string>("DSW", "Dust Storm Warning"),
            new KeyValuePair<string, string>("EAN", "Emergency Action Notification"),
            new KeyValuePair<string, string>("EQW", "Earthquake Warning"),
            new KeyValuePair<string, string>("EVI", "Evacuation Immediate"),
            new KeyValuePair<string, string>("EWW", "Extreme Wind Warning"),
            new KeyValuePair<string, string>("FFA", "Flash Flood Watch"),
            new KeyValuePair<string, string>("FFS", "Flash Flood Statement"),
            new KeyValuePair<string, string>("FFW", "Flash Flood Warning"),
            new KeyValuePair<string, string>("FLA", "Flood Watch"),
            new KeyValuePair<string, string>("FLS", "Flood Statement"),
            new KeyValuePair<string, string>("FLW", "Flood Warning"),
            new KeyValuePair<string, string>("FRW", "Fire Warning"),
            new KeyValuePair<string, string>("HLS", "Hurricane Statement"),
            new KeyValuePair<string, string>("HMW", "Hazardous Materials Warning"),
            new KeyValuePair<string, string>("HUA", "Hurricane Watch"),
            new KeyValuePair<string, string>("HUW", "Hurricane Warning"),
            new KeyValuePair<string, string>("HWA", "High Wind Watch"),
            new KeyValuePair<string, string>("HWW", "High Wind Warning"),
            new KeyValuePair<string, string>("LAE", "Local Area Emergency"),
            new KeyValuePair<string, string>("LEW", "Law Enforcement Warning"),
            new KeyValuePair<string, string>("NIC", "National Information Center"),
            new KeyValuePair<string, string>("NMN", "Network Message Notification"),
            new KeyValuePair<string, string>("NPT", "National Periodic Test"),
            new KeyValuePair<string, string>("NUW", "Nuclear Power Plant Warning"),
            new KeyValuePair<string, string>("RHW", "Radiological Hazard Warning"),
            new KeyValuePair<string, string>("RMT", "Required Monthly Test"),
            new KeyValuePair<string, string>("RWT", "Required Weekly Test"),
            new KeyValuePair<string, string>("SMW", "Special Marine Warning"),
            new KeyValuePair<string, string>("SPS", "Special Weather Statement"),
            new KeyValuePair<string, string>("SPW", "Shelter in Place Warning"),
            new KeyValuePair<string, string>("SSA", "Storm Surge Watch"),
            new KeyValuePair<string, string>("SSW", "Storm Surge Warning"),
            new KeyValuePair<string, string>("SVR", "Severe Thunderstorm Warning"),
            new KeyValuePair<string, string>("SVR", "Severe Thunderstorm Warning"),
            new KeyValuePair<string, string>("SVS", "Severe Weather Statement"),
            new KeyValuePair<string, string>("TOA", "Tornado Watch"),
            new KeyValuePair<string, string>("TOE", "911 Telephone Outage Emergency"),
            new KeyValuePair<string, string>("TOR", "Tornado Warning"),
            new KeyValuePair<string, string>("TRA", "Tropical Storm Watch"),
            new KeyValuePair<string, string>("TRW", "Tropical Storm Warning"),
            new KeyValuePair<string, string>("TSA", "Tsunami Watch"),
            new KeyValuePair<string, string>("TSW", "Tsunami Warning"),
            new KeyValuePair<string, string>("VOW", "Volcano Warning"),
            new KeyValuePair<string, string>("WSA", "Winter Storm Watch"),
            new KeyValuePair<string, string>("WSW", "Winter Storm Warning")
        };

        static void Main(string[] args)
        {
            string finalString = "";
            Console.WriteLine("Specific Area Message Encoding Decoder");
            Console.WriteLine("Enter header in the form of ZCZC-ORG-EEE-PSSCCC+TTTT-JJJHHMM-LLLLLLLL-");
            Console.Write(":");
            string sameHeader = Console.ReadLine();
            // First format check
            if (sameHeader.Length < 42 || sameHeader.Substring(0, 4) != "ZCZC")
            {
                Console.WriteLine("[!] Please make sure your string is in the proper format.");
                return;
            }

            // Organize the string into 2 arrays, split after the PSSCCC location codes
            string[] shEventLoc = sameHeader.Split('+')[0].Split('-');
            string[] shTime = sameHeader.Split('+')[1].Split('-');

            // Get EAS Event Code and Originator Code, make the final message look pretty
            if (!HasKey(originators, shEventLoc[1]))
            {
                Console.WriteLine("[!] Originator code not valid or not supported (ORG).");
                return;
            }
            if (!HasKey(eventCodes, shEventLoc[2]))
            {
                Console.WriteLine("[!] Event code not valid or not supported (EEE).");
                return;
            }
            string originatorCode = originators.First(kvp => kvp.Key == shEventLoc[1]).Value;
            string eventCode = eventCodes.First(kvp => kvp.Key == shEventLoc[2]).Value;
            finalString += (eventCode[0] == 'A' || eventCode[0] == 'E') ? "An " : "A ";
            finalString += eventCode + " has been issued by the " + originatorCode + " for the following counties/areas: ";

            // Get location information from FIPS_national_counties.txt database
            for (int i = 3; i < shEventLoc.Length; ++i)
            {
                if (shEventLoc[i].Length != 6)
                {
                    Console.WriteLine("[!] Location codes not valid or not supported.");
                    return;
                }
                finalString += (i == shEventLoc.Length - 1) ? GetCounty(shEventLoc[i]) : GetCounty(shEventLoc[i]) + ", ";
            }

            // Convert string times and dates to ints for conversion and math
            if (shTime[0].Length != 4 || shTime[1].Length != 7)
            {
                Console.WriteLine("[!] Purge time (TTTT) and/or exact alert time (JJJHHMM) is invalid.");
                return;
            }
            int.TryParse(shTime[1].Substring(0, 3), out int ordinalDate);
            int.TryParse(shTime[1].Substring(3, 2), out int utcHour);
            int.TryParse(shTime[1].Substring(5, 2), out int utcMinute);
            int.TryParse(shTime[0].Substring(0, 2), out int endHour);
            int.TryParse(shTime[0].Substring(2, 2), out int endMinute);
            
            // Convert UTC dates/times to local timezone and change ordinal date to a more friendly date.
            // Format output and create the final string.
            int year = DateTime.Now.Year;
            DateTime utcDate = new DateTime(year, 1, 1, utcHour, utcMinute, 0).AddDays(ordinalDate - 1);
            DateTime date = utcDate.ToLocalTime();
            finalString += " at " + date.ToString("hh:mm tt ") + TimeZoneInfo.Local.Id + " on " + date.ToString("MMMM dd") + " effective until " + date.AddHours(endHour).AddMinutes(endMinute).ToString("hh:mm tt ") + TimeZoneInfo.Local.Id + ". ";
            finalString += "Message from " + shTime[2] + ".";
            Console.WriteLine("\nDecoded SAME Headere:\n");
            Console.WriteLine(finalString);
        }

        public static bool HasKey(List<KeyValuePair<string, string>> list, string key)
        {
            bool hasKey = false;
            foreach (KeyValuePair<string, string> pair in list)
            {
                if (pair.Key == key)
                {
                    hasKey = true;
                }
            }
            return hasKey;
        }

        public static string GetCounty(string fips)
        {
            string line;
            StreamReader sr = new StreamReader("Codes\\FIPS_national_counties.txt");
            string state = fips.Substring(1, 2);
            string county = fips.Substring(3, 3);

            while ((line = sr.ReadLine()) != null)
            {
                string[] split = line.Split(',');
                if (split[1] == state && split[2] == county)
                    return split[3] + " " + split[0];
            }

            return "ERROR";
        }
    }
}
