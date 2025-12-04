using System;
using System.Collections.Generic;
using Greenshot.Base.Interfaces.Plugin;
using System.Windows.Forms;
using System.Timers;
using Greenshot.Base.Core;
namespace Greenshot.Plugin.Pin
{
    public class PinPlugin : IGreenshotPlugin
    {
        private ToolStripMenuItem toolStripMenuItem;
        private List<Pinshot> openPinshotWindows = new List<Pinshot>();

        //private System.Timers.Timer timer;
        public void Dispose()
        {
            Dispose(true);
            //if (timer != null)
            //{
            //    timer.Stop();
            //    timer.Dispose();
            //    timer = null;
            //}
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (toolStripMenuItem != null)
            {
                toolStripMenuItem.Dispose();
                toolStripMenuItem = null;
            }

            // 关闭并释放所有打开的窗口
            CloseAllPinshotWindows();
        }

        public string Name => "Pin";

        public bool IsConfigurable => false;

        public bool Initialize()
        {

            Console.WriteLine($"{Name} plugin initialized");
            toolStripMenuItem = new ToolStripMenuItem("Pinshot");
            toolStripMenuItem.Click += ToolStripMenuItem_Click;


            PluginUtils.AddToContextMenu(toolStripMenuItem);

            return true;
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pinshot pinshotWindow = new Pinshot();
            pinshotWindow.Show();
            // 跟踪打开的窗口
            openPinshotWindows.Add(pinshotWindow);
            // 注册窗口关闭事件，以便在窗口关闭时从列表中移除
            pinshotWindow.Closed += PinshotWindow_Closed;
        }

        private void PinshotWindow_Closed(object sender, EventArgs e)
        {
            // 窗口关闭时从列表中移除
            Pinshot closedWindow = sender as Pinshot;
            if (closedWindow != null)
            {
                openPinshotWindows.Remove(closedWindow);
                // 显式释放资源
                closedWindow.Dispose();
            }
        }

        public void Shutdown()
        {
            // 关闭所有打开的窗口
            CloseAllPinshotWindows();
        }

        private void CloseAllPinshotWindows()
        {
            // 关闭并释放所有打开的窗口
            foreach (Pinshot window in openPinshotWindows)
            {
                if (window != null)
                {
                    try
                    {
                        window.Close();
                        window.Dispose();
                    }
                    catch (Exception)
                    {
                        // 忽略关闭窗口时可能发生的异常
                    }
                }
            }
            openPinshotWindows.Clear();
        }

        public void Configure()
        {

        }
    }
}
