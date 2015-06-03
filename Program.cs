//FastEvents C# Class Driver: Queries/Sorts/Outputs CSV of Windows Event Logs, quickly.
//Copyright (C) 2015  Adam M. Quintero  http://angularadam.com 
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Security;
using System.IO;

namespace FastEvents
{
    class Program
    {
        static string getElapsed(TimeSpan ts)
        {
            // Format and display the TimeSpan value. 
            string elapsedTime = String.Format("{0:00} mins : {1:00} secs : {1:00} ms",
                ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            return "Total Run Time " + elapsedTime + "\n";
        }
        static void showHelp()
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("  FastEvents v1.0 - Adam Quintero (C) 2015                                      ");
            Console.WriteLine("  Quickly queries event logs, sorts Id then TimeCreated, and outputs CSV file.  ");
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("  Query Local Live Event Logs\n");
            Console.WriteLine("---------------------------------\n");
            Console.WriteLine(" Syntax:\nfastevents /live [logName] [csvFileName]\n");
            Console.WriteLine(" Example:\nfastevents /live Application output.csv\n");
            Console.WriteLine("  Query Local .evtx File\n");
            Console.WriteLine("---------------------------------\n");
            Console.WriteLine(" Syntax:\nfastevents /file [evtx fileName] [fileName]\n");
            Console.WriteLine(" Example:\nfastevents /file savedEvents.evtx output.csv\n");
            Console.WriteLine("  Query Remote Live Event Logs\n");
            Console.WriteLine("---------------------------------\n");
            Console.WriteLine(" Syntax:\nfastevents /remote [computerName] [domain] [userName] [logName] [fileName]\n");
            Console.WriteLine(" Example:\nfastevents /remote ComputerName Domain Username Application output.csv\n");
            Console.WriteLine("---------------------------------\n");
            Console.WriteLine(" Remote fetching is untested. There is no guarantee it will do anything.\n");
        }

        static void Main(string[] args)
        {
            //instantiate FastEvents Class
            FastEvents fast = new FastEvents();
            
            //Start processing timer
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            
            //analyze arguments and execute
            if (args.Length == 0)
            {
                showHelp();
                stopWatch.Stop();
            }
            else
            {
                if(args[0] == "/live")
                {
                    if(args.Length == 3)
                    {
                        EventLogReader reader = fast.QueryActiveLog(args[1], "*");
                        fast.ExportCSV(reader, args[2]);
                        //display elapsed time
                        stopWatch.Stop();
                        Console.WriteLine(getElapsed(stopWatch.Elapsed));
                    }
                    else
                    {
                        Console.WriteLine("\n Both a logname and csv filename are required.");
                        stopWatch.Stop();
                    }
                }
                else if(args[0] == "/file")
                {
                    if (args.Length == 3)
                    {
                        EventLogReader reader = fast.QueryExternalFile(args[1], "*");
                        fast.ExportCSV(reader, args[2]);
                        //display elapsed time
                        stopWatch.Stop();
                        Console.WriteLine(getElapsed(stopWatch.Elapsed));
                    }
                    else
                    {
                        Console.WriteLine("\n Both a evtx and csv filename are required.");
                        stopWatch.Stop();
                    }
                }
                else if(args[0] == "/remote")
                {
                    if (args.Length == 6)
                    {
                        EventLogReader reader = fast.QueryRemoteComputer(args[1], args[2], args[3], args[4], "*");
                        fast.ExportCSV(reader, args[5]);
                        //display elapsed time
                        stopWatch.Stop();
                        Console.WriteLine(getElapsed(stopWatch.Elapsed));
                    }
                    else
                    {
                        Console.WriteLine("\n All params are required:\n [computerName] [domain] [userName] [logName] [fileName]");
                        stopWatch.Stop();
                    }
                }
                else
                {
                    showHelp();
                    stopWatch.Stop();
                }

            }

        }
    }

