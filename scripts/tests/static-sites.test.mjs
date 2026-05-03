import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

function read(path) {
  return readFileSync(new URL(path, import.meta.url), 'utf8');
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
