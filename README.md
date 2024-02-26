I've modified template code to work across many logical drives so far.

My plans are to constrain the folder by the current working directory using a simple if statement.
The code is showing how many fragments files are in. I'll try and rewrite the files using a filesystem accessor
as contiguous. The code is also finding duplicate files, I'd like it to hard link the files to conserve space.
I think that when the hard link is copied to an external device, that windows resolves the link and copies the actual file.
There's no point in having lots of the same files.

[DllImport("Kernel32.dll", CharSet = CharSet.Unicode )]
static extern bool CreateHardLink(
string lpFileName,
string lpExistingFileName,
IntPtr lpSecurityAttributes
)

