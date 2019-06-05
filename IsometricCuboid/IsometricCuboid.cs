using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Drawing.Text;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

namespace IsometricCuboidEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author => base.GetType().Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        public string Copyright => base.GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        public string DisplayName => base.GetType().Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        public Version Version => base.GetType().Assembly.GetName().Version;
        public Uri WebsiteUri => new Uri("https://forums.getpaint.net/index.php?showtopic=82358");
    }

    [PluginSupportInfo(typeof(PluginSupportInfo))]
    public class IsometricCuboidEffectPlugin : PropertyBasedEffect
    {
        private static readonly Image StaticIcon = new Bitmap(typeof(IsometricCuboidEffectPlugin), "IsometricCuboid.png");

        public IsometricCuboidEffectPlugin()
            : base("Isometric Cuboid", StaticIcon, SubmenuNames.Render, new EffectOptions { Flags = EffectFlags.Configurable })
        {
        }

        private enum PropertyNames
        {
            Height,
            Width,
            Length,
            DrawHiddenEdges,
            DrawFootprint,
            CuboidPosition,
            EdgeOutlineWidth,
            EdgeOutlineColor,
            Shape,
            FillStyle,
            FillColor,
            AntiAlias
        }

        private enum Shape
        {
            Cuboid,
            Pyramid
        }

        private enum FillStyle
        {
            None,
            Solid,
            Shaded
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            Property[] props = new Property[]
            {
                new Int32Property(PropertyNames.Height, 175, 0, 1000),
                new Int32Property(PropertyNames.Width, 150, 0, 1000),
                new Int32Property(PropertyNames.Length, 200, 0, 1000),
                new BooleanProperty(PropertyNames.DrawFootprint, false),
                StaticListChoiceProperty.CreateForEnum<Shape>(PropertyNames.Shape, 0, false),
                new Int32Property(PropertyNames.EdgeOutlineWidth, 2, 0, 10),
                new BooleanProperty(PropertyNames.DrawHiddenEdges, false),
                new Int32Property(PropertyNames.EdgeOutlineColor, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff),
                StaticListChoiceProperty.CreateForEnum<FillStyle>(PropertyNames.FillStyle, 0, false),
                new Int32Property(PropertyNames.FillColor, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.SecondaryColor.B, EnvironmentParameters.SecondaryColor.G, EnvironmentParameters.SecondaryColor.R, 255)), 0, 0xffffff),
                new DoubleVectorProperty(PropertyNames.CuboidPosition, Pair.Create(0.0, 0.0), Pair.Create(-1.0, -1.0), Pair.Create(+1.0, +1.0)),
                new BooleanProperty(PropertyNames.AntiAlias, true)
            };

            PropertyCollectionRule[] propRules = new PropertyCollectionRule[]
            {
                new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.FillColor, PropertyNames.FillStyle, FillStyle.None, false),
                new ReadOnlyBoundToValueRule<int, Int32Property>(PropertyNames.EdgeOutlineColor, PropertyNames.EdgeOutlineWidth, 0, false),
                new ReadOnlyBoundToValueRule<int, Int32Property>(PropertyNames.DrawHiddenEdges, PropertyNames.EdgeOutlineWidth, 0, false)
            };

            return new PropertyCollection(props, propRules);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Height, ControlInfoPropertyNames.DisplayName, "Height");
            configUI.SetPropertyControlValue(PropertyNames.Width, ControlInfoPropertyNames.DisplayName, "Width");
            configUI.SetPropertyControlValue(PropertyNames.Length, ControlInfoPropertyNames.DisplayName, "Length");
            configUI.SetPropertyControlValue(PropertyNames.DrawHiddenEdges, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.DrawHiddenEdges, ControlInfoPropertyNames.Description, "Draw Hidden Edges");
            configUI.SetPropertyControlValue(PropertyNames.DrawFootprint, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.DrawFootprint, ControlInfoPropertyNames.Description, "Draw dimensions of cuboid's footprint");
            configUI.SetPropertyControlValue(PropertyNames.CuboidPosition, ControlInfoPropertyNames.DisplayName, "Cuboid Position");
            configUI.SetPropertyControlValue(PropertyNames.CuboidPosition, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPosition, ControlInfoPropertyNames.SliderLargeChangeX, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPosition, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPosition, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPosition, ControlInfoPropertyNames.SliderLargeChangeY, 0.25);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPosition, ControlInfoPropertyNames.UpDownIncrementY, 0.01);
            configUI.SetPropertyControlValue(PropertyNames.CuboidPosition, ControlInfoPropertyNames.DecimalPlaces, 3);
            Rectangle selRect = EnvironmentParameters.GetSelection(EnvironmentParameters.SourceSurface.Bounds).GetBoundsInt();
            ImageResource selImage = ImageResource.FromImage(EnvironmentParameters.SourceSurface.CreateAliasedBitmap(selRect));
            configUI.SetPropertyControlValue(PropertyNames.CuboidPosition, ControlInfoPropertyNames.StaticImageUnderlay, selImage);
            configUI.SetPropertyControlValue(PropertyNames.EdgeOutlineWidth, ControlInfoPropertyNames.DisplayName, "Edge Outline Width");
            configUI.SetPropertyControlValue(PropertyNames.EdgeOutlineColor, ControlInfoPropertyNames.DisplayName, "Edge Outline Color");
            configUI.SetPropertyControlType(PropertyNames.EdgeOutlineColor, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.Shape, ControlInfoPropertyNames.DisplayName, "Shape");
            PropertyControlInfo Amount9Control = configUI.FindControlForPropertyName(PropertyNames.Shape);
            Amount9Control.SetValueDisplayName(Shape.Cuboid, "Cuboid");
            Amount9Control.SetValueDisplayName(Shape.Pyramid, "Pyramid");
            configUI.SetPropertyControlValue(PropertyNames.FillStyle, ControlInfoPropertyNames.DisplayName, "Fill Style");
            PropertyControlInfo Amount10Control = configUI.FindControlForPropertyName(PropertyNames.FillStyle);
            Amount10Control.SetValueDisplayName(FillStyle.None, "None");
            Amount10Control.SetValueDisplayName(FillStyle.Solid, "Solid");
            Amount10Control.SetValueDisplayName(FillStyle.Shaded, "Shaded");
            configUI.SetPropertyControlValue(PropertyNames.FillColor, ControlInfoPropertyNames.DisplayName, "Fill Color");
            configUI.SetPropertyControlType(PropertyNames.FillColor, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.AntiAlias, ControlInfoPropertyNames.DisplayName, "Misc");
            configUI.SetPropertyControlValue(PropertyNames.AntiAlias, ControlInfoPropertyNames.Description, "Anti-aliasing");

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Amount1 = newToken.GetProperty<Int32Property>(PropertyNames.Height).Value;
            Amount2 = newToken.GetProperty<Int32Property>(PropertyNames.Width).Value;
            Amount3 = newToken.GetProperty<Int32Property>(PropertyNames.Length).Value;
            Amount4 = newToken.GetProperty<BooleanProperty>(PropertyNames.DrawHiddenEdges).Value;
            Amount5 = newToken.GetProperty<BooleanProperty>(PropertyNames.DrawFootprint).Value;
            Amount6 = newToken.GetProperty<DoubleVectorProperty>(PropertyNames.CuboidPosition).Value;
            Amount7 = newToken.GetProperty<Int32Property>(PropertyNames.EdgeOutlineWidth).Value;
            Amount8 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.EdgeOutlineColor).Value);
            Amount9 = (Shape)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Shape).Value;
            Amount10 = (FillStyle)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.FillStyle).Value;
            Amount11 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.FillColor).Value);
            Amount12 = newToken.GetProperty<BooleanProperty>(PropertyNames.AntiAlias).Value;

            Size selSize = EnvironmentParameters.GetSelection(srcArgs.Surface.Bounds).GetBoundsInt().Size;
            float centerX = selSize.Width / 2f;

            // Convert degrees into radians
            const double rad30 = Math.PI / 180 * 30;
            const double rad60 = Math.PI / 180 * 60;

            SizeF leftLengths = new SizeF
            {
                Width = (float)(Amount2 * Math.Sin(rad60)),
                Height = (float)(Amount2 * Math.Sin(rad30))
            };
            SizeF rightLengths = new SizeF
            {
                Width = (float)(Amount3 * Math.Sin(rad60)),
                Height = (float)(Amount3 * Math.Sin(rad30))
            };

            // Allows the cuboid to be centered
            PointF basePoint = new PointF
            {
                X = (centerX - (rightLengths.Width - leftLengths.Width) / 2f),
                Y = 0
            };
            switch (Amount9)
            {
                case Shape.Pyramid:
                    float pyraHeight = Math.Max(leftLengths.Height / 2 + rightLengths.Height / 2 + Amount1, rightLengths.Height + leftLengths.Height);
                    basePoint.Y = selSize.Height - (selSize.Height - pyraHeight) / 2f;
                    break;
                case Shape.Cuboid:
                default:
                    basePoint.Y = selSize.Height - (selSize.Height - leftLengths.Height - rightLengths.Height - Amount1) / 2f;
                    break;
            }

            // Offsets
            basePoint.X = (float)(basePoint.X + basePoint.X * Amount6.First);
            basePoint.Y = (float)(basePoint.Y + basePoint.Y * Amount6.Second);

            // Define Vertices Points
            PointF frontBottom = new PointF(basePoint.X, basePoint.Y);
            PointF frontTop = new PointF(basePoint.X, basePoint.Y - Amount1);

            PointF leftBottom = new PointF(basePoint.X - leftLengths.Width, basePoint.Y - leftLengths.Height);
            PointF leftTop = new PointF(basePoint.X - leftLengths.Width, basePoint.Y - Amount1 - leftLengths.Height);

            PointF rightBottom = new PointF(basePoint.X + rightLengths.Width, basePoint.Y - rightLengths.Height);
            PointF rightTop = new PointF(basePoint.X + rightLengths.Width, basePoint.Y - Amount1 - rightLengths.Height);

            PointF backBottom = new PointF(basePoint.X - leftLengths.Width + rightLengths.Width, basePoint.Y - rightLengths.Height - leftLengths.Height);
            PointF backTop = new PointF(basePoint.X - leftLengths.Width + rightLengths.Width, basePoint.Y - Amount1 - rightLengths.Height - leftLengths.Height);

            PointF baseCenterBottom = new PointF(basePoint.X - leftLengths.Width / 2 + rightLengths.Width / 2, basePoint.Y - leftLengths.Height / 2 - rightLengths.Height / 2);
            PointF baseCenterTop = new PointF(basePoint.X - leftLengths.Width / 2 + rightLengths.Width / 2, basePoint.Y - leftLengths.Height / 2 - rightLengths.Height / 2 - Amount1);

            // Drawing Resources
            Bitmap cuboidBitmap = new Bitmap(selSize.Width, selSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics cuboidGraphics = Graphics.FromImage(cuboidBitmap);
            cuboidGraphics.SmoothingMode = Amount12 ? SmoothingMode.AntiAlias : SmoothingMode.None;

            Pen cuboidPen = new Pen(Amount8, Amount7)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            Pen hiddenPen = new Pen(Amount8, Amount7)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                DashStyle = DashStyle.Dot
            };

            SolidBrush fillBrush = new SolidBrush(Color.Transparent);

            Color fillColorSolid = Amount11;

            HsvColor fillColorBase = HsvColor.FromColor(Amount11);
            fillColorBase.Saturation = 100;

            HsvColor fillColorDark = fillColorBase;
            fillColorDark.Value = 80;

            HsvColor fillColorLight = fillColorBase;
            fillColorLight.Saturation = 66;

            HsvColor fillColorLighter = fillColorBase;
            fillColorLighter.Saturation = 33;

            // Shapes
            switch (Amount9)
            {
                case Shape.Cuboid:

                    // Fill Type
                    switch (Amount10)
                    {
                        case FillStyle.None:
                            break;
                        case FillStyle.Solid:
                            fillBrush.Color = fillColorSolid;

                            PointF[] solidFillPoints = { frontBottom, leftBottom, leftTop, backTop, rightTop, rightBottom };
                            cuboidGraphics.FillPolygon(fillBrush, solidFillPoints);
                            break;
                        case FillStyle.Shaded:
                            fillBrush.Color = fillColorLight.ToColor();
                            PointF[] topFillPoints = { frontTop, leftTop, backTop, rightTop };
                            cuboidGraphics.FillPolygon(fillBrush, topFillPoints);

                            fillBrush.Color = fillColorBase.ToColor();
                            PointF[] leftFillPoints = { frontBottom, leftBottom, leftTop, frontTop };
                            cuboidGraphics.FillPolygon(fillBrush, leftFillPoints);

                            fillBrush.Color = fillColorDark.ToColor();
                            PointF[] rightFillPoints = { frontBottom, rightBottom, rightTop, frontTop };
                            cuboidGraphics.FillPolygon(fillBrush, rightFillPoints);
                            break;
                    }

                    // Edge Outlines
                    if (Amount7 != 0)
                    {
                        cuboidGraphics.DrawLine(cuboidPen, frontTop, frontBottom);
                        cuboidGraphics.DrawLine(cuboidPen, leftTop, frontTop);
                        cuboidGraphics.DrawLine(cuboidPen, leftBottom, frontBottom);
                        cuboidGraphics.DrawLine(cuboidPen, leftTop, leftBottom);
                        cuboidGraphics.DrawLine(cuboidPen, rightTop, frontTop);
                        cuboidGraphics.DrawLine(cuboidPen, rightBottom, frontBottom);
                        cuboidGraphics.DrawLine(cuboidPen, rightTop, rightBottom);
                        cuboidGraphics.DrawLine(cuboidPen, backTop, rightTop);
                        cuboidGraphics.DrawLine(cuboidPen, backTop, leftTop);

                        // Hidden Outlines 
                        if (Amount4)
                        {
                            cuboidGraphics.DrawLine(hiddenPen, backBottom, rightBottom);
                            cuboidGraphics.DrawLine(hiddenPen, backBottom, leftBottom);
                            cuboidGraphics.DrawLine(hiddenPen, backBottom, backTop);
                        }
                    }

                    break;
                case Shape.Pyramid:

                    double xDis1 = leftLengths.Width / 2 + rightLengths.Width / 2;
                    double yDis1 = leftLengths.Height / 2 + rightLengths.Height / 2 + Amount1 - leftLengths.Height;
                    double helperLength1 = Math.Sqrt(Math.Pow(xDis1, 2) + Math.Pow(yDis1, 2));
                    double helperAngle1 = 180 / Math.PI * Math.Asin(yDis1 / helperLength1);

                    double xDis2 = leftLengths.Width / 2 + rightLengths.Width / 2;
                    double yDis2 = leftLengths.Height / 2 + rightLengths.Height / 2 + Amount1 - rightLengths.Height;
                    double helperLength2 = Math.Sqrt(Math.Pow(xDis2, 2) + Math.Pow(yDis2, 2));
                    double helperAngle2 = 180 / Math.PI * Math.Asin(yDis2 / helperLength2);

                    double xDis3 = rightLengths.Width - leftLengths.Width / 2 - rightLengths.Width / 2;
                    double yDis3 = leftLengths.Height / 2 + rightLengths.Height / 2 + Amount1 - rightLengths.Height - leftLengths.Height;
                    double helperLength3 = Math.Sqrt(Math.Pow(xDis3, 2) + Math.Pow(yDis3, 2));
                    double helperAngle3 = 180 / Math.PI * Math.Asin(yDis3 / helperLength3);

                    // Fill Type
                    switch (Amount10)
                    {
                        case FillStyle.None:
                            break;
                        case FillStyle.Solid:
                            fillBrush.Color = fillColorSolid;

                            PointF[] solidFillPoints = { frontBottom, leftBottom, baseCenterTop, rightBottom };
                            cuboidGraphics.FillPolygon(fillBrush, solidFillPoints);

                            if (helperAngle1 < 30)
                            {
                                PointF[] solidFillPointsBL = { leftBottom, baseCenterTop, backBottom };
                                cuboidGraphics.FillPolygon(fillBrush, solidFillPointsBL);
                            }

                            if (helperAngle2 < 30)
                            {
                                PointF[] solidFillPointsBR = { rightBottom, baseCenterTop, backBottom };
                                cuboidGraphics.FillPolygon(fillBrush, solidFillPointsBR);
                            }

                            break;
                        case FillStyle.Shaded:
                            fillBrush.Color = fillColorBase.ToColor();
                            PointF[] leftFrontFillPoints = { frontBottom, leftBottom, baseCenterTop };
                            cuboidGraphics.FillPolygon(fillBrush, leftFrontFillPoints);

                            fillBrush.Color = fillColorDark.ToColor();
                            PointF[] rightFrontFillPoints = { frontBottom, rightBottom, baseCenterTop };
                            cuboidGraphics.FillPolygon(fillBrush, rightFrontFillPoints);

                            if (helperAngle1 < 30)
                            {
                                fillBrush.Color = fillColorLighter.ToColor();
                                PointF[] solidFillPointsBL = { leftBottom, baseCenterTop, backBottom };
                                cuboidGraphics.FillPolygon(fillBrush, solidFillPointsBL);
                            }

                            if (helperAngle2 < 30)
                            {
                                fillBrush.Color = fillColorLight.ToColor();
                                PointF[] solidFillPointsBR = { rightBottom, baseCenterTop, backBottom };
                                cuboidGraphics.FillPolygon(fillBrush, solidFillPointsBR);
                            }

                            break;
                    }

                    // Edge Outlines
                    if (Amount7 != 0)
                    {
                        cuboidGraphics.DrawLine(cuboidPen, frontBottom, leftBottom);
                        cuboidGraphics.DrawLine(cuboidPen, frontBottom, rightBottom);
                        cuboidGraphics.DrawLine(cuboidPen, baseCenterTop, frontBottom);
                        cuboidGraphics.DrawLine(cuboidPen, baseCenterTop, leftBottom);
                        cuboidGraphics.DrawLine(cuboidPen, baseCenterTop, rightBottom);

                        if (helperAngle1 < 30)
                            cuboidGraphics.DrawLine(cuboidPen, backBottom, leftBottom);
                        else if (Amount4)
                            cuboidGraphics.DrawLine(hiddenPen, backBottom, leftBottom);

                        if (helperAngle2 < 30)
                            cuboidGraphics.DrawLine(cuboidPen, backBottom, rightBottom);
                        else if (Amount4)
                            cuboidGraphics.DrawLine(hiddenPen, backBottom, rightBottom);

                        if ((xDis3 > 0 && helperAngle3 < helperAngle2) || (xDis3 < 0 && helperAngle3 < helperAngle1))
                            cuboidGraphics.DrawLine(cuboidPen, baseCenterTop, backBottom);
                        else if (Amount4)
                            cuboidGraphics.DrawLine(hiddenPen, baseCenterTop, backBottom);
                    }

                    break;
            }

            cuboidPen.Dispose();
            hiddenPen.Dispose();
            fillBrush.Dispose();

            if (Amount5)
            {
                int objectWidth = (int)(leftLengths.Width + rightLengths.Width);
                int objectHeight;
                switch (Amount9)
                {
                    case Shape.Pyramid:
                        objectHeight = (int)Math.Max(leftLengths.Height / 2 + rightLengths.Height / 2 + Amount1, rightLengths.Height + leftLengths.Height);
                        break;
                    case Shape.Cuboid:
                    default:
                        objectHeight = (int)(leftLengths.Height + rightLengths.Height + Amount1);
                        break;
                }

                cuboidGraphics.SmoothingMode = SmoothingMode.AntiAlias;

                Pen dimPen = new Pen(Color.Red, 1)
                {
                    StartCap = LineCap.ArrowAnchor,
                    EndCap = LineCap.ArrowAnchor
                };

                PointF heightTop = new PointF(basePoint.X - leftLengths.Width - 10, basePoint.Y - objectHeight);
                PointF heightBottom = new PointF(basePoint.X - leftLengths.Width - 10, basePoint.Y);
                cuboidGraphics.DrawLine(dimPen, heightTop, heightBottom);

                PointF widthLeft = new PointF(basePoint.X - leftLengths.Width, basePoint.Y - objectHeight - 10);
                PointF widthRight = new PointF(basePoint.X + rightLengths.Width, basePoint.Y - objectHeight - 10);
                cuboidGraphics.DrawLine(dimPen, widthLeft, widthRight);

                dimPen.Dispose();

                cuboidGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                StringFormat dimFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center
                };

                using (SolidBrush fontBrush = new SolidBrush(Color.Red))
                using (Font font = new Font(new FontFamily("Arial"), 14))
                {
                    cuboidGraphics.DrawString(objectWidth.ToString() + "px", font, fontBrush, basePoint.X - leftLengths.Width + objectWidth / 2, basePoint.Y - objectHeight - 40, dimFormat);
                    cuboidGraphics.DrawString(objectHeight.ToString() + "px", font, fontBrush, basePoint.X - leftLengths.Width - 50, basePoint.Y - objectHeight / 2, dimFormat);
                }
            }

            cuboidSurface = Surface.CopyFromBitmap(cuboidBitmap);
            cuboidBitmap.Dispose();

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, renderRects[i]);
            }
        }

        private int Amount1 = 175; // [0,1000] Height
        private int Amount2 = 150; // [0,1000] Left
        private int Amount3 = 200; // [0,1000] Right
        private bool Amount4 = false; // [0,1] Draw Back Edges
        private bool Amount5 = false; // [0,1] Draw Perspective Dimensions
        private Pair<double, double> Amount6 = Pair.Create(0.0, 0.0); // Offset
        private int Amount7 = 2; // Line Width
        private ColorBgra Amount8 = ColorBgra.FromBgr(0, 0, 0); // Line Color
        private Shape Amount9 = 0; // Shape|Cuboid|Pyramid
        private FillStyle Amount10 = 0; // Fill|None|Solid|Shaded
        private ColorBgra Amount11 = ColorBgra.FromBgr(0, 0, 0); // Fill Color
        private bool Amount12 = true; // [0,1] Anti-aliasing

        private Surface cuboidSurface;
        private readonly BinaryPixelOp normalOp = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal);

        private void Render(Surface dst, Surface src, Rectangle rect)
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
