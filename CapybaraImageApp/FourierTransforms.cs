using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class FourierTransform
{
    public static Bitmap ApplyFilter(Bitmap img, Func<int, int, double> frequencyMask)
    {
        int width = img.Width;
        int height = img.Height;

        // 1. Преобразование изображения в комплексную матрицу
        Complex[,] imageData = ConvertToComplex(img);

        // 2. Выполнение прямого 2D БПФ
        Complex[,] freqData = Fourier2DTransform(imageData, width, height);

        // 3. Применение маски-фильтра в частотной области
        Complex[,] filteredFreqData = ApplyFrequencyMask(freqData, frequencyMask);

        // 4. Обратное БПФ
        Complex[,] spatialData = InverseFourier2D(filteredFreqData, width, height);

        // 5. Нормализация и преобразование обратно в изображение
        double max = 0;
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            max = Math.Max(max, spatialData[x, y].Magnitude);

        if (max < 1e-5)
        {
            Console.WriteLine("Warning: Resulting image intensity is too small!");
            max = 1; // Принудительная нормализация, чтобы избежать черноты
        }

        Bitmap result = new Bitmap(width, height);
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            double intensity = spatialData[x, y].Magnitude / max;
            int gray = (int)(255 * intensity);
            gray = Math.Min(255, Math.Max(0, gray));
            result.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
        }

        return result;
    }
    
    //медленно работающая версия
    
    /*public static Bitmap VisualizeSpectrum(Complex[,] freqData, int width, int height)
    {
        // Создаем изображение для визуализации спектра
        Bitmap spectrumImage = new Bitmap(width, height);

        // Применение смещения спектра
        ShiftSpectrum(freqData);

        // Вычисление амплитуды для каждого пикселя
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Амплитуда спектра = Модуль комплексного числа
                double magnitude = freqData[x, y].Magnitude;

                // Нормализация амплитуды для отображения
                int intensity = (int)(255 * Math.Log(1 + magnitude)); // Используем логарифмическую шкалу
                intensity = Math.Min(255, Math.Max(0, intensity));

                spectrumImage.SetPixel(x, y, Color.FromArgb(intensity, intensity, intensity));
            }
        }

        return spectrumImage;
    }*/
    
    public static Bitmap VisualizeSpectrum(Complex[,] freqData, int width, int height)
    {
        ShiftSpectrum(freqData);

        Bitmap spectrumImage = new Bitmap(width, height);
        BitmapData bitmapData = spectrumImage.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);

        IntPtr ptr = bitmapData.Scan0;
        int bytes = Math.Abs(bitmapData.Stride) * height;
        byte[] rgbValues = new byte[bytes];
        
        double maxMagnitude = 0.0;
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            maxMagnitude = Math.Max(maxMagnitude, freqData[x, y].Magnitude);

        // --- Заполняем пиксельные данные ---
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double magnitude = freqData[x, y].Magnitude;
                double scaled = Math.Log(1 + magnitude) / Math.Log(1 + maxMagnitude);
                int intensity = (int)(scaled * 255);
                intensity = Math.Min(255, Math.Max(0, intensity));

                int index = y * bitmapData.Stride + x * 4;
                rgbValues[index] = (byte)intensity;        // B
                rgbValues[index + 1] = (byte)intensity;    // G
                rgbValues[index + 2] = (byte)intensity;    // R
                rgbValues[index + 3] = 255;                // A
            }
        }

        Marshal.Copy(rgbValues, 0, ptr, bytes);
        spectrumImage.UnlockBits(bitmapData);

        return spectrumImage;
    }
    
    public static Bitmap PassThrough(Bitmap img)
    {
        int w = img.Width, h = img.Height;
        Complex[,] data = ConvertToComplex(img);
        Complex[,] freq = Fourier2DTransform(data, w, h);
        Complex[,] inv = InverseFourier2D(freq, w, h);

        // Преобразуем обратно в Bitmap
        Bitmap res = new Bitmap(w, h);
        double max = 0;
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
            max = Math.Max(max, inv[x, y].Magnitude);

        if (max == 0) max = 1;
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            double v = inv[x, y].Magnitude / max;
            int gray = (int)(255 * v);
            res.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
        }

        return res;
    }

    public static Complex[,] ConvertToComplex(Bitmap img)
    {
        int width = img.Width;
        int height = img.Height;
        Complex[,] data = new Complex[width, height];

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            Color color = img.GetPixel(x, y);
            double intensity = (color.R + color.G + color.B) / 3.0;
            data[x, y] = new Complex(intensity, 0);
        }

        return data;
    }

    private static void ShiftSpectrum(Complex[,] data)
    {
        int width = data.GetLength(0);
        int height = data.GetLength(1);

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            if ((x + y) % 2 != 0)
                data[x, y] = -data[x, y];
    }

    private static Complex[,] ApplyFrequencyMask(Complex[,] freqData, Func<int, int, double> maskFunc)
    {
        int width = freqData.GetLength(0);
        int height = freqData.GetLength(1);
        Complex[,] result = new Complex[width, height];
        int cx = width / 2;
        int cy = height / 2;

        for (int x = 0; x < width; x++)
        {
            int dx = x - cx;
            for (int y = 0; y < height; y++)
            {
                int dy = y - cy;
                double mask = maskFunc(dx, dy);
                result[x, y] = freqData[x, y] * mask;
            }
        }

        return result;
    }

    public static Complex[,] Fourier2DTransform(Complex[,] data, int width, int height)
    {
        Complex[,] result = (Complex[,])data.Clone();

        Parallel.For(0, height, y =>
        {
            Complex[] row = new Complex[width];
            for (int x = 0; x < width; x++) row[x] = result[x, y];
            row = FFT(row);
            for (int x = 0; x < width; x++) result[x, y] = row[x];
        });

        Parallel.For(0, width, x =>
        {
            Complex[] col = new Complex[height];
            for (int y = 0; y < height; y++) col[y] = result[x, y];
            col = FFT(col);
            for (int y = 0; y < height; y++) result[x, y] = col[y];
        });

        return result;
    }

    private static Complex[,] InverseFourier2D(Complex[,] data, int width, int height)
    {
        Complex[,] result = (Complex[,])data.Clone();

        Parallel.For(0, height, y =>
        {
            Complex[] row = new Complex[width];
            for (int x = 0; x < width; x++) row[x] = result[x, y];
            row = FFTInverse(row);
            for (int x = 0; x < width; x++) result[x, y] = row[x];
        });

        Parallel.For(0, width, x =>
        {
            Complex[] col = new Complex[height];
            for (int y = 0; y < height; y++) col[y] = result[x, y];
            col = FFTInverse(col);
            for (int y = 0; y < height; y++) result[x, y] = col[y];
        });

        // Нормализация
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            result[x, y] /= (width * height);

        return result;
    }

    private static Complex[] FFT(Complex[] data)
    {
        int n = data.Length;
        if (n <= 1) return data;

        Complex[] even = new Complex[n / 2];
        Complex[] odd = new Complex[n / 2];
        for (int i = 0; i < n / 2; i++)
        {
            even[i] = data[2 * i];
            odd[i] = data[2 * i + 1];
        }

        even = FFT(even);
        odd = FFT(odd);

        Complex[] result = new Complex[n];
        for (int k = 0; k < n / 2; k++)
        {
            Complex t = Complex.Exp(new Complex(0, -2 * Math.PI * k / n)) * odd[k];
            result[k] = even[k] + t;
            result[k + n / 2] = even[k] - t;
        }
        return result;
    }

    private static Complex[] FFTInverse(Complex[] data)
    {
        int n = data.Length;
        if (n <= 1) return data;

        Complex[] even = new Complex[n / 2];
        Complex[] odd = new Complex[n / 2];
        for (int i = 0; i < n / 2; i++)
        {
            even[i] = data[2 * i];
            odd[i] = data[2 * i + 1];
        }

        even = FFTInverse(even);
        odd = FFTInverse(odd);

        Complex[] result = new Complex[n];
        for (int k = 0; k < n / 2; k++)
        {
            Complex t = Complex.Exp(new Complex(0, 2 * Math.PI * k / n)) * odd[k];
            result[k] = even[k] + t;
            result[k + n / 2] = even[k] - t;
        }

        return result;
    }
}
