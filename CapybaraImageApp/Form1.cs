using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CapybaraImageApp.Models;

namespace CapybaraImageApp;

public class Form1 : Form
{
    private Button btnProcess;

    private Button btnSave;

    private FlowLayoutPanel panelImages;

    private PictureBox pictureBoxHistogram;


    public Form1()
    {
        InitializeComponent();
        comboBoxOperations.DataSource = Enum.GetValues(typeof(ImageOperation));
    }

    /// <summary>
    ///     Required method for Designer support - do not modify
    ///     the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        // Настройки формы
        ClientSize = new Size(850, 600); // Увеличим высоту
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        Text = "Capybara Image App";

        int leftCol = 500;
        int spacingY = 20;
        int currentY = 20;

        // --- Основное изображение ---
        pictureBoxResult = new PictureBox();
        pictureBoxResult.Location = new Point(50, 20);
        pictureBoxResult.Size = new Size(400, 300);
        pictureBoxResult.SizeMode = PictureBoxSizeMode.Zoom;
        Controls.Add(pictureBoxResult);

        // --- Гистограмма ---
        pictureBoxHistogram = new PictureBox();
        pictureBoxHistogram.Location = new Point(50, 330);
        pictureBoxHistogram.Size = new Size(400, 100);
        pictureBoxHistogram.BorderStyle = BorderStyle.FixedSingle;
        Controls.Add(pictureBoxHistogram);

        // --- Картинка с Фурье ---
        pictureBoxFourier = new PictureBox();
        pictureBoxFourier.Location = new Point(50, 450);
        pictureBoxFourier.Size = new Size(50, 50);
        pictureBoxFourier.BorderStyle = BorderStyle.FixedSingle;
        Controls.Add(pictureBoxFourier);

        // --- Панель изображений ---
        panelImages = new FlowLayoutPanel();
        panelImages.Location = new Point(leftCol, currentY);
        panelImages.Size = new Size(300, 100);
        panelImages.AutoScroll = false;
        panelImages.WrapContents = false;
        panelImages.FlowDirection = FlowDirection.LeftToRight;
        Controls.Add(panelImages);
        currentY += panelImages.Height + spacingY;

        // --- Выпадающий список операций ---
        comboBoxOperations = new ComboBox();
        comboBoxOperations.Location = new Point(leftCol, currentY);
        comboBoxOperations.Size = new Size(180, 25);
        Controls.Add(comboBoxOperations);
        comboBoxOperations.Items.AddRange(Enum.GetNames(typeof(ImageOperation)));
        comboBoxOperations.SelectedIndex = 0;
        currentY += comboBoxOperations.Height + spacingY;

        // --- Кнопка "Добавить изображение" ---
        btnAddImage = new Button();
        btnAddImage.Location = new Point(leftCol, currentY);
        btnAddImage.Size = new Size(180, 30);
        btnAddImage.Text = "Добавить изображение";
        btnAddImage.Click += BtnAddImage_Click;
        Controls.Add(btnAddImage);
        currentY += btnAddImage.Height + spacingY;

        // --- Кнопка "Обработать" ---
        btnProcess = new Button();
        btnProcess.Location = new Point(leftCol, currentY);
        btnProcess.Size = new Size(180, 30);
        btnProcess.Text = "Обработать";
        btnProcess.Click += BtnProcess_Click;
        Controls.Add(btnProcess);
        currentY += btnProcess.Height + spacingY;

        // --- Кнопка "Сохранить" ---
        btnSave = new Button();
        btnSave.Location = new Point(leftCol, currentY);
        btnSave.Size = new Size(180, 30);
        btnSave.Text = "Сохранить";
        btnSave.Click += BtnSave_Click;
        Controls.Add(btnSave);
        currentY += btnSave.Height + spacingY;

        // --- Поля ввода координат + кнопка
        txtPointX = new TextBox { Location = new Point(leftCol, currentY), Width = 50 };
        txtPointY = new TextBox { Location = new Point(leftCol + 60, currentY), Width = 50 };
        btnAddPoint = new Button
        {
            Location = new Point(leftCol + 120, currentY),
            Size = new Size(120, 30),
            Text = "Добавить точку"
        };
        btnAddPoint.Click += BtnAddPoint_Click;
        Controls.Add(txtPointX);
        Controls.Add(txtPointY);
        Controls.Add(btnAddPoint);
        currentY += btnAddPoint.Height + spacingY;

