using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

[Route("api/models")]
[ApiController]
public class ModelsAPIController : ControllerBase
{

    private readonly ILogger<ModelsAPIController> _logger;
    private readonly IHostEnvironment _hostingEnvironment;

    public ModelsAPIController(ILogger<ModelsAPIController> logger, IHostEnvironment hostingEnvironment)
    {
        _logger = logger;
        _hostingEnvironment = hostingEnvironment;
    }


    private static float RgbToGray(Color pixel) => 0.299f * pixel.R + 0.587f * pixel.G + 0.114f * pixel.B;
    private static float[][] PreprocessTestImage(Bitmap img)
    {
        if (img == null)
        {
            throw new ArgumentNullException(nameof(img));
        }
        var result = new float[img.Width][];

        for (int i = 0; i < img.Width; i++)
        {
            result[i] = new float[img.Height];
            for (int j = 0; j < img.Height; j++)
            {
                var pixel = img.GetPixel(i, j);

                var gray = RgbToGray(pixel);

                // Normalize the Gray value to 0-1 range
                var normalized = gray / 255;

                result[i][j] = normalized;
            }
        }
        return result;
    }
    private static float[] Predict(string modelPath, float[][] image)
    {
        using var session = new InferenceSession(modelPath);
        var modelInputLayerName = session.InputMetadata.Keys.Single();

        var imageFlattened = image.SelectMany(x => x).ToArray();
        int[] dimensions = { 1, 28, 28 };
        var inputTensor = new DenseTensor<float>(imageFlattened, dimensions);
        var modelInput = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(modelInputLayerName, inputTensor)
            };

        var result = session.Run(modelInput);
        return ((DenseTensor<float>)result.Single().Value).ToArray();
    }


    [HttpPost("mnist")]
    public async Task<IActionResult> mnist()
    {
        // check if the request contains a file
        if (!Request.HasFormContentType)
        {
            return BadRequest();
        }

        // get the image file from the request
        Bitmap img;
        var file = Request.Form.Files.First();

        // var path = Path.Combine(Directory.GetCurrentDirectory(), "images", file.FileName);
        // using (var stream = new FileStream(path, FileMode.Create))
        // {
        //     await file.CopyToAsync(stream);
        // }
        // string imagePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Resources", @"mnist_test_eight.png");
        // img = new Bitmap(imagePath);
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            img = new Bitmap(Image.FromStream(stream));
        }

        var resized = new Bitmap(28, 28);
        using (var g = Graphics.FromImage(resized))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, 0, 0, 28, 28);
        }
        float[][] image = PreprocessTestImage(resized);

        string modelPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Resources", @"mnist-model.onnx");
        float[] probabilities = Predict(modelPath, image);

        // The predicted number is the index of the largest value(probability) in the array.
        int prediction = probabilities.ToList().IndexOf(probabilities.Max());
        return Ok($"Predicted number: {prediction}");
    }
}
