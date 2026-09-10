using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace RoachParadise
{
    internal enum MotionState { Resting, Running, Turning }
    internal enum LifeStage { StageOne, StageTwo, Adult }

    internal sealed class Roach
    {
        public double X, Y, Angle, Speed, Age, EggDue, WalkPhase, StateTime, TurnRate, ThreatCooldown, AdultLifeRemaining;
        public bool Dead;
        public double DeadAge;
        public MotionState Motion;
        public LifeStage Stage;
    }

    internal sealed class Egg
    {
        public double X, Y, Age, Rotation;
    }

    internal sealed class SprayCloud
    {
        public double X, Y, Age;
    }

    internal sealed class ControlPanelWindow : Window
    {
        private readonly OverlayWindow habitat;
        private readonly TextBlock stageOneValue;
        private readonly TextBlock stageTwoValue;
        private readonly TextBlock adultValue;
        private readonly TextBlock eggValue;
        private readonly TextBlock totalValue;
        private readonly TextBlock behaviorValue;
        private readonly TextBlock limitValue;
        private readonly Button sprayButton;
        private readonly TextBlock sprayLabel;
        private readonly Button breedButton;
        private bool allowClose;

        public ControlPanelWindow(OverlayWindow habitat)
        {
            this.habitat = habitat;
            Width = 286;
            Height = 532;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = true;
            FontFamily = new FontFamily("Microsoft YaHei");

            Border frame = new Border();
            frame.Background = new SolidColorBrush(Color.FromArgb(244, 20, 15, 10));
            frame.BorderBrush = new SolidColorBrush(Color.FromRgb(214, 143, 45));
            frame.BorderThickness = new Thickness(3);
            frame.Padding = new Thickness(15);

            StackPanel stack = new StackPanel();
            frame.Child = stack;

            Grid header = new Grid();
            header.Margin = new Thickness(0, 0, 0, 13);
            header.ColumnDefinitions.Add(new ColumnDefinition());
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            TextBlock title = new TextBlock();
            title.Text = "蟑螂乐园 // 控制台";
            title.Foreground = new SolidColorBrush(Color.FromRgb(243, 180, 63));
            title.FontFamily = new FontFamily("Consolas");
            title.FontWeight = FontWeights.Bold;
            title.FontSize = 15;
            title.VerticalAlignment = VerticalAlignment.Center;
            Button close = MakeButton("×", 34, new SolidColorBrush(Color.FromRgb(57, 38, 23)));
            close.Click += delegate { Hide(); };
            Grid.SetColumn(close, 1);
            header.Children.Add(title); header.Children.Add(close);
            header.MouseLeftButtonDown += delegate { try { DragMove(); } catch { } };
            stack.Children.Add(header);

            Border totalBox = new Border();
            totalBox.Background = new SolidColorBrush(Color.FromRgb(35, 26, 16));
            totalBox.BorderBrush = new SolidColorBrush(Color.FromRgb(97, 69, 35));
            totalBox.BorderThickness = new Thickness(2);
            totalBox.Padding = new Thickness(12, 7, 12, 7);
            totalBox.Margin = new Thickness(0, 0, 0, 10);
            Grid totalGrid = new Grid();
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition());
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            TextBlock totalLabel = new TextBlock();
            totalLabel.Text = "当前活体总数";
            totalLabel.Foreground = new SolidColorBrush(Color.FromRgb(190, 170, 130));
            totalLabel.VerticalAlignment = VerticalAlignment.Center;
            totalValue = new TextBlock();
            totalValue.Text = "0";
            totalValue.FontFamily = new FontFamily("Consolas");
            totalValue.FontSize = 28;
            totalValue.FontWeight = FontWeights.Bold;
            totalValue.Foreground = new SolidColorBrush(Color.FromRgb(201, 238, 103));
            Grid.SetColumn(totalValue, 1);
            totalGrid.Children.Add(totalLabel); totalGrid.Children.Add(totalValue);
            totalBox.Child = totalGrid;
            stack.Children.Add(totalBox);

            Grid stats = new Grid();
            stats.Margin = new Thickness(0, 0, 0, 13);
            stats.ColumnDefinitions.Add(new ColumnDefinition());
            stats.ColumnDefinitions.Add(new ColumnDefinition());
            stats.RowDefinitions.Add(new RowDefinition());
            stats.RowDefinitions.Add(new RowDefinition());
            Border stageOneBox = MakeStatBox("阶段一");
            stageOneValue = (TextBlock)((StackPanel)stageOneBox.Child).Children[1];
            stageOneValue.Foreground = new SolidColorBrush(Color.FromRgb(244, 221, 170));
            Border stageTwoBox = MakeStatBox("阶段二");
            stageTwoBox.Margin = new Thickness(8, 0, 0, 0);
            stageTwoValue = (TextBlock)((StackPanel)stageTwoBox.Child).Children[1];
            stageTwoValue.Foreground = new SolidColorBrush(Color.FromRgb(218, 119, 61));
            Border adultBox = MakeStatBox("成虫");
            adultBox.Margin = new Thickness(0, 8, 0, 0);
            adultValue = (TextBlock)((StackPanel)adultBox.Child).Children[1];
            adultValue.Foreground = new SolidColorBrush(Color.FromRgb(197, 137, 63));
            Border eggBox = MakeStatBox("卵鞘");
            eggBox.Margin = new Thickness(8, 8, 0, 0);
            eggValue = (TextBlock)((StackPanel)eggBox.Child).Children[1];
            eggValue.Foreground = new SolidColorBrush(Color.FromRgb(242, 185, 68));
            Grid.SetColumn(stageTwoBox, 1);
            Grid.SetRow(adultBox, 1);
            Grid.SetColumn(eggBox, 1); Grid.SetRow(eggBox, 1);
            stats.Children.Add(stageOneBox); stats.Children.Add(stageTwoBox);
            stats.Children.Add(adultBox); stats.Children.Add(eggBox);
            stack.Children.Add(stats);

            Grid controls = new Grid();
            controls.ColumnDefinitions.Add(new ColumnDefinition());
            controls.ColumnDefinitions.Add(new ColumnDefinition());
            controls.ColumnDefinitions.Add(new ColumnDefinition());
            Button minus = MakeButton("－ 1", 46, new SolidColorBrush(Color.FromRgb(67, 42, 24)));
            Button clear = MakeButton("清 空", 46, new SolidColorBrush(Color.FromRgb(92, 37, 25)));
            Button plus = MakeButton("＋ 1", 46, new SolidColorBrush(Color.FromRgb(67, 42, 24)));
            minus.Margin = new Thickness(0, 0, 5, 0); clear.Margin = new Thickness(3, 0, 3, 0); plus.Margin = new Thickness(5, 0, 0, 0);
            minus.Click += delegate { habitat.ChangeCount(-1); };
            clear.Click += delegate { habitat.ChangeCount(-999); };
            plus.Click += delegate { habitat.ChangeCount(1); };
            Grid.SetColumn(clear, 1); Grid.SetColumn(plus, 2);
            controls.Children.Add(minus); controls.Children.Add(clear); controls.Children.Add(plus);
            stack.Children.Add(controls);

            Grid limitControls = new Grid();
            limitControls.Margin = new Thickness(0, 9, 0, 0);
            limitControls.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52) });
            limitControls.ColumnDefinitions.Add(new ColumnDefinition());
            limitControls.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52) });
            Button limitMinus = MakeButton("－5", 36, new SolidColorBrush(Color.FromRgb(49, 36, 23)));
            Button limitPlus = MakeButton("＋5", 36, new SolidColorBrush(Color.FromRgb(49, 36, 23)));
            limitValue = new TextBlock();
            limitValue.Text = "数量上限 20";
            limitValue.Foreground = new SolidColorBrush(Color.FromRgb(190, 170, 130));
            limitValue.HorizontalAlignment = HorizontalAlignment.Center;
            limitValue.VerticalAlignment = VerticalAlignment.Center;
            limitValue.FontSize = 13;
            limitMinus.Click += delegate { habitat.ChangePopulationLimit(-5); };
            limitPlus.Click += delegate { habitat.ChangePopulationLimit(5); };
            Grid.SetColumn(limitValue, 1); Grid.SetColumn(limitPlus, 2);
            limitControls.Children.Add(limitMinus); limitControls.Children.Add(limitValue); limitControls.Children.Add(limitPlus);
            stack.Children.Add(limitControls);

            behaviorValue = new TextBlock();
            behaviorValue.Text = "静止 0 · 冲刺 0 · 转向 0";
            behaviorValue.FontFamily = new FontFamily("Consolas");
            behaviorValue.Foreground = new SolidColorBrush(Color.FromRgb(144, 129, 101));
            behaviorValue.HorizontalAlignment = HorizontalAlignment.Center;
            behaviorValue.Margin = new Thickness(0, 9, 0, 0);
            behaviorValue.FontSize = 12;
            stack.Children.Add(behaviorValue);

            breedButton = MakeButton("自然繁殖：开启", 38, new SolidColorBrush(Color.FromRgb(66, 47, 25)));
            breedButton.Margin = new Thickness(0, 9, 0, 0);
            breedButton.Click += delegate { habitat.ToggleBreeding(); };
            stack.Children.Add(breedButton);

            sprayButton = MakeButton("", 58, new SolidColorBrush(Color.FromRgb(50, 65, 26)));
            sprayButton.Margin = new Thickness(0, 7, 0, 0);
            StackPanel sprayContent = new StackPanel();
            sprayContent.Orientation = Orientation.Horizontal;
            sprayContent.HorizontalAlignment = HorizontalAlignment.Center;
            Image sprayIcon = LoadSprayIcon();
            if (sprayIcon != null) sprayContent.Children.Add(sprayIcon);
            sprayLabel = new TextBlock();
            sprayLabel.Text = "杀虫剂：关闭（F8）";
            sprayLabel.Margin = new Thickness(8, 0, 0, 0);
            sprayLabel.VerticalAlignment = VerticalAlignment.Center;
            sprayContent.Children.Add(sprayLabel);
            sprayButton.Content = sprayContent;
            sprayButton.Click += delegate { habitat.ToggleSprayFromPanel(); };
            stack.Children.Add(sprayButton);

            Content = frame;
            Loaded += delegate
            {
                Left = SystemParameters.WorkArea.Right - Width - 22;
                Top = SystemParameters.WorkArea.Top + 22;
            };
            Closing += OnClosing;
        }

        private static Border MakeStatBox(string label)
        {
            StackPanel stack = new StackPanel();
            TextBlock name = new TextBlock();
            name.Text = label; name.Foreground = new SolidColorBrush(Color.FromRgb(165, 147, 112)); name.FontSize = 13;
            TextBlock value = new TextBlock();
            value.Text = "0"; value.Foreground = new SolidColorBrush(Color.FromRgb(201, 238, 103));
            value.FontFamily = new FontFamily("Consolas"); value.FontSize = 30; value.FontWeight = FontWeights.Bold;
            stack.Children.Add(name); stack.Children.Add(value);
            Border box = new Border();
            box.Background = new SolidColorBrush(Color.FromRgb(12, 10, 7));
            box.BorderBrush = new SolidColorBrush(Color.FromRgb(75, 57, 32));
            box.BorderThickness = new Thickness(2); box.Padding = new Thickness(11, 8, 11, 6); box.Child = stack;
            return box;
        }

        private static Button MakeButton(string text, double height, Brush background)
        {
            Button button = new Button();
            button.Content = text; button.Height = height; button.Background = background;
            button.Foreground = new SolidColorBrush(Color.FromRgb(246, 226, 188));
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(126, 88, 43));
            button.BorderThickness = new Thickness(2); button.FontWeight = FontWeights.Bold;
            button.Cursor = Cursors.Hand;
            return button;
        }

        private static Image LoadSprayIcon()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("pesticide-spray.png");
            if (stream == null) return null;
            BitmapImage sheet = new BitmapImage();
            sheet.BeginInit(); sheet.CacheOption = BitmapCacheOption.OnLoad; sheet.StreamSource = stream; sheet.EndInit(); sheet.Freeze(); stream.Dispose();
            int frameWidth = sheet.PixelWidth / 6;
            CroppedBitmap firstFrame = new CroppedBitmap(sheet, new Int32Rect(0, 0, frameWidth, sheet.PixelHeight));
            Image icon = new Image();
            icon.Source = firstFrame; icon.Width = 34; icon.Height = 42; icon.Stretch = Stretch.Uniform;
            RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.NearestNeighbor);
            return icon;
        }

        public void UpdateCounts(int stageOne, int stageTwo, int adults, int eggs, int resting, int running, int turning, int limit, bool night, bool breeding, bool spraying)
        {
            stageOneValue.Text = stageOne.ToString();
            stageTwoValue.Text = stageTwo.ToString();
            adultValue.Text = adults.ToString();
            eggValue.Text = eggs.ToString();
            totalValue.Text = (stageOne + stageTwo + adults).ToString();
            behaviorValue.Text = (night ? "夜间活跃" : "日间蛰伏") + " · 静止 " + resting + " · 冲刺 " + running + " · 转向 " + turning;
            limitValue.Text = "数量上限 " + limit;
            breedButton.Content = breeding ? "自然繁殖：开启" : "自然繁殖：暂停";
            breedButton.Background = new SolidColorBrush(breeding ? Color.FromRgb(66, 47, 25) : Color.FromRgb(83, 37, 29));
            sprayLabel.Text = spraying ? "杀虫剂：开启（F8）" : "杀虫剂：关闭（F8）";
            sprayButton.Background = new SolidColorBrush(spraying ? Color.FromRgb(99, 125, 39) : Color.FromRgb(50, 65, 26));
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!allowClose) { e.Cancel = true; Hide(); }
        }

        public void ForceClose() { allowClose = true; Close(); }
    }

    internal sealed class HabitatSurface : FrameworkElement
    {
        private readonly OverlayWindow owner;
        private readonly ImageBrush[][] walkFrames = new ImageBrush[3][];
        private readonly ImageBrush[] sprayFrames;
        private ImageBrush eggSprite;

        public HabitatSurface(OverlayWindow owner)
        {
            this.owner = owner;
            IsHitTestVisible = true;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            walkFrames[(int)LifeStage.StageOne] = LoadWalkFrames("roach-stage1.png");
            walkFrames[(int)LifeStage.StageTwo] = LoadWalkFrames("roach-stage2.png");
            walkFrames[(int)LifeStage.Adult] = LoadWalkFrames("roach-adult.png");
            eggSprite = LoadCroppedBrush("roach-egg.png");
            sprayFrames = LoadDirectFrames("pesticide-spray.png", 6);
        }

        private static ImageBrush[] LoadDirectFrames(string resourceName, int frameCount)
        {
            ImageBrush[] frames = new ImageBrush[frameCount];
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null) return frames;
            BitmapImage image = new BitmapImage();
            image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad; image.StreamSource = stream; image.EndInit(); image.Freeze(); stream.Dispose();
            FormatConvertedBitmap pixels = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
            int stride = pixels.PixelWidth * 4;
            byte[] data = new byte[stride * pixels.PixelHeight];
            pixels.CopyPixels(data, stride, 0);
            int minY = pixels.PixelHeight, maxY = 0;
            for (int y = 0; y < pixels.PixelHeight; y++)
            for (int x = 0; x < pixels.PixelWidth; x++)
            {
                if (data[y * stride + x * 4 + 3] < 45) continue;
                if (y < minY) minY = y; if (y > maxY) maxY = y;
            }
            minY = Math.Max(0, minY - 8); maxY = Math.Min(pixels.PixelHeight - 1, maxY + 8);
            double frameWidth = image.PixelWidth / (double)frameCount;
            for (int i = 0; i < frameCount; i++)
            {
                int frameLeft = (int)Math.Round(i * frameWidth);
                int frameRight = (int)Math.Round((i + 1) * frameWidth);
                int cropWidth = Math.Max(1, Math.Min(image.PixelWidth, frameRight) - frameLeft);
                int cropHeight = Math.Max(1, maxY - minY + 1);
                CroppedBitmap croppedFrame = new CroppedBitmap(image, new Int32Rect(frameLeft, minY, cropWidth, cropHeight));
                croppedFrame.Freeze();
                BitmapSource isolatedFrame = IsolateSprayFrame(croppedFrame);
                ImageBrush brush = new ImageBrush(isolatedFrame);
                brush.Stretch = Stretch.Uniform;
                brush.TileMode = TileMode.None;
                frames[i] = brush;
            }
            return frames;
        }

        private static BitmapSource IsolateSprayFrame(BitmapSource frame)
        {
            FormatConvertedBitmap pixels = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);
            int width = pixels.PixelWidth, height = pixels.PixelHeight, stride = width * 4;
            byte[] data = new byte[stride * height];
            pixels.CopyPixels(data, stride, 0);
            int rightCut = (int)(width * .64);
            int upperCut = (int)(height * .48);
            for (int y = 0; y < upperCut; y++)
            for (int x = rightCut; x < width; x++)
                data[y * stride + x * 4 + 3] = 0;
            BitmapSource isolated = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, data, stride);
            isolated.Freeze();
            return isolated;
        }

        private ImageBrush[] LoadWalkFrames(string resourceName)
        {
            ImageBrush[] frames = new ImageBrush[8];
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null) return frames;
            BitmapImage sheet = new BitmapImage();
            sheet.BeginInit();
            sheet.CacheOption = BitmapCacheOption.OnLoad;
            sheet.StreamSource = stream;
            sheet.EndInit();
            sheet.Freeze();
            stream.Dispose();
            FormatConvertedBitmap pixels = new FormatConvertedBitmap(sheet, PixelFormats.Bgra32, null, 0);
            int sourceStride = pixels.PixelWidth * 4;
            byte[] source = new byte[sourceStride * pixels.PixelHeight];
            pixels.CopyPixels(source, sourceStride, 0);
            int contentTop = pixels.PixelHeight;
            int contentBottom = 0;
            for (int y = 0; y < pixels.PixelHeight; y++)
            {
                for (int x = 0; x < pixels.PixelWidth; x++)
                {
                    if (source[y * sourceStride + x * 4 + 3] >= 45)
                    {
                        if (y < contentTop) contentTop = y;
                        if (y > contentBottom) contentBottom = y;
                    }
                }
            }
            contentTop = Math.Max(0, contentTop - 10);
            contentBottom = Math.Min(pixels.PixelHeight - 1, contentBottom + 10);
            double contentHeight = Math.Max(1, contentBottom - contentTop + 1);
            double frameWidth = sheet.PixelWidth / 8.0;
            for (int i = 0; i < 8; i++)
            {
                BitmapSource pixelFrame = MakePixelFrame(source, pixels.PixelWidth, pixels.PixelHeight, sourceStride, i * frameWidth, frameWidth, contentTop, contentHeight);
                ImageBrush brush = new ImageBrush(pixelFrame);
                brush.Stretch = Stretch.Uniform;
                brush.TileMode = TileMode.None;
                frames[i] = brush;
            }
            return frames;
        }

        private static ImageBrush LoadCroppedBrush(string resourceName)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null) return null;
            BitmapImage image = new BitmapImage();
            image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad; image.StreamSource = stream; image.EndInit(); image.Freeze(); stream.Dispose();
            FormatConvertedBitmap pixels = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
            int stride = pixels.PixelWidth * 4;
            byte[] data = new byte[stride * pixels.PixelHeight];
            pixels.CopyPixels(data, stride, 0);
            int minX = pixels.PixelWidth, minY = pixels.PixelHeight, maxX = 0, maxY = 0;
            for (int y = 0; y < pixels.PixelHeight; y++)
            for (int x = 0; x < pixels.PixelWidth; x++)
            {
                if (data[y * stride + x * 4 + 3] < 45) continue;
                if (x < minX) minX = x; if (x > maxX) maxX = x;
                if (y < minY) minY = y; if (y > maxY) maxY = y;
            }
            minX = Math.Max(0, minX - 8); minY = Math.Max(0, minY - 8);
            maxX = Math.Min(pixels.PixelWidth - 1, maxX + 8); maxY = Math.Min(pixels.PixelHeight - 1, maxY + 8);
            ImageBrush brush = new ImageBrush(image);
            brush.ViewboxUnits = BrushMappingMode.Absolute;
            brush.Viewbox = new Rect(minX, minY, Math.Max(1, maxX - minX + 1), Math.Max(1, maxY - minY + 1));
            brush.Stretch = Stretch.Uniform;
            return brush;
        }

        private static BitmapSource MakePixelFrame(byte[] source, int sourceWidth, int sourceHeight, int stride, double left, double width, double top, double height)
        {
            const int targetWidth = 48;
            const int targetHeight = 64;
            byte[] output = new byte[targetWidth * targetHeight * 4];
            Color[] palette = new Color[] {
                Color.FromRgb(19, 10, 6), Color.FromRgb(43, 20, 10), Color.FromRgb(70, 31, 13),
                Color.FromRgb(100, 43, 16), Color.FromRgb(132, 57, 19), Color.FromRgb(166, 75, 24),
                Color.FromRgb(198, 99, 35), Color.FromRgb(226, 132, 52), Color.FromRgb(246, 178, 82)
            };
            for (int y = 0; y < targetHeight; y++)
            {
                int sy = Math.Min(sourceHeight - 1, Math.Max(0, (int)(top + (y + .5) * height / targetHeight)));
                for (int x = 0; x < targetWidth; x++)
                {
                    int sx = Math.Min(sourceWidth - 1, Math.Max(0, (int)(left + (x + .5) * width / targetWidth)));
                    int si = sy * stride + sx * 4;
                    int di = (y * targetWidth + x) * 4;
                    byte a = source[si + 3];
                    if (a < 45) continue;
                    int best = 0, bestDistance = int.MaxValue;
                    for (int p = 0; p < palette.Length; p++)
                    {
                        int db = source[si] - palette[p].B;
                        int dg = source[si + 1] - palette[p].G;
                        int dr = source[si + 2] - palette[p].R;
                        int distance = db * db + dg * dg + dr * dr;
                        if (distance < bestDistance) { bestDistance = distance; best = p; }
                    }
                    output[di] = palette[best].B;
                    output[di + 1] = palette[best].G;
                    output[di + 2] = palette[best].R;
                    output[di + 3] = a > 150 ? (byte)255 : a;
                }
            }
            BitmapSource bitmap = BitmapSource.Create(targetWidth, targetHeight, 96, 96, PixelFormats.Bgra32, null, output, targetWidth * 4);
            bitmap.Freeze();
            return bitmap;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            foreach (Egg egg in owner.Eggs) DrawEgg(dc, egg);
            foreach (Roach roach in owner.Roaches) DrawRoach(dc, roach);
            foreach (SprayCloud cloud in owner.Clouds) DrawCloud(dc, cloud);

            if (owner.Spraying)
            {
                Point p = owner.PointerPosition;
                Pen ring = new Pen(new SolidColorBrush(Color.FromArgb(165, 201, 238, 103)), 2);
                ring.DashStyle = DashStyles.Dash;
                dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(18, 201, 238, 103)), ring, p, 88, 88);
                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(180, 245, 255, 220)), 1), new Point(p.X - 12, p.Y), new Point(p.X + 12, p.Y));
                dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(180, 245, 255, 220)), 1), new Point(p.X, p.Y - 12), new Point(p.X, p.Y + 12));
                Point toolPoint = p;
                double toolRotation = 0;
                double toolScale = 1;
                if (owner.SprayAnimationAge < .72)
                {
                    toolPoint = owner.SprayAnimationPoint;
                    double progress = owner.SprayAnimationAge / .72;
                    if (progress < .22)
                    {
                        toolRotation = -4 * (progress / .22);
                        toolScale = 1 - .045 * (progress / .22);
                    }
                    else
                    {
                        double rebound = (progress - .22) / .78;
                        toolRotation = -4 * (1 - rebound) + Math.Sin(rebound * Math.PI * 2) * 1.6 * (1 - rebound);
                        toolScale = .955 + .045 * rebound;
                    }
                }
                if (sprayFrames[0] != null)
                {
                    dc.PushTransform(new TranslateTransform(toolPoint.X + 24, toolPoint.Y + 25));
                    dc.PushTransform(new RotateTransform(toolRotation));
                    dc.PushTransform(new ScaleTransform(toolScale, toolScale));
                    dc.DrawRectangle(sprayFrames[0], null, new Rect(-61, -64, 122, 122));
                    dc.Pop(); dc.Pop(); dc.Pop();
                }
            }
        }

        private void DrawEgg(DrawingContext dc, Egg egg)
        {
            dc.PushTransform(new TranslateTransform(egg.X, egg.Y));
            dc.PushTransform(new RotateTransform(egg.Rotation));
            double pulse = egg.Age > 20 ? 1 + Math.Sin(egg.Age * 7) * .06 : 1;
            dc.PushTransform(new ScaleTransform(pulse, pulse));
            if (eggSprite != null) dc.DrawRectangle(eggSprite, null, new Rect(-23, -15, 46, 30));
            dc.Pop();
            dc.Pop(); dc.Pop();
        }

        private void DrawRoach(DrawingContext dc, Roach r)
        {
            double scale = r.Stage == LifeStage.StageOne ? .48 : (r.Stage == LifeStage.StageTwo ? .72 : 1.0);
            double opacity = r.Dead ? Math.Max(0, .85 - r.DeadAge * .65) : 1;
            if (opacity <= 0) return;

            dc.PushTransform(new TranslateTransform(r.X, r.Y));
            dc.PushTransform(new RotateTransform(r.Angle * 180 / Math.PI + 90 + (r.Dead ? 90 : 0)));
            dc.PushTransform(new ScaleTransform(scale, scale));
            dc.PushOpacity(opacity);
            int frame = ((int)Math.Floor(r.WalkPhase)) % 8;
            ImageBrush sprite = walkFrames[(int)r.Stage][frame];
            if (sprite != null) dc.DrawRectangle(sprite, null, new Rect(-48, -48, 96, 96));
            dc.Pop();
            dc.Pop(); dc.Pop(); dc.Pop();
        }

        private static void DrawCloud(DrawingContext dc, SprayCloud cloud)
        {
            double t = Math.Min(1, cloud.Age / 0.55);
            double radius = 25 + t * 92;
            byte alpha = (byte)(Math.Max(0, 1 - t) * 115);
            RadialGradientBrush mist = new RadialGradientBrush(Color.FromArgb(alpha, 205, 246, 114), Color.FromArgb(0, 160, 220, 70));
            dc.DrawEllipse(mist, null, new Point(cloud.X, cloud.Y), radius, radius);
            Brush pixelMist = new SolidColorBrush(Color.FromArgb((byte)(Math.Max(0, 1 - t) * 205), 215, 248, 127));
            for (int i = 0; i < 18; i++)
            {
                double angle = i * 2.399963 + .35;
                double distance = t * (24 + (i % 6) * 13);
                double jitter = ((i * 17) % 9) - 4;
                double x = cloud.X + Math.Cos(angle) * distance + jitter;
                double y = cloud.Y + Math.Sin(angle) * distance - jitter * .5;
                double size = 3 + (i % 3) * 2;
                dc.DrawRectangle(pixelMist, null, new Rect(Math.Round(x), Math.Round(y), size, size));
            }
        }
    }

    internal sealed class OverlayWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 701;
        private const int PANEL_HOTKEY_ID = 702;
        private const uint MOD_NOREPEAT = 0x4000;
        private const uint VK_F8 = 0x77;
        private const uint VK_F9 = 0x78;

        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")] private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")] private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr value);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")] private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")] private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr value);

        private readonly Random rng = new Random();
        private readonly DispatcherTimer timer;
        private readonly HabitatSurface surface;
        private readonly Forms.NotifyIcon tray;
        private readonly Forms.ToolStripMenuItem sprayItem;
        private readonly Forms.ToolStripMenuItem pauseItem;
        private ControlPanelWindow controlPanel;
        private DateTime lastTick = DateTime.UtcNow;
        private IntPtr handle;
        private bool paused;
        private bool breedingEnabled = true;
        private int populationLimit = 20;

        public readonly List<Roach> Roaches = new List<Roach>();
        public readonly List<Egg> Eggs = new List<Egg>();
        public readonly List<SprayCloud> Clouds = new List<SprayCloud>();
        public bool Spraying { get; private set; }
        public double SprayAnimationAge { get; private set; }
        public Point SprayAnimationPoint { get; private set; }
        public Point PointerPosition { get; private set; }

        public OverlayWindow()
        {
            SprayAnimationAge = 10;
            Title = "蟑螂乐园";
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = false;
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            surface = new HabitatSurface(this);
            Content = surface;
            SourceInitialized += OnSourceInitialized;
            Closed += OnClosed;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseRightButtonDown += delegate { if (Spraying) SetSpraying(false); };

            tray = new Forms.NotifyIcon();
            tray.Icon = Drawing.SystemIcons.Application;
            tray.Text = "蟑螂乐园";
            tray.Visible = true;
            Forms.ContextMenuStrip menu = new Forms.ContextMenuStrip();
            menu.Items.Add(new Forms.ToolStripMenuItem("显示控制面板（F9）", null, delegate { ToggleControlPanel(); }));
            sprayItem = new Forms.ToolStripMenuItem("杀虫剂模式（F8）", null, delegate { SetSpraying(!Spraying); });
            pauseItem = new Forms.ToolStripMenuItem("暂停所有活动", null, delegate { TogglePause(); });
            menu.Items.Add(sprayItem);
            menu.Items.Add(new Forms.ToolStripMenuItem("投放一只成虫", null, delegate { AddRoach(LifeStage.Adult, null, null); }));
            menu.Items.Add(pauseItem);
            menu.Items.Add(new Forms.ToolStripMenuItem("重启乐园", null, delegate { ResetHabitat(); }));
            menu.Items.Add(new Forms.ToolStripSeparator());
            menu.Items.Add(new Forms.ToolStripMenuItem("退出蟑螂乐园", null, delegate { Close(); }));
            tray.ContextMenuStrip = menu;
            tray.DoubleClick += delegate { SetSpraying(!Spraying); };

            timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromMilliseconds(33);
            timer.Tick += Tick;
            timer.Start();

            Loaded += delegate
            {
                ResetHabitat();
                controlPanel = new ControlPanelWindow(this);
                controlPanel.Owner = this;
                controlPanel.Show();
                UpdateTrayText();
                tray.BalloonTipTitle = "蟑螂乐园正在运行";
                tray.BalloonTipText = "控制面板可直接调整数量；按 F8 开启杀虫剂，F9 显示或隐藏面板。";
                tray.ShowBalloonTip(3500);
            };
        }

        private static IntPtr GetStyle(IntPtr hwnd) { return IntPtr.Size == 8 ? GetWindowLongPtr64(hwnd, GWL_EXSTYLE) : GetWindowLong32(hwnd, GWL_EXSTYLE); }
        private static void SetStyle(IntPtr hwnd, IntPtr style) { if (IntPtr.Size == 8) SetWindowLongPtr64(hwnd, GWL_EXSTYLE, style); else SetWindowLong32(hwnd, GWL_EXSTYLE, style); }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle).AddHook(WndProc);
            RegisterHotKey(handle, HOTKEY_ID, MOD_NOREPEAT, VK_F8);
            RegisterHotKey(handle, PANEL_HOTKEY_ID, MOD_NOREPEAT, VK_F9);
            SetClickThrough(true);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                SetSpraying(!Spraying);
                handled = true;
            }
            else if (msg == WM_HOTKEY && wParam.ToInt32() == PANEL_HOTKEY_ID)
            {
                ToggleControlPanel();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void SetClickThrough(bool enabled)
        {
            if (handle == IntPtr.Zero) return;
            long style = GetStyle(handle).ToInt64() | WS_EX_TOOLWINDOW;
            if (enabled) style |= WS_EX_TRANSPARENT; else style &= ~WS_EX_TRANSPARENT;
            SetStyle(handle, new IntPtr(style));
        }

        private void SetSpraying(bool enabled)
        {
            UpdatePointerPosition();
            Spraying = enabled;
            sprayItem.Checked = enabled;
            SetClickThrough(!enabled);
            Topmost = enabled;
            Cursor = enabled ? Cursors.None : Cursors.Arrow;
            if (!enabled) SprayAnimationAge = 10;
            if (enabled) { Activate(); Focus(); }
            surface.InvalidateVisual();
            UpdateTrayText();
        }

        private void UpdatePointerPosition()
        {
            try
            {
                System.Drawing.Point screenPointer = Forms.Cursor.Position;
                PointerPosition = PointFromScreen(new Point(screenPointer.X, screenPointer.Y));
            }
            catch
            {
                PointerPosition = new Point(ActualWidth / 2, ActualHeight / 2);
            }
        }

        public void ToggleSprayFromPanel() { SetSpraying(!Spraying); }

        private void ToggleControlPanel()
        {
            if (controlPanel == null) return;
            if (controlPanel.IsVisible) controlPanel.Hide();
            else { controlPanel.Show(); controlPanel.Activate(); }
        }

        public void ChangeCount(int delta)
        {
            if (delta <= -999)
            {
                Roaches.Clear();
                Eggs.Clear();
            }
            else if (delta > 0)
            {
                for (int i = 0; i < delta; i++) AddRoach(LifeStage.Adult, null, null);
            }
            else
            {
                int remaining = -delta;
                for (int i = Roaches.Count - 1; i >= 0 && remaining > 0; i--)
                {
                    if (!Roaches[i].Dead) { Roaches.RemoveAt(i); remaining--; }
                }
            }
            surface.InvalidateVisual();
            UpdateTrayText();
        }

        public void ChangePopulationLimit(int delta)
        {
            populationLimit = Math.Max(5, Math.Min(60, populationLimit + delta));
            UpdateTrayText();
        }

        public void ToggleBreeding()
        {
            breedingEnabled = !breedingEnabled;
            UpdateTrayText();
        }

        private void TogglePause()
        {
            paused = !paused;
            pauseItem.Text = paused ? "继续所有活动" : "暂停所有活动";
        }

        private void ResetHabitat()
        {
            Roaches.Clear(); Eggs.Clear(); Clouds.Clear();
            AddRoach(LifeStage.StageOne, null, null);
            AddRoach(LifeStage.StageTwo, null, null);
            AddRoach(LifeStage.Adult, null, null);
            Egg starterEgg = new Egg();
            starterEgg.X = Math.Max(50, ActualWidth * .42);
            starterEgg.Y = Math.Max(50, ActualHeight * .58);
            starterEgg.Rotation = rng.Next(-30, 31);
            Eggs.Add(starterEgg);
            surface.InvalidateVisual();
            UpdateTrayText();
        }

        private Roach AddRoach(LifeStage stage, double? x, double? y)
        {
            if (AliveCount() >= populationLimit) return null;
            double margin = stage == LifeStage.StageOne ? 24 : (stage == LifeStage.StageTwo ? 31 : 42);
            Roach r = new Roach();
            r.X = x.HasValue ? x.Value : margin + rng.NextDouble() * Math.Max(1, ActualWidth - margin * 2);
            r.Y = y.HasValue ? y.Value : margin + rng.NextDouble() * Math.Max(1, ActualHeight - margin * 2);
            r.Angle = rng.NextDouble() * Math.PI * 2;
            r.Stage = stage;
            r.Age = 0;
            r.EggDue = stage == LifeStage.Adult ? 45 + rng.NextDouble() * 30 : 0;
            r.AdultLifeRemaining = stage == LifeStage.Adult ? 480 + rng.NextDouble() * 420 : 0;
            if (rng.NextDouble() < .58) BeginRest(r); else BeginRun(r, false);
            Roaches.Add(r);
            UpdateTrayText();
            return r;
        }

        private int AliveCount()
        {
            int n = 0;
            foreach (Roach r in Roaches) if (!r.Dead) n++;
            return n;
        }

        private int StageCount(LifeStage stage)
        {
            int n = 0;
            foreach (Roach r in Roaches) if (!r.Dead && r.Stage == stage) n++;
            return n;
        }

        private int MotionCount(MotionState motion)
        {
            int n = 0;
            foreach (Roach r in Roaches) if (!r.Dead && r.Motion == motion) n++;
            return n;
        }

        private void BeginRest(Roach r)
        {
            r.Motion = MotionState.Resting;
            r.Speed = 0;
            double stageFactor = r.Stage == LifeStage.StageOne ? 1.35 : (r.Stage == LifeStage.StageTwo ? 1.12 : 1);
            double rhythmFactor = IsNightTime() ? .72 : 1.28;
            r.StateTime = stageFactor * rhythmFactor * (.7 + rng.NextDouble() * 3.8);
            bool nearWall = r.X < 105 || r.Y < 105 || r.X > ActualWidth - 105 || r.Y > ActualHeight - 105;
            if (nearWall) r.StateTime *= 1.45;
            bool nearCorner = (r.X < 130 || r.X > ActualWidth - 130) && (r.Y < 130 || r.Y > ActualHeight - 130);
            if (nearCorner) r.StateTime *= 1.55;
            if (rng.NextDouble() < .12) r.StateTime += 4 + rng.NextDouble() * 7;
        }

        private void BeginRun(Roach r, bool escaping)
        {
            r.Motion = MotionState.Running;
            double activityFactor = IsNightTime() ? 1.22 : .86;
            r.StateTime = escaping ? .75 + rng.NextDouble() * .8 : activityFactor * (.55 + rng.NextDouble() * 1.75);
            double sizeFactor = r.Stage == LifeStage.StageOne ? .72 : (r.Stage == LifeStage.StageTwo ? .86 : 1);
            double nightSpeed = IsNightTime() ? 1.08 : 1;
            r.Speed = sizeFactor * nightSpeed * (escaping ? 285 + rng.NextDouble() * 95 : 145 + rng.NextDouble() * 105);
        }

        private static bool IsNightTime()
        {
            int hour = DateTime.Now.Hour;
            return hour >= 20 || hour < 6;
        }

        private void BeginEcologyTurn(Roach r)
        {
            Roach nearest = null;
            double nearestDistance = double.MaxValue;
            foreach (Roach other in Roaches)
            {
                if (other == r || other.Dead || other.Motion != MotionState.Resting) continue;
                double distance = Math.Sqrt((other.X - r.X) * (other.X - r.X) + (other.Y - r.Y) * (other.Y - r.Y));
                if (distance > 70 && distance < nearestDistance && distance < 430) { nearest = other; nearestDistance = distance; }
            }
            double roll = rng.NextDouble();
            if (nearest != null && roll < .42)
            {
                BeginTurn(r, Math.Atan2(nearest.Y - r.Y, nearest.X - r.X) + (rng.NextDouble() - .5) * .22);
            }
            else if (roll < .72)
            {
                double targetX = r.X < ActualWidth / 2 ? 45 : ActualWidth - 45;
                double targetY = r.Y < ActualHeight / 2 ? 45 : ActualHeight - 45;
                BeginTurn(r, Math.Atan2(targetY - r.Y, targetX - r.X) + (rng.NextDouble() - .5) * .3);
            }
            else BeginTurn(r, null);
        }

        private void BeginTurn(Roach r, double? desiredAngle)
        {
            r.Motion = MotionState.Turning;
            r.Speed = 0;
            double duration = .16 + rng.NextDouble() * .34;
            double delta;
            if (desiredAngle.HasValue)
            {
                delta = NormalizeAngle(desiredAngle.Value - r.Angle);
            }
            else
            {
                double direction = rng.NextDouble() < .5 ? -1 : 1;
                delta = direction * (.38 + rng.NextDouble() * 1.65);
            }
            r.StateTime = duration;
            r.TurnRate = delta / duration;
        }

        private static double NormalizeAngle(double angle)
        {
            while (angle > Math.PI) angle -= Math.PI * 2;
            while (angle < -Math.PI) angle += Math.PI * 2;
            return angle;
        }

        private void TurnAlongWall(Roach r, bool verticalWall, bool lowSide)
        {
            double target;
            if (verticalWall)
            {
                target = rng.NextDouble() < .5 ? Math.PI / 2 : -Math.PI / 2;
                target += lowSide ? (target > 0 ? -.13 : .13) : (target > 0 ? .13 : -.13);
            }
            else
            {
                target = rng.NextDouble() < .5 ? 0 : Math.PI;
                if (lowSide) target += target == 0 ? .13 : -.13;
                else target += target == 0 ? -.13 : .13;
            }
            BeginTurn(r, target);
        }

        private void Tick(object sender, EventArgs args)
        {
            DateTime now = DateTime.UtcNow;
            double dt = Math.Min(0.08, (now - lastTick).TotalSeconds);
            lastTick = now;
            UpdatePointerPosition();
            if (!paused) UpdateWorld(dt);
            for (int i = Clouds.Count - 1; i >= 0; i--) { Clouds[i].Age += dt; if (Clouds[i].Age > 0.55) Clouds.RemoveAt(i); }
            if (SprayAnimationAge < 10) SprayAnimationAge += dt;
            surface.InvalidateVisual();
        }

        private void UpdateWorld(double dt)
        {
            double w = Math.Max(100, ActualWidth), h = Math.Max(100, ActualHeight);
            Point pointer = PointerPosition;
            for (int i = Roaches.Count - 1; i >= 0; i--)
            {
                Roach r = Roaches[i];
                if (r.Dead)
                {
                    r.DeadAge += dt;
                    if (r.DeadAge > 1.45) Roaches.RemoveAt(i);
                    continue;
                }
                r.Age += dt;
                r.ThreatCooldown = Math.Max(0, r.ThreatCooldown - dt);
                if (r.Stage == LifeStage.StageOne && r.Age >= 20)
                {
                    r.Stage = LifeStage.StageTwo;
                    r.Age = 0;
                }
                else if (r.Stage == LifeStage.StageTwo && r.Age >= 30)
                {
                    r.Stage = LifeStage.Adult;
                    r.Age = 0;
                    r.EggDue = 45 + rng.NextDouble() * 30;
                    r.AdultLifeRemaining = 480 + rng.NextDouble() * 420;
                }

                if (r.Stage == LifeStage.Adult)
                {
                    r.AdultLifeRemaining -= dt;
                    if (r.AdultLifeRemaining <= 0)
                    {
                        r.Dead = true;
                        r.DeadAge = 0;
                        continue;
                    }
                }

                double pointerDistance = Math.Sqrt((r.X - pointer.X) * (r.X - pointer.X) + (r.Y - pointer.Y) * (r.Y - pointer.Y));
                if (!Spraying && pointerDistance < 92 && r.ThreatCooldown <= 0)
                {
                    r.Angle = Math.Atan2(r.Y - pointer.Y, r.X - pointer.X) + (rng.NextDouble() - .5) * .24;
                    r.ThreatCooldown = 1.7;
                    BeginRun(r, true);
                }
                r.StateTime -= dt;
                if (r.Motion == MotionState.Resting)
                {
                    if (r.StateTime <= 0)
                    {
                        double activityChance = IsNightTime() ? .78 : .52;
                        if (rng.NextDouble() < activityChance) BeginEcologyTurn(r); else BeginRest(r);
                    }
                }
                else if (r.Motion == MotionState.Turning)
                {
                    r.Angle = NormalizeAngle(r.Angle + r.TurnRate * dt);
                    r.WalkPhase += dt * 5;
                    if (r.StateTime <= 0) BeginRun(r, false);
                }
                else
                {
                    r.X += Math.Cos(r.Angle) * r.Speed * dt;
                    r.Y += Math.Sin(r.Angle) * r.Speed * dt;
                    r.WalkPhase += dt * (7 + r.Speed / 18);
                    if (r.StateTime <= 0)
                    {
                        if (rng.NextDouble() < .62) BeginRest(r); else BeginTurn(r, null);
                    }
                }
                double pad = r.Stage == LifeStage.StageOne ? 18 : (r.Stage == LifeStage.StageTwo ? 27 : 38);
                if (r.X < pad) { r.X = pad; TurnAlongWall(r, true, true); }
                else if (r.X > w - pad) { r.X = w - pad; TurnAlongWall(r, true, false); }
                else if (r.Y < pad) { r.Y = pad; TurnAlongWall(r, false, true); }
                else if (r.Y > h - pad) { r.Y = h - pad; TurnAlongWall(r, false, false); }
                if (r.Stage == LifeStage.Adult && breedingEnabled) r.EggDue -= dt;
                bool nearEggEdge = r.X < 115 || r.Y < 115 || r.X > w - 115 || r.Y > h - 115;
                if (r.Stage == LifeStage.Adult && breedingEnabled && r.EggDue <= 0 && !nearEggEdge && r.Motion == MotionState.Resting)
                {
                    double leftDistance = r.X, rightDistance = w - r.X, topDistance = r.Y, bottomDistance = h - r.Y;
                    double nearest = Math.Min(Math.Min(leftDistance, rightDistance), Math.Min(topDistance, bottomDistance));
                    if (nearest == leftDistance) r.Angle = Math.PI;
                    else if (nearest == rightDistance) r.Angle = 0;
                    else if (nearest == topDistance) r.Angle = -Math.PI / 2;
                    else r.Angle = Math.PI / 2;
                    BeginRun(r, false);
                    r.StateTime = 1.7 + rng.NextDouble() * 1.4;
                }
                if (r.Stage == LifeStage.Adult && breedingEnabled && r.EggDue <= 0 && nearEggEdge && Eggs.Count < 6 && AliveCount() < populationLimit)
                {
                    Egg egg = new Egg(); egg.X = r.X; egg.Y = r.Y; egg.Rotation = rng.Next(-30, 31);
                    Eggs.Add(egg); r.EggDue = 45 + rng.NextDouble() * 30;
                }
            }
            for (int i = Eggs.Count - 1; i >= 0; i--)
            {
                Egg egg = Eggs[i]; egg.Age += dt;
                if (egg.Age >= 25)
                {
                    Eggs.RemoveAt(i);
                    int babies = rng.Next(1, 3);
                    for (int n = 0; n < babies; n++) AddRoach(LifeStage.StageOne, egg.X + rng.Next(-12, 13), egg.Y + rng.Next(-12, 13));
                }
            }
            UpdateTrayText();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Spraying) return;
            Point p = e.GetPosition(this);
            SprayAnimationPoint = p;
            SprayAnimationAge = 0;
            SprayCloud cloud = new SprayCloud(); cloud.X = p.X; cloud.Y = p.Y; Clouds.Add(cloud);
            int hit = 0;
            foreach (Roach r in Roaches)
            {
                if (r.Dead) continue;
                double distance = Math.Sqrt((r.X - p.X) * (r.X - p.X) + (r.Y - p.Y) * (r.Y - p.Y));
                if (distance <= 88 && rng.NextDouble() < 0.84)
                { r.Dead = true; r.DeadAge = 0; hit++; }
                else if (distance <= 190)
                {
                    r.Angle = Math.Atan2(r.Y - p.Y, r.X - p.X) + (rng.NextDouble() - .5) * .35;
                    BeginRun(r, true);
                }
            }
            if (hit > 0)
            {
                tray.BalloonTipTitle = "命中";
                tray.BalloonTipText = "消灭了 " + hit + " 只蟑螂。";
                tray.ShowBalloonTip(900);
            }
            UpdateTrayText();
        }

        private void UpdateTrayText()
        {
            string text = "蟑螂乐园 · 活体 " + AliveCount() + " · 卵鞘 " + Eggs.Count;
            tray.Text = text.Length > 63 ? text.Substring(0, 63) : text;
            if (controlPanel != null) controlPanel.UpdateCounts(
                StageCount(LifeStage.StageOne), StageCount(LifeStage.StageTwo), StageCount(LifeStage.Adult), Eggs.Count,
                MotionCount(MotionState.Resting), MotionCount(MotionState.Running), MotionCount(MotionState.Turning),
                populationLimit, IsNightTime(), breedingEnabled, Spraying);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            timer.Stop();
            if (handle != IntPtr.Zero) UnregisterHotKey(handle, HOTKEY_ID);
            if (handle != IntPtr.Zero) UnregisterHotKey(handle, PANEL_HOTKEY_ID);
            if (controlPanel != null) controlPanel.ForceClose();
            tray.Visible = false;
            tray.Dispose();
            Application.Current.Shutdown();
        }
    }

    internal static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            app.Run(new OverlayWindow());
        }
    }
}
