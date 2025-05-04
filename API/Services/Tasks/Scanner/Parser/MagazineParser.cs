using System.IO;
using System.Linq;
using API.Data.Metadata;
using API.Entities.Enums;

namespace API.Services.Tasks.Scanner.Parser;
#nullable enable

public class MagazineParser(IDirectoryService directoryService) : DefaultParser(directoryService)
{
    public override ParserInfo? Parse(string filePath, string rootPath, string libraryRoot, LibraryType type,
        ComicInfo? comicInfo = null)
    {
        if (!IsApplicable(filePath, type)) return null;

        var ret = new ParserInfo
        {
            Volumes = Parser.LooseLeafVolume,
            Chapters = Parser.DefaultChapter,
            ComicInfo = comicInfo,
            Format = Parser.ParseFormat(filePath),
            Filename = Path.GetFileName(filePath),
            FullFilePath = Parser.NormalizePath(filePath),
            Series = string.Empty,
        };

        // Try to parse Series from the filename
        var libraryPath = directoryService.FileSystem.DirectoryInfo.New(rootPath).Parent?.FullName ?? rootPath;
        var fileName = directoryService.FileSystem.Path.GetFileNameWithoutExtension(filePath);
        ret.Series = Parser.ParseMagazineSeries(fileName);
        ret.Volumes = Parser.ParseMagazineVolume(fileName);
        ret.Chapters = Parser.ParseMagazineChapter(fileName);

        if (string.IsNullOrEmpty(ret.Series) || (string.IsNullOrEmpty(ret.Chapters) && string.IsNullOrEmpty(ret.Volumes)))
        {
            // Fallback to the parent folder. We can also likely grab Volume (year) from here
            var folders = directoryService.GetFoldersTillRoot(libraryPath, filePath).ToList();
            // Usually the LAST folder is the Series and everything up to can have Volume


            if (string.IsNullOrEmpty(ret.Series))
            {
                ret.Series = Parser.CleanTitle(folders[^1]);
            }

            var hasGeoCode = !string.IsNullOrEmpty(Parser.ParseGeoCode(ret.Series));
            foreach (var folder in folders[..^1])
            {
                if (ret.Volumes == Parser.LooseLeafVolume)
                {
                    var vol = Parser.ParseYear(folder); // TODO: This might be better as YearFromSeries
                    if (!string.IsNullOrEmpty(vol) && vol != folder)
                    {
                        ret.Volumes = vol;
                    }
                }

                // If folder has a language code in it, then we add that to the Series (Wired (UK))
                if (!hasGeoCode)
                {
                    var geoCode = Parser.ParseGeoCode(folder);
                    if (!string.IsNullOrEmpty(geoCode))
                    {
                        ret.Series = $"{ret.Series} ({geoCode})";
                        hasGeoCode = true;
                    }
                }

            }
        }

        return ret;
    }

    /// <summary>
    /// Only applicable for PDF Files and Magazine library type
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public override bool IsApplicable(string filePath, LibraryType type)
    {
        return type == LibraryType.Magazine && Parser.IsPdf(filePath);
    }

}
