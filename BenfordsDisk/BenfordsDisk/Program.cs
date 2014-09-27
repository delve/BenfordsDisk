//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="None">
//     MIT License (MIT)
//     Copyright (c) 2014 Grady Brandt
// </copyright>
// <author>Grady Brandt</author>
//-----------------------------------------------------------------------
namespace BenfordsDisk
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Just a console program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main, program entry point
        /// </summary>
        /// <param name="args">Command line args, which are ignored.</param>
        public static void Main(string[] args)
        {
            DriveInfo[] systemDrives = DriveInfo.GetDrives();
            bool selectedOption = false;
            short driveChoice;
            string input;

            while (!selectedOption)
            {
                Console.WriteLine("---Enter the disk you wish to analyze---\n");

                for (int i = 1; i <= systemDrives.Length; i++)
                {
                    Console.WriteLine(string.Concat(i.ToString(), ": ", systemDrives[i - 1].Name));
                }

                Console.WriteLine("X: Exit the program");
                input = Console.ReadLine();
                if (input.Contains("X"))
                {
                    Console.WriteLine("Goodbye.\n--------");
                    Console.ReadLine();
                    return;
                }

                selectedOption = short.TryParse(input, out driveChoice);

                // did they type an int
                if (!selectedOption)
                {
                    // and is it in the right range
                    if (driveChoice < 1 || driveChoice > systemDrives.Length)
                    {
                        Console.WriteLine("Please select a valid number from above. Use X to exit.");
                    }
                }

                // -----End user input

                int fileCount;
                int[] counts = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                double[] percents = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                Stopwatch stopwatch;
                StringBuilder output = new StringBuilder();

                // time the function
                stopwatch = Stopwatch.StartNew();
                fileCount = BenfordWithList(systemDrives[driveChoice - 1], counts);
                stopwatch.Stop();

                for (int i = 0; i < percents.Length; i++)
                {
                    percents[i] = Math.Round((counts[i] / (double)fileCount) * 100, 2);
                }

                output.Append("Digit results for List based function: \n");
                for (int i = 1; i <= counts.Length; i++)
                {
                    output.Append(string.Format("{0} -> {1} = {2}% of total\n", i, counts[i - 1], percents[i - 1]));
                }

                output.Append(string.Format("Directories searched in {0} milliseconds\n", stopwatch.ElapsedMilliseconds));
                
                //reset data
                fileCount = 0;
                counts = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                percents = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                // time the function
                stopwatch = Stopwatch.StartNew();
                fileCount = BenfordWithArray(systemDrives[driveChoice - 1], counts);
                stopwatch.Stop();

                for (int i = 0; i < percents.Length; i++)
                {
                    percents[i] = Math.Round((counts[i] / (double)fileCount) * 100, 2);
                }

                output.Append("Digit results for array based function: \n");
                for (int i = 1; i <= counts.Length; i++)
                {
                    output.Append(string.Format("{0} -> {1} = {2}% of total\n", i, counts[i - 1], percents[i - 1]));
                }

                output.Append(string.Format("Directories searched in {0} milliseconds\n", stopwatch.ElapsedMilliseconds));

                Console.Write(output.ToString());
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Implements the algorithm using just the count array
        /// </summary>
        /// <param name="selectedDrive">The drive to be searched</param>
        /// <returns>Returns a count of all files found</returns>
        public static int BenfordWithArray(DriveInfo selectedDrive, int[] counts)
        {
            int numFiles = 0;
            numFiles = RecurseDirWithArray(new DirectoryInfo(selectedDrive.Name), counts);
            Console.WriteLine("Found {0} files.", numFiles);

            return numFiles;
        }

        /// <summary>
        /// Performs recursive walk on the directory tree accumulating file size leading digit counts into the int[]
        /// </summary>
        /// <param name="directory">Starting directory context</param>
        /// <param name="counts">The array to accumulate digit counts into (int[0] is digit 1)</param>
        /// <returns>Returns a count of files found in this directory and its children</returns>
        public static int RecurseDirWithArray(DirectoryInfo directory, int[] counts)
        {
            int numFiles = 0;
            int digit;
            DirectoryInfo[] subDirs = null;
            /* first try looking for subdirectories. If we fail with any exception (it's likely
             *   an UnauthorizedAccessException error but frankly this code doesn't really care
             *   why) then skip that sub-dir
             *   */
            try
            {
                subDirs = directory.GetDirectories();
            }
            catch (Exception)
            {
                // We can't read this directory and there's no point in trying for this code
                return 0;
            }

            // walk the subdirs
            foreach (DirectoryInfo dir in subDirs)
            {
                numFiles += RecurseDirWithArray(dir, counts);
            }

            // accumulate the sizes of the files in this dir
            foreach (FileInfo file in directory.GetFiles())
            {
                digit = int.Parse(file.Length.ToString().Substring(0, 1));
                // having 0s in the mix makes no sense mathematically, though apparently 0-byte files technically exist
                if (digit > 0)
                {
                    counts[digit - 1]++;
                    numFiles++;
                }
            }

            return numFiles;
        }

        /// <summary>
        /// Implements the algorithm using a List object
        /// </summary>
        /// <param name="selectedDrive">The drive to be searched</param>
        /// <returns>Returns a count of all files found</returns>
        public static int BenfordWithList(DriveInfo selectedDrive, int[] counts)
        {
            /* we have a valid drive selection, get sizes for a recursive list of files
             *   this can't be done with a simple call like Directory.GetFiles(systemDrives[driveChoice - 1].Name, "*", SearchOption.AllDirectories)
             *   because we run into directories that we aren't allowed access to (no read privs, at the very least on System Volume Information) so
             *   we'll use a recursive function to generate the needed info
             *   */
            List<long> filesizes = new List<long>();
            RecurseDirWithList(new DirectoryInfo(selectedDrive.Name), filesizes);
            Console.WriteLine("Found {0} files.", filesizes.Count);
            for (int i = 0; i < filesizes.Count; i++)
            {
                filesizes[i] = long.Parse(filesizes[i].ToString().Substring(0, 1));
            }

            foreach (long digit in filesizes)
            {
                // beware off-by-one errors. digit value 1 is counted into array element 0
                counts[digit - 1]++;
            }

            return filesizes.Count;
        }

        /// <summary>
        /// Performs recursion through directory tree, dodging directories that the program doesn't have access to
        /// </summary>
        /// <param name="directory">The directory context to start from</param>
        /// <param name="acummulator">The list object accumulating all the file sizes</param>
        public static void RecurseDirWithList(DirectoryInfo directory, List<long> acummulator)
        {
            DirectoryInfo[] subDirs = null;
            /* first try looking for subdirectories. If we fail with any exception (it's likely
             *   an UnauthorizedAccessException error but frankly this code doesn't really care
             *   why) then skip that sub-dir
             *   */
            try
            {
                subDirs = directory.GetDirectories();
            }
            catch (Exception)
            {
                // We can't read this directory and there's no point in trying for this code
                return;
            }

            // walk the subdirs
            foreach (DirectoryInfo dir in subDirs)
            {
                RecurseDirWithList(dir, acummulator);
            }

            // accumulate the sizes of the files in this dir
            foreach (FileInfo file in directory.GetFiles())
            {
                // having 0s in the mix makes no sense mathematically, though apparently 0-byte files technically exist
                if (file.Length > 0)
                {
                    acummulator.Add(file.Length);
                }
            }

            return;
        }
    }
}
