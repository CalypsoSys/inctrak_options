import test from 'node:test';
import assert from 'node:assert/strict';
import { existsSync, readFileSync } from 'node:fs';

function read(path) {
  return readFileSync(new URL(path, import.meta.url), 'utf8');
}

function exists(path) {
  return existsSync(new URL(path, import.meta.url));
}

test('inctrak.com loads the matrixease-style site runtime', () => {
  const html = read('../../inctrak.com/index.html');

  assert.match(html, /<script src="js\/site\.js"><\/script>/);
  assert.match(html, /class="display-3 load-reveal reveal-fade-up"/);
  assert.match(html, /class="display-4 scroll-reveal reveal-up"/);
  assert.doesNotMatch(html, /jquery-2\.2\.3\.min\.js/);
  assert.doesNotMatch(html, /bootstrap\.js/);
  assert.doesNotMatch(html, /popper\.js/);
  assert.doesNotMatch(html, /wow\.min\.js/);
  assert.doesNotMatch(html, /css\/animate\.css/);
});

test('inctrak.com sticky header matches matrixease about-section trigger', () => {
  const script = read('../../inctrak.com/js/site.js');

  assert.match(script, /\(function \(global\) \{/);
  assert.match(script, /function shouldEnableSticky\(scrollY, triggerTop, offset\)/);
  assert.match(script, /var trigger = global\.document\.querySelector\("\.aboutUs"\);/);
  assert.match(script, /var triggerTop = trigger\.getBoundingClientRect\(\)\.top \+ global\.scrollY;/);
  assert.match(script, /stickyHost\.classList\.toggle\(\s*"sticky",\s*shouldEnableSticky\(global\.scrollY, triggerTop, 300\)\s*\);/);
});

test('inctrak.com reveal animations match matrixease transition behavior', () => {
  const script = read('../../inctrak.com/js/site.js');
  const css = read('../../inctrak.com/style.css');

  assert.match(script, /function parseTimingValue\(value, fallbackSeconds\)/);
  assert.match(script, /function shouldAnimateEntry\(entry\)/);
  assert.match(script, /function applyRevealState\(element\)/);
  assert.match(script, /function applyLoadRevealState\(element\)/);
  assert.match(script, /var loadAnimatedItems = global\.document\.querySelectorAll\("\.load-reveal"\);/);
  assert.match(script, /var animatedItems = global\.document\.querySelectorAll\("\.scroll-reveal"\);/);
  assert.match(script, /if \(!global\.IntersectionObserver\) \{/);
  assert.match(script, /item\.style\.visibility = "hidden";/);
  assert.match(script, /parseTimingValue\(element\.dataset\.revealDelay, 0\) \+ "ms";/);
  assert.match(script, /parseTimingValue\(element\.dataset\.loadDuration, 0\.75\) \+ "ms";/);
  assert.match(script, /var observer = new global\.IntersectionObserver\(/);
  assert.match(script, /if \(!shouldAnimateEntry\(entry\)\) \{/);
  assert.match(script, /rootMargin: "0px 0px -12% 0px"/);
  assert.match(css, /\.load-reveal,\s*\.scroll-reveal \{/);
  assert.match(css, /\.reveal-fade-up \{/);
  assert.match(css, /\.reveal-left \{/);
  assert.match(css, /\.reveal-right \{/);
});

test('inctrak.com contact form uses the marketing contact-us message type and neutral success copy', async () => {
  const runtime = await import(new URL('../../inctrak.com/js/site.js', import.meta.url));
  const payload = runtime.buildFeedbackPayload({
    emailAddress: 'founder@example.test',
    name: 'Founder',
    subject: 'Need help',
    message: 'Please contact me.'
  });

  assert.equal(payload.MessageTypeFk, '8');
  assert.equal(
    runtime.getFeedbackSuccessMessage({ success: true, message: 'Thanks for the compliment, Founder' }),
    'Your message has been sent. We will reach out soon.'
  );
});

test('docs pages load the shared vanilla docs script', () => {
  const pages = [
    '../../docs.inctrak.com/index.html',
    '../../docs.inctrak.com/quick.html',
    '../../docs.inctrak.com/components.html',
    '../../docs.inctrak.com/faqs.html',
    '../../docs.inctrak.com/showcase.html',
    '../../docs.inctrak.com/videos.html',
    '../../docs.inctrak.com/license.html'
  ];

  for (const page of pages) {
    const html = read(page);

    assert.match(html, /<script src="assets\/js\/main\.js"><\/script>/);
    assert.doesNotMatch(html, /jquery-1\.12\.3/);
    assert.doesNotMatch(html, /jquery-scrollTo/);
    assert.doesNotMatch(html, /matchHeight/);
    assert.doesNotMatch(html, /ekko-lightbox/);
    assert.doesNotMatch(html, /prism/);
  }
});

test('public IncTrak sites publish SEO metadata, robots, and sitemaps', () => {
  const pages = [
    {
      htmlPath: '../../inctrak.com/index.html',
      robotsPath: '../../inctrak.com/robots.txt',
      sitemapPath: '../../inctrak.com/sitemap.xml',
      canonical: 'https://inctrak.com/',
      sitemapHost: 'https://inctrak.com/'
    },
    {
      htmlPath: '../../docs.inctrak.com/index.html',
      robotsPath: '../../docs.inctrak.com/robots.txt',
      sitemapPath: '../../docs.inctrak.com/sitemap.xml',
      canonical: 'https://docs.inctrak.com/',
      sitemapHost: 'https://docs.inctrak.com/'
    },
    {
      htmlPath: '../../frontend/index.html',
      robotsPath: '../../frontend/public/robots.txt',
      sitemapPath: '../../frontend/public/sitemap.xml',
      canonical: 'https://shared.inctrak.com/',
      sitemapHost: 'https://shared.inctrak.com/'
    },
    {
      htmlPath: '../../frontend-signup/index.html',
      robotsPath: '../../frontend-signup/public/robots.txt',
      sitemapPath: '../../frontend-signup/public/sitemap.xml',
      canonical: 'https://signup.inctrak.com/',
      sitemapHost: 'https://signup.inctrak.com/'
    },
    {
      htmlPath: '../../frontend-vesting/index.html',
      robotsPath: '../../frontend-vesting/public/robots.txt',
      sitemapPath: '../../frontend-vesting/public/sitemap.xml',
      canonical: 'https://vesting.inctrak.com/',
      sitemapHost: 'https://vesting.inctrak.com/'
    },
    {
      htmlPath: '../../blog.inctrak.com/index.html',
      robotsPath: '../../blog.inctrak.com/robots.txt',
      sitemapPath: '../../blog.inctrak.com/sitemap.xml',
      canonical: 'https://blog.inctrak.com/',
      sitemapHost: 'https://blog.inctrak.com/'
    }
  ];

  for (const page of pages) {
    const html = read(page.htmlPath);
    const robots = read(page.robotsPath);
    const sitemap = read(page.sitemapPath);

    assert.match(html, /<meta\s+[^>]*name="description"[^>]*content="[^"]+"|<meta\s+[^>]*content="[^"]+"[^>]*name="description"/s);
    assert.match(html, /<meta name="robots" content="index, follow"/);
    assert.match(html, new RegExp(`<link rel="canonical" href="${page.canonical.replace(/\./g, '\\.')}"`));
    assert.match(html, /<meta property="og:title" content="[^"]+"/);
    assert.match(html, /<meta property="og:url" content="https:\/\/[^"]+"/);
    assert.match(html, /<meta name="twitter:card" content="summary_large_image"/);
    assert.match(robots, /User-agent: \*/);
    assert.match(robots, /Sitemap: https:\/\//);
    assert.match(sitemap, new RegExp(`<loc>${page.sitemapHost.replace(/\./g, '\\.')}`));
  }
});

test('docs pages have page-specific canonical metadata without blank descriptions', () => {
  const pages = [
    ['../../docs.inctrak.com/quick.html', 'https://docs.inctrak.com/quick.html'],
    ['../../docs.inctrak.com/components.html', 'https://docs.inctrak.com/components.html'],
    ['../../docs.inctrak.com/faqs.html', 'https://docs.inctrak.com/faqs.html'],
    ['../../docs.inctrak.com/showcase.html', 'https://docs.inctrak.com/showcase.html'],
    ['../../docs.inctrak.com/videos.html', 'https://docs.inctrak.com/videos.html'],
    ['../../docs.inctrak.com/license.html', 'https://docs.inctrak.com/license.html']
  ];

  for (const [path, canonical] of pages) {
    const html = read(path);

    assert.match(html, new RegExp(`<link rel="canonical" href="${canonical.replace(/\./g, '\\.')}"`));
    assert.doesNotMatch(html, /<meta name="description" content="">/);
    assert.match(html, /<meta property="og:image" content="https:\/\/inctrak\.com\/img\/dashboard\.jpg">/);
  }
});

test('SEO files exist for the public IncTrak host roots', () => {
  const paths = [
    '../../inctrak.com/robots.txt',
    '../../inctrak.com/sitemap.xml',
    '../../docs.inctrak.com/robots.txt',
    '../../docs.inctrak.com/sitemap.xml',
    '../../frontend/public/robots.txt',
    '../../frontend/public/sitemap.xml',
    '../../frontend-signup/public/robots.txt',
    '../../frontend-signup/public/sitemap.xml',
    '../../frontend-vesting/public/robots.txt',
    '../../frontend-vesting/public/sitemap.xml',
    '../../blog.inctrak.com/index.html',
    '../../blog.inctrak.com/robots.txt',
    '../../blog.inctrak.com/sitemap.xml'
  ];

  for (const path of paths) {
    assert.equal(exists(path), true);
  }
});
