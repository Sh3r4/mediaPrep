using Microsoft.VisualStudio.TestTools.UnitTesting;
using mediaPrep;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mediaPrep.FileOps;
using mediaPrep.Model;

namespace mediaPrep.Tests
{
    [TestClass()]
    public class PrepRunnerTests
    {
        /// <summary>
        /// Test the file name building constraints
        /// </summary>
        [TestMethod()]
        public void BuildAfileNameTests()
        {
            var runner = new PrepRunner();
            var tests = new List<Tuple<string, MediaPrepFile>>
            {
                new Tuple<string, MediaPrepFile>(
                    // Test renaming with only a show name
                    "Z:\\newFile.mkv", new MediaPrepFile()
                    {
                        Directory = "Z:",
                        ShowOrMovieName = "newFile",
                        FileName = "OldFile.mkv"
                    }),
                 new Tuple<string, MediaPrepFile>(
                
                    // Test renaming with short numbers
                    "Z:\\newFile - S00E02 - title.mkv", new MediaPrepFile()
                    {
                        Directory = "Z:",
                        ShowOrMovieName = "newFile",
                        FileName = "OldFile.mkv",
                        EpisodeNumber = 2,
                        SeasonNumber = 0,
                        EpisodeTitle = "title"
                    }
                ), new Tuple<string, MediaPrepFile>(
                
                    // Test renaming with long numbers
                    "Z:\\newFile - S3335663E24566223 - title.mkv", new MediaPrepFile()
                    {
                        Directory = "Z:",
                        ShowOrMovieName = "newFile",
                        FileName = "OldFile.mkv",
                        EpisodeNumber = 24566223,
                        SeasonNumber = 3335663,
                        EpisodeTitle = "title"
                    }
                ), new Tuple<string, MediaPrepFile>(
                
                    // Test renaming with multi-part episodes
                    "Z:\\newFile - S01E01 - title - Part01.mkv", new MediaPrepFile()
                    {
                        Directory = "Z:",
                        ShowOrMovieName = "newFile",
                        FileName = "OldFile.mkv",
                        EpisodeNumber = 1,
                        SeasonNumber = 1,
                        PartNumber = 1,
                        EpisodeTitle = "title"
                    }

                ), new Tuple<string, MediaPrepFile>(
                
                    // Test renaming with multi-part episodes which are also condensed somehow
                    "Z:\\newFile - S01E01-E04 - title - Part01.mkv", new MediaPrepFile()
                    {
                        Directory = "Z:",
                        ShowOrMovieName = "newFile",
                        FileName = "OldFile.mkv",
                        EpisodeNumber = 1,
                        SeasonNumber = 1,
                        CompactedEpisodesEndNumber = 4,
                        PartNumber = 1,
                        EpisodeTitle = "title"
                    }
                ), new Tuple<string, MediaPrepFile>(
                
                    // Test renaming with condensed episodes
                    "Z:\\newFile - S01E01-E04 - title.mkv", new MediaPrepFile()
                    {
                        Directory = "Z:",
                        ShowOrMovieName = "newFile",
                        FileName = "OldFile.mkv",
                        EpisodeNumber = 1,
                        SeasonNumber = 1,
                        CompactedEpisodesEndNumber = 4,
                        EpisodeTitle = "title"
                    }
                ), new Tuple<string, MediaPrepFile>(
                
                    // Test no renaming with no show name
                    string.Empty, new MediaPrepFile()
                    {
                        Directory = "Z:",
                        EpisodeNumber = 1,
                        FileName = "OldFile.mkv",
                        SeasonNumber = 1,
                        CompactedEpisodesEndNumber = 4,
                        EpisodeTitle = "title"
                    }
                ), new Tuple<string, MediaPrepFile>(
                
                    // Test no renaming with no previous filename
                    string.Empty, new MediaPrepFile()
                    {
                        Directory = "Z:",
                        ShowOrMovieName = "newFile",
                        EpisodeNumber = 1,
                        SeasonNumber = 1,
                        CompactedEpisodesEndNumber = 4,
                        EpisodeTitle = "title"
                    }
                ), new Tuple<string, MediaPrepFile>(
                
                    // Test no renaming with no directory
                    string.Empty, new MediaPrepFile()
                    {
                        FileName = "OldFile.mkv",
                        ShowOrMovieName = "newFile",
                        EpisodeNumber = 1,
                        SeasonNumber = 1,
                        CompactedEpisodesEndNumber = 4,
                        EpisodeTitle = "title"
                    }
                ), new Tuple<string, MediaPrepFile>(
                
                    // Test no renaming with empty dir
                    string.Empty, new MediaPrepFile()
                    {
                        Directory = string.Empty,
                        FileName = "OldFile.mkv",
                        ShowOrMovieName = "newFile",
                        EpisodeNumber = 1,
                        SeasonNumber = 1,
                        CompactedEpisodesEndNumber = 4,
                        EpisodeTitle = "title"
                    }
                ), new Tuple<string, MediaPrepFile>(
                
                    // Test no renaming with empty FileName
                    string.Empty, new MediaPrepFile()
                    {
                        Directory = "Z:",
                        FileName = string.Empty,
                        ShowOrMovieName = "newFile",
                        EpisodeNumber = 1,
                        SeasonNumber = 1,
                        CompactedEpisodesEndNumber = 4,
                        EpisodeTitle = "title"
                    }
                ), new Tuple<string, MediaPrepFile>(
                
                    // Test no renaming with empty showOrMovieName
                    string.Empty, new MediaPrepFile()
                    {
                        Directory = "Z:",
                        FileName = "OldFile.mkv",
                        ShowOrMovieName = string.Empty,
                        EpisodeNumber = 1,
                        SeasonNumber = 1,
                        CompactedEpisodesEndNumber = 4,
                        EpisodeTitle = "title"
                    }
                )
            };



            // Run through the list of tests
            foreach (var file in tests)
                Assert.AreEqual(file.Item1, runner.BuildAfileName(file.Item2));
        }

