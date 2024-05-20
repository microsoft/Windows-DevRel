using TorchSharp;

namespace Libs.SemanticSearch
{
    public static class Similarity
    {
        public static (torch.Tensor Values, torch.Tensor Indexes) TopKByCosineSimilarity(
            torch.Tensor corpus,
            torch.Tensor query,
            int limit)
        {
            // Cosine similarity of two tensors of different dimensions.
            // cos_sim(a, b) = dot_product(a_norm, transpose(b_norm))
            // a_norm and b_norm are L2 norms of the tensors.
            var corpusNorm = corpus / corpus.norm(1).unsqueeze(-1);
            var queryNorm = query / query.norm(1).unsqueeze(-1);
            var similar = queryNorm.mm(corpusNorm.transpose(0, 1));

            // Compute top K values in the similarity result and return the
            // values and indexes of the elements.
            return similar.topk(limit);
        }

        // Calculates cosine similarity between two tensors
        public static float CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            torch.Tensor A = torch.tensor(
                    vectorA,
                    [1, vectorA.Length]);
            torch.Tensor B = torch.tensor(
                    vectorB,
                    [1, vectorB.Length]);

            var dotProduct = (A * B).sum().ToSingle();
            var normA = A.pow(2).sum().sqrt().ToSingle();
            var normB = B.pow(2).sum().sqrt().ToSingle();
            return dotProduct / (normA * normB);
        }

        public static float DotProduct(float[] array1, float[] array2)
        {
            float result = 0f;
            for (int x = 0; x < array1.Length; x++)
            {
                result += array1[x] * array2[x];
            }

            return result;
        }
    }
}
