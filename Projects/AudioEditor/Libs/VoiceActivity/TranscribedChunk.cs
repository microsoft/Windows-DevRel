using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Libs.VoiceActivity
{
    public class TranscribedChunk
    {
        [JsonInclude]
        public string Transcript;
        private double _end;
        private double _start;
        public double Length;

        [JsonInclude]
        public double End
        {
            get => _end;
            set
            {
                _end = value;
                RecomputeLength();
            }
        }

        [JsonInclude]
        public double Start
        {
            get => _start;
            set
            {
                _start = value;
                RecomputeLength();
            }
        }

        private void RecomputeLength()
        {
            // Recalculate Length whenever Start/End changes
            Length = _end - _start;
        }

        [JsonConstructor]
        public TranscribedChunk(string transcript, double start, double end)
        {
            Transcript = transcript;
            this.Start = start;
            this.End = end;
        }

        public TranscribedChunk(TranscribedChunk chunk)
        {
            Transcript = chunk.Transcript;
            this.Start = chunk.Start;
            this.End = chunk.End;
        }
    }
}