        // --- Кнопка "Бинаризация" ---
        btnBinarize = new Button();
        btnBinarize.Location = new Point(leftCol, currentY);
        btnBinarize.Size = new Size(180, 30);
        btnBinarize.Text = "Бинаризация";
        btnBinarize.Click += BtnBinarize_Click;
        Controls.Add(btnBinarize);
        currentY += btnBinarize.Height + spacingY;

        // --- Label и NumericUpDown для WindowSize ---
        Label lblWindowSize = new Label();
        lblWindowSize.Text = "Размер окна (%)";
        lblWindowSize.Location = new Point(leftCol, currentY);
        lblWindowSize.Size = new Size(100, 20);
        Controls.Add(lblWindowSize);
        currentY += lblWindowSize.Height;

        numWindowSize = new NumericUpDown();
        numWindowSize.Location = new Point(leftCol, currentY);
        numWindowSize.Size = new Size(80, 25);
        numWindowSize.Minimum = 1;
        numWindowSize.Maximum = 100;
        numWindowSize.Value = 8;
        Controls.Add(numWindowSize);

        // --- Label и NumericUpDown для Threshold ---
        Label lblThreshold = new Label();
        lblThreshold.Text = "Порог бинаризации";
        lblThreshold.Location = new Point(leftCol + 100, currentY - lblWindowSize.Height);
        lblThreshold.Size = new Size(130, 20);
        Controls.Add(lblThreshold);

        numThreshold = new NumericUpDown();
        numThreshold.Location = new Point(leftCol + 100, currentY);
        numThreshold.Size = new Size(80, 25);
        numThreshold.DecimalPlaces = 2;
        numThreshold.Minimum = 0.1M;
        numThreshold.Maximum = 1.0M;
        numThreshold.Increment = 0.05M;
        numThreshold.Value = 0.85M;
        Controls.Add(numThreshold);
        currentY += numThreshold.Height + spacingY;

        // --- Label и NumericUpDown для Sigma ---
        Label lblSigma = new Label();
        lblSigma.Text = "Sigma (Гаусс)";
        lblSigma.Location = new Point(leftCol, currentY);
        lblSigma.Size = new Size(100, 20);
        Controls.Add(lblSigma);

        Label lblKernelSize = new Label();
        lblKernelSize.Text = "Размер ядра";
        lblKernelSize.Location = new Point(leftCol + 100, currentY);
        lblKernelSize.Size = new Size(100, 20);
        Controls.Add(lblKernelSize);
        currentY += lblSigma.Height;

        numSigma = new NumericUpDown();
        numSigma.Location = new Point(leftCol, currentY);
        numSigma.Size = new Size(80, 25);
        numSigma.DecimalPlaces = 2;
        numSigma.Minimum = 0.1M;
        numSigma.Maximum = 10.0M;
        numSigma.Increment = 0.1M;
        numSigma.Value = 3.0M;
        Controls.Add(numSigma);

        numKernelSize = new NumericUpDown();
        numKernelSize.Location = new Point(leftCol + 100, currentY);
        numKernelSize.Size = new Size(80, 25);
        numKernelSize.Minimum = 3;
        numKernelSize.Maximum = 99;
        numKernelSize.Increment = 2;
        numKernelSize.Value = 13;
        Controls.Add(numKernelSize);
        currentY += numKernelSize.Height + spacingY;

        // --- Label и NumericUpDown для LowPass Sigma ---
        Label lblLowPassSigma = new Label();
        lblLowPassSigma.Text = "Sigma (Фурье)";
        lblLowPassSigma.Location = new Point(leftCol, currentY - 20);
        lblLowPassSigma.Size = new Size(120, 20);
        Controls.Add(lblLowPassSigma);
        currentY += lblLowPassSigma.Height;

