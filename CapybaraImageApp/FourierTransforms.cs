using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

public class FourierTransform
{
    public static Bitmap ApplyFilter(Bitmap img, Func<int, int, double> frequencyMask)
    {
        var width = img.Width;
        var height = img.Height;

        // 1. Преобразование изображения в комплексную матрицу
        var imageData = ConvertToComplex(img);

        // 2. Выполнение прямого 2D БПФ
        var freqData = Fourier2DTransform(imageData, width, height);

        // 3. Применение маски-фильтра в частотной области
        var filteredFreqData = ApplyFrequencyMask(freqData, frequencyMask);

        // 4. Обратное БПФ
        var spatialData = InverseFourier2D(filteredFreqData, width, height);

        // 5. Нормализация и преобразование обратно в изображение
        double max = 0;
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            max = Math.Max(max, spatialData[x, y].Magnitude);

        if (max < 1e-5)
        {
            Console.WriteLine("Warning: Resulting image intensity is too small!");
            max = 1; // Принудительная нормализация, чтобы избежать черноты
        }

        var result = new Bitmap(width, height);
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var intensity = spatialData[x, y].Magnitude / max;
            var gray = (int)(255 * intensity);
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

        var spectrumImage = new Bitmap(width, height);
        var bitmapData = spectrumImage.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);

        var ptr = bitmapData.Scan0;
        var bytes = Math.Abs(bitmapData.Stride) * height;
        var rgbValues = new byte[bytes];

        var maxMagnitude = 0.0;
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            maxMagnitude = Math.Max(maxMagnitude, freqData[x, y].Magnitude);

        // --- Заполняем пиксельные данные ---
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var magnitude = freqData[x, y].Magnitude;
            var scaled = Math.Log(1 + magnitude) / Math.Log(1 + maxMagnitude);
            var intensity = (int)(scaled * 255);
            intensity = Math.Min(255, Math.Max(0, intensity));

            var index = y * bitmapData.Stride + x * 4;
            rgbValues[index] = (byte)intensity; // B
            rgbValues[index + 1] = (byte)intensity; // G
            rgbValues[index + 2] = (byte)intensity; // R
            rgbValues[index + 3] = 255; // A
        }

        Marshal.Copy(rgbValues, 0, ptr, bytes);
        spectrumImage.UnlockBits(bitmapData);

        return spectrumImage;
    }

    public static Bitmap PassThrough(Bitmap img)
    {
        int w = img.Width, h = img.Height;
        var data = ConvertToComplex(img);
        var freq = Fourier2DTransform(data, w, h);
        var inv = InverseFourier2D(freq, w, h);

        // Преобразуем обратно в Bitmap
        var res = new Bitmap(w, h);
        double max = 0;
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
            max = Math.Max(max, inv[x, y].Magnitude);

        if (max == 0) max = 1;
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
        {
            var v = inv[x, y].Magnitude / max;
            var gray = (int)(255 * v);
            res.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
        }

        return res;
    }

    public static Complex[,] ConvertToComplex(Bitmap img)
    {
        var width = img.Width;
        var height = img.Height;
        var data = new Complex[width, height];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var color = img.GetPixel(x, y);
            var intensity = (color.R + color.G + color.B) / 3.0;
            data[x, y] = new Complex(intensity, 0);
        }

        return data;
    }

    private static void ShiftSpectrum(Complex[,] data)
    {
        var width = data.GetLength(0);
        var height = data.GetLength(1);

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            if ((x + y) % 2 != 0)
                data[x, y] = -data[x, y];
    }

    private static Complex[,] ApplyFrequencyMask(Complex[,] freqData, Func<int, int, double> maskFunc)
    {
        var width = freqData.GetLength(0);
        var height = freqData.GetLength(1);
        var result = new Complex[width, height];
        var cx = width / 2;
        var cy = height / 2;

        for (var x = 0; x < width; x++)
        {
            var dx = x - cx;
            for (var y = 0; y < height; y++)
            {
                var dy = y - cy;
                var mask = maskFunc(dx, dy);
                result[x, y] = freqData[x, y] * mask;
            }
        }

        return result;
    }

    public static Complex[,] Fourier2DTransform(Complex[,] data, int width, int height)
    {
        var result = (Complex[,])data.Clone();

        Parallel.For(0, height, y =>
        {
            var row = new Complex[width];
            for (var x = 0; x < width; x++) row[x] = result[x, y];
            row = FFT(row);
            for (var x = 0; x < width; x++) result[x, y] = row[x];
        });

        Parallel.For(0, width, x =>
        {
            var col = new Complex[height];
            for (var y = 0; y < height; y++) col[y] = result[x, y];
            col = FFT(col);
            for (var y = 0; y < height; y++) result[x, y] = col[y];
        });

        return result;
    }

    private static Complex[,] InverseFourier2D(Complex[,] data, int width, int height)
    {
        var result = (Complex[,])data.Clone();

        Parallel.For(0, height, y =>
        {
            var row = new Complex[width];
            for (var x = 0; x < width; x++) row[x] = result[x, y];
            row = FFTInverse(row);
            for (var x = 0; x < width; x++) result[x, y] = row[x];
        });

        Parallel.For(0, width, x =>
        {
            var col = new Complex[height];
            for (var y = 0; y < height; y++) col[y] = result[x, y];
            col = FFTInverse(col);
            for (var y = 0; y < height; y++) result[x, y] = col[y];
        });

        // Нормализация
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            result[x, y] /= width * height;

        return result;
    }

    private static Complex[] FFT(Complex[] data)
    {
        var n = data.Length;
        if (n <= 1) return data;

        var even = new Complex[n / 2];
        var odd = new Complex[n / 2];
        for (var i = 0; i < n / 2; i++)
        {
            even[i] = data[2 * i];
            odd[i] = data[2 * i + 1];
        }

        even = FFT(even);
        odd = FFT(odd);

        var result = new Complex[n];
        for (var k = 0; k < n / 2; k++)
        {
            var t = Complex.Exp(new Complex(0, -2 * Math.PI * k / n)) * odd[k];
            result[k] = even[k] + t;
            result[k + n / 2] = even[k] - t;
        }

        return result;
    }

    private static Complex[] FFTInverse(Complex[] data)
    {
        var n = data.Length;
        if (n <= 1) return data;

        var even = new Complex[n / 2];
        var odd = new Complex[n / 2];
        for (var i = 0; i < n / 2; i++)
        {
            even[i] = data[2 * i];
            odd[i] = data[2 * i + 1];
        }

        even = FFTInverse(even);
        odd = FFTInverse(odd);

        var result = new Complex[n];
        for (var k = 0; k < n / 2; k++)
        {
            var t = Complex.Exp(new Complex(0, 2 * Math.PI * k / n)) * odd[k];
            result[k] = even[k] + t;
            result[k + n / 2] = even[k] - t;
        }

        return result;
    }
}