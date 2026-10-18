using Raylib_cs;
using static Raylib_cs.Raylib;

namespace LudumDare60.Generators;

public static class AudioBank {
    public readonly static Dictionary<string, (Int32 Root, int[] Notes)> Chords = new() {
        ["Am"] = (45, [60, 64, 69]),
        ["F"] = (41, [57, 60, 65]),
        ["E"] = (40, [56, 59, 64]),
        ["Dm"] = (38, [57, 62, 65]),
    };

    public const Int32 Rate = 44100;
}

public sealed class MusicGenerator<TStrategy>(TStrategy strategy, Int32 chunkSize = 2048) : IDisposable where TStrategy : MusicGeneratorStrategy {
    private readonly Single[] _chunk = new Single[chunkSize];
    private AudioStream _stream;
    private Boolean _loaded;

    public Boolean IsPlaying => _loaded && IsAudioStreamPlaying(_stream);

    public void Play() {
        if (!_loaded) {
            SetAudioStreamBufferSizeDefault(chunkSize);
            _stream = LoadAudioStream(AudioBank.Rate, 32, 1);
            _loaded = true;
            Fill();
            Fill();
        }
        PlayAudioStream(_stream);
    }
    public void Pause() {
        if (_loaded) {
            PauseAudioStream(_stream);
        }
    }
    public void Stop() {
        if (_loaded) {
            StopAudioStream(_stream);
        }
    }
    public Single Volume {
        set {
            if (_loaded) {
                SetAudioStreamVolume(_stream, value);
            }
        }
    }

    public void Progress() {
        if (_loaded && IsAudioStreamProcessed(_stream)) {
            Fill();
        }
    }

    private void Fill() {
        strategy.Generate(_chunk);
        UpdateAudioStream(_stream, _chunk, _chunk.Length);
    }

    public void Dispose() {
        if (_loaded) {
            UnloadAudioStream(_stream);
            _loaded = false;
        }
    }
}

public interface MusicGeneratorStrategy {
    void Generate(Single[] buffer);
}

public abstract class StepSynthStrategy(Int32 seed, Int32 bpm) : MusicGeneratorStrategy {
    protected enum Kind { Box, Triangle, Saw, Tick }

    private sealed class Voice(Kind kind, Double freq, Double gain, Double decay, Double seconds) {
        private readonly Int64 _length = (Int64)(seconds * AudioBank.Rate);
        private Int64 _age;
        private Double _phase;

        public Boolean Done => _age >= _length;

        public Double Next(Random random) {
            Double w;
            if (kind == Kind.Tick) {
                w = (random.NextDouble() * 2 - 1) * (1 - (Double)_age / _length);
            } else {
                var x = _phase - Math.Floor(_phase);
                w = kind switch {
                    Kind.Box => Math.Sin(2 * Math.PI * _phase) + 0.2 * Math.Sin(8 * Math.PI * _phase),
                    Kind.Triangle => 4 * Math.Abs(x - 0.5) - 1,
                    _ => (2 * x - 1) * 0.5,
                };
                _phase += freq / AudioBank.Rate;
            }
            var env = Math.Min(1, _age / (0.01 * AudioBank.Rate)) * Math.Exp(-decay * _age / AudioBank.Rate);
            _age++;
            return w * env * gain;
        }
    }

    private readonly Double _eighthSamples = 60.0 / bpm / 2 * AudioBank.Rate;
    private readonly Random _random = seed == -1 ? new Random() : new Random(seed);
    private readonly List<Voice> _voices = [];
    private Int64 _position;
    private Int64 _step;
    private Double _nextStepAt;

    protected static Double Hz(Int32 midi) => 440 * Math.Pow(2, (midi - 69) / 12.0);

    protected void Add(Kind kind, Double freq, Double gain, Double decay, Double seconds) => _voices.Add(new(kind, freq, gain, decay, seconds));

    protected abstract void Trigger(Int64 step);

