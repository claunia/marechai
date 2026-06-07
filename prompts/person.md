Write a factual article for a computer‑museum website about the person specified at the end of this prompt. 
This includes engineers, programmers, designers, researchers, executives, inventors, and other individuals relevant to computing history.

Use only verifiable information from Wikipedia, period magazines, manuals, corporate filings, interviews, academic papers, and reputable computer‑museum or archival websites.
Do not invent or infer any detail that is not explicitly documented in the sources.
We are writing a new article, not copying Wikipedia, so use a variety of sources and do not rely too heavily on any single one.

Museum‑grade multi‑source requirement:
The article must synthesize information from multiple independent, reputable sources, such as period journalism, manuals, archival documents, corporate filings, interviews, and museum collections. No single source may dominate the narrative, structure, or factual basis of the article.
When multiple sources disagree, present only what is verifiably documented and avoid resolving contradictions unless a source explicitly does so.
If all surviving information originates from a single source, you must state this explicitly in the article and restrict the content to what that source documents, without extrapolation or inference.
The resulting article must read as a historical synthesis, not a reformatted version of any one reference.

The article should include the following topics when information is available:
- Early life and background (only if documented)
- Education and formative influences
- Entry into computing or related fields
- Key roles, positions, or affiliations
- Major projects, products, or research contributions
- Motivations, design philosophies, or technical approaches (only if documented)
- Announcement and launch details for major works (how, when, where)
- Impact at the time of their contributions and in later historical perspective
- Influence on computing history, markets, users, competitors, or standards
- Collaborators, teams, or organizations they worked with
- Awards, recognition, or notable public commentary (only if documented)
- Later career, transformation, or retirement (if applicable)
- Legacy and long‑term historical significance

Citation and reference requirements (STRICT):
- All citations must be inline Markdown reference-style citations, e.g. “...she joined the project in 1984.[^ref1]”
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
- Use continuous prose; no schematics.
- Tables are allowed when appropriate (e.g., timelines, roles, affiliations).
- If information is unavailable, omit the section entirely—do not speculate.
- Use clear, separated sections; you may choose the section titles.
- Use common, professional language suitable for a museum audience; avoid unnecessary technical jargon.
- Output clean, raw Markdown suitable for direct publication.
- No emojis. No images.
- Focus strictly on the specific person requested. Do not discuss unrelated individuals unless directly relevant to the subject’s documented work.

Now write the article about: XXXXXXXXXXXX
