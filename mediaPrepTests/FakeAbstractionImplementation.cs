using System;
using System.Collections.Generic;
using System.IO;
using mediaPrep.FileOps;
using mediaPrep.Model;
using NDesk.Options;

namespace mediaPrep.Tests
{
    public class FakeAbstractionImplementation : IMediaPrepAbstractions
    {
        public bool IsAValidFileAndNotNullReturnValue { get; set; }
        public int IsAValidFileAndNotNullHitCounter { get; set; }
        public int IsAValidFileAndNotNullHitLimit { get; set; }

        public FakeAbstractionImplementation()
        {
            // Set the limits to max to start with
            IsAValidFileAndNotNullHitLimit = int.MaxValue;
        }

        public void WriteCsv(string fileName, List<MediaPrepFile> data)
        {
            throw new NotImplementedException();
        }

        public string GetNewFileName(string outputDirectory, string fileName, string fileExtension)
        {
            throw new NotImplementedException();
        }

        public List<string> GetFilesInDirectory(string directory)
        {
            throw new NotImplementedException();
        }

        public List<string> GetDirsInDirectory(string directory)
        {
            throw new NotImplementedException();
        }

        public List<MediaPrepFile> GetMediaPrepFiles(string planFilePath)
        {
            throw new NotImplementedException();
        }

        public bool IsAFile(string path)
        {
            throw new NotImplementedException();
        }

        public void RenameFile(string directory, string currentName, string futureName)
        {
            throw new NotImplementedException();
        }

        public string GetAbsoluteDirectoryPath(string directoryPath)
        {
            throw new NotImplementedException();
        }

        public string GetFileName(string filePath)
        {
            throw new NotImplementedException();
        }

        public string GetExtension(string filePath)
        {
            throw new NotImplementedException();
        }

        public void RenameFile(string currentPath, string futurePath)
        {
            throw new NotImplementedException();
        }

        public void Write(string message)
        {
            throw new NotImplementedException();
        }

        public void PrintHelpText(OptionSet inputs)
        {
            throw new NotImplementedException();
        }

        public bool FileExistsAndIsNotNull(string input)
        {
            if (IsAValidFileAndNotNullHitCounter != IsAValidFileAndNotNullHitLimit)
            {
                IsAValidFileAndNotNullHitCounter++;
                return IsAValidFileAndNotNullReturnValue;
            }
            return !IsAValidFileAndNotNullReturnValue;
        }

        public bool IsValidDirectoryAndNotNull(string input)
        {
            throw new NotImplementedException();
        }

        public void WaitForKey()
        {
            throw new NotImplementedException();
        }

        public void WriteConversionPlan(string outputDirectory, List<MediaPrepFile> data)
        {
            throw new NotImplementedException();
        }

        public void WriteErrorMessage(Exception e)
        {
            throw new NotImplementedException();
        }

        public string GetDirectoryNameFromPath(string directoryPath)
        {
            return Path.GetDirectoryName(directoryPath);
        }

        public string GetExecutingAssemblyDirectory()
        {
            throw new NotImplementedException();
        }
    }
}