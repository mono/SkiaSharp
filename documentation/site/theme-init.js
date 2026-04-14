// Runs synchronously in <head> to prevent flash of wrong theme
const storedTheme = localStorage.getItem('theme');
const preferredTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
const activeTheme = storedTheme || preferredTheme;
document.documentElement.setAttribute('data-theme', activeTheme);
document.addEventListener('DOMContentLoaded', () => {
  const btn = document.getElementById('theme-toggle');
  if (btn) btn.setAttribute('aria-pressed', activeTheme === 'dark' ? 'true' : 'false');
});
