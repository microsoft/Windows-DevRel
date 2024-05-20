using TorchSharp;

namespace Libs.SemanticSearch
{
    public static class Pooling
    {
        public static torch.Tensor MeanPooling(float[] embeddings, long[] attentionMask, long batchSize, long sequence)
        {
            var hiddenSize = 384L;

            // See https://huggingface.co/sentence-transformers/msmarco-distilbert-base-v3#usage-huggingface-transformers
            // Note how the python code below translates to dotnet, thanks to the
            // awesome TorchSharp library.
            //
            // def mean_pooling(model_output, attention_mask):
            //  # First element of model_output contains all token embeddings
            //  token_embeddings = model_output[0]
            //  input_mask_expanded = attention_mask.unsqueeze(-1).expand(token_embeddings.size()).float()
            //  sum_embeddings = torch.sum(token_embeddings * input_mask_expanded, 1)
            //  sum_mask = torch.clamp(input_mask_expanded.sum(1), min=1e-9)
            //  return sum_embeddings / sum_mask
            try
            {
                var tokenEmbeddings = torch.tensor(
                        embeddings,
                        [batchSize, sequence, hiddenSize]);
                var attentionMaskExpanded = torch.tensor(
                        attentionMask,
                        [batchSize, sequence])
                    .unsqueeze(-1).expand(tokenEmbeddings.shape).@float();
                var sumEmbeddings = (tokenEmbeddings * attentionMaskExpanded).sum(new[] { 1L });
                var sumMask = attentionMaskExpanded.sum(new[] { 1L }).clamp(1e-9, float.MaxValue);

                return sumEmbeddings / sumMask;
            }
            catch
            {
                return null;
            }
        }
    }
}
