using GeneralShare;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.TextureAtlases;

namespace Testing
{
    public class GameFrame : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch _spriteBatch;

        private BitmapFont _font;

        private TextureRegion2D[] _weaponRegions;
        private TextureRegion2D[] _backgroundRegions;

        public GameFrame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Window.AllowUserResizing = true;
            IsMouseVisible = true;

            base.Initialize();

            Input.SetWindow(Window);
        }
        
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<BitmapFont>("Fonts/Pixel-Unicode_normal");
        }

        protected override void UnloadContent()
        {
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
            
            _spriteBatch.Begin(samplerState: SamplerState.PointWrap, blendState: BlendState.NonPremultiplied);

            _spriteBatch.End();

            base.Draw(time);
        }
    }
}