using Microsoft.ML.Data;

namespace Libs.SemanticSearch.MiniLM
{
    public class MiniLML6v2Input
    {
        // Dimensions: batch, sequence
        [VectorType(1, 256)]
        [ColumnName("input_ids")]
        public long[] InputIds { get; set; }

        // Dimensions: batch, sequence
        [VectorType(1, 256)]
        [ColumnName("attention_mask")]
        public long[] AttentionMask { get; set; }

        // Dimensions: batch, sequence
        [VectorType(1, 256)]
        [ColumnName("token_type_ids")]
        public long[] TokenTypeIds { get; set; }
    }
}
