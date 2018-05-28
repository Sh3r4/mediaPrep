using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace mediaPrep.Model
{
    public class MediaTypeRegister
    {
        // Logging
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly List<string> _videoExtensionsList = new List<string>(){
            ".webm",
            ".mkv",
            ".flv",
            ".flv",
            ".vob",
            ".ogg",
            ".ogv",
            ".drc",
            ".gif",
            ".gifv",
            ".mng",
            ".avi",
            ".mov",
            ".qt",
            ".wmv",
            ".yuv",
            ".rm",
            ".rmvb",
            ".asf",
            ".amv",
            ".m4p",
            ".m4v",
            ".mp4",
            ".mpv",
            ".mpe",
            ".mpeg",
            ".mp2",
            ".mpg",
            ".m2v",
            ".mpeg",
            ".mpg ",
            ".m4v",
            ".svi",
            ".3gp",
            ".3g2",
            ".mxf",
            ".roq",
            ".nsv",
            ".flv",
            ".f4a",
            ".f4b",
            ".f4v",
            ".f4p",
        };


        public bool IsVideo(string extension)
        {
            var output = false;
            // Defend against files with no extension sending an empty or whitespace string
            if (!string.IsNullOrWhiteSpace(extension))
            {
                if (!extension.Contains("."))
                    throw new NotAnExtensionException();

                output = _videoExtensionsList.Any(knownExtension => string.Equals(knownExtension, extension));
            }  

            _logger.Trace($"IsVideo Evaluated {extension} as {output}");
            return output;
        }
    }

    internal class NotAnExtensionException : RegisterException
    {
        
    }

    internal class RegisterException : Exception
    {
        
    }
}