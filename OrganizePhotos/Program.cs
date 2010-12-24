using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

namespace OrganizePhotos
{
    class Program
    {
        static void Main(string[] args)
        {
            const string photosFolderPath = @"C:\NotBackedUp\Public\Pictures";

            OrganizePhotosFolder(photosFolderPath);
        }

        private static string GetCameraMapping(
            string cameraModel)
        {
            if (cameraModel == null)
            {
                return null;
            }

            switch (cameraModel)
            {
                case "Canon EOS DIGITAL REBEL XSi":
                    return "Pictures from Corinne";

                case "Canon PowerShot A590 IS":
                    return "Pictures from Russ";

                case "Canon PowerShot A620":
                    return "Pictures from Mandy";

                case "Canon PowerShot A75":
                    return "Pictures from Barby";

                case "Canon PowerShot S400":
                    return "Pictures from Morgan";

                case "DIGITAL CAMERA":
                    return null;

                case "DSC-P100":
                    return "Pictures from Amy";

                case "E3100":
                    return "Pictures from Scott";

                case "E950":
                    return "Pictures from Doug";

                case "E995":
                    return "Pictures from Jeremy";

                case "NIKON D50":
                    return "Pictures from Doug";

                default:
                    return cameraModel;
            }
        }

        private static string GetExpectedPathForPhoto(
            FileInfo fileInfo,
            BitmapMetadata metadata,
            string photosFolderPath)
        {
            string cameraModel = metadata.CameraModel;

            string cameraMapping = GetCameraMapping(cameraModel);

            DateTime dateTaken = DateTime.Parse(metadata.DateTaken);

            string expectedPath = string.Format(
                @"{0}\{1:0000}\{2:00}\{3:00}",
                photosFolderPath,
                dateTaken.Year,
                dateTaken.Month,
                dateTaken.Day);

            if (cameraMapping != null)
            {
                expectedPath += @"\" + cameraMapping;
            }

            expectedPath += @"\" + fileInfo.Name;

            return expectedPath;
        }

        private static void MovePhoto(
            string source,
            string destination)
        {
            if (source == destination)
            {
                return;
            }

            if (File.Exists(destination) == true)
            {
                Console.WriteLine(
                    "The destination file already exists ({0})",
                    destination);

                return;
            }

            string folder = Path.GetDirectoryName(destination);

            if (Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }

            Console.WriteLine(
                "Moving file ({0}) to {1})...",
                source,
                destination);

            File.Move(source, destination);
        }

        private static void OrganizePhotosFolder(
            string photosFolderPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(photosFolderPath);

            foreach (FileInfo fileInfo in dirInfo.EnumerateFiles(
                "*",
                SearchOption.AllDirectories))
            {
                if (string.Compare(
                    fileInfo.Extension,
                    ".jpg",
                    StringComparison.OrdinalIgnoreCase) == 0)
                {
                    try
                    {
                        ProcessFile(fileInfo, photosFolderPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            "Error processing file ({0}): {1}",
                            fileInfo.FullName,
                            ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine(
                        "Skipping file: {0}",
                        fileInfo.FullName);
                }
            }
        }

        private static void ProcessFile(
            FileInfo fileInfo,
            string photosFolderPath)
        {
            Console.WriteLine(fileInfo.FullName);

            string expectedPath = null;

            using (Stream stream = new FileStream(
                fileInfo.FullName,
                FileMode.Open,
                FileAccess.Read))
            {
                BitmapDecoder decoder = BitmapDecoder.Create(
                    stream,
                    BitmapCreateOptions.None,
                    BitmapCacheOption.Default);

                BitmapFrame frame = decoder.Frames[0];

                BitmapMetadata metadata = (BitmapMetadata)frame.Metadata;

                if (string.IsNullOrEmpty(metadata.DateTaken) == true)
                {
                    return;
                }

                expectedPath = GetExpectedPathForPhoto(
                    fileInfo,
                    metadata,
                    photosFolderPath);
            }

            if (expectedPath != fileInfo.FullName)
            {
                MovePhoto(fileInfo.FullName, expectedPath);
            }
        }
    }
}
