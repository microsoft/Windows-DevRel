using Microsoft.ML.Data;

namespace Libs.SemanticSearch.MiniLM
{
    internal class MiniLML6v2Output
    {
        // Dimensions: batch, sequence, hidden_size
        [VectorType(1, 7, 384)]
        [ColumnName("last_hidden_state")]
        public float[] Embedding { get; set; }
    }
}
