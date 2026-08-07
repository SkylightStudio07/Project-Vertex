# Pull request review instructions

When reviewing pull requests:

- Write all review comments in Korean.
- Leave comments on the relevant changed lines.
- Focus on correctness, performance, security, memory allocation, and maintainability.
- For Unity C# code, check for:
  - GC allocations inside Update, FixedUpdate, and LateUpdate
  - unnecessary string interpolation and ToString calls per frame
  - repeated SetActive calls
  - missing MonoBehaviour and serialized-field null handling
  - expensive GetComponent or Find calls in frequently called methods
  - lifecycle and initialization-order problems
- Assign a severity to each issue:
  - Critical
  - High Priority
  - Medium Priority
  - Low Priority
- Explain why the issue matters.
- Provide a concrete fix or code example.
- Do not comment on cosmetic formatting handled by automated formatters.