using ProgAssign1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using System.Text;

namespace ProgAssign1
{
    public class Program
    {
        static Program()
        {
            Main();
        }
        public static void Main()
        {
            // Program Execution recorder
            Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

            //////////
            // Path to read files from
            ////////
            string rootDir = @"./"; /* ENTER DIRECTORY TO FOLDER HERE*/
            string dataDir = Path.Join(rootDir, @"\\Sample Data\\"); /* ENTER SUB-DIRECTORY PATH OF DATA HERE*/


            string logDir = Path.Join(rootDir, @"logs\\");
            string outputFileDir = Path.Join(rootDir, @"\\Output\\output.csv");

            // Validate creating Dirs
            try
            {
                if (!Directory.Exists(rootDir)) { Debug.WriteLine("Invalid Root Directory! Exiting..."); return; }
                if (!Directory.Exists(dataDir)) { Debug.WriteLine("No Sample Data Directory! Exiting..."); return; }
                if (!Directory.Exists(logDir)) { Directory.CreateDirectory(Path.GetDirectoryName(logDir)); }
            }
            catch (Exception ex) { Trace.WriteLine("Fatal Exception occured! " + ex.ToString()); return; }

            // Configuring logger
            Trace.Listeners.Clear();
            TextWriterTraceListener logFile = new TextWriterTraceListener($"{logDir}log.log");
            Trace.Listeners.Add(logFile);

            // Extentions to read and store
            string ext = ".csv";

            DirWalker csv_file_walker = new DirWalker(dataDir, ext);
            Trace.WriteLine($"\n\nRead {csv_file_walker.fileCount} files with \"{csv_file_walker.ext}\" extension.\n\n");

            CSVReader csv_reader = new CSVReader();

            // Appending Header Values
            csv_reader.validRows.AppendLine("First Name,Last Name,Street Number,Street,City,Province,Country,Postal Code,Phone Number,Email Address,Date"); 
            
            foreach (string filePath in csv_file_walker.file_dirs)
            {
                csv_reader.Parse(filePath, dataDir);
            }

            csv_reader.writeValidRowsToFile(outputFileDir);

            Trace.WriteLine($"\n\nValid Row Count: {csv_reader.validRowCount}\nInvalid Row Count: {csv_reader.invalidRowCount}");
            
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;
            Trace.WriteLine($"Total Execution Time: {elapsedMs}ms\n\n");
            
            Trace.Flush();
        }
    }
    public class DirWalker
    {
        public List<string> file_dirs = new List<string>();
        public string ext;
        public string rootDir;
        public int fileCount = 0;

        public DirWalker(string rootDir, string ext)
        {
            this.ext = ext;
            this.rootDir = rootDir;

            try { this.Walk(rootDir); }
            catch (Exception ex) { Trace.WriteLine("Exception occured! " + ex.ToString()); }
        }
        public void Walk(String path)
        {
            //////////
            // Walk through sub-directories 
            ////////

            string[] subdirs = Directory.GetDirectories(path);

            // Return if no sub-directories found
            if (subdirs == null) { return; }

            // Walk each sub-directory
            foreach (string dirpath in subdirs)
            {
                if (Directory.Exists(dirpath))
                {
                    try { this.Walk(dirpath); }
                    catch (Exception ex) { Trace.WriteLine("Exception occured! " + ex.ToString()); }
                    Trace.WriteLine("\nDir: " + Path.GetRelativePath(this.rootDir, dirpath) + "\\");
                }
            }

            /////////
            // Walk through files in directory
            ///////

            string[] fileList = Directory.GetFiles(path);

            // Check number of files that match spe
            if (fileList.Count(p => Path.GetExtension(p) == this.ext) == 0) { Trace.WriteLine("\t\tNo Files found with specified extension!"); return; }

            foreach (string filepath in fileList)
            {
                // Append CSV File directories to list
                if (Path.GetExtension(filepath) == this.ext)
                {
                    Trace.WriteLine("\t\tFile: " + Path.GetFileName(filepath));
                    try
                    {
                        this.file_dirs.Add(filepath);
                        this.fileCount++;
                    }
                    catch (OverflowException oe) { Trace.WriteLine($"Too many files to count!{oe.Message}"); }
                }
            }
        }
    }
    public class CSVReader
    {
        public int invalidRowCount = 0;
        public int validRowCount = 0;
        // public List<string[]> validRows = new List<string[]>();
        public StringBuilder validRows = new StringBuilder();

        public void Parse(String filePath, String rootPath)
        {
            String Date = Path.GetRelativePath(rootPath, Path.GetDirectoryName(filePath)).ToString();

            try
            {
                /////////
                // Parse CSV data row by row
                ///////
              
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    //Skip headers
                    parser.ReadFields();

                    while (!parser.EndOfData)
                    {
                        // Parse fields as list because we need to the append date
                        List<String> fields = parser.ReadFields().ToList();
                        // Check if null value exists in row
                        if (fields.Count(p => p.Equals("")) == 0)
                        {
                            // Count and store valid rows
                            try
                            {
                                this.validRowCount++;
                                fields.Add(Date);
                                this.validRows.AppendLine(string.Join(",", fields));
                            }
                            catch (OverflowException oe){ Trace.WriteLine($"Too many rows to store!{oe.Message}"); }
                        }
                        else
                        {
                            // Count and log invalid rows
                            Trace.WriteLine($"Skipped row! Row fields (\"{string.Join("\",\"", fields)}\") from file {Path.GetFileName(filePath)} in folder \\{Date}");
                            this.invalidRowCount++;
                        }
                    }
                    parser.Close();
                }
            }
            catch (IOException ioe)
            {
                Trace.WriteLine(ioe.StackTrace);
            }

        }

        public void writeValidRowsToFile(string filePath)
        {
            /////////
            // Write valid rows to file
            ///////
            
            // Check if directory is valid
            if (filePath.EndsWith(".csv"))
            {
                try
                {
                    // Create directory if it doesn't exist (inside try clause)
                    if (!Directory.Exists(filePath)) Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    // Write all the text
                    File.WriteAllText(filePath, this.validRows.ToString());
                }
                catch (Exception ex) { Trace.WriteLine("Exception occured! " + ex.ToString()); }
            }
            else
            {
                throw new FileNotFoundException("File name not mentioned in specified save path!");
            }
        }
    }
}
