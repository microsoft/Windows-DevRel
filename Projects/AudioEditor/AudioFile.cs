namespace AudioEditor
{
    public class AudioFile
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public AudioFile(string fileName, string absoluteFilePath)
        {
            FileName = fileName;
            FilePath = absoluteFilePath;
        }
    }
}
