using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CapybaraImageApp.Models;

namespace CapybaraImageApp
{
    public partial class Form1 : Form
    {
        private List<Bitmap> loadedImages = new List<Bitmap>();
  private Button btnAddImage;
        private ComboBox comboBoxOperations;
        private System.Windows.Forms.PictureBox pictureBoxResult;
        private List<PointF> points = new List<PointF>();

        private TextBox txtPointX;
        private TextBox txtPointY;
        private Button btnAddPoint;
        
        
        public Form1()
        {
            InitializeComponent();
            comboBoxOperations.DataSource = Enum.GetValues(typeof(ImageOperation));
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // Настройки формы
            ClientSize = new System.Drawing.Size(850, 500);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Text = "Capybara Image App";

            // --- Основное изображение ---
            pictureBoxResult = new System.Windows.Forms.PictureBox();
            pictureBoxResult.Location = new System.Drawing.Point(50, 20);
            pictureBoxResult.Size = new System.Drawing.Size(400, 300);
            pictureBoxResult.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(pictureBoxResult);

            // --- Гистограмма ---
            pictureBoxHistogram = new System.Windows.Forms.PictureBox();
            pictureBoxHistogram.Location = new System.Drawing.Point(50, 330);
            pictureBoxHistogram.Size = new System.Drawing.Size(400, 100);
            pictureBoxHistogram.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(pictureBoxHistogram);

            // --- Панель изображений ---
            panelImages = new System.Windows.Forms.FlowLayoutPanel();
            panelImages.Location = new System.Drawing.Point(500, 20);
            panelImages.Size = new System.Drawing.Size(300, 100);
            panelImages.AutoScroll = false;
            panelImages.WrapContents = false;
            panelImages.FlowDirection = FlowDirection.LeftToRight;
            Controls.Add(panelImages);

            // --- Выпадающий список операций ---
            comboBoxOperations = new System.Windows.Forms.ComboBox();
            comboBoxOperations.Location = new System.Drawing.Point(500, 140);
            comboBoxOperations.Size = new System.Drawing.Size(180, 25);
            Controls.Add(comboBoxOperations);

            // --- Кнопка "Добавить изображение" ---
            btnAddImage = new System.Windows.Forms.Button();
            btnAddImage.Location = new System.Drawing.Point(500, 180);
            btnAddImage.Size = new System.Drawing.Size(180, 30);
            btnAddImage.Text = "Добавить изображение";
            btnAddImage.UseVisualStyleBackColor = true;
            btnAddImage.Click += BtnAddImage_Click;
            Controls.Add(btnAddImage);

            // --- Кнопка "Обработать" ---
            btnProcess = new System.Windows.Forms.Button();
            btnProcess.Location = new System.Drawing.Point(500, 220);
            btnProcess.Size = new System.Drawing.Size(180, 30);
            btnProcess.Text = "Обработать";
            btnProcess.UseVisualStyleBackColor = true;
            btnProcess.Click += BtnProcess_Click;
            Controls.Add(btnProcess);

            // --- Кнопка "Сохранить" ---
            btnSave = new System.Windows.Forms.Button();
            btnSave.Location = new System.Drawing.Point(500, 260);
            btnSave.Size = new System.Drawing.Size(180, 30);
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            // --- Поля для ввода точек интерполяции ---
            txtPointX = new TextBox { Location = new Point(500, 310), Width = 50 };
            txtPointY = new TextBox { Location = new Point(560, 310), Width = 50 };
            Controls.Add(txtPointX);
            Controls.Add(txtPointY);

            // --- Кнопка "Добавить точку" ---
            btnAddPoint = new Button();
            btnAddPoint.Location = new Point(620, 310);
            btnAddPoint.Size = new Size(120, 30);
            btnAddPoint.Text = "Добавить точку";
            btnAddPoint.Click += BtnAddPoint_Click;
            Controls.Add(btnAddPoint);
        }

        private System.Windows.Forms.Button btnSave;

