using Raylib_cs;
using static Raylib_cs.Raylib;

namespace LudumDare60;

public readonly record struct TextureHandle(String Key, Texture2D Texture) {
    public static implicit operator Texture2D(TextureHandle h) => h.Texture;
}
public readonly record struct ModelHandle(String Key, Model Model) {
    public static implicit operator Model(ModelHandle h) => h.Model;
}
public readonly record struct ModelAnimationHandle(String Key, ModelAnimation[] Animations) {
    public static implicit operator ModelAnimation[](ModelAnimationHandle h) => h.Animations;
}
public readonly record struct SoundHandle(String Key, Sound Sound) {
    public static implicit operator Sound(SoundHandle h) => h.Sound;
}

public sealed class ResourceManager {
    class ReferenceCount<TResource>(TResource resource, Int32 count) {
        public TResource Resource { get; } = resource;
        public Int32 Count { get => count; }
        public void Grab() {
            if (count < 1) {
                throw new Exception();
            }

            count++; 
        }
        public void Release() { 
            count--; 

            if (count < 0) {
                throw new Exception();
            }
        }
    }

    private readonly Dictionary<String, ReferenceCount<Texture2D>> _textureReferences = [];
    public TextureHandle GetOrLoadTexture(String fileName) {
        var fullFileName = Path.GetFullPath(fileName);
        if (_textureReferences.TryGetValue(fullFileName, out var reference)) {
            reference.Grab();
            return new(fullFileName, reference.Resource);
        }

        var texture = LoadTexture(fullFileName);
        if (!IsTextureValid(texture)) {
            throw new Exception("Failed to load texture");
        }

        _textureReferences.Add(fullFileName, new(texture, 1));

        return new(fullFileName, texture);
    }
    public void ReleaseTexture(TextureHandle texture) {
        if (!_textureReferences.TryGetValue(texture.Key, out var reference)) {
            throw new Exception("Does not exist");
        }

        reference.Release();

        if (reference.Count == 0) {
            UnloadTexture(reference.Resource);
            _textureReferences.Remove(texture.Key);
        }
    }

    private readonly Dictionary<String, ReferenceCount<Model>> _modelReferences = [];
    public ModelHandle GetOrLoadModel(String fileName) {
        var fullFileName = Path.GetFullPath(fileName);
        if (_modelReferences.TryGetValue(fullFileName, out var reference)) {
            reference.Grab();
            return new(fullFileName, reference.Resource);
        }

        var model = LoadModel(fullFileName);
        SmoothTextures(model);

        _modelReferences.Add(fullFileName, new(model, 1));

        return new(fullFileName, model);
    }
    private static unsafe void SmoothTextures(Model model) {
        var seen = new HashSet<UInt32>();
        for (var i = 0; i < model.MaterialCount; i++) {
            var texture = model.Materials[i].Maps[(Int32)MaterialMapIndex.Albedo].Texture;
            if (texture.Id == 0 || !seen.Add(texture.Id)) {
                continue;
            }
            GenTextureMipmaps(ref texture);
            SetTextureFilter(texture, TextureFilter.Anisotropic8X);
        }
    }

    public void ReleaseModel(ModelHandle model) {
        if (!_modelReferences.TryGetValue(model.Key, out var reference)) {
            throw new Exception("Does not exist");
        }

        reference.Release();

        if (reference.Count == 0) {
            UnloadModel(reference.Resource);
            _modelReferences.Remove(model.Key);
        }
    }

    private readonly Dictionary<String, ReferenceCount<ModelAnimation[]>> _animationReferences = [];
    public ModelAnimationHandle GetOrLoadAnimations(String fileName) {
        var fullFileName = Path.GetFullPath(fileName);
        if (_animationReferences.TryGetValue(fullFileName, out var reference)) {
            return new(fullFileName, reference.Resource);
        }

        var animations = LoadModelAnimations(fullFileName).ToArray();
        _animationReferences.Add(fullFileName, new(animations, 1));

        return new(fullFileName, animations);
    }
    public void ReleaseAnimations(ModelAnimationHandle model) {
        if (!_animationReferences.TryGetValue(model.Key, out var reference)) {
            throw new Exception("Does not exist");
        }

        reference.Release();

        if (reference.Count == 0) {
            UnloadModelAnimations(reference.Resource);
            _animationReferences.Remove(model.Key);
        }
    }

    private readonly Dictionary<String, ReferenceCount<Sound>> _soundReferences = [];
    private readonly Dictionary<String, Wave> _waveReferences = [];
    public SoundHandle GetOrLoadSound(String fileName) {
        var fullFileName = Path.GetFullPath(fileName);
        if (_soundReferences.TryGetValue(fullFileName, out var reference)) {
            reference.Grab();
            return new(fullFileName, LoadSoundAlias(reference.Resource));
        }

        var sound = LoadSound(fullFileName);
        _soundReferences.Add(fullFileName, new(sound, 1));

        return new(fullFileName, LoadSoundAlias(sound));
    }
    public void ReleaseSound(SoundHandle sound) {
        if (!_soundReferences.TryGetValue(sound.Key, out var reference)) {
            throw new Exception("Does not exist");
        }

        UnloadSoundAlias(sound);

        reference.Release();

        if (reference.Count == 0) {
            UnloadSound(reference.Resource);
            _soundReferences.Remove(sound.Key);

            if (_waveReferences.TryGetValue(sound.Key, out var value)) {
                UnloadWave(value);
            }
        }
    }
}
