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

                /* we have a valid drive selection, get sizes for a recursive list of files
                 *   this can't be done with a simple call like Directory.GetFiles(systemDrives[driveChoice - 1].Name, "*", SearchOption.AllDirectories)
                 *   because we run into directories that we aren't allowed access to (no read privs, at the very least on System Volume Information) so
                 *   we'll use a recursive function to generate the needed info
                 *   */
                List<long> filesizes = new List<long>();
                RecurseDirWithList(new DirectoryInfo(systemDrives[driveChoice - 1].Name), filesizes);
                Console.WriteLine("Found {0} files.", filesizes.Count);
                for (int i = 0; i < filesizes.Count; i++)
                {
                    filesizes[i] = long.Parse(filesizes[i].ToString().Substring(0, 1));
                }

                Console.WriteLine("The raw digits: {0}", string.Join<long>(" ", filesizes));
                int[] counts = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                double[] percents = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                foreach (long digit in filesizes)
                {   
                    if (digit < 1)
                    {   // having 0s in the mix makes no sense mathematically, though apparently 0-byte files technically exist
                        continue;
                    }

                    // beware off-by-one errors. digit value 1 is counted into array element 0
                    counts[digit - 1]++;
                }

                for (int i = 0; i < percents.Length; i++)
                {
                    percents[i] = Math.Round((counts[i] / (double)filesizes.Count) * 100, 2);
                }

                Console.WriteLine("Digit results: ");
                for (int i = 1; i <= counts.Length; i++)
                {
                    Console.WriteLine("{0} -> {1} = {2}% of total", i, counts[i - 1], percents[i - 1]);
                }

                Console.ReadLine();
            }
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
             *   why
             *   */
            try
            {
                subDirs = directory.GetDirectories();
            }
            catch (Exception)
            {
                // just break out of this branch of the recursion, regardless of the precise
                //   failure. We can't read this directory and there's no point in trying for
                //   this code
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
                acummulator.Add(file.Length);
            }

            return;
        }
    }
}
