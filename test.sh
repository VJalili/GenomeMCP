#!/bin/bash

# We use -s to hide the download progress bar, so you only see the response.
# We pass the exact query and variables as a JSON payload.

curl -s -X POST "https://gnomad.broadinstitute.org/api" \
  -H "Content-Type: application/json" \
  -H "User-Agent: Terminal-Debug-Client" \
  -d '{
  "query": "query VariantsInGene($symbol: String!) { gene(gene_symbol: $symbol, reference_genome: GRCh38) { gene_id gene_symbol variants(dataset: gnomad_r4) { variant_id pos exome { ac ac_hemi ac_hom an af } } } }",
  "variables": {
    "symbol": "BRCA1"
  }
}'