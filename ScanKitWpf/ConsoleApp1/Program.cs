// See https://aka.ms/new-console-template for more information
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;
using System;
using System.Diagnostics;
//using static System.Net.Mime.MediaTypeNames;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.OpenCV;
using static ZXing.Rendering.SvgRenderer;
using OpenCvSharp.Extensions;

//Process[] processes = Process.GetProcessesByName("BarTend");
//foreach (Process process in processes)
//{
//    try
//    {
//        process.Kill();
//        process.WaitForExit(); // 等待进程退出
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error killing process: {ex.Message}");
//    }
//}
//return;

var res = ScanQRCode("d:/ng/20.bmp");

return;

Mat mat = new Mat("d:/ng/17.png");

Mat result = new Mat();
//Cv2.Resize(mat, result, new OpenCvSharp.Size(900, 600));
Cv2.NamedWindow("17", WindowFlags.Normal);
Cv2.ImShow("17", mat);
Cv2.WaitKey();
Cv2.DestroyAllWindows();
DetectQRCode(mat);

static string DetectQRCode(Mat src)
{
    //src = Cv2.ImRead(@"C:\Users\1617\Desktop\Test\qr.png");
    //图像灰度
    Mat gray = new Mat();
    Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
    Cv2.NamedWindow("gray", WindowFlags.Normal);
    Cv2.ImShow("gray", gray);
    Cv2.WaitKey();
    Cv2.DestroyAllWindows();
    //二值化
    Mat threshold = new Mat();
    //Cv2.AdaptiveThreshold(gray, threshold, 255.0, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 13, 2);
    Cv2.Threshold(gray, threshold, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
    Cv2.NamedWindow("threshold", WindowFlags.Normal);
    Cv2.ImShow("threshold", threshold);

    Cv2.WaitKey();
    Cv2.DestroyAllWindows();
    //绘制轮廓

    //截取二维码有效区域
    //识别二维码
    QRCodeDetector qRCodeDetector = new QRCodeDetector();
    Point2f[] point2Fs;
    //Cv2.ImShow("gray", gray);
    string[] qrCodeTexts = null;
    Mat mat = new Mat();
    var hasQrCode = qRCodeDetector.DetectMulti(gray, out point2Fs);
    if (hasQrCode)
    {
        qRCodeDetector.DecodeMulti(gray, point2Fs, out qrCodeTexts);
    }
    if (qrCodeTexts != null)
    {
        Console.WriteLine($"检测到{qrCodeTexts.Length}个二维码：");
        for (int i = 0; i < qrCodeTexts.Length; i++)
        {
            Console.WriteLine($"第{(i + 1)}个的内容为：{qrCodeTexts[i]}");
        }
    }
    //Point[][] contours;
    //HierarchyIndex[] hierarchies;
    //Cv2.FindContours(src, out contours, out hierarchies, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
    //RNG rng = new RNG(12345);
    //for (int i = 0; i < point2Fs.Length; i++)
    //{
    //  Cv2.DrawContours(src, point2Fs, i, new Scalar(rng.Uniform(0, 255), rng.Uniform(0, 255), rng.Uniform(0, 255)), 6, LineTypes.Link4);

    //}
    Cv2.Rectangle(src, new Point((int)point2Fs[3].X, (int)point2Fs[1].Y), new Point((int)point2Fs[2].X, (int)point2Fs[2].Y), new Scalar(0, 255, 255), 2);
    Cv2.ImShow("src", src);
    Cv2.DestroyAllWindows();
    return string.Join(";", qrCodeTexts);
}

static Result[] ScanQRCode(string imagePath)
{
    using (Mat mat = new Mat(imagePath, ImreadModes.Grayscale))
    {
        using (var src_gray = new Mat())
        {
            Mat dst = mat.ConvertScaleAbs(1.5,0);
            Cv2.NamedWindow("对比", WindowFlags.KeepRatio);
            Cv2.ImShow("对比", dst);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();

            Cv2.Blur(mat, src_gray, new Size(7, 7));
            Cv2.NamedWindow("滤波", WindowFlags.KeepRatio);
            Cv2.ImShow("滤波", src_gray);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
            /*Mat src_gray1 = new Mat();
            Cv2.Threshold(src_gray, src_gray1, 150, 255, ThresholdTypes.Binary);
            Cv2.NamedWindow("二值化", WindowFlags.KeepRatio);
            Cv2.ImShow("二值化", src_gray1);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
            //Canny边缘检测
            Mat canny_Image = new Mat();
            Cv2.Canny(src_gray1, canny_Image, 100, 200);
            Cv2.NamedWindow("边缘检测", WindowFlags.KeepRatio);
            Cv2.ImShow("边缘检测", canny_Image);
            Mat[] contours = new Mat[10000];
            contours = Cv2.FindContoursAsMat(canny_Image, RetrievalModes.CComp, ContourApproximationModes.ApproxNone, null);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();

            Mat dst_Image = Mat.Zeros(canny_Image.Size(), mat.Type());
            Cv2.NamedWindow("边框", WindowFlags.KeepRatio);
            Cv2.ImShow("边框", canny_Image);
            Cv2.WaitKey();
            Cv2.DestroyAllWindows();
            int z = 0;
            List<Result> results = new List<Result>(); ;
            for (int i = 0; i < contours.Length; i++)
            {
                Rect rect = Cv2.BoundingRect(contours[i]);
                //只判断DataMatrix码的宽度才进入解析
                if (rect.Width > 10 && rect.Height > 10 && rect.Width < 100 && rect.Height < 60)
                {
                    //将DataMatrix码范围的框框裁剪出图片
                    Rect rect1 = new Rect(rect.X, rect.Y, rect.Width, rect.Height);
                    //srcImage 为原图像 rect1 裁剪的范围。
                    Mat RectMat = new Mat(mat, rect1);
                    //Bitmap bitmap = BitmapConverter.ToBitmap(RectMat);
                    //将裁剪出来的图片进行放大，如果不放大ZXing解析的结果就不理想。（会漏了好多DataMatrix码）
                    OpenCvSharp.Size size = new OpenCvSharp.Size(mat.Width * 5, mat.Height * 5);
                    Mat SizeMat = new Mat();
                    Cv2.Resize(RectMat, SizeMat, size);
                    Cv2.NamedWindow(i + "边框", WindowFlags.KeepRatio);
                    Cv2.ImShow(i + "边框", SizeMat);
                    Cv2.WaitKey();
                    Cv2.DestroyAllWindows();

                    //bitmap.Dispose();
                    //Bitmap bitmap1 = BitmapConverter.ToBitmap(SizeMat);
                    //bitmap.Save(i + ".png");
                    for (int y = 0; y < 1; y++)
                    {
                        //解析DataMatrix码
                        var reader = new BarcodeReader
                        {
                            Options = new DecodingOptions
                            {
                                    TryHarder = true,
                                    PossibleFormats = new[]
                                    {
                                        BarcodeFormat.CODE_128,
                                        BarcodeFormat.QR_CODE,//这个是二维码
                                        BarcodeFormat.CODE_128,
                                        BarcodeFormat.DATA_MATRIX,
                                        BarcodeFormat.CODE_39,
                                        BarcodeFormat.CODE_93
                                    }
                            },
                            AutoRotate = true,
                        };
                        //Result result = reader.Decode(SizeMat);
                        //if (result == null)
                        //{
                        //    //翻转图片让ZXing重新解析，防止DataMatrix码因为位置识别不出来
                        //    SizeMat.Flip(FlipMode.X);
                        //    continue;
                        //}
                        //else
                        //{
                        //    z++;
                        //    Console.WriteLine("第" + z + "个码:" + result.ToString() + Environment.NewLine);
                        //    results.Add(result);
                        //    break;
                        //}
                    }
                    RectMat.Dispose();
                    SizeMat.Dispose();
                    Scalar color = Scalar.Red;
                    Cv2.DrawContours(dst_Image, contours, i, color, 2, LineTypes.Link8, null);

                }

            }
            dst_Image.SaveImage("1.bmp");
            return results.ToArray();*/
            var barcodeReader = new BarcodeReader
            {
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new[]
                            {
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.QR_CODE,//这个是二维码
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.DATA_MATRIX,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.CODE_93
                    }
                },
                AutoRotate = true,
            };

            return barcodeReader.DecodeMultiple(dst);
        }
    }

    static Mat BoostBrightness(Mat image, double alpha)
    {
        Mat result = image.Clone();
        image.ConvertTo(result, -1, alpha, 0);
        return result;
    }
}