        [TestMethod()]
        public void GetUniqueFileName_NameNeverUsedTests()
        {
            // Set up the abstraction implementation and verify it
            var abstractions = new FakeAbstractionImplementation {IsAValidFileAndNotNullReturnValue = false};
            var runner = new PrepRunner(abstractions);
            Assert.AreSame(runner.MediaPrepAbstractions, abstractions);

            // Set up our fake file building components
            var linkedDirectory = "directory";
            var fileName = "file";
            var fileExtension = ".fake";

            // get the results to compare
            var fileCorrect = linkedDirectory + "_" + fileName + fileExtension;
            var actualResult = runner.GetUniqueFileName(linkedDirectory, fileName, fileExtension);

            Assert.IsTrue(abstractions.IsAValidFileAndNotNullHitCounter == 1);
            Assert.AreEqual(fileCorrect, actualResult);

        }

        [TestMethod()]
        public void GetUniqueFileName_AlwaysCreatesUniqueNameTests()
        {
            // Set up the abstraction implementation and verify it
            var abstractions = new FakeAbstractionImplementation { IsAValidFileAndNotNullReturnValue = true };
            var runner = new PrepRunner(abstractions);
            Assert.AreSame(runner.MediaPrepAbstractions, abstractions);

            // Set up our fake file building components
            var linkedDirectory = "Q:\\";
            var fileName = "file";
            var fileExtension = ".fake";

            var createdName = runner.GetUniqueFileName(linkedDirectory, fileName, fileExtension);

            // Did it hit the valid file to a crazy number?
            Assert.IsTrue(abstractions.IsAValidFileAndNotNullHitCounter > 150);

            // Are all the required pieces of info there
            Assert.IsTrue(createdName.Contains(linkedDirectory));
            Assert.IsTrue(createdName.Contains(fileName));
            Assert.IsTrue(createdName.Contains(fileExtension));
        }

        [TestMethod()]
        public void GetUniqueFileName_FormatsIncrementedFilesCorrectly()
        {
            // Set up the abstraction implementation and verify it
            var abstractions = new FakeAbstractionImplementation { IsAValidFileAndNotNullReturnValue = true };
            var runner = new PrepRunner(abstractions);
            Assert.AreSame(runner.MediaPrepAbstractions, abstractions);

            // Set up our fake file building components
            var linkedDirectory = "Q:\\";
            var fileName = "file";
            var fileExtension = ".fake";

            // Set the number at which to say it is a valid file in the abstractions
            var hitMax = 91;
            abstractions.IsAValidFileAndNotNullHitLimit = hitMax;

            // Get both names to compare
            var expectedName = linkedDirectory + "_" + fileName + " (" + hitMax + ")" + fileExtension;
            var createdName = runner.GetUniqueFileName(linkedDirectory, fileName, fileExtension);

            // Did it run to the correct number
            Assert.IsTrue(abstractions.IsAValidFileAndNotNullHitCounter == hitMax);

            // Are all the required pieces of info there
            Assert.IsTrue(createdName.Contains(linkedDirectory));
            Assert.IsTrue(createdName.Contains(fileName));
            Assert.IsTrue(createdName.Contains(fileExtension));
            Assert.IsTrue(createdName.Contains(hitMax.ToString()));

            // Is it in the format we want?
            Assert.AreEqual(expectedName, createdName);
        }
    }
}