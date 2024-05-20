using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using NAudio.Wave;
using NAudio.WaveFormRenderer;

namespace AudioEditor
{
    public class WaveformRenderer
    {
        public enum PeakProvider
        {
            Max,
            RMS,
            Sampling,
            Average
        }

        private WaveFormRenderer renderer = new WaveFormRenderer();
        private AudioFileReader audioFileReader;
        private AudioFile currentAudioFile;
        public AudioFile CurrentAudioFile
        { 
            get
            {
                return currentAudioFile;
            }
            set
            {
                if(currentAudioFile != value)
                {
                    currentAudioFile = value;
                    audioFileReader = new AudioFileReader(currentAudioFile.FilePath);
                }
            }
        }

        public WaveFormRendererSettings Settings { get; set; }
        public PeakProvider Provider { get; set; } = PeakProvider.Average;
        
        public WaveformRenderer(AudioFile audioFile, int height, int width)
        {
            CurrentAudioFile = audioFile;
            Settings = new StandardWaveFormRendererSettings();
            Settings.BackgroundColor = Color.Transparent;
            Settings.SpacerPixels = 0;
            Settings.TopHeight = height;
            Settings.BottomHeight = height;
            Settings.Width = width;
            Settings.TopPeakPen = new Pen(Color.LightGray);
            Settings.BottomPeakPen = new Pen(Color.LightGray);
            audioFileReader = new AudioFileReader(audioFile.FilePath);
        }

        public void SetHeight(int height)
        {
            Settings.TopHeight = height;
            Settings.BottomHeight = height;
        }

        public void SetWidth(int width) 
        { 
            Settings.Width = width;
        }

        public Image Render()
        {
            IPeakProvider provider;
            switch (this.Provider)
            {
                case PeakProvider.Max:
                    provider = new MaxPeakProvider();
                    break;
                case PeakProvider.RMS:
                    provider = new RmsPeakProvider(200);
                    break;
                case PeakProvider.Sampling:
                    provider = new SamplingPeakProvider(1600);
                    break;
                default:
                    provider = new AveragePeakProvider(4);
                    break;
            }

            return this.renderer.Render(audioFileReader, provider, Settings);
        }
       

    }
}
