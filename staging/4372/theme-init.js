// Runs synchronously in <head> to prevent flash of wrong theme
let storedTheme = null;
let preferredTheme = 'light';

try {
  storedTheme = localStorage.getItem('theme');
  preferredTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
} catch {
  // Fall back to a safe default when storage or media queries are unavailable.
}

const activeTheme = storedTheme || preferredTheme;
document.documentElement.setAttribute('data-theme', activeTheme);
document.addEventListener('DOMContentLoaded', () => {
  const btn = document.getElementById('theme-toggle');
  if (btn) btn.setAttribute('aria-pressed', activeTheme === 'dark' ? 'true' : 'false');
});
