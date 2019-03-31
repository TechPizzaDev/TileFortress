using AtlasShare;
using GeneralShare;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TileFortress.Client
{
    public class Atlas : IDisposable
    {
        public delegate void AtlasLoadDelegate(Atlas sender);
        
        private bool _disposed;
        private bool _firstLoadOccured;
        private JsonSerializer _jsonSerializer;
        private DirectoryInfo _atlasRoot;
        private DirectoryInfo _atlasCacheDirectory;
        
        private GraphicsDevice _graphicsDevice;
        private Texture2D[] _textures;
        private Dictionary<string, Region> _regions;

        public event AtlasLoadDelegate OnLoad;
        public event AtlasLoadDelegate OnUnload;

        public bool IsLoaded { get; private set; }
        public ReadOnlyWrapper Regions { get; private set; }

        public Atlas(GraphicsDevice device, DirectoryInfo atlasRoot, DirectoryInfo atlasCache)
        {
            _graphicsDevice = device;

            _atlasRoot = atlasRoot;
            _atlasCacheDirectory = atlasCache;
            
            _jsonSerializer = new JsonSerializer
            {
                Converters =
                {
                    new CustomComparerDictionaryCreationConverter<string>(StringComparer.Ordinal)
                }
            };
            
            _regions = new Dictionary<string, Region>(StringComparer.Ordinal);
            Regions = new ReadOnlyWrapper(this);
        }

        public bool TryGetRegion(string textureName, out TextureRegion2D region)
        {
            bool success = _regions.TryGetValue(textureName, out Region reg);
            region = reg; // 'out' cannot implicitly cast Region to TextureRegion2D
            return success;
        }

        public TextureRegion2D GetRegion(string textureName)
        {
            if(TryGetRegion(textureName, out var tex))
                return tex;
            throw new Exception($"Region \"{textureName}\" was not found.");
        }
        
        private (AtlasData, Texture2D[]) StitchAtlas(ProgressDelegate onProgress)
        {
            int maxSize = _graphicsDevice.MaxTextureSize;
            Log.Debug($"Max Texture Size: {maxSize}x{maxSize}");
            
            var retreiver = new AtlasRootRetreiver(_atlasRoot);
            var packer = new AtlasPacker(maxSize, new ImageSpacing(1, 1, 1, 1));

            List<AtlasRootDirectory> dirs = retreiver.GetDirectories();
            for (int i = 0; i < dirs.Count; i++)
            {
                AtlasRootDirectory dir = dirs[i];
                AtlasImageBatch batch = retreiver.GetImageBatch(dir);

                /*
                if (dir.Description.ForceSingle)
                    packer.PackSingleBatch(batch);
                else*/
                    packer.PackBatch(batch);

                onProgress.Invoke((i + 1f) / dirs.Count * 0.09f); // 9% weight
            }

            packer.TrimStates();
            onProgress.Invoke(0.1f); // 1% weight (10% because we just finished the 9% weighted packing)

            var textures = new Texture2D[packer.PackCount];

            unsafe void OnTexture(IntPtr img, int width, int height, int texture)
            {
                textures[texture] = new Texture2D(_graphicsDevice, width, height);
                textures[texture].SetData(img, 0, sizeof(Rgba32), width * height);
            }

            var serializer = new AtlasSerializer(MonoGame.Imaging.ImageSaveFormat.Tga);
            var data = serializer.Serialize(packer, _atlasCacheDirectory, OnTexture, (x) => onProgress(x * 0.9f + 0.1f));
            onProgress.Invoke(1f);

            return (data, textures);
        }

        public void Load(ProgressDelegate onProgress)
        {
            if (IsLoaded)
            {
                if (_firstLoadOccured)
                    Unload();
            }
            else
                _firstLoadOccured = true;

            AtlasData atlasData;
            var cachedFile = new FileInfo(Path.Combine(_atlasCacheDirectory.FullName, "CachedAtlas.json"));
            if (cachedFile.Exists)
            {
                try
                {
                    atlasData = _jsonSerializer.Deserialize<AtlasData>(cachedFile);
                }
                finally
                {
                    cachedFile.Delete();
                }
                                                                                            // TODO: this just screams for some kind of progress chain,
                void WeightedProgress(float x) => onProgress?.Invoke(x * 49 / 50f); //       there are like 5 places where a progress chain is needed
                                                                                            // Progress Chain: multiple progress delegates with offsets and weights
                _textures = LoadTextures(atlasData, WeightedProgress);
            }
            else
            {
                var watch = Stopwatch.StartNew();
                (atlasData, _textures) = StitchAtlas((x) => onProgress?.Invoke(x * 49 / 50f));
                watch.Stop();

                Log.Info("Atlas stiching took " + watch.Elapsed.ToPreciseString());
            }
            UpdateKeys(atlasData);
            
            IsLoaded = true;
            OnLoad?.Invoke(this);
            onProgress?.Invoke(1f);
        }

        private Texture2D[] LoadTextures(AtlasData data, ProgressDelegate onProgress)
        {
            var textures = new Texture2D[data.Textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                string file = $"{_atlasCacheDirectory.FullName}/{data.Textures[i]}";
                using (var fs = new FileStream(file, FileMode.Open))
                //using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                    textures[i] = Texture2D.FromStream(_graphicsDevice, fs);

                onProgress?.Invoke((i + 1) / textures.Length);
            }
            return textures;
        }

        private void UpdateKeys(AtlasData data)
        {
            var oldRegions = _regions;
            _regions = new Dictionary<string, Region>(data.Items.Count, oldRegions.Comparer);
                
            foreach(var item in data.Items)
            {
                var keyBounds = new Rectangle(item.X, item.Y, item.Width, item.Height);
                var texture = _textures[item.Texture];

                if (oldRegions.TryGetValue(item.Key, out var existingRegion))
                {
                    oldRegions.Remove(item.Key);

                    existingRegion.Set(texture, keyBounds);
                    _regions.Add(item.Key, existingRegion);
                }
                else
                    _regions.Add(item.Key, new Region(item.Key, texture, keyBounds));
            }

            foreach (var unusedRegion in oldRegions)
                unusedRegion.Value.Set(null, Rectangle.Empty);
        }

        public void Unload()
        {
            UnloadInternal(true);
        }

        private void UnloadInternal(bool invokeUnloadEvent)
        {
            if(invokeUnloadEvent)
                OnUnload?.Invoke(this);

            if (_textures != null)
            {
                for (int i = 0; i < _textures.Length; i++)
                    _textures[i]?.Dispose();
            }

            IsLoaded = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                UnloadInternal(disposing);
                _regions.Clear();

                Regions = null;
                _jsonSerializer = null;
                _atlasRoot = null;
                _atlasCacheDirectory = null;
                _graphicsDevice = null;
                _textures = null;
                _regions = null;

                _disposed = true;
            }
        }

        ~Atlas()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        internal class Region : TextureRegion2D
        {
            public Region(string name, Texture2D texture, Rectangle bounds) :
                base(name, texture, bounds)
            {
            }

            public void Set(Texture2D texture, Rectangle bounds)
            {
                Texture = texture;
                Bounds = bounds;
            }
        }
        
        public class ReadOnlyWrapper : IReadOnlyDictionary<string, TextureRegion2D>
        {
            private Atlas _atlas;

            public TextureRegion2D this[string key] => _atlas._regions[key];
            public int Count => _atlas._regions.Count;

            public IEnumerable<string> Keys => _atlas._regions.Keys;
            public IEnumerable<TextureRegion2D> Values => _atlas._regions.Values;

            public ReadOnlyWrapper(Atlas atlas)
            {
                _atlas = atlas;
            }

            public bool ContainsKey(string key)
            {
                return _atlas._regions.ContainsKey(key);
            }

            public bool TryGetValue(string key, out TextureRegion2D value)
            {
                bool exists = _atlas._regions.TryGetValue(key, out Region region);
                value = region;
                return exists;
            }

            public Enumerator GetEnumerator() => new Enumerator(_atlas._regions);
            IEnumerator<KeyValuePair<string, TextureRegion2D>> IEnumerable<KeyValuePair<string, TextureRegion2D>>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public struct Enumerator : IEnumerator<KeyValuePair<string, TextureRegion2D>>
            {
                private Dictionary<string, Region>.Enumerator _enumerator;

                internal Enumerator(Dictionary<string, Region> dictionary)
                {
                    _enumerator = dictionary.GetEnumerator();
                }

                public KeyValuePair<string, TextureRegion2D> Current
                {
                    get
                    {
                        var c = _enumerator.Current;
                        return new KeyValuePair<string, TextureRegion2D>(c.Key, c.Value);
                    }
                }

                object IEnumerator.Current => Current;

                public bool MoveNext()
                {
                    return _enumerator.MoveNext();
                }

                public void Reset()
                {
                    throw new NotSupportedException();
                }

                public void Dispose()
                {
                    _enumerator.Dispose();
                }
            }
        }
    }
}
