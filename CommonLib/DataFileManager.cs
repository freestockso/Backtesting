using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Compression;

namespace CommonLib
{
    public class DataFile
    {
        public ZipArchive TargetArchive { get; set; }

        private string _FilePath;
        public string FilePath
        {
            get { return _FilePath; }
            set
            {
                _FilePath = value;
            }
        }
        public void WriteDic(string folderPath)
        {
            using (FileStream zipToOpen = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                using (TargetArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry entry = TargetArchive.GetEntry(folderPath);
                    if(entry==null)
                        TargetArchive.CreateEntry(folderPath);
                }
            }
        }

        public void WriteFile(string folderPath, string fileName, string content)
        {
            WriteFile(folderPath + fileName, content);
        }
        public void WriteFile(string filePath, string content)
        {
            filePath = FilterFileName(filePath);
            using (FileStream zipToOpen = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                using (TargetArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    var zf = TargetArchive.GetEntry(filePath);
                    if (zf == null)
                        zf=TargetArchive.CreateEntry(filePath);
                    using (StreamWriter writer = new StreamWriter(zf.Open()))
                    {
                        writer.Write(content);
                    }
                }
            }
        }
        public string ReadFile(string path, string fileName)
        {
            return ReadFile(path + fileName);
        }
        public byte[] ReadBinaryFile(string zipFilePath,string TargetFilePath)
        {
            TargetFilePath = FilterFileName(TargetFilePath);
            if (string.IsNullOrEmpty(TargetFilePath)) return null;
            List<byte> b = new List<byte>();
            using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.OpenOrCreate))
            {
                using (TargetArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    var zf = TargetArchive.GetEntry(TargetFilePath);
                    if (zf != null)
                        using (BinaryReader  r = new BinaryReader (zf.Open()))
                        {
                            r.BaseStream.Seek(0, SeekOrigin.Begin); 
                            while (r.PeekChar() > -1)
                            {
                                b.Add(r.ReadByte());
                            }
                        }
                }
            }
            return b.ToArray();
        }
        public string ReadFile(string filePath)
        {
            filePath = FilterFileName(filePath);
            string s=null;
            using (FileStream zipToOpen = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                using (TargetArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    var zf = TargetArchive.GetEntry(filePath);
                    if(zf!=null)
                        using (StreamReader r = new StreamReader(zf.Open()))
                        {
                            s = r.ReadToEnd();
                        }
                }
            }
            return s;
        }
        public List<string> ReadDicFiles(string folderPath)//not include sub folder
        {
            List<string> sl = new List<string>();
            using (FileStream zipToOpen = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                using (TargetArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    var zf = TargetArchive.GetEntry(folderPath);
                    if (zf != null)
                    {
                        foreach (var entry in zf.Archive.Entries)
                        {
                            if (entry.FullName.StartsWith(folderPath) && !entry.FullName.EndsWith("\\"))
                            {
                                var fullpath = entry.FullName.Substring(folderPath.Length-1);
                                if(!fullpath.Contains("\\"))
                                    sl.Add(entry.FullName);
                            }
                        }
                    }
                }
            }
            return sl;
        }
        public List<string> ReadDicFilesContent(string folderPath)//not include sub folder
        {
            List<string> sl=new List<string>();
            using (FileStream zipToOpen = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                using (TargetArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    var zf = TargetArchive.GetEntry(folderPath);
                    if (zf != null)
                    {
                        foreach (var entry in zf.Archive.Entries)
                        {
                            if(entry.FullName.StartsWith(folderPath)&&!entry.FullName.EndsWith("\\"))
                                using (StreamReader r = new StreamReader(entry.Open()))
                                {
                                    sl.Add(r.ReadToEnd());
                                }
                        }
                    }
                }
            }
            return sl;
        }
        public void DeleteDic(string folderPath)
        {
            using (FileStream zipToOpen = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                using (TargetArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    var el = new List<ZipArchiveEntry>();
                    foreach (var ent in TargetArchive.Entries)
                    {
                        if(ent.FullName.StartsWith(folderPath))
                            el.Add(ent);
                    }
                    el.ForEach(e =>
                        {
                            var d = TargetArchive.Entries[TargetArchive.Entries.IndexOf(e)];
                            d.Delete();
                        });
                }
            }
        }
        public void DeleteFile(string folderPath, string fileName)
        {
            DeleteFile(folderPath + fileName);
        }
        public void DeleteFile(string filePath)
        {
            filePath = FilterFileName(filePath);
            using (FileStream zipToOpen = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                using (TargetArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry entry = TargetArchive.GetEntry(filePath);
                    if (entry != null)
                    {
                        var d = TargetArchive.Entries[TargetArchive.Entries.IndexOf(entry)];
                        d.Delete();
                    }
                }
            }
        }

        public void CopyFileIn(string sourceURL, string targetURL)
        {
            if (!File.Exists(sourceURL)) return;
            targetURL = FilterFileName(targetURL);
            using (FileStream zipToOpen = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                using (TargetArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    TargetArchive.CreateEntryFromFile(sourceURL, targetURL);
                }
            }
        }

        public void CopyFileOut(string sourceURL, string targetURL)
        {
            if (File.Exists(targetURL)) File.Delete(targetURL);
            sourceURL = FilterFileName(sourceURL);
            using (FileStream zipToOpen = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                using (TargetArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry entry = TargetArchive.GetEntry(sourceURL);
                    if (entry != null)
                    {
                        entry.ExtractToFile(targetURL);
                    }
                }
            }
        }
        public bool IsExist(string zipFilePath,string filePath)
        {
            if (!File.Exists(zipFilePath)) return false;
            filePath = FilterFileName(filePath);
            using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.OpenOrCreate))
            {
                using (TargetArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry entry = TargetArchive.GetEntry(filePath);
                    if (entry != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public string GetFileName(string url)
        {
            var index = url.LastIndexOf("\\");
            if (index == -1||index==url.Length-1) return null;
            return url.Substring(index + 1);
        }
        public string FilterFileName(string fileName)
        {
            var s= fileName.Replace("/", "_");
            return s.Replace("-", "_");
        }

        public void CompressDirection(string path,string zipFilePath)
        {
            if (!Directory.Exists(path))
                throw new Exception("No valid path:" + path);
            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);
            ZipFile.CreateFromDirectory(path, zipFilePath);

        }
        public void DeCompressDirection(string path, string zipFilePath)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (!File.Exists(zipFilePath))
                throw new Exception("Not valid file:"+zipFilePath);
            ZipFile.ExtractToDirectory(zipFilePath,path);

        }
    }

}
