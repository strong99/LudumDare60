using Raylib_cs;
using static Raylib_cs.Raylib;

namespace LudumDare60;

public readonly record struct TextureHandle(String Key, Texture2D Texture) {
    public static implicit operator Texture2D(TextureHandle h) => h.Texture;
}
public readonly record struct ModelHandle(String Key, Model Model) {
    public static implicit operator Model(ModelHandle h) => h.Model;
}

public sealed class ResourceManager {
    class ReferenceCount<TResource>(TResource resource, Int32 count) {
        public TResource Resource { get; } = resource;
        public Int32 Count { get; private set; }
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
            return new(fileName, reference.Resource);
        }

        var texture = LoadTexture(fullFileName);
        if (!IsTextureValid(texture)) {
            throw new Exception("Failed to load texture");
        }

        _textureReferences.Add(fullFileName, new(texture, 1));

        return new(fileName, texture);
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

        _modelReferences.Add(fullFileName, new(model, 1));

        return new(fullFileName, model);
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
}
