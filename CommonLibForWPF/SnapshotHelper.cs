using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonLibForWPF
{
    public static class SnapshotHelper
    {
        private static readonly Transform ShrinkTransform = new ScaleTransform(0.6, 0.6);
        public static RenderTargetBitmap TakeSnapshot(FrameworkElement view, Size snapshotSize)
        {
            var bound = VisualTreeHelper.GetDescendantBounds(view);
            var intWidth = (int)snapshotSize.Width;
            var intHeight = (int)snapshotSize.Height;
            if (intHeight == 0 || intWidth == 0) throw new ArgumentException("Neither width nor height of the snapshotSize can be zero.", "snapshotSize");

            var rtb = new RenderTargetBitmap(intWidth, intHeight, 0, 0, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            dv.Transform = ShrinkTransform;
            using (var ctx = dv.RenderOpen())
            {
                var b = new VisualBrush();
                b.AutoLayoutContent = false;
                b.Stretch = Stretch.None;
                b.Visual = view;
                ctx.DrawRectangle(b, null, bound);
            }
            rtb.Render(dv);
            return rtb;
        }

        private static RenderTargetBitmap TakeSnapshot(FrameworkElement view)
        {
            var bound = VisualTreeHelper.GetDescendantBounds(view);
            var intHeight = (int)view.Height;
            var intWidth = (int)view.Width;

            if (intHeight == 0 || intWidth == 0) return null;
            var rtb = new RenderTargetBitmap(intWidth, intHeight, 0, 0, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                var b = new VisualBrush();
                b.AutoLayoutContent = false;
                b.Stretch = Stretch.None;
                b.Visual = view;
                ctx.DrawRectangle(b, null, bound);
            }
            rtb.Render(dv);
            return rtb;
        }

        public static void SaveSnapshotToFile(this FrameworkElement view, string filename)
        {
            var bitmap = TakeSnapshot(view);
            if (bitmap != null)
                using (var s = File.OpenWrite(filename))
                {
                    var pngBitmapEncoder = new PngBitmapEncoder();
                    var bitmapFrame = BitmapFrame.Create(bitmap);
                    pngBitmapEncoder.Frames.Add(bitmapFrame);
                    pngBitmapEncoder.Save(s);
                }
        }
    }

}
