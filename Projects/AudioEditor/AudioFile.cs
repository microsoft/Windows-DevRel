using System.Runtime.Serialization;

namespace AudioEditor
{
    [DataContract]
    public class AudioFile
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        public AudioFile(string fileName, string filePath)
        {
            FileName = fileName;
            FilePath = filePath;
        }
    }
}
