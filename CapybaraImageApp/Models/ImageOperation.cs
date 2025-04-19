using System.Diagnostics.CodeAnalysis;

namespace CapybaraImageApp.Models;

public enum ImageOperation
{
    Sum, 
    Multiply,
    Average,
    Min,
    Max,
    Mask,
    Median,
    Gaussian
}