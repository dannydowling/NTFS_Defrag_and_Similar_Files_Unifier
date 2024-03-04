/*
    The NtfsReader library.

    Copyright (C) 2008 Danny Couture

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
  
    For the full text of the license see the "License.txt" file.

    This library is based on the work of Jeroen Kessels, Author of JkDefrag.
    http://www.kessels.com/Jkdefrag/
    
    Special thanks goes to him.
  
    Danny Couture
    Software Architect
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Filesystem.Ntfs;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using NTFS_Defrag_and_Similar_Files_Unifier;

namespace NtfsReaderSample
{
    class Program
    {
        static void AnalyzeSimilarity(IEnumerable<INode> nodes, DriveInfo driveInfo)
        {
            IDictionary<UInt64, List<INode>> sizeAggregate = Algorithms.AggregateBySize(nodes, 10 * 1024 * 1024);

            List<UInt64> sizes = new List<UInt64>(sizeAggregate.Keys);
            sizes.Sort();
            sizes.Reverse();    //descending order

            foreach (UInt64 size in sizes)
            {
                List<INode> similarNodes = sizeAggregate[size];

                if (similarNodes.Count > 1) //this part ensures that there are 2 or more similar files that have been found.
                {

                    for (int i = 0; i < sizeAggregate[size].Count - 1; i++)
                    {
                        Console.WriteLine("-----------------------------------------");
                        Console.WriteLine(string.Format(
                               "Index {0}, {1}, {2}, size {3}, path {4}",
                               similarNodes[i].NodeIndex,
                               (similarNodes[i].Attributes & Attributes.Directory) != 0 ? "Dir" : "File",
                               similarNodes[i].Name,
                               similarNodes[i].Size,
                               similarNodes[i].FullName
                           ));

                        Console.WriteLine("-----------------------------------------");
                        Console.WriteLine("Look at the files above, would you like to create a hard link instead of keeping multiple files around?");
                        var choice = Console.ReadLine();
                        switch (choice)
                        {
                            case "y":
                                HardLinkCreator.CreateHardLink(similarNodes[i].Name, similarNodes[i + 1].ToString(), (nint)similarNodes[i + 1].Attributes);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        static void AnalyzeFragmentation(IEnumerable<INode> nodes, DriveInfo driveInfo)
        {
            //Fragmentation Example
            IDictionary<UInt32, List<INode>> fragmentsAggregate = Algorithms.AggregateByFragments(nodes, 2);

            List<UInt32> fragments = new List<UInt32>(fragmentsAggregate.Keys);
            fragments.Sort();
            fragments.Reverse(); //make the most fragmented ones appear first

            string targetFile = Path.Combine(driveInfo.Name, "fragmentation.txt");
            using (StreamWriter fs = new StreamWriter("c:\\fragmentation.txt"))
            {
                foreach (UInt32 fragment in fragments)
                {
                    List<INode> fragmentedNodes = fragmentsAggregate[fragment];

                    fs.WriteLine("-----------------------------------------");
                    fs.WriteLine("FRAGMENTS: {0}", fragment);
                    fs.WriteLine("-----------------------------------------");

                    foreach (INode node in fragmentedNodes)
                        fs.WriteLine(string.Format("Index {0}, {1}, {2}, size {3}, path {4}, lastModification {5}", 
                            node.NodeIndex, 
                            (node.Attributes & Attributes.Directory) != 0 ? "Dir" : "File", 
                            node.Name, node.Size, 
                            node.FullName, 
                            node.LastChangeTime.ToLocalTime())); 
                }
            }

            Console.WriteLine("Fragmentation Report has been saved to {0}", targetFile);
        }


        static void Main()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            string[] logicalDiskList = Directory.GetLogicalDrives();

            foreach (string logicalDisk in logicalDiskList)
            {
                DriveInfo disk = new DriveInfo(logicalDisk);                
                NtfsReader ntfsReader = new NtfsReader(disk, RetrieveMode.All);
                IEnumerable<INode> nodes = ntfsReader.GetNodes(disk.Name);

                int directoryCount = 0, fileCount = 0;
                foreach (INode node in nodes)
                {
                    if ((node.Attributes & Attributes.Directory) != 0)
                        directoryCount++;
                    else
                        fileCount++;
                }

                Console.WriteLine(string.Format("Directory Count: {0}, File Count {1}", directoryCount, fileCount));

                AnalyzeFragmentation(nodes, disk);

                AnalyzeSimilarity(nodes, disk);
            }
        }
        
    }
}
