using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowerCosineTransform;

/// <summary>
/// Encoded image model object to simplify parameter passing
/// </summary>
internal class EncodedImage
{
    public List<List<(byte, short)>> RedDCTCoefficientBlocks;
    public List<List<(byte, short)>> GreenDCTCoefficientBlocks;
    public List<List<(byte, short)>> BlueDCTCoefficientBlocks;
}
