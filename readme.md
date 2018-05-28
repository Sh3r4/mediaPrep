# mediaPrep

mediaPrep is a windows application for renaming media files into a standardised format for archiving. I wrote it to save some time. It was never intended to be released but it's just been kicking around in my source folder for ages, so I'm just going to release it. Use at your own risk!

Based on the details provided by the user in a .csv, files will be renamed into one of the following formats:

| Media Type                             | Format Example                                   |
|----------------------------------------|--------------------------------------------------|
| films                                  | MediaTitle.ext                                   |
| tv (single episode)                    | SeriesTitle - S01E01 - EpisodeTitle.ext          |
| tv (multipart episode, multiple files) | SeriesTitle - S01E01 - EpisodeTitle - Part01.ext |
| tv (multipart episode, condensed file) | SeriesTitle - S01E01-E04 - EpisodeTitle.ext      |

It can deal with naming conventions for:

* films
* tv series
	* multiple episodes compacted into a single file
	* multipart episdoes split into multiple files

## Usage

The intended usage is an executable drag/drop folders onto, or to drag/drop the conversion instructions onto.
There is no GUI, but there is no need to use the commandline.

* mediaPrep ingests folders to create .csv files which can be filled out with the relevant details.
* mediPrep ingests the .csv files to rename all files denoted in the .csv into the standardised format.

The created .csv files contain the following headers. Depending on which are filled out, a naming format is inferred.

| Renaming Plan Header       | Required? |
|----------------------------|-----------|
| Directory                  | Required  |
| FileName                   | Required  |
| ShowOrMovieName            | Required  |
| SeasonNumber               |           |
| EpisodeNumber              |           |
| CompactedEpisodesEndNumber |           |
| PartNumber                 |           |
| EpisodeTitle               |           |

There are also commandline options:

``` txt
$ ./mediaPrep.exe -h
Options:
  -h, --help                 show this message and exit
  -v, --video                Only process video files; This flag can only be
                               set when creating a conversion template
  -t, --trace                Set the logging to enable trace level logs for
                               detailed reporting
  -f, --file[=VALUE]         A file with name relationships to process
  -d, --directory[=VALUE]    A directory for which to make a conversion
                               realationship template
  -o, --output[=VALUE]       Directory to save the created file to
```