    class FastEvents
    {
        //reads a local active log name and displays events
        public EventLogReader QueryActiveLog(string logName, string queryString)
        {

            EventLogQuery eventsQuery = new EventLogQuery(logName, PathType.LogName, queryString);
            EventLogReader logReader = new EventLogReader(eventsQuery);
            return logReader;
        }
        //reads evtx file and displays events
        public EventLogReader QueryExternalFile(string filePath, string queryString)
        {

            EventLogQuery eventsQuery = new EventLogQuery(filePath, PathType.FilePath, queryString);

            try
            {
                EventLogReader logReader = new EventLogReader(eventsQuery);
                return logReader;
            }
            catch (EventLogException e)
            {
                EventLogReader logReader = new EventLogReader("");
                Console.WriteLine("Could not query the external file!" + e.Message);
                return logReader;
            }
        }
        //reads active log from remote computer and displays events
        public EventLogReader QueryRemoteComputer(string remoteComputerName, string domain, string username, string logName, string queryString)
        {
            SecureString pw = GetPassword();

            EventLogSession session = new EventLogSession(
                remoteComputerName, // Remote Computer
                domain,             // Domain
                username,           // Username
                pw,                 // Password
                SessionAuthentication.Default);

            pw.Dispose();

            // Query the Application log on the remote computer.
            EventLogQuery query = new EventLogQuery(logName, PathType.LogName, queryString);
            query.Session = session;

            try
            {
                EventLogReader logReader = new EventLogReader(query);
                return logReader;
            }
            catch (EventLogException e)
            {
                EventLogReader logReader = new EventLogReader("");
                Console.WriteLine("Could not query the remote computer! " + e.Message);
                return logReader;
            }
        }      
        /// Writes events sorted by id and timeCreated to specified filePath CSV file
        public void ExportCSV(EventLogReader logReader, string filePath)
        {
            int eventCounter = 0;
            List<EventRecord> records = new List<EventRecord>();
            for (EventRecord eventInstance = logReader.ReadEvent(); null != eventInstance; eventInstance = logReader.ReadEvent())
            {
                records.Add(eventInstance);
            }
            using (StreamWriter file = new StreamWriter(filePath))
            {
                file.WriteLine("\"EventId\",\"TimeCreated\",\"Level\",\"Source\"");
                foreach (EventRecord ev in records.OrderBy(r => r.Id).ThenByDescending(r => r.TimeCreated))
                {
                    eventCounter++;
                    string eventLevel;
                    switch(ev.Level)
                    {
                        case 0:
                            eventLevel = "00-Other";
                            break;
                        case 2:
                            eventLevel = "20-Other";
                            break;
                        case 3:
                            eventLevel = "30-Critical";
                            break;
                        case 4:
                            eventLevel = "40-Error";
                            break;
                        case 5:
                            eventLevel = "50-Warning";
                            break;
                        case 8:
                            eventLevel = "80-Information";
                            break;
                        case 10:
                            eventLevel = "100-Verbose";
                            break;
                        default:
                            eventLevel = ev.Level.ToString() + "-UnkownLevel";
                            break;
                    }
                    Console.Write("\r{0}                         ",  "Writing " + eventCounter + " of " + records.Count);
                    file.WriteLine("\"" + ev.Id + "\",\"" + ev.TimeCreated + "\",\"" + eventLevel + "\",\"" + ev.ProviderName + "\"");
                }
                Console.WriteLine("\n\n" + eventCounter + " events found, sorted, and exported to " + filePath + "\n");
            }
        }
        /// Read a password from the console into a SecureString
        /// Password stored in a secure string
        public static SecureString GetPassword()
        {
            SecureString password = new SecureString();
            Console.WriteLine("Enter password: ");

            // get the first character of the password
            ConsoleKeyInfo nextKey = Console.ReadKey(true);

            while (nextKey.Key != ConsoleKey.Enter)
            {
                if (nextKey.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.RemoveAt(password.Length - 1);

                        // erase the last * as well
                        Console.Write(nextKey.KeyChar);
                        Console.Write(" ");
                        Console.Write(nextKey.KeyChar);
                    }
                }
                else
                {
                    password.AppendChar(nextKey.KeyChar);
                    Console.Write("*");
                }

                nextKey = Console.ReadKey(true);
            }

            Console.WriteLine();

            // lock the password down
            password.MakeReadOnly();
            return password;
        }
    }
}
