using TorchSharp;

namespace Libs.SemanticSearch
{
    public static class Normalization
    {
        public static float[] Normalize( torch.Tensor input, float p = 2f, int dim = -1, bool keep = true, float eps = 1e-12f)
        {
            torch.Tensor denom;
            if (keep)
            {
                denom = input.norm(dim, keep, p).clamp_min(eps).expand_as(input);
            }
            else
            {
                denom = input.norm(dim, keep, p).clamp_min(eps);
            }
            return (input / denom).data<float>().ToArray();
        }
    }
}
