Write a factual article for a computer‑museum website about the hardware machine specified at the end of this prompt. 
This includes computers, consoles, tablets, PDAs, arcade boards, GPUs, sound synthesizers, and other classes of computing hardware.

Use only verifiable information from Wikipedia, period magazines, manuals, corporate filings, and reputable computer‑museum or archival websites.
Do not invent or infer any detail that is not explicitly documented in the sources.
We are writing a new article, not copying Wikipedia, so use a variety of sources and do not rely too heavily on any single one.

Museum‑grade multi‑source requirement:
The article must synthesize information from multiple independent, reputable sources, such as period magazines, manuals, archival documents, corporate filings, and museum collections. No single source may dominate the narrative, structure, or factual basis of the article.
When multiple sources disagree, present only what is verifiably documented and avoid resolving contradictions unless a source explicitly does so.
If all surviving information originates from a single source, you must state this explicitly in the article and restrict the content to what that source documents, without extrapolation or inference.
The resulting article must read as a historical synthesis, not a reformatted version of any one reference.

The article should include the following topics when information is available:
- History and origins of the machine
- Design: what the machine is, how it works, and why its manufacturer chose that design
- Component decisions made by the manufacturer
- Reasons the manufacturer created it
- Announcement: how, when, and where it was announced
- Launch: how, when, and where it was launched
- Impact at announcement, at launch, throughout its commercial life, and in later historical perspective
- Major technological, architectural, or business innovations introduced by the machine
- Technical specifications (tables/schematics allowed only here)
- Variants (only if it is a family/series)
- Standards the machine followed, contributed to, or introduced
- Launch price in different markets
- Weight and dimensions (only if documented)
- Software ecosystem
- Legacy

Citation and reference requirements (STRICT):
- All citations must be inline Markdown reference-style citations, e.g. “...released in 1982.[^ref1]”
- Each citation must correspond to a single entry in a “References” section at the end of the article.
- Each entry must be a Markdown reference definition of the form:
  [^ref1]: https://example.com/page
- Do not include bibliographic metadata, titles, authors, or publication details.
- Do not wrap citation sentences in Markdown link syntax.
- Every reference must contain a real URL. No placeholders, no empty links.
- Only one URL per reference key.
- Do not include any references not explicitly cited in the article body.
- Do not include uncited references.

Uniqueness and deduplication rules (VERY STRICT):
- Before writing the article, internally deduplicate all sources.
- The References section must contain no repeated URLs, even if cited multiple times.
- If multiple statements rely on the same source, they must all cite the same footnote key.
- Do not create new footnote keys for the same URL.
- The total number of references must equal the number of unique URLs used.
- Do not generate more than one reference entry for any single URL, domain, or page.
- Treat URLs that differ only by parameters, fragments, or tracking codes as identical; use only the canonical one.

General requirements:
- Prose for all sections except Technical Specifications.
- Omit any section with no documented information.
- Clean, raw Markdown. No emojis. No images.
- Focus strictly on the specific machine requested.

Now write the article about: XXXXXXXXXXXX
