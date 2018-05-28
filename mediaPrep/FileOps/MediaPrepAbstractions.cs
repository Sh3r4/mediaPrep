using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using mediaPrep.Model;
using NDesk.Options;
using NLog;

namespace mediaPrep.FileOps
{
    public class MediaPrepAbstractions : IMediaPrepAbstractions
    {
        // Logging
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();



        #region File System Interactions

        public bool FileExistsAndIsNotNull(string path)
        {
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }

        public bool IsValidDirectoryAndNotNull(string input)
        {
            return !string.IsNullOrEmpty(input) && Directory.Exists(input);
        }

        public List<string> GetFilesInDirectory(string directoryPath)
        {
            if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
                return Directory.EnumerateFiles(directoryPath).ToList();
            throw new DirectoryNotFoundException("The provided directory path does not correspond to a valid directory.");
        }

        public List<string> GetDirsInDirectory(string directoryPath)
        {
            if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
                return Directory.EnumerateDirectories(directoryPath).ToList();
            throw new DirectoryNotFoundException("The provided directory path does not correspond to a valid directory.");
        }

        public void RenameFile(string currentPath, string futurePath)
        {
            if (File.Exists(currentPath) && !File.Exists(futurePath))
                File.Move(currentPath, futurePath);
        }

        public string GetAbsoluteDirectoryPath(string directoryPath)
        {
            if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
                return Path.GetFullPath(directoryPath);
            throw new DirectoryNotFoundException("The provided directory path does not correspond to a valid directory.");
        }

        public string GetFileName(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                return Path.GetFileName(filePath);
            throw new FileNotFoundException("The provided filepath does not correspond to a valid file.");
        }

        

        public string GetExtension(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                return Path.GetExtension(filePath);
            throw new FileNotFoundException("The provided filepath does not correspond to a valid file.");
        }
        #endregion

        #region User Interaction

        public void WaitForKey()
        {
            Console.ReadKey();
        }

        public void Write(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine(message);
        }

        public void PrintHelpText(OptionSet inputs)
        {
            Console.WriteLine("Options:");
            inputs.WriteOptionDescriptions(Console.Out);
            Console.ReadKey();
        }

        public void WriteErrorMessage(Exception e)
        {
            Console.Write("mediaPrep: ");
            Console.WriteLine(e.Message);
            Console.WriteLine("Try `mediaPrep --help' for more information.");
        }

        #endregion

        #region CSV IO

        

        public void WriteCsv(string fileName, List<MediaPrepFile> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            // just in case we got a file name that already exists, we can just make a non-colliding name
            if (File.Exists(fileName))
                fileName = GetFileName(fileName);

            try
            {
                using (var textWriter = File.CreateText(fileName))
                {
                    var csv = new CsvWriter(textWriter);
                    csv.WriteRecords(data);
                    _logger.Info($"Wrote instructions template based on the {data.Count} discovered items to {fileName}.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public List<MediaPrepFile> GetMediaPrepFiles(string planFilePath)
        {
            try
            {
                if (this.FileExistsAndIsNotNull(planFilePath))
                {
                    using (var reader = File.OpenText(planFilePath))
                    {
                        var csv = new CsvReader(reader);
                        var records = csv.GetRecords<MediaPrepFile>();
                        return records.ToList();
                    }
                }
                else
                {
                    _logger.Error("Provided argument is not a file.");
                    Console.WriteLine("argument is not a file");
                    return new List<MediaPrepFile>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public string GetDirectoryNameFromPath(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
                return new DirectoryInfo(directoryPath).Name;
            return string.Empty;
        }

        public string GetExecutingAssemblyDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        #endregion
    }

}