        private System.Windows.Forms.FlowLayoutPanel panelImages;

        private System.Windows.Forms.Button btnProcess;
        
        private System.Windows.Forms.PictureBox pictureBoxHistogram;


        private void BtnAddImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                Bitmap newImage = new Bitmap(openFile.FileName);
                loadedImages.Add(newImage);

                PictureBox newPictureBox = new PictureBox
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
            Bitmap resized = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
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

            ImageOperation selectedOperation = (ImageOperation)comboBoxOperations.SelectedItem;
            Bitmap result = new Bitmap(loadedImages[0]);

            for (int i = 1; i < loadedImages.Count; i++)
            {
                Bitmap resizedImg2 = ResizeImage(loadedImages[i], result.Width, result.Height);
                result = ProcessImages(result, resizedImg2, selectedOperation);
            }

            // Масштабируем изображение, чтобы оно поместилось в pictureBoxResult
            int maxWidth = pictureBoxResult.Width;
            int maxHeight = pictureBoxResult.Height;
            float scale = Math.Min((float)maxWidth / result.Width, (float)maxHeight / result.Height);
            int newWidth = (int)(result.Width * scale);
            int newHeight = (int)(result.Height * scale);

            Bitmap resizedImage = new Bitmap(result, newWidth, newHeight);

            // Обновляем изображение
            pictureBoxResult.Image = resizedImage;

            // Вычисляем гистограмму
            int[] histogramData = CalculateHistogram(result);

            // Проверяем данные
            if (histogramData.Sum() == 0)
            {
                MessageBox.Show("Гистограмма пустая. Возможно, изображение полностью белое.");
            }
            else
            {
                pictureBoxHistogram.Image = DrawHistogram(histogramData);
            }


        }

