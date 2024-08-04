using CombExtLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace CombExtLib
{
   /// <summary>
   /// A class to combine or extract to files.
   /// </summary>
    public class CombExt
    {
        private string Dir { get; set; }
        private List<string> files = new List<string>();
        private List<MetaData> combineMetaDataCollection = new List<MetaData>();
        private List<MetaData> extractMetaDataCollection = new List<MetaData>();

        /// <summary>
        /// Adds the given path to files list which will be combined.
        /// </summary>
        /// <param name="path">File path</param>
        public void AddFile(string path)
        {
            files.Add(path);
        }
        /// <summary>
        /// Sets the directory of extractor.
        /// </summary>
        /// <param name="dir">Directory</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void SetDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                Dir = dir;
            }
            else
            {
                throw new DirectoryNotFoundException($"Directory not found: {dir}");
            }
        }
        /// <summary>
        /// Combines the file which are added to collection.
        /// </summary>
        /// <param name="outputFileName">Output file name. For examle:combined.bin</param>
        /// <param name="removeOriginalFiles">Remove original file after combine process.</param>
        /// <returns>Returns the combined file paths</returns>

        public string Combine(string outputFileName,bool removeOriginalFiles=false)
        {
            if (Dir == null)
                throw new InvalidOperationException("Directory not set.");

            string rawDataPath = Path.Combine(Dir, "raw.bin");
            string metaDataFilePath = Path.Combine(Dir, "metadata.bin");
            string combinedFilePath = Path.Combine(Dir, outputFileName);

            using (var outputStream = new FileStream(rawDataPath, FileMode.Create, FileAccess.Write))
            {
                long currentOffset = 0;
                foreach (var file in files)
                {
                    using (var inputStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        MetaData metaData = new MetaData(Path.GetFileName(file), currentOffset, inputStream.Length);
                        combineMetaDataCollection.Add(metaData);

                        inputStream.CopyTo(outputStream);
                        currentOffset += inputStream.Length;
                    }
                }
            }

            using (var metadataStream = new StreamWriter(metaDataFilePath))
            {
                metadataStream.WriteLine("*STARTMETADATA");
                foreach (var item in combineMetaDataCollection)
                {
                    metadataStream.WriteLine($"*FILE,{item.FileName},{item.Offset},{item.Size}");
                }
                metadataStream.WriteLine("*ENDMETADATA");
            }

            using (var combinedStream = new FileStream(combinedFilePath, FileMode.Create, FileAccess.Write))
            {
                using (var metadataStream = new FileStream(metaDataFilePath, FileMode.Open, FileAccess.Read))
                {
                    metadataStream.CopyTo(combinedStream);
                }
                using (var rawDataStream = new FileStream(rawDataPath, FileMode.Open, FileAccess.Read))
                {
                    rawDataStream.CopyTo(combinedStream);
                }
            }

            try
            {
                File.Delete(rawDataPath);
                File.Delete(metaDataFilePath);
                if (removeOriginalFiles)
                {
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }
            return combinedFilePath;
        }

        /// <summary>
        /// Extracts the combined file.
        /// </summary>
        /// <param name="combinedFilePath">Combined file path.</param>
        /// <returns>Returns the directory of extracted files.</returns>
        public string Extract(string combinedFilePath)
        {
            if (Dir == null)
                throw new InvalidOperationException("Directory not set.");

            string extractDir = Path.Combine(Dir, "Ext");
            CreateOrClearDir(extractDir);

            long startPos = 0;
            // Read metadata
            using (var reader = new StreamReader(combinedFilePath,Encoding.UTF8))
            {
                string line;
                StringBuilder stringBuilder = new StringBuilder();
                while ((line = reader.ReadLine()) != null)
                {
                    stringBuilder.AppendLine(line);
                    if (line.StartsWith("*FILE"))
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length == 4)
                        {
                            string fileName = parts[1];
                            long offset = long.Parse(parts[2]);
                            long size = long.Parse(parts[3]);

                            MetaData metaData = new MetaData(fileName, offset, size);
                            extractMetaDataCollection.Add(metaData);
                        }
                    }
                    if (line.StartsWith("*ENDMETADATA"))
                    {
                        Encoding encoding = Encoding.UTF8;
                        byte[] bytes = encoding.GetBytes(stringBuilder.ToString());
                        startPos = bytes.Length;
                        break;
                    }
                }
            }

            using (var combinedStream = new FileStream(combinedFilePath, FileMode.Open, FileAccess.Read))
            {
                foreach (var metaData in extractMetaDataCollection)
                {
                    string outputFilePath = Path.Combine(extractDir, metaData.FileName);
                    using (var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                    {
                        combinedStream.Seek(metaData.Offset+startPos, SeekOrigin.Begin);

                        byte[] buffer = new byte[metaData.Size];
                        combinedStream.Read(buffer, 0, (int)metaData.Size);

                        outputStream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            return extractDir;
        }


        private void CreateOrClearDir(string dirName)
        {
            if (Directory.Exists(dirName))
            {
                ClearDirectory(dirName);
            }
            else
            {
                Directory.CreateDirectory(dirName);
            }
        }

        private void ClearDirectory(string dirName)
        {
            foreach (var file in Directory.GetFiles(dirName))
            {
                File.Delete(file);
            }

            foreach (var subDir in Directory.GetDirectories(dirName))
            {
                Directory.Delete(subDir, true);
            }
        }
   
    }
}
