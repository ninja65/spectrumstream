using System.Diagnostics;

namespace AmbiMass.SpectrumStream.Contracts.Interfaces
{
    public interface ISysEnvironment
    {
        string getFileNameWithoutExtension(string filePath);
        string getFileName(string filePath);
        string buildPath(string targetDirectory, string fileName);
        string buildPath(string folder, string path, string fileName);
        bool folderExists(string targetDirectory);
        void createFolder(string targetDirectory);
        DateTime getCurrentTime();
        string getParentFolder(string filePath);
        int getFileLength(string filePath);
        bool fileExist(string fullPath);
        string getFileExtension(string fullPath);
        string[] getFilesInFolder(string folderpath);
        string[] getFilesInFolder(string folderpath, string searchPattern);
        void deleteFile(string xpropFileName);

        string pathInSpecialFolder(Environment.SpecialFolder specialFolder, params string[] pathPart);

        void sleep(double milliSeconds);
        StreamReader openText(string fileName);

        StreamWriter openTextForWriting(string fileName);

        string toUtf8Hex(string source);
        string newGuid();
        Stream openStream(string fileUrl);
        Stream createStream(string fileUrl);

        string fullPathFromExe(string path);

        DateTime getCurrentTimeUtc();
    }
}
