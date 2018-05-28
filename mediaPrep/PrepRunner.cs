using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using mediaPrep.FileOps;
using mediaPrep.Model;
using NDesk.Options;
using NLog;

namespace mediaPrep
{
    public class PrepRunner
    {
        // Logging
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // Publicly accessible get properties
        public bool ShowHelp { get; private set; }
        public bool Errored { get; private set; }
        public bool Video { get; private set; }
        public string Directory { get; private set; }
        public string File { get; private set; }
        public string Output { get; private set; }
        public IMediaPrepAbstractions MediaPrepAbstractions { get; }

        // private objects

        private OptionSet _inputs;
        private readonly MediaTypeRegister _register = new MediaTypeRegister();

        // Private fields
        private bool _startedFromCommandline = true;

        public PrepRunner()
        {
            Initialise();
            MediaPrepAbstractions = new MediaPrepAbstractions();
        }

        public PrepRunner(IMediaPrepAbstractions abstractions)
        {
            Initialise();
            MediaPrepAbstractions = abstractions;
        }

        private void Initialise()
        {
            // Set up the public get, private set fields to defaults
            Errored = false;
            ShowHelp = false;
            Video = false;
            Directory = string.Empty;
            File = string.Empty;
            Output = string.Empty;

            // Set up to parse arguments later on
            _inputs = new OptionSet()
            {
                { "h|help",  "show this message and exit",
                    h => ShowHelp = h != null },
                { "v|video",  "Only process video files; This flag can only be set when creating a conversion template",
                    v => Video = v != null },
                { "t|trace", "Set the logging to enable trace level logs for detailed reporting", t => {
                        if (!string.IsNullOrEmpty(t))
                            LoggerWrapper.EnableTraceLevelLogging();
                }},
                { "f:|file:", "A file with name relationships to process",f => {
                    if (MediaPrepAbstractions.FileExistsAndIsNotNull(f))
                        File = f;
                }},
                { "d:|directory:", "A directory for which to make a conversion realationship template",d => {
                    if (MediaPrepAbstractions.IsValidDirectoryAndNotNull(d))
                        Directory = d;
                }},
                { "o:|output:", "Directory to save the created file to",d => {
                    if (MediaPrepAbstractions.IsValidDirectoryAndNotNull(d))
                        Output = MediaPrepAbstractions.GetAbsoluteDirectoryPath(d);
                    else
                        Output = MediaPrepAbstractions.GetExecutingAssemblyDirectory();
                }}
            };
        }


        public void Run(string[] args)
        {
            //Process arguments using the OptionSet
            ConsumeStartingArguments(args);

            // Show help if required
            if (ShowHelp)
            {
                MediaPrepAbstractions.PrintHelpText(_inputs);
                return;
            }

            if (!Errored)
            {
                Execute();
                // If a non-fatal error occurred during the execution, report to console
                if (Errored)
                {
                    MediaPrepAbstractions.Write("Errors occurred during the run. Please check the log file for details.");
                }
            }

#if DEBUG
            MediaPrepAbstractions.WaitForKey();
#else
            if (!_startedFromCommandline && Errored)
            {
                MediaPrepAbstractions.WaitForKey();
            }
#endif
        }

        private void ConsumeStartingArguments(string[] args)
        {
            // Assume the lack of input is a cry for help
            if (!args.Any())
            {
                ShowHelp = true;
                return;
            }

            try
            {
                // Parse the arguments
                var parsedArgs = _inputs.Parse(args);

                // Support something dropped onto the exe
                // detected if a file/directory was added already, or if there is more than one argument
                if (string.IsNullOrEmpty(File) && string.IsNullOrEmpty(Directory) && parsedArgs.Count <= 1)
                {
                    var dropped = parsedArgs.FirstOrDefault();
                    if (!string.IsNullOrEmpty(dropped))
                    {
                        // remember that we didn't get this from the commandline
                        _startedFromCommandline = false;

                        // assign the argument based on if it is a file or folder
                        if (MediaPrepAbstractions.FileExistsAndIsNotNull(dropped))
                            File = dropped;
                        else if (MediaPrepAbstractions.IsValidDirectoryAndNotNull(dropped))
                            Directory = dropped;
                        else
                        {
                            ShowHelp = true;
                        }
                    }
                }
            }

            // Catch exceptions thrown when parsing the arguments
            catch (OptionException e)
            {
                _logger.Error(e);
                Errored = true;
                return;
            }

            if (FilterIsActive() && !string.IsNullOrEmpty(File))
            {
                MediaPrepAbstractions.Write("Filter flags are not valid when renaming files.");
            }
        }

