namespace CombExtLib
{
    internal class MetaData
    {
        public string FileName { get; set; }
        public long Offset { get; set; }
        public long Size { get; set; }

        public MetaData(string fileName, long offset, long size)
        {
            FileName = fileName;
            Offset = offset;
            Size = size;
        }
    }
}