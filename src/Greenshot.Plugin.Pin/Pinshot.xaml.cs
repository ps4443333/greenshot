using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Controls;

namespace Greenshot.Plugin.Pin
{
    public partial class Pinshot : Window, IDisposable
    {
        private Point? _dragStart;
        private bool _isScaling = false;  // 防止重复缩放
        private int width_change = 0;
        private int height_change = 0;
        private double wh_rate = 0;
        private double modify = 0.12;
        private BitmapImage lagerImg = null;
        private const int shadowSize = 10;
        private Color activeColor = Color.FromRgb(30, 128, 255);
        private Color deactiveColor = Color.FromRgb(170, 170, 170);
        private bool _disposed = false;

        public Pinshot()
        {
            InitializeComponent();

            this.Activated += (sender, e) =>
            {
                this.shadowEffect.Color = activeColor;
            };
            this.Deactivated += (sender, e) =>
            {
                this.shadowEffect.Color = deactiveColor; ;
            };

            this.Topmost = true;

            this.MainBorder.Margin = new Thickness(shadowSize);

            // 启用双缓冲（重要！）
            this.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            // 动态设置图片
            string imagePath = @"C:\Users\pypy\Desktop\git\shot.png"; // 这里改成你的图片路径
            LoadImage(imagePath);

            // 设置窗口位置
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // 鼠标事件：拖动窗口
            this.MouseDown += Window_MouseDown;
            this.MouseUp += Window_MouseUp;
            this.MouseMove += Window_MouseMove;
            this.MouseWheel += Window_MouseWheel;
        }

        // 动态加载图片
        private void LoadImage(string imagePath)
        {
            try
            {
                // 设置图片源
                MainImage.Source = new BitmapImage(new Uri(imagePath));

                // 设置窗口初始大小 = 图片原始大小
                if (MainImage.Source != null)
                {
                    this.wh_rate = MainImage.Source.Width / MainImage.Source.Height;
                    this.Width = MainImage.Source.Width + 2 * shadowSize;
                    this.Height = MainImage.Source.Height + 2 * shadowSize;
                }
            }
            catch (Exception)
            {
                // 如果图片加载失败，设置一个默认大小
                this.Width = 400;
                this.Height = 300;
            }
        }

        // 鼠标滚轮缩放：改变窗口大小
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 防抖处理，避免快速滚轮触发太多次
            if (_isScaling) return;
            _isScaling = true;

            this.Dispatcher.Invoke(() =>
            {
                this.height_change = (int)(e.Delta * this.modify);
                //this.width_change = (int)(this.height_change * wh_rate);
                this.Height += height_change;
                this.Width = (Height - shadowSize * 2) * wh_rate + shadowSize * 2;
                _isScaling = false;
            }, DispatcherPriority.Render);

        }

        // 鼠标按下：开始拖动窗口
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _dragStart = e.GetPosition(this);
                this.CaptureMouse();
            }
        }

        // 鼠标释放：停止拖动
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _dragStart = null;
                this.ReleaseMouseCapture();
            }
        }

        // 鼠标移动：拖动窗口
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragStart.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                Point current = e.GetPosition(this);
                double deltaX = current.X - _dragStart.Value.X;
                double deltaY = current.Y - _dragStart.Value.Y;

                // 移动窗口位置
                this.Left += deltaX;
                this.Top += deltaY;
            }
        }

        // 右键菜单 - 关闭
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 右键菜单 - 保存
        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建保存文件对话框
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|Bitmap Image (*.bmp)|*.bmp",
                    DefaultExt = ".png",
                    FileName = "Pinshot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")
                };

                // 显示保存文件对话框
                bool? result = saveFileDialog.ShowDialog();

                // 如果用户点击确定且选择了文件路径
                if (result == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
                {
                    // 获取图片源
                    var bitmapSource = MainImage.Source as System.Windows.Media.Imaging.BitmapSource;
                    if (bitmapSource != null)
                    {
                        // 根据用户选择的格式保存图片
                        string extension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                        System.Windows.Media.Imaging.BitmapEncoder encoder;

                        switch (extension)
                        {
                            case ".jpg":
                            case ".jpeg":
                                encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
                                break;
                            case ".bmp":
                                encoder = new System.Windows.Media.Imaging.BmpBitmapEncoder();
                                break;
                            default: // 默认保存为PNG
                                encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                                break;
                        }

                        // 添加图片到编码器
                        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapSource));

                        // 保存图片
                        using (var fileStream = new System.IO.FileStream(saveFileDialog.FileName, System.IO.FileMode.Create))
                        {
                            encoder.Save(fileStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 显示保存失败的消息
                MessageBox.Show("保存图片失败：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 右键菜单 - 取消
        private void CancelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // 关闭右键菜单
            ContextMenu contextMenu = ((MenuItem)sender).Parent as ContextMenu;
            if (contextMenu != null)
            {
                contextMenu.IsOpen = false;
            }
        }

        // 实现IDisposable接口
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 移除事件处理程序
                this.MouseDown -= Window_MouseDown;
                this.MouseUp -= Window_MouseUp;
                this.MouseMove -= Window_MouseMove;
                this.MouseWheel -= Window_MouseWheel;
                this.Activated -= (sender, e) => { this.shadowEffect.Color = activeColor; };
                this.Deactivated -= (sender, e) => { this.shadowEffect.Color = deactiveColor; };

                // 释放图片资源
                if (MainImage.Source != null)
                {
                    // 尝试释放BitmapImage资源
                    var bitmapImage = MainImage.Source as BitmapImage;
                    if (bitmapImage != null)
                    {
                        bitmapImage.UriSource = null;
                    }
                    MainImage.Source = null;
                }

                // 关闭窗口
                if (this.IsLoaded)
                {
                    this.Close();
                }
            }

            _disposed = true;
        }

        // 析构函数
        ~Pinshot()
        {
            Dispose(false);
        }
    }
}