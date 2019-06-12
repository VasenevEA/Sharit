using Sharit.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(RecordSourceManager))]
namespace Sharit.WPF
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    class RecordSourceManager : IRecordSourceManager
    {

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        public async Task<RecordSource[]> GetSources()
        {
            var processes = Process.GetProcesses();

            var thisProcess = Process.GetCurrentProcess();
            var list = new List<RecordSource>();
            foreach (var process in processes)
            {
                if (process.Id == thisProcess.Id)
                {
                    continue;
                }
                IntPtr mainWindowHandle = process.MainWindowHandle;
                RECT rect = new RECT();
                if (GetWindowRect(mainWindowHandle, ref rect))
                {
                    var height = rect.right - rect.left;
                    var width = rect.bottom - rect.top;
                    if (width == 0 || height == 0)
                    {
                        continue;
                    }
                    //MoveWindow(mainWindowHandle, Rect.left, Rect.right, Rect.right - Rect.left, Rect.bottom - Rect.top + 50, true);

                    
                    //CaptureWindow(mainWindowHandle, process.ProcessName);
                    var stream = CaptureApplication(process.ProcessName, rect);
                    var source = new RecordSource
                    {
                        Name = process.ProcessName,
                        Id = process.Id,
                        Height = height,
                        Width = width
                    };
                    list.Add(source);
                    await Task.Run(() =>
                    {
                        source.SetImage(stream);
                    });
                }
            }
            return list.ToArray();
        }
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        public MemoryStream CaptureApplication(string processName, RECT rect)
        {
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;


            using (var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (Graphics graphics = Graphics.FromImage(bmp))
                {
                    var myImageCodecInfo = GetEncoderInfo("image/png");
                    graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
                    var quality = Encoder.Quality;
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(quality, 255);
                    var stream = new MemoryStream();
                    using (var bmp1 = new Bitmap(bmp,width/20, height/20))
                    {
                        bmp1.Save(stream, myImageCodecInfo, encoderParams);
                        return stream;
                    }
                }
            }
            //bmp.Save($"{processName}.png", ImageFormat.Png);
        }

        public void CaptureApplication(string processName, RECT rect, IntPtr whandle)
        {
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;


            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bmp);
            //graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            var hdc = graphics.GetHdc();
            PrintWindow(whandle, hdc, 0);
            graphics.ReleaseHdc(hdc);
            bmp.Save($"{processName}.png", ImageFormat.Png);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);




        public void CaptureWindow(IntPtr handle, string name)
        {
            
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up 
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            img.Save($"{name}.png");
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
        }
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        }
        private class GDI32
        {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }
    }


}
