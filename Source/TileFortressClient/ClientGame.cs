using GeneralShare;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TileFortress.Client.Net;
using TileFortress.GameWorld;
using TileFortress.Net;
using TileFortress.Utils;

namespace TileFortress.Client
{
    public class ClientGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // content
        private BitmapFont _font;
        private Atlas _atlas;
        private TextureRegion2D _whitePixel;
        private Dictionary<int, string> _tileRegions;

        // world
        private NetGameClient _client;
        private World _world;
        private Dictionary<ChunkPosition, Chunk> _requests = new Dictionary<ChunkPosition, Chunk>();
        private Tile _brushTile = new Tile(0);

        // rendering
        public const int DrawDistance = 5;

        private Effect _chunkShader;
        private SimplexNoise _noise = new SimplexNoise(12345);
        private Vector2 _offset;
        private float _zoom = 1;

        private float[,] _chunkUpdates;
        private const float ChunkUpdateDuration = 0.75f;

        public ClientGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Window.AllowUserResizing = true;
            IsMouseVisible = true;

            base.Initialize();
            Input.SetWindow(Window);

            _chunkUpdates = new float[DrawDistance, DrawDistance];

            _world = new World(World_ChunkRequest);
            _world.OnBuildOrder += (w, c, order) =>
            {
                _chunkUpdates[c.Position.X, c.Position.Y] = ChunkUpdateDuration;
            };

