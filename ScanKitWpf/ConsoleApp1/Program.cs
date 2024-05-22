// See https://aka.ms/new-console-template for more information
using OpenCvSharp;
using System.Diagnostics;

Process[] processes = Process.GetProcessesByName("BarTend");
foreach (Process process in processes)
{
    try
    {
        process.Kill();
        process.WaitForExit(); // 等待进程退出
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error killing process: {ex.Message}");
    }
}

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
    Cv2.NamedWindow("gray",WindowFlags.Normal);
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
    Mat mat = new Mat();
    string code = qRCodeDetector.DetectAndDecode(gray, out point2Fs, mat);
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
    return code;
}