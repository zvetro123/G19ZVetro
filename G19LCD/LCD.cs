using G19LCD.Pages;
using G19LCD.Transactions;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Media.Imaging;

namespace G19LCD
{
    public class LCD
    {
        private const int MAX_FRAMES = 20 * 15; //20fps for 15 sec

        UsbEndpointWriter writer;
        UsbEndpointReader reader;
        ErrorCode ec = ErrorCode.None;
        IUsbDevice wholeUsbDevice;

        private UsbDevice MyUsbDevice;

        public void SetBacklightColor(
            byte red,
            byte green,
            byte blue)
        {
            byte[] data =
            {
                255,
                red,
                green,
                blue
            };

            var packet = new UsbSetupPacket(
                0x21,
                0x09,
                0x0307,
                0x01,
                (short)data.Length);

            bool success =
                MyUsbDevice.ControlTransfer(
                    ref packet,
                    data,
                    data.Length,
                    out int transferred);

            Debug.WriteLine(
                $"RGB success={success} bytes={transferred}");
        }

        public UsbDeviceFinder MyUsbFinder;

        private int cPageIndex;

        private Boolean captureImages;

        private Bitmap currentImageVisible;

        private Transaction pageTrans = null;

        public List<LcdPage> Pages { get; set; }

        List<Bitmap> images = new List<Bitmap>();

        // Lock to serialize access to the USB writer
        private readonly object writerLock = new object();
        private Thread buttonThread;

        private bool shuttingDown;

        public event Action<string> ButtonPressed;
        
        /// <summary>
        /// Get or Set the current Page displayed on the screen
        /// </summary>
        public LcdPage CurrentPage {
            get { return Pages[CurrentPageIndex]; }
            set
            {
                cPageIndex = Pages.IndexOf(value);
                pageTrans = new FadeTransaction(this);
                UpdatePage();
            }
        }

        /// <summary>
        ///  Get or Set the index of the current Page displayed on the screen
        /// </summary>
        public int CurrentPageIndex
        {
            get { return cPageIndex; }
            set
            {
                cPageIndex = value;
                pageTrans = new FadeTransaction(this);
                UpdatePage();
            }
        }

        /// <summary>
        ///  Get or Set if frames are captured
        /// </summary>
        public bool captureFrames
        {
            get { return captureImages; }

            set
            {
                if (!value) { images.Clear(); }
                captureImages = value;
            }
        }

        public List<Bitmap> getImages
        {
            get { return images; } //This list will always be empty as It's just used it to create the gif
        }

        /// <summary>
        ///  Create a new instance of the LCD class
        /// </summary>
        /// <param name="vid">Device vendor ID</param>
        /// <param name="pid">Device product ID</param>
        public LCD(int vid, int pid)
        {
            MyUsbFinder = new UsbDeviceFinder(vid, pid);
        }

        /// <summary>
        /// Open connection to the device
        /// </summary>
        public void OpenDevice()
        {
            MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

            if (MyUsbDevice == null)
                throw new Exception("Device Not Found.");

            Debug.WriteLine("=== ALL USB DEVICES ===");

            foreach (UsbRegistry usbRegistry in UsbDevice.AllDevices)
            {
                Debug.WriteLine(
                    $"VID={usbRegistry.Vid:X4} " +
                    $"PID={usbRegistry.Pid:X4} " +
                    $"Name={usbRegistry.FullName}");
            }

            wholeUsbDevice = MyUsbDevice as IUsbDevice;
            if (!ReferenceEquals(wholeUsbDevice, null))
            {
                wholeUsbDevice.SetConfiguration(1);

                Debug.WriteLine("=== CONFIG INFO ===");

                foreach (UsbConfigInfo config in MyUsbDevice.Configs)
                {
                    Debug.WriteLine(
                        $"Config {config.Descriptor.ConfigID}");

                    foreach (UsbInterfaceInfo iface in config.InterfaceInfoList)
                    {
                        Debug.WriteLine(
                            $"  Interface {iface.Descriptor.InterfaceID}");

                        foreach (UsbEndpointInfo ep in iface.EndpointInfoList)
                        {
                            Debug.WriteLine(
                                $"    Endpoint: {ep.Descriptor.EndpointID:X2}");
                        }
                    }
                }

                Debug.WriteLine("=== INTERFACE TEST ===");

                for (int i = 0; i < 8; i++)
                {
                    try
                    {
                        wholeUsbDevice.ReleaseInterface(i);
                    }
                    catch
                    {
                    }

                    try
                    {
                        wholeUsbDevice.ClaimInterface(i);

                        Debug.WriteLine(
                            $"Successfully claimed interface {i}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(
                            $"Interface {i} failed: {ex.Message}");
                    }
                }
                try
                {
                    wholeUsbDevice.ReleaseInterface(0);
                }
                catch
                {
                }

                wholeUsbDevice.ClaimInterface(1);
            }

            reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
            writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep02);

