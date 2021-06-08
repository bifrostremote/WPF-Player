using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public MainWindow()
        {
            InitializeComponent();
            Thread thread = new Thread(new ThreadStart(MainProgram));
            thread.IsBackground = true;
            thread.Start();
        }

        private void OnWindowclose(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode); // Prevent memory leak
                                                    // System.Windows.Application.Current.Shutdown(); // Not sure if needed
        }

        void MainProgram()
        {
            Console.WriteLine("Hello World!");
            UdpClient udpServer = new UdpClient(11000);
            //Program test = new Program();
            while (true)
            {
                var remoteEP = new IPEndPoint(IPAddress.Any, 11000);
                byte[] data = udpServer.Receive(ref remoteEP); // listen on port 11000
                Console.WriteLine("receive data from " + remoteEP.ToString());
                Console.WriteLine(ByteArrayToString(data));
                //HandleData(data);
                // NOTE: Possible speed increse.
                Task.Run(() => HandleData(data));
                //udpServer.Send(new byte[] { 1 }, 1, remoteEP); // reply back
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        //byte[][] imageData = null;
        int headerLen = 5; // 0 = type, 1 & 2 = index, 3

        // UDP info
        //int chunkSize = 0;
        //int imageWidth = 0;
        //int imageHeight = 0;
        //ulong totalLength = 0;
        //string identifier = "";

        //List<FrameData> frameDatas = new List<FrameData>();
        FrameData bufferFrame = new FrameData();
        FrameData handleFrame = new FrameData();
        Thread frameHandle;

        public void HandleData(byte[] data)
        {
            int headerInfo = data[0];

            // If new frame is recieved, then handle the data.
            string identifier = BitConverter.ToString(new byte[] { data[1], data[2] }).Replace("-", string.Empty);
            if (headerInfo == 101)
            {
                if (bufferFrame != null)
                {
                    // Check if the handle thread isn't running.
                    if (frameHandle == null || !frameHandle.IsAlive)
                    {
                        // If no id is here, then just return.
                        if (!String.IsNullOrEmpty(bufferFrame.ID))
                        {
                            handleFrame = (FrameData)bufferFrame.Clone();
                            frameHandle = new Thread(new ThreadStart(HandleFrame));
                            frameHandle.Start();
                        }
                    }
                }
                bufferFrame = new FrameData(data);
                //HandleFrame();
                // NOTE: Is this necessary?
                //imageData = null;

                //// Prepare info for new header.
                //identifier = BitConverter.ToString(new byte[] { data[1], data[2] }).Replace("-", string.Empty);
                //chunkSize = (data[4] * 255) + data[3];
                //imageWidth = (data[9] * 255) + data[8];
                //imageHeight = (data[11] * 255) + data[10];
                //totalLength = (ulong)(((data[7] * 255) * 255) + (data[6] * 255) + data[5]);
                //// Prepare for new image data.
                //imageData = new byte[chunkSize][];
            }
            else
            {
                //string identifier = BitConverter.ToString(new byte[] { data[1], data[2] }).Replace("-", string.Empty);
                if (bufferFrame.ID == identifier)
                    bufferFrame.AddChunk(data);

                //int index = (data[4] * 255) + data[3];
                //// Define and copy the data to that part.
                //imageData[index] = data;
            }
        }

        //private static byte[] DecompressBitmap(byte[] input)
        //{
        //    MemoryStream output = new MemoryStream();

        //    using (MemoryStream compressStream = new MemoryStream(input))
        //    using (DeflateStream decompressor = new DeflateStream(compressStream, CompressionMode.Decompress))
        //        decompressor.CopyTo(output);

        //    output.Position = 0;
        //    return output.ToArray();
        //}

        //public ulong _currentVideoStreamPos = 0;
        //public List<UnpackItem> unpackList = null;

        //public byte[] BuildBitmap(byte[][] bitPart)
        //{
        //    // Build a new array, for the combined parts.
        //    byte[] compressSize = new byte[totalLength];
        //    int index = 0;
        //    foreach (var bytePart in bitPart)
        //    {
        //        if (bytePart != null)
        //        {
        //            index = (bytePart[2] * 255) + bytePart[1];
        //            Array.Copy(bytePart, headerLen, compressSize, index * 1000, bytePart.Length - headerLen);
        //        }
        //    }
        //    return compressSize;
        //}

        

        //int WriteBitmapFile(string filename, int width, int height, byte[] imageData)
        //{
        //    byte[] newData = new byte[imageData.Length];

        //    for (int x = 0; x < imageData.Length; x += 4)
        //    {
        //        byte[] pixel = new byte[4];
        //        Array.Copy(imageData, x, pixel, 0, 4);

        //        byte b = pixel[0];
        //        byte g = pixel[1];
        //        byte r = pixel[2];
        //        byte a = pixel[3];

        //        byte[] newPixel = new byte[] { b, g, r, a };

        //        Array.Copy(newPixel, 0, newData, x, 4);
        //    }

        //    imageData = newData;

        //    using (var stream = new MemoryStream(imageData))
        //    using (var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    {
        //        BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0,
        //                                                        bmp.Width,
        //                                                        bmp.Height),
        //                                          ImageLockMode.WriteOnly,
        //                                          bmp.PixelFormat);

        //        IntPtr pNative = bmpData.Scan0;
        //        Marshal.Copy(imageData, 0, pNative, imageData.Length);

        //        bmp.UnlockBits(bmpData);

        //        bmp.Save(filename);
        //    }

        //    return 1;
        //}

        //byte[] previousFrame;
        //public byte[] CompareFrame(byte[] newFrame)
        //{
        //    byte[] returnFrame = new byte[newFrame.Length];

        //    if (previousFrame != null)
        //    {
        //        Parallel.ForEach(newFrame, (frame, fx, index) =>
        //        {
        //            if (frame == 0)
        //                returnFrame[index] = previousFrame[index];
        //            else
        //                returnFrame[index] = frame;
        //        });
        //    }
        //    else
        //        returnFrame = newFrame;

        //    previousFrame = returnFrame;

        //    return returnFrame;
        //}

        public void HandleFrame()
        {
            //byte[] bitmapComImage = BuildBitmap(imageData);
            //byte[] bitmapRawImage = DecompressBitmap(bitmapComImage);

            byte[] bitmapRawImage = handleFrame.BuildBitmapRaw();
            // Return if no image is found. Or the first 10 bytes is empty.
            if (bitmapRawImage.All(singleByte => singleByte == 0) || bitmapRawImage.Take(10).All(singleByte => singleByte == 0))
                return;

            //bitmapRawImage = CompareFrame(bitmapRawImage);

            //WriteBitmapFile("file.png", imageWidth, imageHeight, bitmapRawImage);

            //Bitmap bmp;
            //using (var ms = new MemoryStream(bitmapRawImage))
            //{
            //    bmp = new Bitmap(ms);
            //}
            ////Bitmap img = new Bitmap("file.jpg");
            //bmp.Save("file.png", ImageFormat.Png); // ImageFormat.Jpeg, etc

            #region Bytearray to Bitmap
            //byte[] newData = new byte[bitmapRawImage.Length];
            //for (int x = 0; x < bitmapRawImage.Length; x += 4)
            //{
            //    byte[] pixel = new byte[4];
            //    Array.Copy(bitmapRawImage, x, pixel, 0, 4);

            //    byte b = pixel[0];
            //    byte g = pixel[1];
            //    byte r = pixel[2];
            //    byte a = pixel[3];

            //    byte[] newPixel = new byte[] { b, g, r, a };

            //    Array.Copy(newPixel, 0, newData, x, 4);
            //}

            //bitmapRawImage = newData;
            #endregion

            UpdateImage(bitmapRawImage);
            //var wb = new WriteableBitmap(imageWidth, imageHeight);

            //using (Stream stream = wb.PixelBuffer.AsStream())
            //{
            //    //write to bitmap
            //    await stream.WriteAsync(bitmapRawImage, 0, bitmapRawImage.Length);
            //}

            //ImageContainer.Source = wb;
        }

        //public BitmapImage ToImage(byte[] array)
        //{
        //    using (var ms = new System.IO.MemoryStream(array))
        //    {
        //        var image = new BitmapImage();
        //        image.BeginInit();
        //        image.CacheOption = BitmapCacheOption.OnLoad; // here
        //        ms.Position = 0;
        //        image.StreamSource = ms;
        //        image.EndInit();
        //        return image;
        //    }
        //}

        //public BitmapSource ByteArrayToImage(Byte[] BArray)
        //{

        //    var width = imageWidth;
        //    var height = imageHeight;
        //    var dpiX = 96d;
        //    var dpiY = 96d;
        //    var pixelFormat = PixelFormats.Pbgra32;
        //    var bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;
        //    var stride = bytesPerPixel * width;

        //    BitmapSource bitmap = BitmapImage.Create(width, height, dpiX, dpiY, pixelFormat, null, BArray, stride);
        //    //WriteableBitmap wbtmMap = new WriteableBitmap(BitmapFactory.ConvertToPbgra32Format(bitmap));
        //    return bitmap;
        //}
        //public static BitmapSource Convert(System.Drawing.Bitmap bitmap)
        //{
        //    var bitmapData = bitmap.LockBits(
        //        new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
        //        System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

        //    var bitmapSource = BitmapSource.Create(
        //        bitmapData.Width, bitmapData.Height,
        //        bitmap.HorizontalResolution, bitmap.VerticalResolution,
        //        PixelFormats.Bgr24, null,
        //        bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

        //    bitmap.UnlockBits(bitmapData);

        //    return bitmapSource;
        //}

        //public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
        //{
        //    if (bitmap == null)
        //        throw new ArgumentNullException("bitmap");

        //    IntPtr hBitmap = bitmap.GetHbitmap();

        //    try
        //    {
        //        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //            hBitmap,
        //            IntPtr.Zero,
        //            Int32Rect.Empty,
        //            BitmapSizeOptions.FromEmptyOptions());
        //    }
        //    finally
        //    {
        //        DeleteObject(hBitmap);
        //    }            
        //}


        public static Bitmap ByteToImage(byte[] bitmap)
        {
            using (var ms = new MemoryStream(bitmap))
            {
                return new Bitmap(ms);
            }

            //using (MemoryStream stream = new MemoryStream())
            //{
            //    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            //    return stream.ToArray();
            //}
        }

        private void UpdateImage(byte[] bitmapImage)
        {
            Application.Current.Dispatcher.Invoke(() => {
                //Matrix m = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;
                //double dx = m.M11;
                //double dy = m.M22;

                //WriteableBitmap writeableBitmap = new WriteableBitmap(imageWidth, imageHeight, dx, dy, PixelFormats.Bgr32, null);
                //WriteableBitmap wb = writeableBitmap;

                //using (Stream stream = wb.PixelBuffer.AsStream())
                //{
                //    //write to bitmap
                //    stream.WriteAsync(bitmapImage, 0, bitmapImage.Length);
                //}

                using (var ms = new MemoryStream(bitmapImage))
                {
                    Bitmap bitmap = new Bitmap(ms);
                    //var bitmap = (BitmapSource)new ImageSourceConverter().ConvertFrom(bitmapImage);
                    //ImageContainer.Source = bitmap;
                    //ImageContainer.Width = imageWidth;
                    //ImageContainer.Height = imageHeight;

                    // Display crap.
                    IntPtr handle = IntPtr.Zero;
                    try
                    {
                        handle = bitmap.GetHbitmap();
                        ImageContainer.Source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());

                        //bitmap.Save("C:\\1.jpg"); //saving
                    }
                    catch (Exception)
                    {

                    }

                    finally
                    {
                        DeleteObject(handle);
                    }
                    //using (var ms = new MemoryStream(bitmapImage))
                    //{
                    //    var image = new BitmapImage();
                    //    image.BeginInit();
                    //    image.CacheOption = BitmapCacheOption.OnLoad; // here
                    //    image.StreamSource = ms;
                    //    image.EndInit();

                    //}
                }
            });
        }
    }
}
