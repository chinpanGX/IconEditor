﻿using System;
using System.Collections.Generic;
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

namespace IconEditor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // ラインの数
        private uint _line = 42;
        // ボックスの大きさ
        private double _canvasSize = 20;

        private double _windowSize = 840;
        
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
                }
            }
        }

        // マウスのクリックをした位置を塗りつぶす
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
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
    }
}