            StartButtonListener();
        }

        public void StartButtonListener()
        {
            if (buttonThread != null)
                return;

            shuttingDown = false;

            buttonThread = new Thread(ButtonDebugLoop);
            buttonThread.IsBackground = true;
            buttonThread.Start();
        }

        private void HandleButton(byte[] buffer)
        {
            Debug.WriteLine(
    $"Button data: {BitConverter.ToString(buffer)}");
            switch (buffer[0])
            {
                case 0x01:
                    ButtonPressed?.Invoke("BTN_SETTINGS");
                    break;

                case 0x02:
                    ButtonPressed?.Invoke("BTN_BACK");
                    break;

                case 0x04:
                    ButtonPressed?.Invoke("BTN_MENU");
                    break;

                case 0x08:
                    ButtonPressed?.Invoke("BTN_OK");
                    break;

                case 0x10:
                    ButtonPressed?.Invoke("BTN_RIGHT");
                    break;

                case 0x20:
                    ButtonPressed?.Invoke("BTN_LEFT");
                    break;

                case 0x40:
                    ButtonPressed?.Invoke("BTN_DOWN");
                    break;

                case 0x80:
                    ButtonPressed?.Invoke("BTN_UP");
                    break;
            }
        }

        private void ButtonDebugLoop()
        {
            try
            {
                byte[] buffer = new byte[32];

                while (!shuttingDown)
                {
                    int transferred = 0;

                    Array.Clear(buffer, 0, buffer.Length);

                    reader.Read(
                        buffer,
                        250,
                        out transferred);

                    if (buffer[0] != 0)
                    {
                        Debug.WriteLine(
                          $"RAW: {BitConverter.ToString(buffer)}");

                        HandleButton(buffer);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "[LCD THREAD CRASHED] " +
                    ex.ToString());
            }
        }

        /// <summary>
        /// Close the connection to the device
        /// </summary>
        public void CloseDevice()
        {
            Debug.WriteLine("[LCD] CloseDevice() called");

            shuttingDown = true;

            if (buttonThread != null &&
                buttonThread.IsAlive)
            {
                try
                {
                    Debug.WriteLine("[LCD] Waiting for button thread...");

                    buttonThread.Join(1000);

                    Debug.WriteLine("[LCD] Button thread exited");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"[LCD] buttonThread.Join failed: {ex}");
                }
            }

            try
            {
                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        if (wholeUsbDevice != null)
                        {
                            try
                            {
                                wholeUsbDevice.ReleaseInterface(0);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(
                                    $"[LCD] ReleaseInterface failed: {ex}");
                            }
                        }

                        Debug.WriteLine("[LCD] Closing USB device");

                        MyUsbDevice.Close();

                        Debug.WriteLine("[LCD] USB device closed");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[LCD] CloseDevice exception: {ex}");
            }
            finally
            {
                reader = null;
                writer = null;
                wholeUsbDevice = null;
                MyUsbDevice = null;
                buttonThread = null;

                Debug.WriteLine("[LCD] Skipping UsbDevice.Exit()");

                Debug.WriteLine("[LCD] CloseDevice() completed");
            }
        }
        public void UpdatePage()
        {
            LcdPage pg = Pages[CurrentPageIndex];
            UpdateScreen(pg.Invalidate(), pageTrans);
            //Screen updated
            pageTrans = null;
        }


        /// <summary>
        ///  Update the screen with the bitmap
        /// </summary>
        /// <param name="bmp">WinForm Bitmap</param>
        public void UpdateScreen(Bitmap bmp)
        {
           // bmp.Save("page" + cPageIndex + ".jpg");
            UpdateScreen(bmp, null);
        }

        /// <summary>
        ///  Update the screen with the bitmap
        /// </summary>
        /// <param name="bmp">WPF BitmapSource</param>
        public void UpdateScreen(BitmapSource bmp)
        {
            UpdateScreen(Utils.BitmapImage2Bitmap(bmp));
        }


        /// <summary>
        /// Update the screen with the bitmapSource
        /// </summary>
        /// <param name="bmp">WPF BitmapSource</param>
        /// <param name="transaction">Transaction between images</param>
        public void UpdateScreen(BitmapSource bmp, Transaction transaction)
        {
            UpdateScreen(Utils.BitmapImage2Bitmap(bmp), transaction);
        }

