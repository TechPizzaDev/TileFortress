using GeneralShare;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.TextureAtlases;
using System;
using System.IO;
using System.Net;
using TileFortress.Client.Net;
using TileFortress.GameWorld;
using TileFortress.Net;

namespace TileFortress.Client
{
    public class GameFrame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private BitmapFont _font;
        private Atlas _atlas;
        private TextureRegion2D _whitePixel;

        private NetGameClient _netClient;

        public GameFrame()
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

            _netClient = new NetGameClient();
            _netClient.Open();
            _netClient.Connect(IPAddress.Loopback, AppConstants.NetDefaultPort);

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    var request = new ChunkRequest(new Point(x, y));
                    _netClient.RequestChunk(request);
                }
            }
        }
        
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<BitmapFont>("Fonts/Pixel-Unicode_normal");

            string texPath = Path.Combine(Content.RootDirectory, "Textures");
            string cachePath = Path.Combine(Content.RootDirectory, "Textures/Cache");
            _atlas = new Atlas(GraphicsDevice, new DirectoryInfo(texPath), new DirectoryInfo(cachePath));
            _atlas.Load(null);

            var grayscale = _atlas.GetRegion("grayscale");
            _whitePixel = new TextureRegion2D(
                grayscale.Texture, new RectangleF(grayscale.X + 1, grayscale.Y + 1, 1, 1));
        }
        
        protected override void UnloadContent()
        {
            _netClient.Close();
        }
        
        protected override void Update(GameTime time)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();



            Input.Update(time);
            base.Update(time);
        }

        protected override void Draw(GameTime time)
        {
            GraphicsDevice.Clear(new Color(25, 40, 31));

            var transform = Matrix.CreateScale(4f);
            
            _spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                blendState: BlendState.NonPremultiplied,
                transformMatrix: transform);

            //DrawChunk(chunk);

            _spriteBatch.End();

            base.Draw(time);
        }

        private void DrawChunk(Chunk chunk)
        {
            // draw chunk
            for (int x = 0; x < Chunk.Size; x++)
            {
                for (int y = 0; y < Chunk.Size; y++)
                {
                    var region = _atlas.GetRegion("Tiles/grass");

                    _spriteBatch.Draw(region, new RectangleF(x * 8, y * 8, 8, 8), Color.White);
                }
            }
        }
    }
}