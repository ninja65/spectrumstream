using AmbiMass.SpectrumStream.Contracts.Interfaces;
using System.Diagnostics;

namespace AmbiMass.SpectrumStream.Utils.SysEnv
{
    public class SysEnvironment : ISysEnvironment
    {
        public SysEnvironment() { }

        public string getFileNameWithoutExtension(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        public string getFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public string buildPath(string folder, string fileName)
        {
            return Path.Combine(folder, fileName);
        }

        public string buildPath(string folder, string path, string fileName)
        {
            return Path.Combine(folder, path, fileName);
        }

        public bool folderExists(string targetDirectory)
        {
            return Directory.Exists(targetDirectory);
        }

        public void createFolder(string targetDirectory)
        {
            Directory.CreateDirectory(targetDirectory);
        }

        public DateTime getCurrentTime()
        {
            return DateTime.Now;
        }

        public DateTime getCurrentTimeUtc()
        {
            return DateTime.UtcNow;
        }

        public string getParentFolder(string filePath)
        {
            return Directory.GetParent(filePath).FullName;
        }

        public int getFileLength(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);

            return Convert.ToInt32(fi.Length);
        }

        public bool fileExist(string fullPath)
        {
            return Path.Exists(fullPath);
        }

        public string getFileExtension(string fullPath)
        {
            return Path.GetExtension(fullPath);
        }

        public string[] getFilesInFolder(string folderpath)
        {
            return Directory.GetFiles(folderpath);
        }

        public string[] getFilesInFolder(string folderpath, string searchPattern)
        {
            return Directory.GetFiles(folderpath, searchPattern);
        }

        public void deleteFile(string xpropFileName)
        {
            if (fileExist(xpropFileName))
            {
                File.Delete(xpropFileName);
            }
        }

        public string pathInSpecialFolder(Environment.SpecialFolder specialFolder, params string[] pathPart)
        {
            string result = string.Empty;

            switch (pathPart.Length)
            {
                case 1:
                    result = Path.Combine(Environment.GetFolderPath(specialFolder),
                        pathPart[0]);
                    break;
                case 2:
                    result = Path.Combine(Environment.GetFolderPath(specialFolder),
                        pathPart[0], pathPart[1]);
                    break;
                case 3:
                    result = Path.Combine(Environment.GetFolderPath(specialFolder),
                        pathPart[0], pathPart[1], pathPart[2]);
                    break;
            }

            return result;
        }

        public void sleep(double milliSeconds)
        {
            Thread.Sleep( TimeSpan.FromMilliseconds( milliSeconds) );
        }

        public StreamReader openText(string fileName)
        {
            return File.OpenText(fileName);
        }

        public Stream openStream(string fileUrl)
        {
            return File.OpenRead(fileUrl);
        }

        public Stream createStream(string fileUrl)
        {
            return File.Create(fileUrl);
        }

        public StreamWriter openTextForWriting(string fileUrl)
        {
            return new StreamWriter(File.Create(fileUrl));
        }

        public string newGuid()
        {
            var guid = Guid.NewGuid();

            return guid.ToString();
        }
        public string toUtf8Hex(string source)
        {
            byte[] utf16Bytes = System.Text.Encoding.Unicode.GetBytes(source);

            // Step 2: Convert UTF-16 bytes to UTF-8 bytes
            byte[] utf8Bytes = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.UTF8, utf16Bytes);

            // Step 3: Convert UTF-8 bytes back to string (if needed)
            string utf8String = System.Text.Encoding.UTF8.GetString(utf8Bytes);

            string result = BitConverter.ToString(utf8Bytes).Replace("-", string.Empty);

            return result;
        }

        public string fullPathFromExe(string path)
        {
            var exeFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
            return Path.Combine(exeFolder ?? Directory.GetCurrentDirectory(), path);
        }

    }
}