        /// <summary>
        ///  Update the screen with the bitmap
        /// </summary>
        /// <param name="bmp">WinForm Bitmap</param>
        /// <param name="transaction">Transaction between images</param>
        public void UpdateScreen(Bitmap bmp, Transaction transaction)
        {
            if (bmp.Width != 320 || bmp.Height != 240)
                throw new SystemException("Incorrect image size! Image size must be 320x240!");


            if (transaction != null && currentImageVisible != null) {
                //We create a clone of the new (or next) bitmap, so that we can dispose the object when the transaction is done.
                //This will reduse the amount of pointers used in memory during transaction, and we can dispose some of them when we're done!
                transaction.start(currentImageVisible, bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat));
            }

            if (captureFrames) {
                if (images.Count == MAX_FRAMES) {
                    images.RemoveAt(0);
                }
                images.Add(bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat));
            }


            byte[] lcd_buffer = Utils.convertToByte(bmp);
            int written;

            lock (writerLock)
            {
                // Basic pre-checks
                if (MyUsbDevice == null || writer == null || !MyUsbDevice.IsOpen)
                {
                    Debug.WriteLine("[LCD] USB device or writer not open. Attempting to reopen...");
                    try
                    {
                        CloseDevice();
                        OpenDevice();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[LCD] Failed to reopen device: {ex.Message}");
                    }
                }

                if (MyUsbDevice == null || writer == null || !MyUsbDevice.IsOpen)
                {
                    Debug.WriteLine("[LCD] Device still not available before write. Aborting update.");
                    throw new Exception("USB device not available for write.");
                }

                // Try single write with extended timeout first
                Debug.WriteLine($"[LCD] Attempting single write. bufferLen={lcd_buffer.Length}");
                ec = writer.Write(lcd_buffer, 1000, out written);
                Debug.WriteLine($"[LCD] Single write result: ec={ec}, lastErr='{UsbDevice.LastErrorString}', written={written}");

                // If single write failed, attempt chunked writes as a fallback
                if (ec != ErrorCode.None)
                {
                    Debug.WriteLine("[LCD] Single write failed, attempting chunked writes...");
                    ec = ErrorCode.None;
                    int offset = 0;
                    const int chunkSize = 16 * 1024; // 16KB chunks

                    while (offset < lcd_buffer.Length)
                    {
                        int len = Math.Min(chunkSize, lcd_buffer.Length - offset);
                        byte[] chunk = new byte[len];
                        Array.Copy(lcd_buffer, offset, chunk, 0, len);
                        int w;
                        ec = writer.Write(chunk, 1000, out w);
                        Debug.WriteLine($"[LCD] Chunk write ec={ec}, written={w}, offset={offset}");
                        if (ec != ErrorCode.None)
                        {
                            break;
                        }
                        offset += len;
                    }
                }

                // If still failing, attempt one recovery cycle (reopen) and retry chunked write
                if (ec != ErrorCode.None)
                {
                    Debug.WriteLine("[LCD] Chunked writes failed, attempting recovery by reopening device...");
                    try
                    {
                        CloseDevice();
                        OpenDevice();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[LCD] Recovery reopen failed: {ex.Message}");
                    }

                    if (MyUsbDevice == null || writer == null || !MyUsbDevice.IsOpen)
                    {
                        Debug.WriteLine("[LCD] Recovery reopen did not make device available. Throwing.");
                        throw new Exception(UsbDevice.LastErrorString ?? "Unknown USB error after recovery");
                    }

                    // Retry chunked write
                    ec = ErrorCode.None;
                    int off = 0;
                    const int cSize = 16 * 1024;
                    while (off < lcd_buffer.Length)
                    {
                        int len = Math.Min(cSize, lcd_buffer.Length - off);
                        byte[] chunk = new byte[len];
                        Array.Copy(lcd_buffer, off, chunk, 0, len);
                        int w;
                        ec = writer.Write(chunk, 1000, out w);
                        Debug.WriteLine($"[LCD] Recovery chunk write ec={ec}, written={w}, offset={off}");
                        if (ec != ErrorCode.None) break;
                        off += len;
                    }
                }

                if (ec != ErrorCode.None)
                {
                    Debug.WriteLine($"[LCD] Final write failure: ec={ec}, lastErr='{UsbDevice.LastErrorString}'");
                    throw new Exception(UsbDevice.LastErrorString ?? "Unknown USB write error");
                }

                currentImageVisible = bmp;
            }
        }

    }
}