        private void Execute()
        {
            // Execute the requested action
            if (!string.IsNullOrEmpty(Directory) && !string.IsNullOrEmpty(File))
            {
                MediaPrepAbstractions.Write(
                    "Syntax Error: Cannot create a file and rename files at the same time. Try '--help' for more information.");
            }
            else if (!string.IsNullOrEmpty(Directory))
            {
                var data = GetMediaPrepList(Directory);
                _logger.Info($"Added {data.Count} media records to memory.");
                WriteConversionPlan(Directory, data);
            }
            else if (!string.IsNullOrEmpty(File))
            {
                ExecuteConversionInstructions(File);
            }
        }

        // Add other filter statements here
        private bool FilterIsActive()
        {
            if (Video)
            {
                return true;
            }
            return false;
        }

        private bool FileConformsToFilters(string filePath)
        {
            if (!FilterIsActive())
                return true;

            // check to make sure this file actually has an extension
            var ext = MediaPrepAbstractions.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(ext))
                return false;

            // go through all active filters and attempt to invalidate this file
            var conforms = true;
            if (Video)
            {
                if (!_register.IsVideo(ext)) conforms = false;
            }

            return conforms;
        }

        private void WriteConversionPlan(string outputDirectory, List<MediaPrepFile> data)
        {
            var fileName = GetUniqueFileName(
                MediaPrepAbstractions.GetDirectoryNameFromPath(MediaPrepAbstractions.GetAbsoluteDirectoryPath(outputDirectory))
                , "MediaPrepInstructions"
                , ".csv");

            var output = string.Empty;
            if (Output.Length > 0)
            {
                output = Output + "\\" + fileName;
            }
            else
            {
                output = fileName;
            }

            MediaPrepAbstractions.WriteCsv(output, data);
        }

        private void ExecuteConversionInstructions(string fileToProcess)
        {
            // Get the list of files to fix up
            var files = MediaPrepAbstractions.GetMediaPrepFiles(fileToProcess);
            _logger.Info($"Preparing to process{files.Count} files listed in instruction set");

            //iterate the list
            files.ForEach(file =>
            {
                if (string.IsNullOrEmpty(file.ShowOrMovieName))
                {
                    _logger.Info($"No new Show or Movie title provided. Skipping: {file.Directory}\\{file.FileName}");   
                    return;
                }

                var name = BuildAfileName(file);

                // Check it is valid before trying to change it
                try
                {
                    var currentPath = file.Directory + "\\" + file.FileName;
                    if (MediaPrepAbstractions.FileExistsAndIsNotNull(currentPath))
                        MediaPrepAbstractions.RenameFile(currentPath, name);
                    _logger.Info($"Renamed File |ORIGINAL:{file.Directory}\\{file.FileName}||NEW:{name}| ");
                }
                catch (Exception e)
                {
                    _logger.Warn($"Unable to rename file at {Path.GetFullPath(file.FileName)}");
                    Errored = true;
                    _logger.Error(e);
                }
            });
        }

        /// <summary>
        /// Attempts to find a non-colliding valid name for a new file. Falls back to appending a GUID
        /// </summary>
        /// <param name="linkedDirectory">The directory path for this file</param>
        /// <param name="fileName">The preferred file name for this file</param>
        /// <param name="fileExtension">The file extension of the file</param>
        /// <returns>A valid, non-colliding, fully qualified filepath</returns>
        public string GetUniqueFileName(string linkedDirectory, string fileName, string fileExtension)
        {
            var fileAttempt = linkedDirectory + "_" + fileName + fileExtension;

            // Finish before having to waste time declaring stuff for counting if possible
            if (!MediaPrepAbstractions.FileExistsAndIsNotNull(fileAttempt))
                return fileAttempt;

            // Prepare to make a file with an incremented thingo
            var counter = 1;
            while (MediaPrepAbstractions.FileExistsAndIsNotNull(fileAttempt))
            {
                // If there are already more than 150 attempts to make a file with an incremented number, fallback to a guid
                if (counter > 150)
                    return linkedDirectory + "_" + fileName + "_" + Guid.NewGuid() + fileExtension;

                counter++;
                fileAttempt = linkedDirectory + "_" + fileName + " (" + counter + ")" + fileExtension;
            }
            return fileAttempt;
        }

