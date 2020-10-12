using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace NetAutumnClassLibrary
{
    public class SingleImageProcessor
    {

        readonly Prediction mPrediction;

        public Prediction GetPrediction()
        {
            return mPrediction;
        }

        public SingleImageProcessor(string imageFilePath)
        {
            Image<Rgb24> image = Image.Load<Rgb24>(imageFilePath, out IImageFormat format);

            Stream imageStream = new MemoryStream();
            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(224, 224),
                    Mode = ResizeMode.Crop
                });
            });
            image.Save(imageStream, format);

            Tensor<float> input = new DenseTensor<float>(new[] { 1, 3, 224, 224 });
            var mean = new[] { 0.485f, 0.456f, 0.406f };
            var stddev = new[] { 0.229f, 0.224f, 0.225f };
            for (int y = 0; y < image.Height; y++)
            {
                Span<Rgb24> pixelSpan = image.GetPixelRowSpan(y);
                for (int x = 0; x < image.Width; x++)
                {
                    input[0, 0, y, x] = ((pixelSpan[x].R / 255f) - mean[0]) / stddev[0];
                    input[0, 1, y, x] = ((pixelSpan[x].G / 255f) - mean[1]) / stddev[1];
                    input[0, 2, y, x] = ((pixelSpan[x].B / 255f) - mean[2]) / stddev[2];
                }
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("data", input)
            };

            var session = new InferenceSession("./../../../NetAutumnClassLibrary/resnet50-v2-7.onnx");
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

            IEnumerable<float> output = results.First().AsEnumerable<float>();
            float sum = output.Sum(x => (float)Math.Exp(x));

            IEnumerable<float> softmax = output.Select(x => (float)Math.Exp(x) / sum);

            IEnumerable<Prediction> topResult = softmax.Select((x, i) => new Prediction { Path = imageFilePath, Label = LabelMap.Labels[i], Confidence = x })
                                       .OrderByDescending(x => x.Confidence);
            
            mPrediction = topResult.First();
        }
    }
}