// Theme toggle
const toggle = document.getElementById('theme-toggle');
toggle.addEventListener('click', () => {
  const current = document.documentElement.getAttribute('data-theme');
  const next = current === 'dark' ? 'light' : 'dark';
  document.documentElement.setAttribute('data-theme', next);
  try {
    localStorage.setItem('theme', next);
  } catch {
    // Ignore storage failures so the theme toggle still works.
  }
  toggle.setAttribute('aria-pressed', next === 'dark' ? 'true' : 'false');
});

// Copy code buttons
document.querySelectorAll('.copy-code-btn').forEach(button => {
  button.addEventListener('click', async () => {
    const source = document.getElementById(button.dataset.copyTarget);
    if (!source)
      return;

    const label = button.querySelector('span');
    const original = label?.textContent || 'Copy';

    try {
      await navigator.clipboard.writeText(source.textContent);
      if (label) label.textContent = 'Copied!';
    } catch {
      if (label) label.textContent = 'Copy failed';
    }

    setTimeout(() => {
      if (label) label.textContent = original;
    }, 1500);
  });
});

// Expand/collapse code buttons
document.querySelectorAll('.expand-code-btn').forEach(button => {
  button.addEventListener('click', () => {
    const target = document.getElementById(button.dataset.expandTarget);
    if (!target)
      return;

    const expanded = target.classList.toggle('is-expanded');
    target.classList.toggle('is-collapsed', !expanded);
    button.setAttribute('aria-expanded', expanded ? 'true' : 'false');
    button.textContent = expanded ? 'Show less' : 'See full code';
  });
});
