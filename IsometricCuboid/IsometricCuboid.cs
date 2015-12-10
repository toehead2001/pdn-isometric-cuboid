using System;
using System.Drawing;
using System.Reflection;
using System.Drawing.Text;
using System.Collections.Generic;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace IsometricCuboidEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author
        {
            get
            {
                return ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
            }
        }
        public string Copyright
        {
            get
            {
                return ((AssemblyDescriptionAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
            }
        }

        public string DisplayName
        {
            get
            {
                return ((AssemblyProductAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
            }
        }

        public Version Version
        {
            get
            {
                return base.GetType().Assembly.GetName().Version;
            }
        }

        public Uri WebsiteUri
        {
            get
            {
                return new Uri("http://www.getpaint.net/redirect/plugins.html");
            }
        }
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Isometric Cuboid")]
    public class IsometricCuboidEffectPlugin : PropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return "Isometric Cuboid";
            }
        }

        public static Image StaticIcon
        {
            get
            {
                return new Bitmap(typeof(IsometricCuboidEffectPlugin), "IsometricCuboid.png");
            }
        }

        public static string SubmenuName
        {
            get
            {
                return SubmenuNames.Render;  // Programmer's chosen default
            }
        }

        public IsometricCuboidEffectPlugin()
            : base(StaticName, StaticIcon, SubmenuName, EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Amount1,
            Amount2,
            Amount3,
            Amount4,
            Amount5,
            Amount6,
            Amount7,
            Amount8
        }


        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Amount1, 100, 0, 1000));
            props.Add(new Int32Property(PropertyNames.Amount2, 150, 0, 1000));
            props.Add(new Int32Property(PropertyNames.Amount3, 200, 0, 1000));
            props.Add(new BooleanProperty(PropertyNames.Amount4, false));
            props.Add(new BooleanProperty(PropertyNames.Amount5, false));
            props.Add(new DoubleVectorProperty(PropertyNames.Amount6, Pair.Create(0.0, 0.0), Pair.Create(-1.0, -1.0), Pair.Create(+1.0, +1.0)));
            props.Add(new Int32Property(PropertyNames.Amount7, 2, 1, 10));
            props.Add(new Int32Property(PropertyNames.Amount8, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff));

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount1, ControlInfoPropertyNames.DisplayName, "Height");
            configUI.SetPropertyControlValue(PropertyNames.Amount2, ControlInfoPropertyNames.DisplayName, "Left");
            configUI.SetPropertyControlValue(PropertyNames.Amount3, ControlInfoPropertyNames.DisplayName, "Right");
            configUI.SetPropertyControlValue(PropertyNames.Amount4, ControlInfoPropertyNames.DisplayName, "Helpers");
            configUI.SetPropertyControlValue(PropertyNames.Amount4, ControlInfoPropertyNames.Description, "Draw back edges");
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.Description, "Draw dimensions of cuboid's footprint");
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.DisplayName, "Offset");
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.SliderLargeChangeX, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.SliderLargeChangeY, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.UpDownIncrementY, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.DecimalPlaces, 3);
            Rectangle selection6 = EnvironmentParameters.GetSelection(EnvironmentParameters.SourceSurface.Bounds).GetBoundsInt();
            ImageResource imageResource6 = ImageResource.FromImage(EnvironmentParameters.SourceSurface.CreateAliasedBitmap(selection6));
            configUI.SetPropertyControlValue(PropertyNames.Amount6, ControlInfoPropertyNames.StaticImageUnderlay, imageResource6);
            configUI.SetPropertyControlValue(PropertyNames.Amount7, ControlInfoPropertyNames.DisplayName, "Line Width");
            configUI.SetPropertyControlValue(PropertyNames.Amount8, ControlInfoPropertyNames.DisplayName, "Line Color");
            configUI.SetPropertyControlType(PropertyNames.Amount8, PropertyControlType.ColorWheel);

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Amount1 = newToken.GetProperty<Int32Property>(PropertyNames.Amount1).Value;
            Amount2 = newToken.GetProperty<Int32Property>(PropertyNames.Amount2).Value;
            Amount3 = newToken.GetProperty<Int32Property>(PropertyNames.Amount3).Value;
            Amount4 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount4).Value;
            Amount5 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount5).Value;
            Amount6 = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.Amount6).Value;
            Amount7 = newToken.GetProperty<Int32Property>(PropertyNames.Amount7).Value;
            Amount8 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount8).Value);


            Rectangle selection = EnvironmentParameters.GetSelection(srcArgs.Surface.Bounds).GetBoundsInt();
            float centerX = selection.Width / 2f;

            // Convert degrees into radians
            double rad30 = Math.PI / 180 * 30;
            double rad60 = Math.PI / 180 * 60;
            double rad90 = Math.PI / 180 * 90;

            // The 'Law of Sines'
            float lx = (float)(Amount2 * Math.Sin(rad60) / Math.Sin(rad90));
            float rx = (float)(Amount3 * Math.Sin(rad60) / Math.Sin(rad90));
            float ly = (float)(Amount2 * Math.Sin(rad30) / Math.Sin(rad90));
            float ry = (float)(Amount3 * Math.Sin(rad30) / Math.Sin(rad90));

            // Allows the cuboid to be centered
            float baseX = (centerX - (rx - lx) / 2f);
            float baseY = (selection.Height - (selection.Height - ly - ry - Amount1) / 2f);

            // Offsets
            baseX = (float)(baseX + baseX * Amount6.First);
            baseY = (float)(baseY + baseY * Amount6.Second);


            Bitmap cuboidBitmap = new Bitmap(selection.Width, selection.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics cuboidGraphics = Graphics.FromImage(cuboidBitmap);
            cuboidGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Pen cuboidPen = new Pen(Amount8, Amount7);
            cuboidPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            cuboidPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            PointF frontBottom = new PointF(baseX, baseY);
            PointF frontTop = new PointF(baseX, baseY - Amount1);

            cuboidGraphics.DrawLine(cuboidPen, frontTop, frontBottom);

            PointF leftBottom = new PointF(baseX - lx, baseY - ly);
            PointF leftTop = new PointF(baseX - lx, baseY - Amount1 - ly);

            cuboidGraphics.DrawLine(cuboidPen, leftTop, frontTop);
            cuboidGraphics.DrawLine(cuboidPen, leftBottom, frontBottom);
            cuboidGraphics.DrawLine(cuboidPen, leftTop, leftBottom);

            PointF rightBottom = new PointF(baseX + rx, baseY - ry);
            PointF rightTop = new PointF(baseX + rx, baseY - Amount1 - ry);

            cuboidGraphics.DrawLine(cuboidPen, rightTop, frontTop);
            cuboidGraphics.DrawLine(cuboidPen, rightBottom, frontBottom);
            cuboidGraphics.DrawLine(cuboidPen, rightTop, rightBottom);

            PointF backTop = new PointF(baseX - lx + rx, baseY - Amount1 - ry - ly);

            cuboidGraphics.DrawLine(cuboidPen, backTop, rightTop);
            cuboidGraphics.DrawLine(cuboidPen, backTop, leftTop);

            if (Amount4)
            {
                PointF backBottom = new PointF(baseX - lx + rx, baseY - ry - ly);

                cuboidPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                cuboidGraphics.DrawLine(cuboidPen, backBottom, rightBottom);
                cuboidGraphics.DrawLine(cuboidPen, backBottom, leftBottom);
                cuboidGraphics.DrawLine(cuboidPen, backBottom, backTop);
            }

            cuboidPen.Dispose();

            if (Amount5)
            {
                Pen dimPen = new Pen(Color.Red, 1);
                dimPen.StartCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                dimPen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;

                PointF heightTop = new PointF(baseX - lx - 10, baseY - Amount1 - ry - ly);
                PointF heightBottom = new PointF(baseX - lx - 10, baseY);

                cuboidGraphics.DrawLine(dimPen, heightTop, heightBottom);

                PointF widthLeft = new PointF(baseX - lx, baseY - Amount1 - ry - ly - 10);
                PointF widthRight = new PointF(baseX + rx, baseY - Amount1 - ry - ly - 10);

                cuboidGraphics.DrawLine(dimPen, widthLeft, widthRight);
                dimPen.Dispose();

                int objectWidth = (int)(lx + rx);
                int objectHeight = (int)(ly + ry + Amount1);

                cuboidGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                StringFormat dimFormat = new StringFormat();
                dimFormat.Alignment = StringAlignment.Center;

                using (SolidBrush fontBrush = new SolidBrush(Color.Red))
                using (Font font = new Font(new FontFamily("Arial"), 14))
                {
                    cuboidGraphics.DrawString(objectWidth.ToString() + "px", font, fontBrush, baseX - lx + objectWidth / 2, baseY - objectHeight - 40, dimFormat);
                    cuboidGraphics.DrawString(objectHeight.ToString() + "px", font, fontBrush, baseX - lx - 50, baseY - objectHeight / 2, dimFormat);
                }
            }

            cuboidSurface = Surface.CopyFromBitmap(cuboidBitmap);
            cuboidBitmap.Dispose();


            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, rois[i]);
            }
        }

        #region CodeLab
        int Amount1 = 100; // [0,1000] Height
        int Amount2 = 150; // [0,1000] Left
        int Amount3 = 200; // [0,1000] Right
        bool Amount4 = false; // [0,1] Draw Back Edges
        bool Amount5 = false; // [0,1] Draw Perspective Dimensions
        Pair<double, double> Amount6 = Pair.Create(0.0, 0.0); // Offset
        int Amount7 = 2; // Line Width
        ColorBgra Amount8 = ColorBgra.FromBgr(0, 0, 0); // Line Color
        #endregion

        Surface cuboidSurface;
        readonly BinaryPixelOp normalOp = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal);

        void Render(Surface dst, Surface src, Rectangle rect)
        {
            Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
            ColorBgra sourcePixel, cuboidPixel;

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    sourcePixel = src[x, y];
                    cuboidPixel = cuboidSurface.GetBilinearSample(x - selection.Left, y - selection.Top);

                    dst[x, y] = normalOp.Apply(sourcePixel, cuboidPixel);
                }
            }
        }
    }
}
