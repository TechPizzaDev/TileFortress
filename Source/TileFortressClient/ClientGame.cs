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

        private BitmapFont _font;
        private TextureRegion2D _whitePixel;
        private Atlas _atlas;
        private Dictionary<int, string> _tileRegions;

        private Effect _chunkShader;

        private NetGameClient _client;
        public static Chunk[,] _chunks;

        private Vector2 offset;
        private float zoom = 1;

        private Tile _brushTile = new Tile(0);

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

            _client = new NetGameClient();
            _client.Open();

            const int radius = 8;
            _chunks = new Chunk[radius, radius];

            System.Threading.Tasks.Task.Run(() =>
            {
                _client.Connect(IPAddress.Loopback, AppConstants.NetDefaultPort);

                for (int y = 0; y < radius; y++)
                {
                    for (int x = 0; x < radius; x++)
                    {
                        var pos = new ChunkPosition(x, y);
                        var request = new ChunkRequest(pos);
                        _client.RequestChunk(request);
                        System.Threading.Thread.Sleep(1);
                    }
                }
            });
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
                Matrix.CreateTranslation(offset.ToVector3()) *
                Matrix.CreateScale(zoom) *
                Matrix.CreateTranslation(view.Width / 2f, view.Height / 2f, 0);

            var invertedWorld = Matrix.Invert(_transform);
            _mouseInWorld = Vector2.Transform(Input.MousePosition.ToVector2(), invertedWorld);

            var tmp = _mouseInWorld / 8f;
            _selectedTile = new Point((int)tmp.X, (int)tmp.Y);
            
            zoom = MathHelper.Clamp(zoom + Input.MouseScroll / 1000, 0.5f, 4f);
            if (Input.IsMouseDown(MouseButton.Left))
                offset += Input.MouseVelocity.ToVector2() / zoom;

            if (Input.IsMouseDown(MouseButton.Right))
            {
                var chunkPos = ChunkPosition.FromTilePos(_selectedTile);
                if (chunkPos.X >= 0 && chunkPos.X < _chunks.GetLength(0) &&
                   chunkPos.Y >= 0 && chunkPos.Y < _chunks.GetLength(1))
                {
                    var tilePos = new Point(_selectedTile.X % 16, _selectedTile.Y % 16);

                    var chunk = _chunks[chunkPos.X, chunkPos.Y];
                    chunk.TrySetTile(tilePos.X, tilePos.Y, _brushTile);
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
               blendState: BlendState.NonPremultiplied,
               effect: _chunkShader,
               transformMatrix: _transform);

            foreach (var chunk in _chunks)
            {
                DrawChunk(chunk);
            }

            _spriteBatch.DrawFilledRectangle(new RectangleF(_mouseInWorld.X - 4, _mouseInWorld.Y - 4, 8, 8), Color.Red);
            _spriteBatch.DrawFilledRectangle(new RectangleF(_selectedTile.X * 8, _selectedTile.Y * 8, 8, 8), Color.Purple);

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
                var region = GetTileRegion(firstTile);
                int x = chunk.Position.TileX;
                int y = chunk.Position.TileY;
                _spriteBatch.Draw(region, new RectangleF(x * 8, y * 8, 8 * Chunk.Size, 8 * Chunk.Size), Color.White);
                return;
            }

            // draw chunk
            for (int y = 0; y < Chunk.Size; y++)
            {
                for (int x = 0; x < Chunk.Size; x++)
                {
                    if (!chunk.TryGetTile(x, y, out Tile tile))
                        continue;

                    var region = GetTileRegion(tile);
                    if (region == null)
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

        private SimplexNoise _noise = new SimplexNoise(12345);

        private TextureRegion2D GetTileRegion(Tile tile)
        {
            if (!_tileRegions.TryGetValue(tile.ID, out string regionName))
                return null;

            if (!_atlas.TryGetRegion(regionName, out var region))
                return null;

            return region;
        }
    }
}