## FastEvents C# Class

Queries/Sorts/Outputs CSV of Windows Event Logs, quickly.

## Motivation

Needed a way to quickly sort events by id then timecreated, which Windows Event Viewer cannot do. Derived mainly from [How to: Query for Events](https://msdn.microsoft.com/en-us/library/bb671200(v=vs.110).aspx).

## Installation

Create a Visual Studio 2013 console application project, and copy in Program.cs.

## Functions

FastEvents.cs contains just the FastEvents class with the following methods:<br>

*`public EventLogReader QueryActiveLog(string logName, string queryString)`<br>
-Queries a local active Windows Event Log name.<br>
-`logName` is the name of a local Windows Event Log<br>
-`queryString` is the xml query (example '*')<br>
-returns an `EventLogReader` object container with found logs<br>

*`public EventLogReader QueryExternalFile(string filePath, string queryString)`<br>
-Queries a local .evtx file<br>
-`filePath` is the path and name of an .evtx file<br>
-`queryString` is the xml query (example '*')<br>
-returns an `EventLogReader` object container with found logs<br>

*`public EventLogReader QueryRemoteComputer(string remoteComputerName, string domain, string username, string logName, string queryString)`<br>
-I have yet to test this function.

*`public void ExportCSV(EventLogReader logReader, string filePath)`<br>
-Sorts found logs and outputs to CSV file
-`logReader' is the 'EventLogReader' returned from one of the previous 3<br>
-`filePath` is the path and name for the output CSV file

## License

	FastEvents C# Class: Queries/Sorts/Outputs CSV of Windows Event Logs, quickly.
	Copyright (C) 2015  Adam M. Quintero  http://angularadam.com 

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.