using System;
using System.Collections.Generic;
using mediaPrep.Model;
using NDesk.Options;

namespace mediaPrep.FileOps
{
    public interface IMediaPrepAbstractions
    {
        List<MediaPrepFile> GetMediaPrepFiles(string planFilePath);
        List<string> GetFilesInDirectory(string directoryPath);
        List<string> GetDirsInDirectory(string directoryPath);

        string GetAbsoluteDirectoryPath(string directoryPath);
        string GetDirectoryNameFromPath(string directoryPath);
        string GetFileName(string filePath);
        string GetExtension(string filePath);
        string GetExecutingAssemblyDirectory();

        void PrintHelpText(OptionSet inputs);
        bool FileExistsAndIsNotNull(string input);
        bool IsValidDirectoryAndNotNull(string input);
        void WaitForKey();

        void RenameFile(string currentPath, string futurePath);
        void WriteCsv(string fileName, List<MediaPrepFile> data);
        void Write(string message);
        void WriteErrorMessage(Exception e);
    }
}