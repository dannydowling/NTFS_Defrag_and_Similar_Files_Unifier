using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NTFS_Defrag_and_Similar_Files_Unifier
{
    class HardLinkCreator
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        public HardLinkCreator()
        {
                
        }

        public HardLinkCreator(string newHardLink, string persistingFilePath)
        {
            if (File.Exists(persistingFilePath))
            {
                // Create the hard link
                if (CreateHardLink(newHardLink, persistingFilePath, IntPtr.Zero))
                {
                    Console.WriteLine("Hard link created successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to create hard link.");
                }
            }
            else
            {
                Console.WriteLine("The existing file does not exist.");
            }
        }
    }
}
