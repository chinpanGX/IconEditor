using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace IconEditor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // ラインの数
        private const int _line = 42;
        // ボックスの大きさ
        private double _canvasSize = 20;
        // ウィンドウの大きさ
        private double _windowSize = 840;

        Rectangle[,] Rectangles = new Rectangle[42, 42];
        Stack<Color[,]> UndoStack = new Stack<Color[,]>();
        Stack<Color[,]> RedoStack = new Stack<Color[,]>();

        public static RoutedCommand UndoCommand { get; } = new RoutedCommand(nameof(UndoCommand), typeof(MainWindow));
        public static RoutedCommand RedoCommand { get; } = new RoutedCommand(nameof(RedoCommand), typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  アイコンエディタ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainCanvas_Initialized(object sender, EventArgs e)
        {
            Canvas canvas = (Canvas)sender;

            // 縦横の枠
            for(uint y = 0; y < _line; y++)
            {
                for(uint x = 0; x < _line; x++)
                {
                    // 白の1マス
                    Rectangle rect;
                    rect = new Rectangle();
                    rect.Fill = new SolidColorBrush(Colors.White);
                    rect.Width = _canvasSize -1;
                    rect.Height = _canvasSize -1;

                    // マウスのクリックした位置を塗りつぶす
                    rect.MouseDown += Rectangle_MouseDown;
                    // マウスでドラッグした位置を塗りつぶす
                    rect.MouseMove += Rectangle_MouseMove;

                    // １マスのキャンパスの設定
                    Canvas.SetLeft(rect, x * _canvasSize);
                    Canvas.SetTop(rect, y * _canvasSize);
                    canvas.Children.Add(rect);

                    Rectangles[y, x] = rect;
                }
            }
        }

        // マウスのクリックをした位置を塗りつぶす
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {            
            Color[,] color = new Color[_line, _line];
            for (int y = 0; y < _line; y++)
            {
                for (int x = 0; x < _line; x++)
                {
                    SolidColorBrush brush = (SolidColorBrush)Rectangles[y, x].Fill;
                    color[y, x] = Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
                }
            }
            RedoStack.Clear();
            UndoStack.Push(color);            

            Rectangle rect = (Rectangle)sender;

            SolidColorBrush paletteBrush = (SolidColorBrush)ColorPalette.Fill;
            rect.Fill = new SolidColorBrush(paletteBrush.Color);
        }

        // ドラッグしている箇所を塗りつぶす
        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle rect = (Rectangle)sender;

            // Canvasの座標を求める
            // マウスの座標を求める
            String statusText;
            double x = Canvas.GetLeft(rect) / _canvasSize;
            double y = Canvas.GetTop(rect) / _canvasSize;

            //　マウスの座標をステータスバーに表示
            statusText = "X" + x.ToString() + " " + "Y" + y.ToString();

            // 塗った色を表示する
            String colorText;
            SolidColorBrush fillBrush = (SolidColorBrush)rect.Fill;
            Color color = fillBrush.Color;
            colorText = "RGB" + " " + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString();

            // 文字列を連結させる
            StatusBarLabel.Content = statusText + " " + colorText;

            // 左ボタンを押し続ける
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // 塗りつぶし
                SolidColorBrush paletteBrush = (SolidColorBrush)ColorPalette.Fill;
                rect.Fill = new SolidColorBrush(paletteBrush.Color);
            }
        }

        // 終了コマンド
        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            // アプリケーションを終了する
            Application.Current.Shutdown();
        }

        // バージョン情報の表示
        private void MenuItem_Ver_Click(object sender, RoutedEventArgs e)
        {
            // メッセージボックスで表示する
            MessageBox.Show("Deai`s Special IconEditor\n Version 0.0.1\n\n", "Deai`s Special IconEditorのバージョン情報", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Yes);
        }

        private void MenuItem_File_Click(object sender, RoutedEventArgs e)
        {

        }

        // ズームイン
        private void MenuItem_ZoonIn_Click(object sender, RoutedEventArgs e)
        {
            int index = Slider_Zoom.Ticks.IndexOf(Slider_Zoom.Value);
            index++;

            // 最大値を超えないようにする
            if (index >= Slider_Zoom.Ticks.Count) return;
            
            Slider_Zoom.Value = Slider_Zoom.Ticks[index];
        }

        // ズームアウト
        private void MemuItem_ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            int index = Slider_Zoom.Ticks.IndexOf(Slider_Zoom.Value);
            index--;

            // 最小値を超えないようにする
            if (index < 0) return;

            Slider_Zoom.Value = Slider_Zoom.Ticks[index];
        }

        // ズームする値の
        private void Slider_Zoom_ValueChaged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MainCanvas == null) { return; }

            // 拡大縮小
            Matrix matrix = new Matrix();
            matrix.Scale(Slider_Zoom.Value * 0.01, Slider_Zoom.Value * 0.01);
            matrixTransform.Matrix = matrix;

            // スライダーのズームサイズを表示
            ZoomLabel.Content = Slider_Zoom.Value + "%";

            // Canvasのサイズを変更
            MainCanvas.Width = _windowSize * Slider_Zoom.Value * 0.01;
            MainCanvas.Height = _windowSize * Slider_Zoom.Value * 0.01;
        }

        // 色を選択する
        private void ColorPalette_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 「色の設定」のダイアログボックスを表示する
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            colorDialog.FullOpen = true;

            // 塗りつぶしをする色を設定
            SolidColorBrush paletteColorBrush = (SolidColorBrush)ColorPalette.Fill;
            colorDialog.Color = System.Drawing.Color.FromArgb(paletteColorBrush.Color.A, paletteColorBrush.Color.R, paletteColorBrush.Color.G, paletteColorBrush.Color.B);

            if(colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // 塗りつぶしを行う
                Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                ColorPalette.Fill = new SolidColorBrush(color);
            }
        }


        // 元に戻す（メニューバー） 
        private void MenuItem_Undo_Click(object sender, RoutedEventArgs e)
        {
            if (UndoStack.Count == 0) return;

            Color[,] color = new Color[_line, _line];
            for (int y = 0; y < _line; y++)
            {
                for (int x = 0; x < _line; x++)
                {
                    SolidColorBrush brush = (SolidColorBrush)Rectangles[y, x].Fill;
                    color[y, x] = Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
                }
            }
            RedoStack.Push(color);

            color = UndoStack.Pop();

            for (int y = 0; y < _line; y++)
            {
                for (int x = 0; x < _line; x++)
                {
                    Rectangles[y, x].Fill = new SolidColorBrush(color[y, x]);
                }
            }
        }

        // 元に戻す（ツールバー）
        private void ToolBar_Undo_Click(object sender, RoutedEventArgs e)
        {
            if (UndoStack.Count == 0) return;

            Color[,] color = new Color[_line, _line];
            for (int y = 0; y < _line; y++)
            {
                for (int x = 0; x < _line; x++)
                {
                    SolidColorBrush brush = (SolidColorBrush)Rectangles[y, x].Fill;
                    color[y, x] = Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
                }
            }
            RedoStack.Push(color);

            color = UndoStack.Pop();
         
            for (int y = 0; y < _line; y++)
            {
                for (int x = 0; x < _line; x++)
                {
                    Rectangles[y, x].Fill = new SolidColorBrush(color[y, x]);
                }
            }            
        }

        // やり直し（メニューバー）
        private void MenuItem_Redo_Click(object sender, RoutedEventArgs e)
        {
            if (RedoStack.Count == 0) return;

            Color[,] color = new Color[_line, _line];
            for (int y = 0; y < _line; y++)
            {
                for (int x = 0; x < _line; x++)
                {
                    SolidColorBrush brush = (SolidColorBrush)Rectangles[y, x].Fill;
                    color[y, x] = Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
                }
            }
            UndoStack.Push(color);

            color = RedoStack.Pop();
            for (int y = 0; y < _line; y++)
            {
                for (int x = 0; x < _line; x++)
                {
                    Rectangles[y, x].Fill = new SolidColorBrush(color[y, x]);
                }
            }
        }

        // やり直し（ツールバー）
        private void ToolBar_Redo_Click(object sender, RoutedEventArgs e)
        {
            if (RedoStack.Count == 0) return;

            Color[,] color = new Color[_line, _line];
            for (int y = 0; y < _line; y++)
            {
                for (int x = 0; x < _line; x++)
                {
                    SolidColorBrush brush = (SolidColorBrush)Rectangles[y, x].Fill;
                    color[y, x] = Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
                }
            }
            UndoStack.Push(color);

            color = RedoStack.Pop();
            for (int y = 0; y < _line; y++)
            {
                for (int x = 0; x < _line; x++)
                {
                    Rectangles[y, x].Fill = new SolidColorBrush(color[y, x]);
                }
            }
        }

        // 名前を付けて保存
        private void MenuItem_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログを開く
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "PNG(*.png) | *.png";
            bool? result = dlg.ShowDialog();

            // 保存をキャンセル
            if (result != true) return;


            // ピクセルの色を保存
            WriteableBitmap bitmap = new WriteableBitmap(_line, _line, 300, 300, PixelFormats.Bgra32, null);
            byte[] pixels = new byte[_line * _line * 4];

            for(int y = 0; y < _line; y++)
            {
                for (int x = 0; x < _line; x++)
                {
                    SolidColorBrush brush = (SolidColorBrush)Rectangles[y, x].Fill;

                    pixels[(y * _line + x) * 4 + 0] = brush.Color.B;
                    pixels[(y * _line + x) * 4 + 1] = brush.Color.G;
                    pixels[(y * _line + x) * 4 + 2] = brush.Color.R;
                    pixels[(y * _line + x) * 4 + 3] = brush.Color.A;
                }
            }

            bitmap.WritePixels(new Int32Rect(0, 0, _line, _line), pixels, _line * 4, 0);

            // 出力
            using (FileStream stream = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
            }
        }

        // ファイルを開く
        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログを開く
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "PNG(*.png) | *.png";
            bool? result = dlg.ShowDialog();

            // 保存をキャンセル
            if (result != true) return;

            byte[] pixels = new byte[_line * _line * 4];

            //
            using (FileStream stream = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
            {
                PngBitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                BitmapSource bitmap = decoder.Frames[0];
                bitmap.CopyPixels(pixels, _line * 4, 0);
            }

            for(int y = 0; y < _line; y++)
            {
                for(int x = 0;x < _line; x++)
                {
                    Color color = new Color();
                    color.B = pixels[(y * _line + x) * 4 + 0];
                    color.G = pixels[(y * _line + x) * 4 + 1];
                    color.R = pixels[(y * _line + x) * 4 + 2];
                    color.A = pixels[(y * _line + x) * 4 + 3];

                    Rectangles[y, x].Fill = new SolidColorBrush(color);
                }
            }
        }
        private void MenuItem_Copy_Click(object sender, RoutedEventArgs e)
        {
            byte[] pixels = new byte[_line * _line * 4];

            for (int y = 0; y < _line; y++)
            {
                for (int x = 0; x < _line; x++)
                {
                    SolidColorBrush brush = (SolidColorBrush)Rectangles[y, x].Fill;

                    pixels[(y * _line + x) * 4 + 0] = brush.Color.B;
                    pixels[(y * _line + x) * 4 + 1] = brush.Color.G;
                    pixels[(y * _line + x) * 4 + 2] = brush.Color.R;
                    pixels[(y * _line + x) * 4 + 3] = brush.Color.A;
                }
            }

            WriteableBitmap bitmap = new WriteableBitmap(_line, _line, 300, 300, PixelFormats.Bgra32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, _line, _line), pixels, _line * 4, 0, 0);

            Clipboard.SetImage(bitmap);
        }

        private void MenuItem_Paste_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bitmap = Clipboard.GetImage();

            if (bitmap == null) return;

            byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
            bitmap.CopyPixels(pixels, _line * 4, 0);

            int w = Math.Min(_line, bitmap.PixelWidth);
            int h = Math.Min(_line, bitmap.PixelHeight);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color color = new Color();
                    color.B = pixels[(y * bitmap.PixelWidth + x) * 4 + 0];
                    color.G = pixels[(y * bitmap.PixelWidth + x) * 4 + 1];
                    color.R = pixels[(y * bitmap.PixelWidth + x) * 4 + 2];
                    color.A = 255;

                    Rectangles[y, x].Fill = new SolidColorBrush(color);
                }
            }
        }

 
    }
}