            _client = new NetGameClient();
            _client.Open();
            System.Threading.Tasks.Task.Run(() =>
            {
                _client.Connect(IPAddress.Loopback, AppConstants.NetDefaultPort);
            });
        }

        private Chunk World_ChunkRequest(World sender, ChunkPosition position)
        {
            if (!_requests.TryGetValue(position, out var chunk))
            {
                var request = new ChunkRequest(position);
                _client.RequestChunk(request);
                _requests.Add(position, null);
                return null;
            }
            return chunk;
        }

        private void ProcessReceivedMessages()
        {
            ReadChunkMessages();
            ReadBuildOrders();
        }

        private void ReadChunkMessages()
        {
            while (_client.Chunks.Count > 0)
            {
                var chunk = _client.Chunks.Dequeue();
                _requests[chunk.Position] = chunk;
            }
        }

        private void ReadBuildOrders()
        {
            while (_client.BuildOrders.Count > 0)
            {
                var order = _client.BuildOrders.Dequeue();
                _world.EnqueueBuildOrder(order);
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<BitmapFont>("Fonts/Pixel-Unicode_normal");

            LoadTextures();

            _chunkShader = Content.Load<Effect>("Shaders/ChunkShader");
        }
        
        private void LoadTextures()
        {
            string texPath = Path.Combine(Content.RootDirectory, "Textures");
            string cachePath = Path.Combine(Content.RootDirectory, "Textures/Cache");
            _atlas = new Atlas(GraphicsDevice, new DirectoryInfo(texPath), new DirectoryInfo(cachePath));
            _atlas.Load(null);

            var grayscale = _atlas.GetRegion("grayscale");
            _whitePixel = new TextureRegion2D(
                grayscale.Texture, new RectangleF(grayscale.X + 1, grayscale.Y + 1, 1, 1));

            _tileRegions = new Dictionary<int, string>()
            {
                { 1, "Tiles/sand" },
                { 2, "Tiles/gravel" },
                { 3, "Tiles/grass" },
            };
        }

        protected override void UnloadContent()
        {
            _client.Dispose();
        }

        private Matrix _transform;

        protected override void Update(GameTime time)
        {
            Input.Update(time);
            if (Input.IsKeyDown(Keys.Escape))
                Exit();

            _client.ReadMessages();
            ProcessReceivedMessages();

            _world.Update(time);

            if (Input.IsKeyDown(Keys.D1))
            {
                _brushTile = new Tile(0);
            }
            else if (Input.IsKeyDown(Keys.D2))
            {
                _brushTile = new Tile(1);
            }
            else if (Input.IsKeyDown(Keys.D3))
            {
                _brushTile = new Tile(2);
            }
            else if (Input.IsKeyDown(Keys.D4))
            {
                _brushTile = new Tile(3);
            }

            var view = GraphicsDevice.Viewport;
            _transform =
                Matrix.CreateTranslation(_offset.ToVector3()) *
                Matrix.CreateScale(_zoom) *
                Matrix.CreateTranslation(view.Width / 2f, view.Height / 2f, 0);

            var invertedWorld = Matrix.Invert(_transform);
            _mouseInWorld = Vector2.Transform(Input.MousePosition.ToVector2(), invertedWorld);

            var tmp = _mouseInWorld / 8f;
            _selectedTile = new Point((int)tmp.X, (int)tmp.Y);
            
            _zoom = MathHelper.Clamp(_zoom + Input.MouseScroll / 1000, 0.5f, 4f);
            if (Input.IsMouseDown(MouseButton.Left))
                _offset += Input.MouseVelocity.ToVector2() / _zoom;

            if (Input.IsMouseDown(MouseButton.Right) && _client.IsConnected)
            {
                const int brushSize = 3;
                for (int y = 0; y < brushSize * 2; y++)
                {
                    for (int x = 0; x < brushSize * 2; x++)
                    {
                        var brushCenter = _selectedTile + new Point(x - brushSize, y - brushSize);
                        var chunkPos = ChunkPosition.FromTile(brushCenter);

                        if (chunkPos.X >= 0 && chunkPos.X < DrawDistance &&
                            chunkPos.Y >= 0 && chunkPos.Y < DrawDistance)
                        {
                            if (_world.TryGetChunk(chunkPos, out var chunk))
                            {
                                var tilePos = new Point(brushCenter.X % 16, brushCenter.Y % 16);
                                chunk.TrySetTile(tilePos.X, tilePos.Y, _brushTile);
                            }
                        }
                    }
                }
            }

            for (int y = 0; y < _chunkUpdates.GetLength(1); y++)
            {
                for (int x = 0; x < _chunkUpdates.GetLength(0); x++)
                {
                    if (_chunkUpdates[x, y] > 0)
                        _chunkUpdates[x, y] -= time.Delta;
                    else
                        _chunkUpdates[x, y] = 0;
                }
            }

            base.Update(time);
        }

        private Vector2 _mouseInWorld;
        private Point _selectedTile;

        protected override void Draw(GameTime time)
        {
            GraphicsDevice.Clear(new Color(25, 40, 31));

            _spriteBatch.Begin(
               samplerState: SamplerState.PointClamp,
               blendState: BlendState.AlphaBlend,
               effect: _chunkShader,
               transformMatrix: _transform);

            if (_client.IsConnected)
            {
                for (int y = 0; y < DrawDistance; y++)
                {
                    for (int x = 0; x < DrawDistance; x++)
                    {
                        if (_world.TryGetChunk(new ChunkPosition(x, y), out var chunk))
                            DrawChunk(chunk);

                        float amount = 1f - _chunkUpdates[x, y] / ChunkUpdateDuration * 0.5f;
                        if (amount > 0.01f)
                        {
                            var color = Color.Lerp(Color.HotPink, Color.Transparent, amount);
                            int size = Chunk.Size * 8;
                            _spriteBatch.DrawFilledRectangle(new RectangleF(x * size, y * size, size, size), color);
                        }
                    }
                }
            }

            _spriteBatch.DrawFilledRectangle(new RectangleF(_selectedTile.X * 8, _selectedTile.Y * 8, 8, 8), Color.Red);

            _spriteBatch.End();

            base.Draw(time);
        }

        private void DrawChunk(Chunk chunk)
        {
            if (chunk == null)
                return;

            Tile firstTile = chunk.GetTile(0, 0);
            bool isRandom = false;
            for (int i = 0; i < Chunk.Size * Chunk.Size; i++)
            {
                Tile tile = chunk.GetTile(i);
                if (firstTile.ID != tile.ID)
                {
                    isRandom = true;
                    break;
                }
            }

            if (!isRandom)
            {
                if (TryGetTileRegion(firstTile, out var region))
                {
                    int x = chunk.Position.TileX;
                    int y = chunk.Position.TileY;
                    _spriteBatch.Draw(region, new RectangleF(x * 8, y * 8, 8 * Chunk.Size, 8 * Chunk.Size), Color.White);
                }
                return;
            }

            // draw chunk
            for (int y = 0; y < Chunk.Size; y++)
            {
                for (int x = 0; x < Chunk.Size; x++)
                {
                    if (!chunk.TryGetTile(x, y, out Tile tile))
                        continue;

                    if(!TryGetTileRegion(tile, out TextureRegion2D region))
                        continue;

                    int xx = x + chunk.Position.TileX;
                    int yy = y + chunk.Position.TileY;
                    var dst = new RectangleF(xx * 8, yy * 8, 8, 8);

                    float flipNoise = _noise.CalcPixel2D(xx, yy, 1f);
                    SpriteEffects flip = SpriteEffects.None;
                    if (flipNoise > 85)
                        flip = flipNoise > 170 ? SpriteEffects.FlipVertically : SpriteEffects.FlipHorizontally;

                    _spriteBatch.Draw(
                        region.Texture, dst, region.Bounds, Color.White, 0, Vector2.Zero, flip, 0);
                }
            }
        }

        private bool TryGetTileRegion(Tile tile, out TextureRegion2D region)
        {
            if (!_tileRegions.TryGetValue(tile.ID, out string regionName))
            {
                region = null;
                return false;
            }
            return _atlas.TryGetRegion(regionName, out region);
        }
    }
}