using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace AudioEditor
{
    [DataContract]
    public class AudioFile : INotifyPropertyChanged
    {

        private string trimmedClipName = "";
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public string FilePath { get; set; }    
        [DataMember]
        public TimeSpan TotalDuration { get; set; }
        [DataMember]
        public int TrimmedDuration { get; set; } = 30;
        [DataMember]
        public string Keyword { get; set; }
        [DataMember]
        public string TrimmedClipName
        {
            get { return this.trimmedClipName; }
            set
            {
                if (value != this.trimmedClipName)
                {
                    this.trimmedClipName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [DataMember]
        public string TotalDurationString
        {
            get { return Utils.FormatTimeSpan(TotalDuration); }
            set { }
        }

        public AudioFile(string fileName, string filePath)
        {
            FileName = fileName;
            FilePath = filePath;
            SetTotalDuration();
        }

        public void SetTotalDuration()
        {
            Mp3FileReader mp3Reader = new Mp3FileReader(this.FilePath);
            TotalDuration = mp3Reader.TotalTime;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
