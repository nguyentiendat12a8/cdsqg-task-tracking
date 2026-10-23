export function aiFeedType(feedUrl, origin = '') {
  try {
    const host = new URL(feedUrl).hostname.toLowerCase();
    if (host === 'news.google.com') return 'Google News';
    if (host === 'www.google.com' || host === 'google.com') return 'Google Alerts';
  } catch {}
  if (origin.startsWith('Google News')) return 'Google News';
  if (origin.startsWith('Google Alerts')) return 'Google Alerts';
  return 'RSS trực tiếp';
}