    public void Generate(Single[] buffer) {
        for (var n = 0; n < buffer.Length; n++, _position++) {
            if (_position >= _nextStepAt) {
                Trigger(_step++);
                _nextStepAt = _step * _eighthSamples;
            }

            Double sum = 0;
            for (var v = _voices.Count - 1; v >= 0; v--) {
                var voice = _voices[v];
                if (voice.Done) {
                    _voices.RemoveAt(v);
                    continue;
                }
                sum += voice.Next(_random);
            }
            buffer[n] = (Single)Math.Clamp(sum, -1, 1);
        }
    }
}

public sealed class MenuMusicGeneratorStrategy(Int32 seed = -1, Int32 bpm = 100) : StepSynthStrategy(seed, bpm) {
    private static readonly (string Chord, int[] Mel)[] _bars = [
        ("Am", [76, -1, 72, -1, 69, -1]),
        ("Am", [72, -1, 76, -1, 81, -1]),
        ("F",  [77, -1, 69, -1, 72, -1]),
        ("E",  [76, -1, 71, -1, 68, -1]),
        ("Am", [72, -1, 76, -1, 81, -1]),
        ("Dm", [77, -1, 74, -1, 69, -1]),
        ("E",  [71, -1, 68, -1, 76, -1]),
        ("Am", [69, -1, -1, -1, -1, -1]),
    ];

    protected override void Trigger(Int64 step) {
        var i = (Int32)(step % 6);
        var (name, mel) = _bars[(Int32)(step / 6 % _bars.Length)];
        var ch = AudioBank.Chords[name];

        Add(Kind.Tick, 0, i % 2 == 0 ? 0.10 : 0.04, 0, 0.02);
        if (i == 0) {
            Add(Kind.Triangle, Hz(ch.Root), 0.45, 4, 0.8);
        }
        if (i is 2 or 4) {
            foreach (var m in ch.Notes) {
                Add(Kind.Saw, Hz(m) * 1.004, 0.04, 6, 0.3);
                Add(Kind.Saw, Hz(m) * 0.996, 0.04, 6, 0.3);
            }
        }
        if (mel[i] > 0) {
            Add(Kind.Box, Hz(mel[i]), 0.18, 3, 1.0);
        }
    }
}

public sealed class DramaticPlayMusicGeneratorStrategy(Int32 seed = -1, Int32 bpm = 76) : StepSynthStrategy(seed, bpm) {
    private static readonly (string Chord, int[] Mel)[] _bars = [
        ("Am", [81, -1, -1, -1, -1, -1]),
        ("F",  [77, -1, -1, -1, 76, -1]),
        ("Dm", [74, -1, -1, -1, 77, -1]),
        ("E",  [76, -1, -1, -1, 80, -1]),
        ("Am", [81, -1, -1, -1, 84, -1]),
        ("F",  [84, -1, -1, -1, 81, -1]),
        ("E",  [83, -1, 80, -1, 76, -1]),
        ("E",  [80, -1, -1, -1, -1, -1]),
    ];

    protected override void Trigger(Int64 step) {
        var i = (Int32)(step % 6);
        var (name, mel) = _bars[(Int32)(step / 6 % _bars.Length)];
        var ch = AudioBank.Chords[name];

        if (i == 0) {
            Add(Kind.Tick, 0, 0.30, 0, 0.06);
            Add(Kind.Triangle, Hz(ch.Root), 0.50, 1.2, 2.4);
            foreach (var m in ch.Notes) {
                Add(Kind.Saw, Hz(m) * 1.004, 0.05, 0.8, 2.4);
                Add(Kind.Saw, Hz(m) * 0.996, 0.05, 0.8, 2.4);
            }
        } else if (i == 3) {
            Add(Kind.Tick, 0, 0.12, 0, 0.04);
        }
        if (mel[i] > 0) {
            Add(Kind.Box, Hz(mel[i]), 0.20, 1.2, 2.0);
        }
    }
}