        /// <summary>
        /// Builds a filename for a TV or Film file using provided data
        /// </summary>
        /// <param name="file">Metadata to use; files with only a ShowOrMovieName field used will use that as the file name;
        /// Requires at a minimum a directory, a filename, and a showOrMovieName</param>
        /// <returns>fully qualified filepath</returns>
        public string BuildAfileName(MediaPrepFile file)
        {
            if (!string.IsNullOrWhiteSpace(file.ShowOrMovieName)
                && !string.IsNullOrWhiteSpace(file.Directory)
                && !string.IsNullOrWhiteSpace(file.FileName))
            {
                StringBuilder newName = new StringBuilder();
                newName.Append(file.Directory + "\\" + file.ShowOrMovieName);

                if (file.SeasonNumber != null && file.EpisodeNumber != null)
                {
                    newName.Append(" - ");
                    newName.Append("S" + NormaliseNumerals(file.SeasonNumber) + "E" + NormaliseNumerals(file.EpisodeNumber));

                    //deal with multiple episode parts in one
                    if (file.CompactedEpisodesEndNumber != null)
                        newName.Append("-E" + NormaliseNumerals(file.CompactedEpisodesEndNumber));
                }
                if (!string.IsNullOrWhiteSpace(file.EpisodeTitle))
                {
                    newName.Append(" - ");
                    newName.Append(file.EpisodeTitle);
                }
                if (file.PartNumber != null)
                {
                    newName.Append(" - ");
                    newName.Append("Part" + NormaliseNumerals(file.PartNumber));
                }
                if (newName.Length > 0)
                    newName.Append(Path.GetExtension(file.FileName));

                return newName.ToString();
            }
            return String.Empty;
        }

        private string NormaliseNumerals(int? numeral)
        {
            if (numeral == null) return string.Empty;
            if (numeral < 10)
                return "0" + numeral;
            return numeral.ToString();
        }

        private List<MediaPrepFile> GetMediaPrepList(string startingDirectory)
        {
            var directories = new Stack<string>();
            directories.Push(startingDirectory);
            var files = new List<MediaPrepFile>();

            while (directories.Count > 0)
            {
                var contents = GetDirectoryContents(directories.Pop());

                if (contents.mediaFiles != null) files.AddRange(contents.Item1);
                if (contents.subDirs != null) contents.Item2.ForEach(d => directories.Push(d));
            }

            return files;
        }

        private (List<MediaPrepFile> mediaFiles, List<string> subDirs) GetDirectoryContents(string directory)
        {
            // Get the file objects
            var files = new List<MediaPrepFile>();
            var filtered = 0;
            var discovered = 0;
            MediaPrepAbstractions.GetFilesInDirectory(directory).ForEach(file =>
            {
                discovered++;

                try
                {
                    if (FileConformsToFilters(file))
                    {
                        files.Add(new MediaPrepFile()
                        {
                            Directory = MediaPrepAbstractions.GetAbsoluteDirectoryPath(directory),
                            FileName = MediaPrepAbstractions.GetFileName(file),
                            ShowOrMovieName = string.Empty,
                            SeasonNumber = null,
                            EpisodeNumber = null,
                            CompactedEpisodesEndNumber = null,
                            PartNumber = null,
                            EpisodeTitle = string.Empty
                        });
                    }
                    else
                    {
                        filtered++;
                    }
                }
                catch (RegisterException e)
                {
                    Errored = true;
                    _logger.Error(e, $"Skipped a file due to bad formatting: LOCATION: {file}");
                }
            });

            _logger.Info($"Discovered {discovered} files in {directory} == Ignored {filtered} -- Ingested {files.Count} ==");
            return (files, MediaPrepAbstractions.GetDirsInDirectory(directory));
        }
    }
}