        numLowPassSigma = new NumericUpDown();
        numLowPassSigma.Location = new Point(leftCol, currentY - 20);
        numLowPassSigma.Size = new Size(80, 25);
        numLowPassSigma.DecimalPlaces = 1;
        numLowPassSigma.Minimum = 1;
        numLowPassSigma.Maximum = 500;
        numLowPassSigma.Value = 50;
        Controls.Add(numLowPassSigma);
    }


    private void BtnAddImage_Click(object sender, EventArgs e)
    {
        var openFile = new OpenFileDialog();
        if (openFile.ShowDialog() == DialogResult.OK)
        {
            var newImage = new Bitmap(openFile.FileName);
            loadedImages.Add(newImage);

            var newPictureBox = new PictureBox
            {
                Image = newImage,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(100, 100),
                BorderStyle = BorderStyle.None
            };
            panelImages.Controls.Add(newPictureBox);
        }

        panelImages.Refresh();
    }

    private Bitmap ResizeImage(Bitmap img, int width, int height)
    {
        var resized = new Bitmap(width, height);
        using (var g = Graphics.FromImage(resized))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, 0, 0, width, height);
        }

        return resized;
    }

    private void BtnProcess_Click(object sender, EventArgs e)
    {
        if (loadedImages.Count < 2)
        {
            MessageBox.Show("Добавьте хотя бы два изображения.");
            return;
        }

        var selectedOperation = (ImageOperation)comboBoxOperations.SelectedItem;
        var result = new Bitmap(loadedImages[0]);

        for (var i = 1; i < loadedImages.Count; i++)
        {
            var resizedImg2 = ResizeImage(loadedImages[i], result.Width, result.Height);
            result = ProcessImages(result, resizedImg2, selectedOperation);
        }

        // Масштабируем изображение, чтобы оно поместилось в pictureBoxResult
        var maxWidth = pictureBoxResult.Width;
        var maxHeight = pictureBoxResult.Height;
        var scale = Math.Min((float)maxWidth / result.Width, (float)maxHeight / result.Height);
        var newWidth = (int)(result.Width * scale);
        var newHeight = (int)(result.Height * scale);

        var resizedImage = new Bitmap(result, newWidth, newHeight);

        pictureBoxResult.Image = resizedImage;

        var histogramData = CalculateHistogram(result);

        if (histogramData.Sum() == 0)
            MessageBox.Show("Гистограмма пустая. Возможно, изображение полностью белое.");
        else
            pictureBoxHistogram.Image = DrawHistogram(histogramData);
    }

    private Bitmap ProcessImages(Bitmap img1, Bitmap img2, ImageOperation operation)
    {
        var width = Math.Max(img1.Width, img2.Width);
        var height = Math.Max(img1.Height, img2.Height);
        var intermediateResult = new Bitmap(width, height);

        switch (operation)
        {
            case ImageOperation.Sum:
            case ImageOperation.Multiply:
            case ImageOperation.Average:
            case ImageOperation.Min:
            case ImageOperation.Max:
            case ImageOperation.Mask:
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var c1 = img1.GetPixel(x % img1.Width, y % img1.Height);
                    var c2 = img2.GetPixel(x % img2.Width, y % img2.Height);
                    Color newColor;

                    switch (operation)
                    {
                        case ImageOperation.Sum:
                            var newR = (c1.R + c2.R) / 2;
                            var newG = (c1.G + c2.G) / 2;
                            var newB = (c1.B + c2.B) / 2;
                            newColor = Color.FromArgb(
                                Math.Min(newR, 255),
                                Math.Min(newG, 255),
                                Math.Min(newB, 255));
                            break;
                        case ImageOperation.Multiply:
                            newColor = Color.FromArgb(
                                c1.R * c2.R / 255,
                                c1.G * c2.G / 255,
                                c1.B * c2.B / 255);
                            break;
                        case ImageOperation.Average:
                            newColor = Color.FromArgb(
                                (c1.R + c2.R) / 2,
                                (c1.G + c2.G) / 2,
                                (c1.B + c2.B) / 2);
                            break;
                        case ImageOperation.Min:
                            newColor = Color.FromArgb(
                                Math.Min(c1.R, c2.R),
                                Math.Min(c1.G, c2.G),
                                Math.Min(c1.B, c2.B));
                            break;
                        case ImageOperation.Max:
                            newColor = Color.FromArgb(
                                Math.Max(c1.R, c2.R),
                                Math.Max(c1.G, c2.G),
                                Math.Max(c1.B, c2.B));
                            break;
                        case ImageOperation.Mask:
                            newColor = ApplyMask(c1, c2);
                            break;
                        default:
                            newColor = c1;
                            break;
                    }

                    intermediateResult.SetPixel(x, y, newColor);
                }

                return intermediateResult;

            case ImageOperation.Median:
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var c1 = img1.GetPixel(x % img1.Width, y % img1.Height);
                    var c2 = img2.GetPixel(x % img2.Width, y % img2.Height);
                    var avg = Color.FromArgb(
                        (c1.R + c2.R) / 2,
                        (c1.G + c2.G) / 2,
                        (c1.B + c2.B) / 2);
                    intermediateResult.SetPixel(x, y, avg);
                }

                var windowSize = (int)numWindowSize.Value | 1;
                return ApplyMedianFilter(intermediateResult, windowSize);

            case ImageOperation.Gaussian:
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var c1 = img1.GetPixel(x % img1.Width, y % img1.Height);
                    var c2 = img2.GetPixel(x % img2.Width, y % img2.Height);
                    var avg = Color.FromArgb(
                        (c1.R + c2.R) / 2,
                        (c1.G + c2.G) / 2,
                        (c1.B + c2.B) / 2);
                    intermediateResult.SetPixel(x, y, avg);
                }

                return ApplyGaussianBlur(intermediateResult, 3.0, 13);

            case ImageOperation.LowPassFilter:

                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var c1 = img1.GetPixel(x % img1.Width, y % img1.Height);
                    var c2 = img2.GetPixel(x % img2.Width, y % img2.Height);
                    var avg = Color.FromArgb(
                        (c1.R + c2.R) / 2,
                        (c1.G + c2.G) / 2,
                        (c1.B + c2.B) / 2);
                    intermediateResult.SetPixel(x, y, avg);
                }

                var freqData =
                    FourierTransform.Fourier2DTransform(FourierTransform.ConvertToComplex(intermediateResult), width,
                        height);
                var maxMagnitude = 0.0;
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    maxMagnitude = Math.Max(maxMagnitude, freqData[x, y].Magnitude);
                var spectrumImage = FourierTransform.VisualizeSpectrum(freqData, width, height);
                pictureBoxFourier.Image = spectrumImage;
                pictureBoxFourier.Refresh();

                return FourierTransform.ApplyFilter(
                    intermediateResult,
                    (dx, dy) =>
                    {
                        var sigma = (double)numLowPassSigma.Value;

                        return Math.Exp(-(dx * dx + dy * dy) / (2 * sigma * sigma));
                    });

            case ImageOperation.HighPassFilter:
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var c1 = img1.GetPixel(x % img1.Width, y % img1.Height);
                    var c2 = img2.GetPixel(x % img2.Width, y % img2.Height);
                    var avg = Color.FromArgb(
                        (c1.R + c2.R) / 2,
                        (c1.G + c2.G) / 2,
                        (c1.B + c2.B) / 2);
                    intermediateResult.SetPixel(x, y, avg);
                }


                freqData = FourierTransform.Fourier2DTransform(FourierTransform.ConvertToComplex(intermediateResult),
                    width, height);
                maxMagnitude = 0.0;
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    maxMagnitude = Math.Max(maxMagnitude, freqData[x, y].Magnitude);
                spectrumImage = FourierTransform.VisualizeSpectrum(freqData, width, height);
                pictureBoxFourier.Image = spectrumImage;
                pictureBoxFourier.Refresh();

                return FourierTransform.ApplyFilter(
                    intermediateResult,
                    (dx, dy) =>
                    {
                        var sigma = (double)numLowPassSigma.Value;
                        return 1 - Math.Exp(-(dx * dx + dy * dy) / (2 * sigma * sigma));
                    });

            default:
                return img1;
        }
    }

    private Color ApplyMask(Color pixel, Color mask)
    {
        var alpha = 128;
        return Color.FromArgb(
            (pixel.R * alpha + mask.R * (255 - alpha)) / 255,
            (pixel.G * alpha + mask.G * (255 - alpha)) / 255,
            (pixel.B * alpha + mask.B * (255 - alpha)) / 255);
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        if (pictureBoxResult.Image == null)
        {
            MessageBox.Show("Нет изображения для сохранения.");
            return;
        }

        var saveFile = new SaveFileDialog { Filter = "PNG Files|*.png" };
        if (saveFile.ShowDialog() == DialogResult.OK)
        {
            pictureBoxResult.Image.Save(saveFile.FileName, ImageFormat.Png);
            MessageBox.Show("Изображение сохранено.");
        }
    }

    private int[] CalculateHistogram(Bitmap image)
    {
        var histogram = new int[256];

        for (var x = 0; x < image.Width; x++)
        for (var y = 0; y < image.Height; y++)
        {
            var pixel = image.GetPixel(x, y);
            var brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B); // Яркость
            histogram[brightness]++;
        }

        return histogram;
    }

    private Bitmap DrawHistogram(int[] histogram)
    {
        var histWidth = pictureBoxHistogram.Width; // Используем ширину pictureBox
        var histHeight = pictureBoxHistogram.Height;
        var histogramImage = new Bitmap(histWidth, histHeight);

        var max = histogram.Max(); // Находим максимальное значение для масштабирования

        using (var g = Graphics.FromImage(histogramImage))
        {
            g.Clear(Color.White);

            for (var i = 0; i < 256; i++)
            {
                // Преобразуем координаты, чтобы гистограмма растянулась по всей ширине pictureBox
                var x = (int)(i / 255.0 * (histWidth - 1));
                var barHeight = (int)(histogram[i] / (float)max * histHeight);

                g.DrawLine(Pens.Black, x, histHeight, x, histHeight - barHeight);
            }
        }

        return histogramImage;
    }


    private void BtnAddPoint_Click(object sender, EventArgs e)
    {
        if (float.TryParse(txtPointX.Text, out var x) && float.TryParse(txtPointY.Text, out var y))
        {
            points.Add(new PointF(x, y));
            DrawCurve();
            if (pictureBoxResult.Image != null)
                pictureBoxResult.Image = ApplyInterpolation((Bitmap)pictureBoxResult.Image);
        }
    }

    private void DrawCurve()
    {
        if (points.Count < 2) return;

        var width = pictureBoxHistogram.Width;
        var height = pictureBoxHistogram.Height;

        var graph = new Bitmap(width, height);
        using (var g = Graphics.FromImage(graph))
        {
            g.Clear(Color.White);
            var pen = new Pen(Color.Black, 2);

            // Убедимся, что точки отсортированы по X для правильной интерполяции
            points.Sort((a, b) => a.X.CompareTo(b.X));

            var curvePoints = new PointF[width];
            for (var i = 0; i < width; i++)
            {
                var xValue = i / (float)width * 255; // Преобразуем в диапазон [0, 255]
                var yValue = Interpolate(xValue);

                // Масштабируем yValue в диапазон [0, height]
                var yPos = height - yValue / 255 * height; // Инверсия оси Y

                curvePoints[i] = new PointF(i, yPos);
            }

            g.DrawLines(pen, curvePoints);
        }

        pictureBoxHistogram.Image = graph;
    }


    private float Interpolate(float x)
    {
        if (points.Count < 2) return Math.Clamp(x, 0, 255);

        points.Sort((a, b) => a.X.CompareTo(b.X));

        for (var i = 0; i < points.Count - 1; i++)
            if (x >= points[i].X && x <= points[i + 1].X)
            {
                float x1 = points[i].X, y1 = points[i].Y;
                float x2 = points[i + 1].X, y2 = points[i + 1].Y;

                if (x1 == x2) return Math.Clamp(y1, 0, 255); // Защита от деления на 0

                var t = (x - x1) / (x2 - x1);
                var interpolatedValue = y1 + t * (y2 - y1);

                return Math.Clamp(interpolatedValue, 0, 255); // Ограничиваем результат
            }

        return Math.Clamp(x, 0, 255); // Если x за пределами точек, просто ограничиваем
    }


    private Bitmap ApplyInterpolation(Bitmap img)
    {
        var result = new Bitmap(img.Width, img.Height);

        for (var x = 0; x < img.Width; x++)
        for (var y = 0; y < img.Height; y++)
        {
            var pixel = img.GetPixel(x, y);
            var brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B); // Яркость пикселя

            var newBrightness = Interpolate(brightness);

            if (newBrightness < 0 || newBrightness > 255)
                Console.WriteLine($"Яркость вне приемлемого диапазона: {newBrightness}");

            // Ограничиваем яркость от 0 до 255
            newBrightness = Math.Clamp(newBrightness, 0, 255);

            var newColor = Color.FromArgb((int)newBrightness, (int)newBrightness, (int)newBrightness);

            result.SetPixel(x, y, newColor);
        }

        return result;
    }

    private Bitmap BradleyThresholding(Bitmap img, int S, double threshold)
    {
        var width = img.Width;
        var height = img.Height;
        var result = new Bitmap(width, height);
        var integralImage = new int[width, height];

        // Заполняем суммированную матрицу
        for (var y = 0; y < height; y++)
        {
            var sum = 0;
            for (var x = 0; x < width; x++)
            {
                var pixel = img.GetPixel(x, y);
                var brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                sum += brightness;
                integralImage[x, y] = sum + (y > 0 ? integralImage[x, y - 1] : 0);
            }
        }

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var x1 = Math.Max(x - S / 2, 0);
            var x2 = Math.Min(x + S / 2, width - 1);
            var y1 = Math.Max(y - S / 2, 0);
            var y2 = Math.Min(y + S / 2, height - 1);

            var count = (x2 - x1) * (y2 - y1);
            var sum = integralImage[x2, y2] - (x1 > 0 ? integralImage[x1 - 1, y2] : 0)
                                            - (y1 > 0 ? integralImage[x2, y1 - 1] : 0)
                      + (x1 > 0 && y1 > 0 ? integralImage[x1 - 1, y1 - 1] : 0);

            var brightness = (int)(0.299 * img.GetPixel(x, y).R + 0.587 * img.GetPixel(x, y).G +
                                   0.114 * img.GetPixel(x, y).B);
            if (brightness * count < sum * threshold)
                result.SetPixel(x, y, Color.Black);
            else
                result.SetPixel(x, y, Color.White);
        }

        return result;
    }


    private void BtnBinarize_Click(object sender, EventArgs e)
    {
        if (pictureBoxResult.Image == null)
        {
            MessageBox.Show("Нет изображения для бинаризации.");
            return;
        }

        var inputImage = new Bitmap(pictureBoxResult.Image);

        var S = (int)(Math.Max(inputImage.Width, inputImage.Height) * (numWindowSize.Value / 100));
        var threshold = (double)numThreshold.Value;

        pictureBoxResult.Image = BradleyThresholding(inputImage, S, threshold);
    }

    /*private Bitmap ApplyMedianFilter(Bitmap source, int windowSize)
    {
        int radius = windowSize / 2;
        Bitmap result = new Bitmap(source.Width, source.Height);

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                List<byte> rList = new List<byte>();
                List<byte> gList = new List<byte>();
                List<byte> bList = new List<byte>();

                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int px = Reflect(x + dx, source.Width);
                        int py = Reflect(y + dy, source.Height);
                        Color pixel = source.GetPixel(px, py);

                        rList.Add(pixel.R);
                        gList.Add(pixel.G);
                        bList.Add(pixel.B);
                    }
                }

                rList.Sort();
                gList.Sort();
                bList.Sort();

                int mid = rList.Count / 2;
                Color median = Color.FromArgb(rList[mid], gList[mid], bList[mid]);
                result.SetPixel(x, y, median);
            }
        }

        return result;
    }*/

    private Bitmap ApplyMedianFilter(Bitmap source, int windowSize)
    {
        var radius = windowSize / 2;
        var width = source.Width;
        var height = source.Height;

        var result = new Bitmap(width, height);

        var srcData = source.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        var dstData = result.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

        var stride = srcData.Stride;
        var bytes = stride * height;
        var srcBuffer = new byte[bytes];
        var dstBuffer = new byte[bytes];

        Marshal.Copy(srcData.Scan0, srcBuffer, 0, bytes);

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var rList = new List<byte>();
            var gList = new List<byte>();
            var bList = new List<byte>();

            for (var dy = -radius; dy <= radius; dy++)
            {
                var py = Math.Clamp(y + dy, 0, height - 1);

                for (var dx = -radius; dx <= radius; dx++)
                {
                    var px = Math.Clamp(x + dx, 0, width - 1);
                    var index = py * stride + px * 3;

                    bList.Add(srcBuffer[index]);
                    gList.Add(srcBuffer[index + 1]);
                    rList.Add(srcBuffer[index + 2]);
                }
            }

            rList.Sort();
            gList.Sort();
            bList.Sort();
            var mid = rList.Count / 2;

            var dstIndex = y * stride + x * 3;
            dstBuffer[dstIndex] = bList[mid];
            dstBuffer[dstIndex + 1] = gList[mid];
            dstBuffer[dstIndex + 2] = rList[mid];
        }

        Marshal.Copy(dstBuffer, 0, dstData.Scan0, bytes);
        source.UnlockBits(srcData);
        result.UnlockBits(dstData);
        return result;
    }

    private int Reflect(int i, int max)
    {
        if (i < 0) return -i;
        if (i >= max) return max - (i - max + 1);
        return i;
    }

    /*private Bitmap ApplyGaussianBlur(Bitmap source, double sigma, int kernelSize)
    {
        float[,] kernel = CreateGaussianKernel(kernelSize, sigma);
        int radius = kernelSize / 2;
        Bitmap result = new Bitmap(source.Width, source.Height);

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                float r = 0, g = 0, b = 0;

                for (int ky = -radius; ky <= radius; ky++)
                {
                    for (int kx = -radius; kx <= radius; kx++)
                    {
                        int px = Reflect(x + kx, source.Width);
                        int py = Reflect(y + ky, source.Height);

                        Color color = source.GetPixel(px, py);
                        float k = kernel[kx + radius, ky + radius];

                        r += color.R * k;
                        g += color.G * k;
                        b += color.B * k;
                    }
                }

                Color blurred = Color.FromArgb(
                    Math.Clamp((int)r, 0, 255),
                    Math.Clamp((int)g, 0, 255),
                    Math.Clamp((int)b, 0, 255)
                );
                result.SetPixel(x, y, blurred);
            }
        }

        return result;
    }*/

    private Bitmap ApplyGaussianBlur(Bitmap source, double sigma, int kernelSize)
    {
        var width = source.Width;
        var height = source.Height;
        var radius = kernelSize / 2;
        var kernel = CreateGaussianKernel(kernelSize, sigma);

        var result = new Bitmap(width, height);

        // Быстрый доступ к байтам
        var srcData = source.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);

        var dstData = result.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format24bppRgb);

        var stride = srcData.Stride;
        var srcScan0 = srcData.Scan0;
        var dstScan0 = dstData.Scan0;

        unsafe
        {
            var srcPtr = (byte*)srcScan0;
            var dstPtr = (byte*)dstScan0;

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                float sumR = 0, sumG = 0, sumB = 0;

                for (var ky = -radius; ky <= radius; ky++)
                {
                    var py = Math.Clamp(y + ky, 0, height - 1);

                    for (var kx = -radius; kx <= radius; kx++)
                    {
                        var px = Math.Clamp(x + kx, 0, width - 1);
                        var k = kernel[kx + radius, ky + radius];

                        var pixel = srcPtr + py * stride + px * 3;

                        sumB += pixel[0] * k;
                        sumG += pixel[1] * k;
                        sumR += pixel[2] * k;
                    }
                }

                var resultPixel = dstPtr + y * stride + x * 3;
                resultPixel[0] = (byte)Math.Clamp((int)sumB, 0, 255);
                resultPixel[1] = (byte)Math.Clamp((int)sumG, 0, 255);
                resultPixel[2] = (byte)Math.Clamp((int)sumR, 0, 255);
            }
        }

        source.UnlockBits(srcData);
        result.UnlockBits(dstData);
        return result;
    }

    private float[,] CreateGaussianKernel(int size, double sigma)
    {
        var kernel = new float[size, size];
        var radius = size / 2;
        var sigma2 = 2 * sigma * sigma;
        double sum = 0;

        for (var y = -radius; y <= radius; y++)
        for (var x = -radius; x <= radius; x++)
        {
            var value = Math.Exp(-(x * x + y * y) / sigma2);
            kernel[x + radius, y + radius] = (float)value;
            sum += value;
        }

        // Нормализация
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            kernel[x, y] /= (float)sum;

        return kernel;
    }

    #region

    private readonly List<Bitmap> loadedImages = new();
    private Button btnAddImage;
    private ComboBox comboBoxOperations;
    private PictureBox pictureBoxResult;
    private PictureBox pictureBoxFourier;
    private readonly List<PointF> points = new();

    private TextBox txtPointX;
    private TextBox txtPointY;
    private Button btnAddPoint;
    private Button btnBinarize;
    private NumericUpDown numWindowSize;
    private NumericUpDown numThreshold;
    private NumericUpDown numSigma;
    private NumericUpDown numKernelSize;
    private NumericUpDown numLowPassSigma;

    #endregion
}