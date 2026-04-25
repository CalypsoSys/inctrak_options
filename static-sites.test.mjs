import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

function read(path) {
  return readFileSync(new URL(path, import.meta.url), 'utf8');
}

test('inctrak.com loads only the local replacement script', () => {
  const html = read('./inctrak.com/index.html');

  assert.match(html, /<script src="js\/script\.js"><\/script>/);
  assert.doesNotMatch(html, /jquery-2\.2\.3\.min\.js/);
  assert.doesNotMatch(html, /bootstrap\.js/);
  assert.doesNotMatch(html, /popper\.js/);
  assert.doesNotMatch(html, /wow\.min\.js/);
});

test('inctrak.com sticky header activates when the home hero leaves the viewport', () => {
  const script = read('./inctrak.com/js/script.js');

  assert.match(script, /const homeHeader = document\.querySelector\('header#home'\);/);
  assert.match(script, /stickyContainer\.classList\.toggle\('sticky', !entry\.isIntersecting && entry\.boundingClientRect\.bottom <= 0\);/);
});

test('inctrak.com uses observer-based reveals for non-hero animations', () => {
  const script = read('./inctrak.com/js/script.js');

  assert.match(script, /const heroElements = Array\.from\(animatedElements\)\.filter\(\(element\) => element\.closest\('header#home'\)\);/);
  assert.match(script, /const scrollRevealElements = Array\.from\(animatedElements\)\.filter\(\(element\) => !element\.closest\('header#home'\)\);/);
  assert.match(script, /const revealObserver = new IntersectionObserver\(/);
  assert.match(script, /observer\.unobserve\(entry\.target\);/);
});

test('docs pages load the shared vanilla docs script', () => {
  const pages = [
    './docs.inctrak.com/index.html',
    './docs.inctrak.com/quick.html',
    './docs.inctrak.com/components.html',
    './docs.inctrak.com/faqs.html',
    './docs.inctrak.com/showcase.html',
    './docs.inctrak.com/videos.html',
    './docs.inctrak.com/license.html'
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
