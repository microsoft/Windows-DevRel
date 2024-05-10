using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace AudioEditor
{
    public class AudioPlayer : INotifyPropertyChanged
    {
        private bool paused = true;
        private AudioFile currentAudioFile;
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFileReader;
        private Timer timer;

        public bool Paused
        {
            get { return this.paused; }
            set
            {
                if (value != this.paused)
                {
                    this.paused = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public AudioFile CurrentAudioFile
        {
            get { return currentAudioFile; }
            set
            {
                if (value != currentAudioFile)
                {
                    this.currentAudioFile = value;
                    if (outputDevice != null && audioFileReader != null) { DisposeAll(); }
                    NotifyPropertyChanged();
                }
            }
        }

        private void FireUpdatedTimestampEvent(object state)
        {
            if(audioFileReader != null)
            {
                OnTimestampUpdated(new TimestampUpdatedEventArgs(audioFileReader.CurrentTime));
            }
        }

        private void StartTimer() {     

            if(audioFileReader != null)
            {
                OnTimestampUpdated(new TimestampUpdatedEventArgs(audioFileReader.CurrentTime));
                this.timer = new Timer(FireUpdatedTimestampEvent, null, 0, 20);
            }
            
        }

        private void StopTimer()
        {
            if(this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
        }

        public AudioPlayer() {
        }
        public AudioPlayer(AudioFile audioFile) { this.currentAudioFile = audioFile; }

        public void Play()
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (audioFileReader == null && currentAudioFile != null)
            {
                audioFileReader = new AudioFileReader(currentAudioFile.FilePath);
                outputDevice.Init(audioFileReader);
            }
            outputDevice.Play();
            StartTimer();
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            this.StopTimer();
        }

        public void Stop()
        {
            outputDevice?.Stop();
        }

        public void Seek(int seconds)
        {
            if (audioFileReader != null) 
            { 
                if(seconds == 0) 
                {
                    audioFileReader.Position = 0;
                } else
                {
                    long newPosition = audioFileReader.Position + (audioFileReader.WaveFormat.AverageBytesPerSecond * seconds);
                    audioFileReader.Position = ((newPosition > 0 || newPosition > audioFileReader.Length) ? newPosition : 0);
                }

                FireUpdatedTimestampEvent(null);
            }
        }

        private void DisposeAll()
        {
            outputDevice.Dispose();
            outputDevice = null;
            audioFileReader.Dispose();
            audioFileReader = null;
        }


        // EVENTS

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<TimestampUpdatedEventArgs> TimestampUpdated;
        protected virtual void OnTimestampUpdated(TimestampUpdatedEventArgs e)
        {
            EventHandler<TimestampUpdatedEventArgs> handler = TimestampUpdated;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class TimestampUpdatedEventArgs : EventArgs
    {
        public TimeSpan Time { get; set; }
        public TimestampUpdatedEventArgs(TimeSpan time)
        {
            Time = time;
        }
    }

}