        private Bitmap ProcessImages(Bitmap img1, Bitmap img2, ImageOperation operation)
        {
            int width = Math.Max(img1.Width, img2.Width);
            int height = Math.Max(img1.Height, img2.Height);
            Bitmap result = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color c1 = img1.GetPixel(x % img1.Width, y % img1.Height);
                    Color c2 = img2.GetPixel(x % img2.Width, y % img2.Height);
                    Color newColor;

                    switch (operation)
                    {
                        case ImageOperation.Sum:
                        int newR = c1.R + c2.R;
                        int newG = c1.G + c2.G;
                        int newB = c1.B + c2.B;

                        // Нормализация яркости
                        newR = (int)(newR / 2.0);
                        newG = (int)(newG / 2.0);
                        newB = (int)(newB / 2.0);

                        newColor = Color.FromArgb(
                            Math.Min(newR, 255),
                            Math.Min(newG, 255),
                            Math.Min(newB, 255)
                        );
                        break;
                        case ImageOperation.Multiply:
                            newColor = Color.FromArgb(
                                (c1.R * c2.R) / 255,
                                (c1.G * c2.G) / 255,
                                (c1.B * c2.B) / 255);
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
                    result.SetPixel(x, y, newColor);
                }
            }
            return result;
        }

        private Color ApplyMask(Color pixel, Color mask)
        {
            int alpha = 128;
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

            SaveFileDialog saveFile = new SaveFileDialog { Filter = "PNG Files|*.png" };
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                pictureBoxResult.Image.Save(saveFile.FileName, ImageFormat.Png);
                MessageBox.Show("Изображение сохранено.");
            }
        }
        
        private int[] CalculateHistogram(Bitmap image)
        {
            int[] histogram = new int[256];

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B); // Яркость
                    histogram[brightness]++;
                }
            }
            return histogram;
        }
        
        private Bitmap DrawHistogram(int[] histogram)
        {
            int histWidth = pictureBoxHistogram.Width;  // Используем ширину pictureBox
            int histHeight = pictureBoxHistogram.Height;
            Bitmap histogramImage = new Bitmap(histWidth, histHeight);

            int max = histogram.Max(); // Находим максимальное значение для масштабирования

            using (Graphics g = Graphics.FromImage(histogramImage))
            {
                g.Clear(Color.White);

                for (int i = 0; i < 256; i++)
                {
                    // Преобразуем координаты, чтобы гистограмма растянулась по всей ширине pictureBox
                    int x = (int)((i / 255.0) * (histWidth - 1));
                    int barHeight = (int)((histogram[i] / (float)max) * histHeight);

                    g.DrawLine(Pens.Black, x, histHeight, x, histHeight - barHeight);
                }
            }

            return histogramImage;
        }

        
        private void BtnAddPoint_Click(object sender, EventArgs e)
        {
            if (float.TryParse(txtPointX.Text, out float x) && float.TryParse(txtPointY.Text, out float y))
            {
                points.Add(new PointF(x, y));
                DrawCurve();
                if (pictureBoxResult.Image != null)
                {
                    pictureBoxResult.Image = ApplyInterpolation((Bitmap)pictureBoxResult.Image);
                }
            }
        }

        private void DrawCurve()
        {
            if (points.Count < 2) return;

            int width = pictureBoxHistogram.Width;
            int height = pictureBoxHistogram.Height;

            Bitmap graph = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(graph))
            {
                g.Clear(Color.White);
                Pen pen = new Pen(Color.Black, 2);

                // Убедимся, что точки отсортированы по X для правильной интерполяции
                points.Sort((a, b) => a.X.CompareTo(b.X));

                // Массив для хранения точек кривой
                PointF[] curvePoints = new PointF[width];

                // Перебор всех возможных значений по X (от 0 до width)
                for (int i = 0; i < width; i++)
                {
                    float xValue = (i / (float)width) * 255; // Преобразуем в диапазон [0, 255]
                    float yValue = Interpolate(xValue);

                    // Масштабируем yValue в диапазон [0, height]
                    float yPos = height - (yValue / 255 * height); // Инверсия оси Y

                    curvePoints[i] = new PointF(i, yPos);
                }

                // Рисуем линию
                g.DrawLines(pen, curvePoints);
            }

            pictureBoxHistogram.Image = graph;
        }


        private float Interpolate(float x)
        {
            if (points.Count < 2) return Math.Clamp(x, 0, 255); // Ограничиваем x

            points.Sort((a, b) => a.X.CompareTo(b.X));

            for (int i = 0; i < points.Count - 1; i++)
            {
                if (x >= points[i].X && x <= points[i + 1].X)
                {
                    float x1 = points[i].X, y1 = points[i].Y;
                    float x2 = points[i + 1].X, y2 = points[i + 1].Y;

                    if (x1 == x2) return Math.Clamp(y1, 0, 255); // Защита от деления на 0

                    float t = (x - x1) / (x2 - x1);
                    float interpolatedValue = y1 + t * (y2 - y1);

                    return Math.Clamp(interpolatedValue, 0, 255); // Ограничиваем результат
                }
            }

            return Math.Clamp(x, 0, 255); // Если x за пределами точек, просто ограничиваем
        }

        
        private Bitmap ApplyInterpolation(Bitmap img)
        {
            Bitmap result = new Bitmap(img.Width, img.Height);

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Color pixel = img.GetPixel(x, y);
                    int brightness = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B); // Яркость пикселя

                    // Применяем интерполяцию на основе кривой
                    float newBrightness = Interpolate(brightness);

                    // Проверяем, не ушло ли значение в недопустимый диапазон
                    if (newBrightness < 0 || newBrightness > 255)
                    {
                        Console.WriteLine($"Яркость вне приемлемого диапазона: {newBrightness}");
                    }

                    // Ограничиваем яркость от 0 до 255
                    newBrightness = Math.Clamp(newBrightness, 0, 255);

                    // Создаем новый цвет
                    Color newColor = Color.FromArgb((int)newBrightness, (int)newBrightness, (int)newBrightness);

                    result.SetPixel(x, y, newColor);
                }
            }

            return result;
        }

    }
}
